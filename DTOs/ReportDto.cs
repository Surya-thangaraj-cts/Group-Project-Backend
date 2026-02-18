using System;

namespace UserApprovalApi.DTOs
{
    public class ReportDto
    {
        public int Id { get; set; }
        public int? AccountId { get; set; }
        public string? DataJson { get; set; }
        public string? FilePath { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedByUserId { get; set; } = string.Empty;
        public decimal GrowthRate { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime? ToDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransactions { get; set; }
        public string? TransactionStatus { get; set; }
        public string? TransactionType { get; set; }
    }
}
