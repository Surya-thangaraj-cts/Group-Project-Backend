namespace UserApi.DTOs;

public class ApprovalDetailsDto
{
    public int ApprovalId { get; set; }
    public string Type { get; set; } = string.Empty; // AccountCreation, AccountUpdate, HighValueTransaction
    public int? AccountId { get; set; }
    public string? CustomerName { get; set; }
    public int ReviewerId { get; set; }
    public string Decision { get; set; } = string.Empty; // Pending, Approve, Reject
    public string? PendingChanges { get; set; }
    public DateTime ApprovalDate { get; set; }
    public string Comments { get; set; } = string.Empty;
}