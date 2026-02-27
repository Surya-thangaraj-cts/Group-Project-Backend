using UserApi.Models;

namespace UserApi.Models;

public class Notification
{
    public string NotificationId { get; set; } = string.Empty; // Format: NOT0001

    public int UserId { get; set; }

    public NotificationType Type { get; set; }
    public string Message { get; set; } = "";
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Relational fields
    public string? ApprovalId { get; set; }
    public string? TransactionId { get; set; }
}