namespace UserApi.Models;

public class Account
{
    public int AccountId { get; set; } // Identity PK

    public string CustomerName { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; }

    public AccountStatus Status { get; set; } = AccountStatus.Active;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
