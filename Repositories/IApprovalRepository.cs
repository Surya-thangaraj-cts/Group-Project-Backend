using UserApi.Models;

namespace UserApi.Repositories;

public interface IApprovalRepository
{
    Task<IEnumerable<Approval>> GetAllAsync();
    Task<Approval?> GetByIdAsync(int id);
    Task<Approval> AddAsync(Approval approval);
    Task<Approval> UpdateAsync(Approval approval);
    Task<bool> DeleteAsync(int id);
    Task<Approval?> GetByTransactionIdAsync(int transactionId);
    Task<Approval?> GetPendingAccountApprovalAsync(int accountId);
    Task<IEnumerable<Approval>> GetAllWithDetailsAsync(); // Add this method
}