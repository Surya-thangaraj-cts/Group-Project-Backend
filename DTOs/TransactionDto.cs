namespace UserApi.DTOs;

public class TransactionDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Display as "Deposit", "Withdrawal", or "Transfer"
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int Status { get; set; } // 0=Completed, 1=Pending, 2=Rejected, 3=Failed
    public string Flag { get; set; } = string.Empty;
    public string? ToAccountId { get; set; } // Only populated for Transfer transactions
}