namespace UserApi.DTOs;

public class TransactionDto
{
    public int TransactionId { get; set; }
    public int AccountId { get; set; }
    public string Type { get; set; } = string.Empty; // Display as "Deposit", "Withdrawal", or "Transfer"
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Flag { get; set; } = string.Empty;
    public int? ToAccountId { get; set; } // Only populated for Transfer transactions
}