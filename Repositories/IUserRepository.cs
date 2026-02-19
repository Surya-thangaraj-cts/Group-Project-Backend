using UserApi.DTOs;
using UserApprovalApi.Models;

namespace UserApprovalApi.Repositories
{
    public interface IUserRepository
    {
        Task<PagedResult<User>> GetPendingUsersAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<PagedResult<User>> GetApprovedUsersAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<User?> GetByIdAsync(string userId, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, string excludeUserId, CancellationToken ct = default);
        Task UpdateAsync(User user, CancellationToken ct = default);
        //Task SaveChangesAsync(CancellationToken ct = default);


        Task<bool> AnyByUserIdOrEmailAsync(string userId, string email, CancellationToken ct = default);
        Task<User?> GetByUserIdAsync(string userId, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

        Task<List<User>> SearchApprovedUsersAsync(string query, CancellationToken ct = default);
        Task<List<User>> SearchPendingUsersAsync(string query, CancellationToken ct = default);
    }
}