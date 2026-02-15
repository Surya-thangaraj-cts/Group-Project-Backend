using UserApi.DTOs;
using UserApi.Models;

namespace UserApi.Services;

public interface IApprovalService
{
    Task<PagedResult<Approval>> GetAllApprovalsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? decision = null,
        string? type = null);
    Task<Approval?> GetApprovalByIdAsync(int id);
    Task<Approval> ProcessApprovalDecisionAsync(int id, UpdateApprovalDto dto);
    Task<PagedResult<ApprovalDetailsDto>> GetAllApprovalDetailsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? decision = null,
        string? type = null);
}