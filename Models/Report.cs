using System;

namespace UserApi.Models
{
    public class Report
    {
        public int Id { get; set; }

        /// <summary>
        /// Optional AccountId if report is account-specific
        /// </summary>
        public int? AccountId { get; set; }
        public Account? Account { get; set; }

        /// <summary>
        /// JSON data containing detailed report metrics
        /// </summary>
        public string? DataJson { get; set; }

        /// <summary>
        /// File path if report is exported to file
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Start date of report period
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Timestamp when report was generated
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User ID of the person who generated the report
        /// </summary>
        public string GeneratedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// Account growth rate percentage
        /// </summary>
        public decimal GrowthRate { get; set; }

        /// <summary>
        /// Type of report: transaction, compliance, growth, etc.
        /// </summary>
        public string ReportType { get; set; } = string.Empty;

        /// <summary>
        /// Title/name of the report
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// End date of report period
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Total transaction amount in the report
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Total number of transactions
        /// </summary>
        public int TotalTransactions { get; set; }

        /// <summary>
        /// Transaction status filter (e.g., "done", "pending")
        /// </summary>
        public string? TransactionStatus { get; set; }

        /// <summary>
        /// Transaction type filter (e.g., "withdraw", "deposit", "transfer")
        /// </summary>
        public string? TransactionType { get; set; }
    }
}