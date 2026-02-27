namespace UserApi.Models;

public class Transaction
{
    public string TransactionId { get; set; } = string.Empty; // Format: TXN0001

    public string AccountId { get; set; } = string.Empty;
    public Account? Account { get; set; }

    public string Type { get; set; } = string.Empty; // Changed from TransactionType enum to string
    public decimal Amount { get; set; }
    public string Narrative { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed; // Changed default to Completed
    public string Flag { get; set; } = "Normal";

    // For transfers (optional)
    public string? TargetAccountId { get; set; }
}
