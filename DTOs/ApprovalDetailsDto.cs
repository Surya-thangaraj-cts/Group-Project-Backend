namespace UserApi.DTOs;

public class ApprovalDetailsDto
{
    public string ApprovalId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // AccountCreation, AccountUpdate, HighValueTransaction
    public string? AccountId { get; set; }
    public string? CustomerName { get; set; }
    public int ReviewerId { get; set; }
    public string Decision { get; set; } = string.Empty; // Pending, Approve, Reject
    public string? PendingChanges { get; set; }
    public DateTime ApprovalDate { get; set; }
    public string Comments { get; set; } = string.Empty;
}