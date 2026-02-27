using UserApi.DTOs;
using UserApi.Models;

namespace UserApi.Services;

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetAllNotificationsAsync(string? type = null, string? status = null);
    Task<Notification?> GetNotificationByIdAsync(string id);
    Task<Notification> UpdateNotificationStatusAsync(string id, UpdateNotificationStatusDto dto);
    Task DeleteNotificationAsync(string id);
    Task CreateNotificationForHighValueTransactionAsync(string transactionId, int userId, decimal amount, string transactionType);
    Task CreateNotificationForApprovalAsync(string approvalId, int userId, ApprovalType approvalType);
    Task DeleteNotificationByApprovalIdAsync(string approvalId);
    Task DeleteNotificationByTransactionIdAsync(string transactionId);
    Task DeleteAllNotificationsAsync();
}