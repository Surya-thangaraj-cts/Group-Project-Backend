namespace UserApi.DTOs
{
    public class AccountDto
    {
        public string AccountId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = "";
        public string CustomerId { get; set; } = "";
        public int AccountType { get; set; } // 0 = Savings, 1 = Current
        public decimal Balance { get; set; }
        public int Status { get; set; } // 0 = Active, 1 = Closed, 2 = Pending
        public DateTime CreatedAtUtc { get; set; }
    }
}