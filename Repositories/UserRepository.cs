using Microsoft.EntityFrameworkCore;
using UserApi.DTOs;
using UserApprovalApi.Data;
using UserApprovalApi.Models;

namespace UserApprovalApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<User>> GetPendingUsersAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var query = _db.Users.Where(u => u.Status == UserStatus.Pending);
            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<User>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<PagedResult<User>> GetApprovedUsersAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var query = _db.Users.Where(u => u.Status != UserStatus.Pending);
            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<User>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<User?> GetByIdAsync(string userId, CancellationToken ct = default)
        {
            return await _db.Users.SingleOrDefaultAsync(u => u.UserId == userId, ct);
        }

        public async Task<bool> EmailExistsAsync(string email, string excludeUserId, CancellationToken ct = default)
        {
            return await _db.Users.AnyAsync(u => u.Email == email && u.UserId != excludeUserId, ct);
        }

        public Task UpdateAsync(User user, CancellationToken ct = default)
        {
            _db.Users.Update(user);
            return Task.CompletedTask;
        }

        //public Task SaveChangesAsync(CancellationToken ct = default)
        //{
        //    return _db.SaveChangesAsync(ct);
        //}

        public Task<bool> AnyByUserIdOrEmailAsync(string userId, string email, CancellationToken ct = default)
        {
            return _db.Users.AnyAsync(u => u.UserId == userId || u.Email == email, ct);
        }

        public Task<User?> GetByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return _db.Users.SingleOrDefaultAsync(u => u.UserId == userId, ct);
        }

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await _db.Users.AddAsync(user, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return _db.SaveChangesAsync(ct);
        }

        public async Task<List<User>> SearchApprovedUsersAsync(string query, CancellationToken ct = default)
        {
            return await _db.Users
                .Where(u => u.Status != UserStatus.Pending &&
                    (u.UserId.Contains(query) || u.Name.Contains(query) || u.Email.Contains(query)))
                .ToListAsync(ct);
        }

        public async Task<List<User>> SearchPendingUsersAsync(string query, CancellationToken ct = default)
        {
            return await _db.Users
                .Where(u => u.Status == UserStatus.Pending &&
                    (u.UserId.Contains(query) || u.Name.Contains(query) || u.Email.Contains(query) || u.Branch.Contains(query)))
                .ToListAsync(ct);
        }
    }
}




