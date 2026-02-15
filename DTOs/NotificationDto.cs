namespace UserApi.DTOs;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public int Type { get; set; } // 0 = ApprovalReminder, 1 = SuspiciousActivity
    public string Message { get; set; } = "";
    public int Status { get; set; } // 0 = Unread, 1 = Read
    public DateTime CreatedDate { get; set; }
    public int? ApprovalId { get; set; }
    public int? TransactionId { get; set; }
}