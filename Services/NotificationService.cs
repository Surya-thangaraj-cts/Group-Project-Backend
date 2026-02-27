using UserApi.DTOs;
using UserApi.Models;
using UserApi.Repositories;
using UserApi.Services;

namespace AccountTrack.Api.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<Notification>> GetAllNotificationsAsync(string? type = null, string? status = null)
    {
        var notifications = await _notificationRepository.GetAllAsync();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(type))
        {
            notifications = notifications.Where(n =>
                n.Type.ToString().Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            notifications = notifications.Where(n =>
                n.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        return notifications;
    }

    public async Task<Notification?> GetNotificationByIdAsync(string id)
    {
        return await _notificationRepository.GetByIdAsync(id);
    }

    public async Task<Notification> UpdateNotificationStatusAsync(string id, UpdateNotificationStatusDto dto)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
        {
            throw new InvalidOperationException($"Notification with ID {id} not found.");
        }

        // Map status from DTO (0 = Unread, 1 = Read)
        notification.Status = dto.Status switch
        {
            0 => NotificationStatus.Unread,
            1 => NotificationStatus.Read,
            _ => throw new ArgumentException("Invalid notification status. Use 0 for Unread or 1 for Read.")
        };

        return await _notificationRepository.UpdateAsync(notification);
    }

    public async Task DeleteNotificationAsync(string id)
    {
        await _notificationRepository.DeleteAsync(id);
    }

    public async Task CreateNotificationForHighValueTransactionAsync(string transactionId, int userId, decimal amount, string transactionType)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = NotificationType.SuspiciousActivity,
            Message = $"High-value {transactionType} transaction of ₹{amount:N2} has been initiated and requires approval.",
            Status = NotificationStatus.Unread,
            CreatedDate = DateTime.UtcNow,
            TransactionId = transactionId,
            ApprovalId = null
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task CreateNotificationForApprovalAsync(string approvalId, int userId, ApprovalType approvalType)
    {
        var message = approvalType switch
        {
            ApprovalType.AccountCreation => "Pending approval for new account creation.",
            ApprovalType.AccountUpdate => "Pending approval for account update.",
            ApprovalType.HighValueTransaction => "Pending approval for high-value transaction.",
            _ => "Pending approval request."
        };

        var notification = new Notification
        {
            UserId = userId,
            Type = NotificationType.ApprovalReminder,
            Message = message,
            Status = NotificationStatus.Unread,
            CreatedDate = DateTime.UtcNow,
            ApprovalId = approvalId,
            TransactionId = null
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task DeleteNotificationByApprovalIdAsync(string approvalId)
    {
        await _notificationRepository.DeleteByApprovalIdAsync(approvalId);
    }

    public async Task DeleteNotificationByTransactionIdAsync(string transactionId)
    {
        await _notificationRepository.DeleteByTransactionIdAsync(transactionId);
    }

    public async Task DeleteAllNotificationsAsync()
    {
        await _notificationRepository.DeleteAllAsync();
    }
}