using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserApi.Helpers;
using UserApprovalApi.DTOs;

namespace UserApprovalApi.Services
{
    public interface IReportService
    {
        Task<ReportDto?> GetReportByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<ReportDto>> GetAllReportsAsync(CancellationToken ct = default);
        Task<PagedResult<ReportDto>> GetFilteredReportsAsync(ReportFilterDto filter, CancellationToken ct = default);
        Task<IEnumerable<ReportDto>> GetReportsByUserAsync(string userId, CancellationToken ct = default);
        Task<ReportDto> GenerateReportAsync(CreateReportDto createDto, string userId, CancellationToken ct = default);
        Task<bool> DeleteReportAsync(int id, CancellationToken ct = default);
    }
}
