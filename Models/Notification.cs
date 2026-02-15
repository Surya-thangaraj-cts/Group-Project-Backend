using UserApi.Models;

namespace UserApi.Models;

public class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public NotificationType Type { get; set; }
    public string Message { get; set; } = "";
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Relational fields
    public int? ApprovalId { get; set; }
    public int? TransactionId { get; set; }
}