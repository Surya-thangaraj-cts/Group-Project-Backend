using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApi.DTOs;
using UserApi.Repositories;
using UserApprovalApi.Data;
using UserApprovalApi.DTOs;
using UserApprovalApi.Models;
using UserApprovalApi.Repositories;

namespace UserApprovalApi.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly ITransactionRepository _transactions;
        private readonly AppDbContext _context;

        public AdminController(IUserRepository users, ITransactionRepository transactions, AppDbContext context)
        {
            _users = users;
            _transactions = transactions;
            _context = context;
        }

        [HttpGet("pending-users")]
        public async Task<IActionResult> PendingUsers(CancellationToken ct)
        {
            var list = (await _users.GetPendingUsersAsync(ct))
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.Branch,
                    u.Role,
                    Status = u.Status.ToString()
                })
                .ToList();

            return Ok(list);
        }

        [HttpPut("deactivate/{userId}")]
        public async Task<IActionResult> Deactivate(string userId, CancellationToken ct)
        {
            var user = await _users.GetByIdAsync(userId, ct);
            if (user == null) return NotFound(new { message = "User not found" });

            user.Status = UserStatus.Inactive;
            user.UpdatedAtUtc = DateTime.UtcNow;

            await _users.UpdateAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            return Ok(new { message = "User deactivated successfully" });
        }


        [HttpGet("approved-users")]
        public async Task<IActionResult> ApprovedUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalCount) = await _users.GetApprovedUsersPagedAsync(page, pageSize, ct);

            var result = new PagedResult<object>
            {
                Items = items.Select(u => (object)new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.Branch,
                    u.Role,
                    Status = u.Status.ToString()
                }).ToList(), // Add .ToList() here to materialize the Items
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }

        //[HttpGet("approved-users")]
        //public async Task<IActionResult> ApprovedUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        //{
        //    if (page < 1) page = 1;
        //    if (pageSize < 1) pageSize = 10;

        //    var (items, totalCount) = await _users.GetApprovedUsersPagedAsync(page, pageSize, ct);

        //    var result = new PagedResult<object>
        //    {
        //        Items = items.Select(u => (object)new
        //        {
        //            u.UserId,
        //            u.Name,
        //            u.Email,
        //            u.Branch,
        //            u.Role,
        //            Status = u.Status.ToString()
        //        }),
        //        TotalCount = totalCount,
        //        PageNumber = page,
        //        PageSize = pageSize,
        //        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        //    };

        //    return Ok(result);
        //}

        [HttpPut("approve/{userId}")]
        public async Task<IActionResult> Approve(string userId, CancellationToken ct)
        {
            var user = await _users.GetByIdAsync(userId, ct);
            if (user == null) return NotFound();

            if (user.Status == UserStatus.Active)
                return BadRequest(new { message = "User already active" });

            var old = user.Status;
            user.Status = UserStatus.Active;
            user.UpdatedAtUtc = DateTime.UtcNow;

            await _users.UpdateAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            return Ok(new { message = "User approved", from = old.ToString(), to = user.Status.ToString() });
        }

        [HttpGet("search-users")]
        public async Task<IActionResult> SearchApprovedUsers([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Query parameter is required." });

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalCount) = await _users.SearchApprovedUsersPagedAsync(query, page, pageSize, ct);

            var result = new PagedResult<object>
            {
                Items = items.Select(u => (object)new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.Branch,
                    u.Role,
                    Status = u.Status.ToString()
                }),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }

        [HttpGet("search-pending")]
        public async Task<IActionResult> SearchPendingUsers([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Query parameter is required." });

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalCount) = await _users.SearchPendingUsersPagedAsync(query, page, pageSize, ct);

            var result = new PagedResult<object>
            {
                Items = items.Select(u => (object)new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.Branch,
                    u.Role,
                    Status = u.Status.ToString()
                }),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }

        [HttpPut("edit/{userId}")]
        public async Task<IActionResult> EditUser(string userId, [FromBody] EditUserRequest req, CancellationToken ct)
        {
            var user = await _users.GetByIdAsync(userId, ct);
            if (user == null) return NotFound(new { message = "User not found" });

            if (!string.IsNullOrWhiteSpace(req.Name))
                user.Name = req.Name;

            if (!string.IsNullOrWhiteSpace(req.Email))
            {
                var emailExists = await _users.EmailExistsAsync(req.Email, userId, ct);
                if (emailExists)
                    return Conflict(new { message = "Email already in use by another user" });
                user.Email = req.Email;
            }

            if (req.Branch != null)
                user.Branch = req.Branch;

            if (!string.IsNullOrWhiteSpace(req.Role))
            {
                if (!Enum.TryParse<UserRole>(req.Role, true, out var parsedRole))
                    return BadRequest(new { message = $"Invalid role: {req.Role}" });
                user.Role = parsedRole;
            }

            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                if (!Enum.TryParse<UserStatus>(req.Status, true, out var parsedStatus))
                    return BadRequest(new { message = $"Invalid status: {req.Status}" });
                user.Status = parsedStatus;
            }

            user.UpdatedAtUtc = DateTime.UtcNow;

            await _users.UpdateAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            return Ok(new
            {
                message = "User updated successfully",
                user = new UserResponse
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Branch = user.Branch,
                    Role = user.Role.ToString(),
                    Status = user.Status.ToString()
                }
            });
        }

        [HttpGet("compliance-metrics")]
        public async Task<IActionResult> GetComplianceMetrics(CancellationToken ct)
        {
            var allTransactions = await _context.Transactions.AsNoTracking().ToListAsync(ct);

            var totalTransactions = allTransactions.Count;

            var highValueCount = allTransactions.Count(t =>
                !string.Equals(t.Flag, "Normal", StringComparison.OrdinalIgnoreCase));

            // Account growth rate: percentage of active accounts vs total
            var totalAccounts = await _context.Accounts.AsNoTracking().CountAsync(ct);
            var activeAccounts = await _context.Accounts.AsNoTracking()
                .CountAsync(a => a.Status == UserApi.Models.AccountStatus.Active, ct);
            var accountGrowthRate = totalAccounts > 0
                ? Math.Round((double)activeAccounts / totalAccounts * 100, 2)
                : 0;

            // Group transactions by (Year, Month) for efficient lookup
            var monthlyGroups = allTransactions
                .GroupBy(t => (Year: t.Date.Year, Month: t.Date.Month))
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Total: g.Count(),
                        Suspicious: g.Count(t =>
                            !string.Equals(t.Flag, "Normal", StringComparison.OrdinalIgnoreCase))
                    ));

            // Derive the 12-month window from actual transaction dates
            var latestDate = allTransactions.Any()
                ? allTransactions.Max(t => t.Date)
                : DateTime.UtcNow;

            var monthlyLabels = new string[12];
            var monthlyTxnVolume = new int[12];
            var monthlySuspicious = new int[12];

            for (int i = 11; i >= 0; i--)
            {
                var month = latestDate.AddMonths(-i);
                var idx = 11 - i;
                monthlyLabels[idx] = month.ToString("MMM yyyy");

                var key = (Year: month.Year, Month: month.Month);
                if (monthlyGroups.TryGetValue(key, out var data))
                {
                    monthlyTxnVolume[idx] = data.Total;
                    monthlySuspicious[idx] = data.Suspicious;
                }
            }

            // Amount buckets
            var buckets = new (string Label, decimal Min, decimal Max)[]
            {
                ("0 - 1K",       0,      1_000),
                ("1K - 5K",  1_000,      5_000),
                ("5K - 10K", 5_000,     10_000),
                ("10K - 50K",10_000,    50_000),
                ("50K+",     50_000, decimal.MaxValue)
            };

            var amountBuckets = buckets.Select(b => new AmountBucketDto
            {
                Label = b.Label,
                Count = allTransactions.Count(t => t.Amount >= b.Min && t.Amount < b.Max)
            }).ToArray();

            var dto = new ComplianceMetricsDto
            {
                TotalTransactions = totalTransactions,
                HighValueCount = highValueCount,
                AccountGrowthRate = accountGrowthRate,
                MonthlyTxnVolume = monthlyTxnVolume,
                MonthlyLabels = monthlyLabels,
                MonthlySuspicious = monthlySuspicious,
                AmountBuckets = amountBuckets
            };

            return Ok(dto);
        }

    }
}