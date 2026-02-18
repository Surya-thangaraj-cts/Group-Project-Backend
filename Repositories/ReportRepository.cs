using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserApi.Models;
using UserApprovalApi.Data;
using UserApprovalApi.DTOs;

namespace UserApprovalApi.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Report?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Reports
                .AsNoTracking()
                .Include(r => r.Account)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task<IEnumerable<Report>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Reports
                .AsNoTracking()
                .Include(r => r.Account)
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Report>> GetFilteredAsync(ReportFilterDto filter, CancellationToken ct = default)
        {
            var query = _context.Reports
                .AsNoTracking()
                .Include(r => r.Account)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.ReportType))
                query = query.Where(r => r.ReportType == filter.ReportType);

            if (filter.AccountId.HasValue)
                query = query.Where(r => r.AccountId == filter.AccountId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(r => r.GeneratedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(r => r.GeneratedAt <= filter.ToDate.Value);

            query = query.OrderByDescending(r => r.GeneratedAt);

            var skip = (filter.PageNumber - 1) * filter.PageSize;
            return await query.Skip(skip).Take(filter.PageSize).ToListAsync(ct);
        }

        public async Task<IEnumerable<Report>> GetByUserAsync(string userId, CancellationToken ct = default)
        {
            return await _context.Reports
                .AsNoTracking()
                .Include(r => r.Account)
                .Where(r => r.GeneratedByUserId == userId)
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync(ct);
        }

        public async Task<Report> CreateAsync(Report report, CancellationToken ct = default)
        {
            await _context.Reports.AddAsync(report, ct);
            await SaveChangesAsync(ct);
            return report;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var report = await _context.Reports.FindAsync(new object[] { id }, ct);
            if (report == null) return false;

            _context.Reports.Remove(report);
            await SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> GetTotalCountAsync(ReportFilterDto filter, CancellationToken ct = default)
        {
            var query = _context.Reports.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.ReportType))
                query = query.Where(r => r.ReportType == filter.ReportType);

            if (filter.AccountId.HasValue)
                query = query.Where(r => r.AccountId == filter.AccountId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(r => r.GeneratedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(r => r.GeneratedAt <= filter.ToDate.Value);

            return await query.CountAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
