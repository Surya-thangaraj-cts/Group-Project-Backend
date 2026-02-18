using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserApi.Helpers;
using UserApi.Models;
using UserApprovalApi.Data;
using UserApprovalApi.DTOs;
using UserApprovalApi.Repositories;

namespace UserApprovalApi.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly AppDbContext _context;

        public ReportService(IReportRepository reportRepository, AppDbContext context)
        {
            _reportRepository = reportRepository;
            _context = context;
        }

        public async Task<ReportDto?> GetReportByIdAsync(int id, CancellationToken ct = default)
        {
            var report = await _reportRepository.GetByIdAsync(id, ct);
            return report == null ? null : MapToDto(report);
        }

        public async Task<IEnumerable<ReportDto>> GetAllReportsAsync(CancellationToken ct = default)
        {
            var reports = await _reportRepository.GetAllAsync(ct);
            return reports.Select(MapToDto);
        }

        public async Task<PagedResult<ReportDto>> GetFilteredReportsAsync(ReportFilterDto filter, CancellationToken ct = default)
        {
            var reports = await _reportRepository.GetFilteredAsync(filter, ct);
            var totalCount = await _reportRepository.GetTotalCountAsync(filter, ct);

            return new PagedResult<ReportDto>
            {
                Items = reports.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<IEnumerable<ReportDto>> GetReportsByUserAsync(string userId, CancellationToken ct = default)
        {
            var reports = await _reportRepository.GetByUserAsync(userId, ct);
            return reports.Select(MapToDto);
        }

        public async Task<ReportDto> GenerateReportAsync(CreateReportDto createDto, string userId, CancellationToken ct = default)
        {
            // Validate AccountId if provided
            if (createDto.AccountId.HasValue)
            {
                var accountExists = await _context.Accounts
                    .AnyAsync(a => a.AccountId == createDto.AccountId.Value, ct);

                if (!accountExists)
                {
                    throw new ArgumentException($"Account with ID {createDto.AccountId.Value} does not exist.");
                }
            }

            // Calculate metrics based on the report parameters
            var metrics = await CalculateMetricsAsync(createDto, ct);

            var report = new Report
            {
                AccountId = createDto.AccountId,
                ReportType = createDto.ReportType,
                Title = createDto.Title,
                TotalTransactions = metrics.TotalTransactions,
                TotalAmount = metrics.TotalAmount,
                GrowthRate = metrics.GrowthRate,
                GeneratedAt = DateTime.UtcNow,
                GeneratedByUserId = userId,
                FromDate = createDto.FromDate,
                ToDate = createDto.ToDate,
                TransactionStatus = createDto.TransactionStatus,
                TransactionType = createDto.TransactionType,
                DataJson = JsonSerializer.Serialize(metrics.AdditionalData)
            };

            var createdReport = await _reportRepository.CreateAsync(report, ct);
            return MapToDto(createdReport);
        }

        public async Task<bool> DeleteReportAsync(int id, CancellationToken ct = default)
        {
            return await _reportRepository.DeleteAsync(id, ct);
        }

        private async Task<(int TotalTransactions, decimal TotalAmount, decimal GrowthRate, Dictionary<string, object> AdditionalData)>
            CalculateMetricsAsync(CreateReportDto createDto, CancellationToken ct)
        {
            var transactionsQuery = _context.Transactions.AsNoTracking().AsQueryable();

            // Apply date filters
            if (createDto.FromDate.HasValue)
                transactionsQuery = transactionsQuery.Where(t => t.Date >= createDto.FromDate.Value);

            if (createDto.ToDate.HasValue)
                transactionsQuery = transactionsQuery.Where(t => t.Date <= createDto.ToDate.Value);

            // Apply account filter
            if (createDto.AccountId.HasValue)
                transactionsQuery = transactionsQuery.Where(t => t.AccountId == createDto.AccountId.Value);

            // Apply transaction type filter
            if (!string.IsNullOrWhiteSpace(createDto.TransactionType))
                transactionsQuery = transactionsQuery.Where(t => t.Type == createDto.TransactionType);

            // Apply transaction status filter
            if (!string.IsNullOrWhiteSpace(createDto.TransactionStatus))
            {
                if (Enum.TryParse<TransactionStatus>(createDto.TransactionStatus, out var status))
                    transactionsQuery = transactionsQuery.Where(t => t.Status == status);
            }

            var transactions = await transactionsQuery.ToListAsync(ct);
            var totalTransactions = transactions.Count;
            var totalAmount = transactions.Sum(t => t.Amount);

            // Calculate growth rate based on account status
            var accountsQuery = _context.Accounts.AsNoTracking().AsQueryable();
            var totalAccounts = await accountsQuery.CountAsync(ct);
            var activeAccounts = await accountsQuery.CountAsync(a => a.Status == AccountStatus.Active, ct);
            var growthRate = totalAccounts > 0
                ? Math.Round((decimal)activeAccounts / totalAccounts * 100, 2)
                : 0;

            // Additional metrics
            var additionalData = new Dictionary<string, object>
            {
                ["AverageTransactionAmount"] = totalTransactions > 0
                    ? Math.Round(totalAmount / totalTransactions, 2)
                    : 0,
                ["HighValueCount"] = transactions.Count(t => !string.Equals(t.Flag, "Normal", StringComparison.OrdinalIgnoreCase)),
                ["UniqueAccounts"] = transactions.Select(t => t.AccountId).Distinct().Count()
            };

            return (totalTransactions, totalAmount, growthRate, additionalData);
        }

        private static ReportDto MapToDto(Report report)
        {
            return new ReportDto
            {
                Id = report.Id,
                AccountId = report.AccountId,
                DataJson = report.DataJson,
                FilePath = report.FilePath,
                FromDate = report.FromDate,
                GeneratedAt = report.GeneratedAt,
                GeneratedByUserId = report.GeneratedByUserId,
                GrowthRate = report.GrowthRate,
                ReportType = report.ReportType,
                Title = report.Title,
                ToDate = report.ToDate,
                TotalAmount = report.TotalAmount,
                TotalTransactions = report.TotalTransactions,
                TransactionStatus = report.TransactionStatus,
                TransactionType = report.TransactionType
            };
        }
    }
}
