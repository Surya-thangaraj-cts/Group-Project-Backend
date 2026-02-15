using UserApi.DTOs;
using UserApi.Models;

namespace UserApi.Services;

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetAllNotificationsAsync(string? type = null, string? status = null);
    Task<Notification?> GetNotificationByIdAsync(int id);
    Task<Notification> UpdateNotificationStatusAsync(int id, UpdateNotificationStatusDto dto);
    Task DeleteNotificationAsync(int id);
    Task CreateNotificationForHighValueTransactionAsync(int transactionId, int userId, decimal amount, string transactionType);
    Task CreateNotificationForApprovalAsync(int approvalId, int userId, ApprovalType approvalType);
    Task DeleteNotificationByApprovalIdAsync(int approvalId);
    Task DeleteNotificationByTransactionIdAsync(int transactionId);
    Task DeleteAllNotificationsAsync();
}