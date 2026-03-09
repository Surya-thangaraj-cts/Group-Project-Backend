namespace UserApi.DTOs;

public class TransactionResponseDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Display as "Deposit", "Withdrawal", or "Transfer"
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty; // "Completed", "Pending", "Rejected"
    public string Flag { get; set; } = string.Empty; // "High", "Normal"
    public string? Narrative { get; set; }
    public string? ToAccountId { get; set; } // Only populated for Transfer transactions
    public string? ApprovalId { get; set; } // Approval ID for high-value transactions
    public bool RequiresApproval { get; set; } // True if transaction requires approval (amount > 100,000)
}
