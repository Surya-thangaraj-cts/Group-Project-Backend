using UserApi.Models;

namespace UserApi.Repositories;

public interface IApprovalRepository
{
    Task<IEnumerable<Approval>> GetAllAsync();
    Task<Approval?> GetByIdAsync(string id);
    Task<bool> ApprovalIdExistsAsync(string approvalId);
    Task<Approval> AddAsync(Approval approval);
    Task<Approval> UpdateAsync(Approval approval);
    Task<bool> DeleteAsync(string id);
    Task<Approval?> GetByTransactionIdAsync(string transactionId);
    Task<Approval?> GetPendingAccountApprovalAsync(string accountId);
    Task<IEnumerable<Approval>> GetAllWithDetailsAsync();
}