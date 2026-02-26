using UserApi.Models;
using AccountTrack.Api.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApi.DTOs;
using UserApprovalApi.Data;
using System.Text;

namespace UserApprovalApi.Controllers;

[ApiController]
[Route("api/manager-transactions")]
[Authorize(Roles = "Manager")]
public class ManagerTransactionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ManagerTransactionsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Get transactions with comprehensive filtering for Manager transaction table
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string? searchText = null,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? viewMode = "all",
        [FromQuery] string? flag = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = _context.Transactions.AsNoTracking().AsQueryable();

            // Search filter (by TransactionId or AccountId)
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(t =>
                    t.TransactionId.ToString().Contains(searchText) ||
                    t.AccountId.ToString().Contains(searchText));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TransactionStatus>(status, true, out var statusEnum))
            {
                query = query.Where(t => t.Status == statusEnum);
            }

            // Type filter
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(t => string.Equals(t.Type, type, StringComparison.OrdinalIgnoreCase));
            }

            // Amount range filter
            if (minAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= maxAmount.Value);
            }

            // Date range filter
            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Date <= endDate.Value);
            }

            // View mode filter (high-value transactions > 100,000)
            if (viewMode == "highvalue")
            {
                query = query.Where(t => t.Amount > 100000);
            }

            // Flag filter
            if (!string.IsNullOrWhiteSpace(flag))
            {
                query = query.Where(t => string.Equals(t.Flag, flag, StringComparison.OrdinalIgnoreCase));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(ct);

            // Apply pagination
            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var transactionDtos = _mapper.Map<IEnumerable<TransactionDto>>(transactions);

            var result = new PagedResult<TransactionDto>
            {
                Items = transactionDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving transactions.", details = ex.Message });
        }
    }

    /// <summary>
    /// Get high-value transaction count (amount > 100,000)
    /// </summary>
    [HttpGet("high-value-count")]
    public async Task<IActionResult> GetHighValueCount(CancellationToken ct)
    {
        try
        {
            var count = await _context.Transactions
                .AsNoTracking()
                .CountAsync(t => t.Amount > 100000, ct);

            return Ok(new { highValueCount = count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while counting high-value transactions.", details = ex.Message });
        }
    }

    /// <summary>
    /// Export transactions to CSV with filters applied
    /// </summary>
    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportToCSV(
        [FromQuery] string? searchText = null,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? viewMode = "all",
        CancellationToken ct = default)
    {
        try
        {
            var query = _context.Transactions.AsNoTracking().AsQueryable();

            // Apply same filters as GetTransactions
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(t =>
                    t.TransactionId.ToString().Contains(searchText) ||
                    t.AccountId.ToString().Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TransactionStatus>(status, true, out var statusEnum))
            {
                query = query.Where(t => t.Status == statusEnum);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(t => string.Equals(t.Type, type, StringComparison.OrdinalIgnoreCase));
            }

            if (minAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= maxAmount.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Date <= endDate.Value);
            }

            if (viewMode == "highvalue")
            {
                query = query.Where(t => t.Amount > 100000);
            }

            var transactions = await query.OrderByDescending(t => t.Date).ToListAsync(ct);

            // Build CSV content
            var csv = new StringBuilder();
            csv.AppendLine("TransactionId,AccountId,Type,Amount,Date,Status,Flag");

            foreach (var txn in transactions)
            {
                csv.AppendLine($"{txn.TransactionId},{txn.AccountId},{txn.Type},{txn.Amount},{txn.Date:yyyy-MM-dd},{txn.Status},{txn.Flag}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"transactions_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while exporting to CSV.", details = ex.Message });
        }
    }

    /// <summary>
    /// Export transactions to Excel (CSV format with .xlsx extension)
    /// Note: For true Excel format, consider using EPPlus or ClosedXML library
    /// </summary>
    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportToExcel(
        [FromQuery] string? searchText = null,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? viewMode = "all",
        CancellationToken ct = default)
    {
        try
        {
            var query = _context.Transactions.AsNoTracking().AsQueryable();

            // Apply same filters
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(t =>
                    t.TransactionId.ToString().Contains(searchText) ||
                    t.AccountId.ToString().Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TransactionStatus>(status, true, out var statusEnum))
            {
                query = query.Where(t => t.Status == statusEnum);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(t => string.Equals(t.Type, type, StringComparison.OrdinalIgnoreCase));
            }

            if (minAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= maxAmount.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Date <= endDate.Value);
            }

            if (viewMode == "highvalue")
            {
                query = query.Where(t => t.Amount > 100000);
            }

            var transactions = await query.OrderByDescending(t => t.Date).ToListAsync(ct);

            // Build CSV content (simple Excel-compatible format)
            var csv = new StringBuilder();
            csv.AppendLine("TransactionId,AccountId,Type,Amount,Date,Status,Flag");

            foreach (var txn in transactions)
            {
                csv.AppendLine($"{txn.TransactionId},{txn.AccountId},{txn.Type},{txn.Amount},{txn.Date:yyyy-MM-dd},{txn.Status},{txn.Flag}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            // Return as Excel file (CSV format that Excel can open)
            return File(bytes, "application/vnd.ms-excel", $"transactions_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while exporting to Excel.", details = ex.Message });
        }
    }

    /// <summary>
    /// Get transaction statistics summary
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken ct)
    {
        try
        {
            var allTransactions = await _context.Transactions.AsNoTracking().ToListAsync(ct);

            var stats = new
            {
                TotalCount = allTransactions.Count,
                HighValueCount = allTransactions.Count(t => t.Amount > 100000),
                CompletedCount = allTransactions.Count(t => t.Status == TransactionStatus.Completed),
                PendingCount = allTransactions.Count(t => t.Status == TransactionStatus.Pending),
                RejectedCount = allTransactions.Count(t => t.Status == TransactionStatus.Rejected),
                FailedCount = allTransactions.Count(t => t.Status == TransactionStatus.Failed),
                TotalAmount = allTransactions.Sum(t => t.Amount),
                AverageAmount = allTransactions.Any() ? allTransactions.Average(t => t.Amount) : 0,
                DepositCount = allTransactions.Count(t => string.Equals(t.Type, "Deposit", StringComparison.OrdinalIgnoreCase)),
                WithdrawalCount = allTransactions.Count(t => string.Equals(t.Type, "Withdrawal", StringComparison.OrdinalIgnoreCase)),
                TransferCount = allTransactions.Count(t => string.Equals(t.Type, "Transfer", StringComparison.OrdinalIgnoreCase))
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving statistics.", details = ex.Message });
        }
    }

    /// <summary>
    /// Get available filter options (statuses, types)
    /// </summary>
    [HttpGet("filter-options")]
    public IActionResult GetFilterOptions()
    {
        try
        {
            var options = new
            {
                Statuses = Enum.GetNames(typeof(TransactionStatus)),
                Types = new[] { "Deposit", "Withdrawal", "Transfer" },
                Flags = new[] { "Normal", "Suspicious", "HighValue" }
            };

            return Ok(options);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving filter options.", details = ex.Message });
        }
    }
}
