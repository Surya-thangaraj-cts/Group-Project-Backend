namespace UserApi.Repositories;
using UserApi.Models;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllAsync();
    Task<Notification?> GetByIdAsync(int id);
    Task<Notification> AddAsync(Notification notification);
    Task<Notification> UpdateAsync(Notification notification);
    Task DeleteAsync(int id);
    Task DeleteByApprovalIdAsync(int approvalId);
    Task DeleteByTransactionIdAsync(int transactionId);
    Task DeleteAllAsync();
}