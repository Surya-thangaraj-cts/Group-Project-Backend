using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApi.Repositories;
using UserApprovalApi.Data;
using UserApprovalApi.DTOs;

namespace UserApprovalApi.Controllers
{
    [ApiController]
    [Route("api/manager-dashboard")]
    [Authorize(Roles = "Manager")]
    public class ManagerDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IApprovalRepository _approvalRepository;

        public ManagerDashboardController(AppDbContext context, IApprovalRepository approvalRepository)
        {
            _context = context;
            _approvalRepository = approvalRepository;
        }

        /// <summary>
        /// Get complete dashboard overview data in a single call
        /// </summary>
        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardOverview(CancellationToken ct)
        {
            try
            {
                // Get all transactions
                var allTransactions = await _context.Transactions.AsNoTracking().ToListAsync(ct);
                var totalTransactions = allTransactions.Count;

                // High value transactions count
                var highValueCount = allTransactions.Count(t =>
                    !string.Equals(t.Flag, "Normal", StringComparison.OrdinalIgnoreCase));

                // Account metrics
                var allAccounts = await _context.Accounts.AsNoTracking().ToListAsync(ct);
                var totalAccounts = allAccounts.Count;
                var activeAccounts = allAccounts.Count(a => a.Status == UserApi.Models.AccountStatus.Active);
                var pendingAccounts = allAccounts.Count(a => a.Status == UserApi.Models.AccountStatus.Pending);

                var accountGrowthRate = totalAccounts > 0
                    ? Math.Round((double)activeAccounts / totalAccounts * 100, 2)
                    : 0;

                // Pending approvals count
                var pendingApprovals = await _context.Approvals.AsNoTracking()
                    .CountAsync(a => a.Decision == UserApi.Models.ApprovalDecision.Pending, ct);

                // Calculate 12-month data
                var latestDate = allTransactions.Any()
                    ? allTransactions.Max(t => t.Date)
                    : DateTime.UtcNow;

                var monthlyLabels = new string[12];
                var monthlyTxnVolume = new int[12];
                var monthlySuspicious = new int[12];
                var monthlyNewAccounts = new int[12];
                var monthlyActiveAccounts = new int[12];

                for (int i = 11; i >= 0; i--)
                {
                    var month = latestDate.AddMonths(-i);
                    var idx = 11 - i;
                    monthlyLabels[idx] = month.ToString("MMM yyyy");

                    // Transaction volume for this month
                    monthlyTxnVolume[idx] = allTransactions.Count(t =>
                        t.Date.Year == month.Year && t.Date.Month == month.Month);

                    // Suspicious transactions for this month
                    monthlySuspicious[idx] = allTransactions.Count(t =>
                        t.Date.Year == month.Year && t.Date.Month == month.Month &&
                        !string.Equals(t.Flag, "Normal", StringComparison.OrdinalIgnoreCase));

                    // New accounts created in this month
                    monthlyNewAccounts[idx] = allAccounts.Count(a =>
                        a.CreatedAtUtc.Year == month.Year && a.CreatedAtUtc.Month == month.Month);

                    // Active accounts at end of this month (cumulative)
                    var endOfMonth = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month), 23, 59, 59, DateTimeKind.Utc);
                    monthlyActiveAccounts[idx] = allAccounts.Count(a =>
                        a.CreatedAtUtc <= endOfMonth && a.Status == UserApi.Models.AccountStatus.Active);
                }

                // Amount distribution buckets
                var buckets = new (string Label, decimal Min, decimal Max)[]
                {
                    ("0 - 1K", 0, 1_000),
                    ("1K - 5K", 1_000, 5_000),
                    ("5K - 10K", 5_000, 10_000),
                    ("10K - 50K", 10_000, 50_000),
                    ("50K+", 50_000, decimal.MaxValue)
                };

                var amountBuckets = buckets.Select(b => new AmountBucketDto
                {
                    Label = b.Label,
                    Count = allTransactions.Count(t => t.Amount >= b.Min && t.Amount < b.Max)
                }).ToArray();

                var result = new ManagerDashboardOverviewDto
                {
                    // Summary metrics
                    TotalTransactions = totalTransactions,
                    HighValueCount = highValueCount,
                    PendingApprovalsCount = pendingApprovals,
                    TotalAccounts = totalAccounts,
                    ActiveAccounts = activeAccounts,
                    PendingAccounts = pendingAccounts,
                    AccountGrowthRate = accountGrowthRate,

                    // Chart data
                    MonthlyLabels = monthlyLabels,
                    MonthlyTxnVolume = monthlyTxnVolume,
                    MonthlySuspicious = monthlySuspicious,
                    MonthlyNewAccounts = monthlyNewAccounts,
                    MonthlyActiveAccounts = monthlyActiveAccounts,
                    AmountBuckets = amountBuckets
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the full exception details
                Console.WriteLine($"ERROR in GetDashboardOverview: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new 
                { 
                    error = "An error occurred while fetching dashboard data.", 
                    details = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Get compliance metrics only
        /// </summary>
        [HttpGet("compliance-metrics")]
        public async Task<IActionResult> GetComplianceMetrics(CancellationToken ct)
        {
            try
            {
                var allTransactions = await _context.Transactions.AsNoTracking().ToListAsync(ct);
                var totalTransactions = allTransactions.Count;

                var highValueCount = allTransactions.Count(t =>
                    !string.Equals(t.Flag, "Normal", StringComparison.OrdinalIgnoreCase));

                // Account metrics
                var totalAccounts = await _context.Accounts.AsNoTracking().CountAsync(ct);
                var activeAccounts = await _context.Accounts.AsNoTracking()
                    .CountAsync(a => a.Status == UserApi.Models.AccountStatus.Active, ct);
                var pendingAccounts = await _context.Accounts.AsNoTracking()
                    .CountAsync(a => a.Status == UserApi.Models.AccountStatus.Pending, ct);

                var accountGrowthRate = totalAccounts > 0
                    ? Math.Round((double)activeAccounts / totalAccounts * 100, 2)
                    : 0;

                // Calculate 12-month data
                var latestDate = allTransactions.Any()
                    ? allTransactions.Max(t => t.Date)
                    : DateTime.UtcNow;

                var monthlyLabels = new string[12];
                var monthlyTxnVolume = new int[12];
                var monthlySuspicious = new int[12];
                var monthlyNewAccounts = new int[12];
                var monthlyActiveAccounts = new int[12];

                var allAccounts = await _context.Accounts.AsNoTracking().ToListAsync(ct);

                for (int i = 11; i >= 0; i--)
                {
                    var month = latestDate.AddMonths(-i);
                    var idx = 11 - i;
                    monthlyLabels[idx] = month.ToString("MMM yyyy");

                    monthlyTxnVolume[idx] = allTransactions.Count(t =>
                        t.Date.Year == month.Year && t.Date.Month == month.Month);

                    monthlySuspicious[idx] = allTransactions.Count(t =>
                        t.Date.Year == month.Year && t.Date.Month == month.Month &&
                        !string.Equals(t.Flag, "Normal", StringComparison.OrdinalIgnoreCase));

                    // New accounts created in this month
                    monthlyNewAccounts[idx] = allAccounts.Count(a =>
                        a.CreatedAtUtc.Year == month.Year && a.CreatedAtUtc.Month == month.Month);

                    // Active accounts at end of this month (cumulative)
                    var endOfMonth = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month), 23, 59, 59, DateTimeKind.Utc);
                    monthlyActiveAccounts[idx] = allAccounts.Count(a =>
                        a.CreatedAtUtc <= endOfMonth && a.Status == UserApi.Models.AccountStatus.Active);
                }

                // Amount buckets
                var buckets = new (string Label, decimal Min, decimal Max)[]
                {
                    ("0 - 1K", 0, 1_000),
                    ("1K - 5K", 1_000, 5_000),
                    ("5K - 10K", 5_000, 10_000),
                    ("10K - 50K", 10_000, 50_000),
                    ("50K+", 50_000, decimal.MaxValue)
                };

                var amountBuckets = buckets.Select(b => new AmountBucketDto
                {
                    Label = b.Label,
                    Count = allTransactions.Count(t => t.Amount >= b.Min && t.Amount < b.Max)
                }).ToArray();

                var dto = new ManagerDashboardOverviewDto
                {
                    TotalTransactions = totalTransactions,
                    HighValueCount = highValueCount,
                    AccountGrowthRate = accountGrowthRate,
                    TotalAccounts = totalAccounts,
                    ActiveAccounts = activeAccounts,
                    PendingAccounts = pendingAccounts,
                    MonthlyTxnVolume = monthlyTxnVolume,
                    MonthlyLabels = monthlyLabels,
                    MonthlySuspicious = monthlySuspicious,
                    MonthlyNewAccounts = monthlyNewAccounts,
                    MonthlyActiveAccounts = monthlyActiveAccounts,
                    AmountBuckets = amountBuckets
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching compliance metrics.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get pending approvals count
        /// </summary>
        [HttpGet("pending-approvals-count")]
        public async Task<IActionResult> GetPendingApprovalsCount(CancellationToken ct)
        {
            try
            {
                var count = await _context.Approvals.AsNoTracking()
                    .CountAsync(a => a.Decision == UserApi.Models.ApprovalDecision.Pending, ct);

                return Ok(new { pendingApprovalsCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching pending approvals count.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get chart data for account growth
        /// </summary>
        [HttpGet("account-growth-chart")]
        public async Task<IActionResult> GetAccountGrowthChart(CancellationToken ct)
        {
            try
            {
                var allAccounts = await _context.Accounts.AsNoTracking().ToListAsync(ct);
                var latestDate = DateTime.UtcNow;

                var chartData = new List<AccountGrowthChartDataDto>();

                for (int i = 11; i >= 0; i--)
                {
                    var month = latestDate.AddMonths(-i);
                    var monthLabel = month.ToString("MMM yyyy");

                    // New accounts created in this month
                    var newAccounts = allAccounts.Count(a =>
                        a.CreatedAtUtc.Year == month.Year && a.CreatedAtUtc.Month == month.Month);

                    // Active accounts at end of this month (cumulative)
                    var endOfMonth = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month), 23, 59, 59, DateTimeKind.Utc);
                    var activeAccounts = allAccounts.Count(a =>
                        a.CreatedAtUtc <= endOfMonth && a.Status == UserApi.Models.AccountStatus.Active);

                    chartData.Add(new AccountGrowthChartDataDto
                    {
                        Month = monthLabel,
                        NewAccounts = newAccounts,
                        ActiveAccounts = activeAccounts
                    });
                }

                return Ok(chartData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching account growth chart data.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get transaction amount distribution
        /// </summary>
        [HttpGet("amount-distribution")]
        public async Task<IActionResult> GetAmountDistribution(CancellationToken ct)
        {
            try
            {
                var allTransactions = await _context.Transactions.AsNoTracking().ToListAsync(ct);

                var buckets = new (string Label, decimal Min, decimal Max)[]
                {
                    ("0 - 1K", 0, 1_000),
                    ("1K - 5K", 1_000, 5_000),
                    ("5K - 10K", 5_000, 10_000),
                    ("10K - 50K", 10_000, 50_000),
                    ("50K+", 50_000, decimal.MaxValue)
                };

                var distribution = buckets.Select(b => new AmountBucketDto
                {
                    Label = b.Label,
                    Count = allTransactions.Count(t => t.Amount >= b.Min && t.Amount < b.Max)
                }).ToArray();

                return Ok(distribution);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching amount distribution.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get monthly transaction trends
        /// </summary>
        [HttpGet("transaction-trends")]
        public async Task<IActionResult> GetTransactionTrends(CancellationToken ct)
        {
            try
            {
                var allTransactions = await _context.Transactions.AsNoTracking().ToListAsync(ct);
                var latestDate = allTransactions.Any()
                    ? allTransactions.Max(t => t.Date)
                    : DateTime.UtcNow;

                var trends = new List<MonthlyTransactionTrendDto>();

                for (int i = 11; i >= 0; i--)
                {
                    var month = latestDate.AddMonths(-i);
                    var monthLabel = month.ToString("MMM yyyy");

                    var totalVolume = allTransactions.Count(t =>
                        t.Date.Year == month.Year && t.Date.Month == month.Month);

                    var suspiciousCount = allTransactions.Count(t =>
                        t.Date.Year == month.Year && t.Date.Month == month.Month &&
                        !string.Equals(t.Flag, "Normal", StringComparison.OrdinalIgnoreCase));

                    trends.Add(new MonthlyTransactionTrendDto
                    {
                        Month = monthLabel,
                        TotalVolume = totalVolume,
                        SuspiciousCount = suspiciousCount
                    });
                }

                return Ok(trends);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching transaction trends.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get summary statistics
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(CancellationToken ct)
        {
            try
            {
                var totalTransactions = await _context.Transactions.CountAsync(ct);
                var highValueCount = await _context.Transactions
                    .CountAsync(t => !string.Equals(t.Flag, "Normal", StringComparison.OrdinalIgnoreCase), ct);

                var totalAccounts = await _context.Accounts.CountAsync(ct);
                var activeAccounts = await _context.Accounts
                    .CountAsync(a => a.Status == UserApi.Models.AccountStatus.Active, ct);
                var pendingAccounts = await _context.Accounts
                    .CountAsync(a => a.Status == UserApi.Models.AccountStatus.Pending, ct);

                var pendingApprovals = await _context.Approvals
                    .CountAsync(a => a.Decision == UserApi.Models.ApprovalDecision.Pending, ct);

                var accountGrowthRate = totalAccounts > 0
                    ? Math.Round((double)activeAccounts / totalAccounts * 100, 2)
                    : 0;

                var summary = new DashboardSummaryDto
                {
                    TotalTransactions = totalTransactions,
                    HighValueCount = highValueCount,
                    TotalAccounts = totalAccounts,
                    ActiveAccounts = activeAccounts,
                    PendingAccounts = pendingAccounts,
                    PendingApprovalsCount = pendingApprovals,
                    AccountGrowthRate = accountGrowthRate
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching summary.", details = ex.Message });
            }
        }
    }
}
