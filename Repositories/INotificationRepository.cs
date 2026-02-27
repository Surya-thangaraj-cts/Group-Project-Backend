namespace UserApi.Repositories;
using UserApi.Models;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllAsync();
    Task<Notification?> GetByIdAsync(string id);
    Task<bool> NotificationIdExistsAsync(string notificationId);
    Task<Notification> AddAsync(Notification notification);
    Task<Notification> UpdateAsync(Notification notification);
    Task DeleteAsync(string id);
    Task DeleteByApprovalIdAsync(string approvalId);
    Task DeleteByTransactionIdAsync(string transactionId);
    Task DeleteAllAsync();
}