using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserApi.Models;
using UserApprovalApi.DTOs;

namespace UserApprovalApi.Repositories
{
    public interface IReportRepository
    {
        Task<Report?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<Report>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<Report>> GetFilteredAsync(ReportFilterDto filter, CancellationToken ct = default);
        Task<IEnumerable<Report>> GetByUserAsync(string userId, CancellationToken ct = default);
        Task<Report> CreateAsync(Report report, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<int> GetTotalCountAsync(ReportFilterDto filter, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
