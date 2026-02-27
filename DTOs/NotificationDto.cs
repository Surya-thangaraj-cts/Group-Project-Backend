namespace UserApi.DTOs;

public class NotificationDto
{
    public string NotificationId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int Type { get; set; } // 0 = ApprovalReminder, 1 = SuspiciousActivity
    public string Message { get; set; } = "";
    public int Status { get; set; } // 0 = Unread, 1 = Read
    public DateTime CreatedDate { get; set; }
    public string? ApprovalId { get; set; }
    public string? TransactionId { get; set; }
}