using Microsoft.EntityFrameworkCore;
using UserApi.Models;
using UserApi.Repositories;
using UserApprovalApi.Data;
using UserApi.Helpers;

namespace UserApi.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await _context.Notifications
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(string id)
    {
        return await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id);
    }

    public async Task<bool> NotificationIdExistsAsync(string notificationId)
    {
        return await _context.Notifications.AnyAsync(n => n.NotificationId == notificationId);
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        // Generate unique Notification ID
        string newNotificationId;
        do
        {
            newNotificationId = IdGenerator.GenerateNotificationId();
        } while (await NotificationIdExistsAsync(newNotificationId));

        notification.NotificationId = newNotificationId;

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task DeleteAsync(string id)
    {
        var notification = await GetByIdAsync(id);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByApprovalIdAsync(string approvalId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.ApprovalId == approvalId);

        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByTransactionIdAsync(string transactionId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.TransactionId == transactionId);

        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllAsync()
    {
        var notifications = await _context.Notifications.ToListAsync();
        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();
    }
}