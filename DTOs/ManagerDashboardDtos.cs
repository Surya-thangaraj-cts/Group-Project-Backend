namespace UserApprovalApi.DTOs
{
    /// <summary>
    /// Complete dashboard overview data for Manager component
    /// </summary>
    public class ManagerDashboardOverviewDto
    {
        // Summary Metrics
        public int TotalTransactions { get; set; }
        public int HighValueCount { get; set; }
        public int PendingApprovalsCount { get; set; }
        public int TotalAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int PendingAccounts { get; set; }
        public double AccountGrowthRate { get; set; }

        // Chart Data - 12 months
        public string[] MonthlyLabels { get; set; } = Array.Empty<string>();
        public int[] MonthlyTxnVolume { get; set; } = Array.Empty<int>();
        public int[] MonthlySuspicious { get; set; } = Array.Empty<int>();
        public int[] MonthlyNewAccounts { get; set; } = Array.Empty<int>();
        public int[] MonthlyActiveAccounts { get; set; } = Array.Empty<int>();

        // Amount Distribution
        public AmountBucketDto[] AmountBuckets { get; set; } = Array.Empty<AmountBucketDto>();
    }

    /// <summary>
    /// Account growth chart data for a single month
    /// </summary>
    public class AccountGrowthChartDataDto
    {
        public string Month { get; set; } = string.Empty;
        public int NewAccounts { get; set; }
        public int ActiveAccounts { get; set; }
    }

    /// <summary>
    /// Monthly transaction trend data
    /// </summary>
    public class MonthlyTransactionTrendDto
    {
        public string Month { get; set; } = string.Empty;
        public int TotalVolume { get; set; }
        public int SuspiciousCount { get; set; }
    }

    /// <summary>
    /// Dashboard summary statistics
    /// </summary>
    public class DashboardSummaryDto
    {
        public int TotalTransactions { get; set; }
        public int HighValueCount { get; set; }
        public int TotalAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int PendingAccounts { get; set; }
        public int PendingApprovalsCount { get; set; }
        public double AccountGrowthRate { get; set; }
    }
}
