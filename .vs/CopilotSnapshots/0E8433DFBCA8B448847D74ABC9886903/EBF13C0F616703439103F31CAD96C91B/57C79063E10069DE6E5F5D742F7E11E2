using System.Text.Json;
using UserApi.DTOs;
using UserApi.Models;
using UserApi.Repositories;
using UserApi.Services;

namespace AccountTrack.Api.Services;

public class ApprovalService : IApprovalService
{
    private readonly IApprovalRepository _approvalRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly INotificationService _notificationService;

    public ApprovalService(
        IApprovalRepository approvalRepository,
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        INotificationService notificationService)
    {
        _approvalRepository = approvalRepository;
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _notificationService = notificationService;
    }

    public async Task<UserApi.DTOs.PagedResult<UserApi.Models.Approval>> GetAllApprovalsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? decision = null,
        string? type = null)
    {
        var approvals = await _approvalRepository.GetAllAsync();

        // Apply filters
        IEnumerable<Approval> filteredApprovals = approvals;

        if (!string.IsNullOrWhiteSpace(decision))
        {
            filteredApprovals = filteredApprovals.Where(a =>
                a.Decision.ToString().Equals(decision, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            filteredApprovals = filteredApprovals.Where(a =>
                a.Type.ToString().Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        // Apply pagination
        var helperResult = UserApi.Helpers.PaginationHelper.CreatePagedResult(filteredApprovals, pageNumber, pageSize);

        // Convert to DTOs.PagedResult
        return new UserApi.DTOs.PagedResult<Approval>
        {
            Items = helperResult.Items,
            PageNumber = helperResult.PageNumber,
            PageSize = helperResult.PageSize,
            TotalCount = helperResult.TotalCount,
            TotalPages = helperResult.TotalPages
        };
    }

    public async Task<Approval?> GetApprovalByIdAsync(int id)
    {
        return await _approvalRepository.GetByIdAsync(id);
    }

    public async Task<Approval> ProcessApprovalDecisionAsync(int approvalId, UpdateApprovalDto dto)
    {
        var approval = await _approvalRepository.GetByIdAsync(approvalId);
        if (approval == null)
        {
            throw new InvalidOperationException($"Approval with ID {approvalId} not found.");
        }

        if (approval.Decision != ApprovalDecision.Pending)
        {
            throw new InvalidOperationException($"Approval has already been processed with decision: {approval.Decision}");
        }

        // Map decision
        var decision = dto.Decision switch
        {
            0 => ApprovalDecision.Pending,
            1 => ApprovalDecision.Approve,
            2 => ApprovalDecision.Reject,
            _ => throw new ArgumentException("Invalid decision value.")
        };

        approval.Decision = decision;
        approval.Comments = dto.Comments;
        approval.ApprovalDate = DateTime.UtcNow;

        // Handle different approval types
        switch (approval.Type)
        {
            case ApprovalType.AccountCreation:
                await HandleAccountCreationApproval(approval, decision);
                break;
            case ApprovalType.HighValueTransaction:
                if (approval.TransactionId.HasValue)
                {
                    var transaction = await _transactionRepository.GetByIdAsync(approval.TransactionId.Value);
                    if (transaction != null)
                    {
                        var sourceAccount = await _accountRepository.GetByIdAsync(transaction.AccountId);
                        if (sourceAccount == null)
                        {
                            throw new InvalidOperationException($"Source account {transaction.AccountId} not found.");
                        }
                        // Check balance for withdrawals and transfers
                        if (transaction.Type == "Withdrawal" || transaction.Type == "Transfer")
                        {
                            if (sourceAccount.Balance < transaction.Amount)
                            {
                                throw new InvalidOperationException($"Cannot approve transaction. Insufficient balance in account {transaction.AccountId}. Available balance: ₹{sourceAccount.Balance:N2}, Transaction amount: ₹{transaction.Amount:N2}");
                            }
                        }

                        // For transfers, validate target account
                        if (transaction.Type == "Transfer")
                        {
                            if (transaction.TargetAccountId == null)
                            {
                                throw new InvalidOperationException("Target account is required for transfer transactions.");
                            }

                            var targetAccount = await _accountRepository.GetByIdAsync(transaction.TargetAccountId.Value);
                            if (targetAccount == null)
                            {
                                throw new InvalidOperationException($"Target account {transaction.TargetAccountId} not found.");
                            }

                            if (targetAccount.Status == AccountStatus.Closed)
                            {
                                throw new InvalidOperationException($"Cannot approve transfer. Target account {transaction.TargetAccountId} is closed.");
                            }
                        }

                        if (decision == ApprovalDecision.Approve)
                        {
                            // Change status to Completed and update balances
                            transaction.Status = TransactionStatus.Completed;
                            await _transactionRepository.UpdateAsync(transaction);

                            // Update account balances
                            await UpdateAccountBalancesForTransaction(transaction);
                        }
                        else if (decision == ApprovalDecision.Reject)
                        {
                            // Change status to Rejected
                            transaction.Status = TransactionStatus.Rejected;
                            await _transactionRepository.UpdateAsync(transaction);
                        }

                        // Delete notification related to this transaction
                        await _notificationService.DeleteNotificationByTransactionIdAsync(transaction.TransactionId);
                    }
                }
                break;
            case ApprovalType.AccountUpdate:
                if (approval.AccountId.HasValue)
                {
                    var account = await _accountRepository.GetByIdAsync(approval.AccountId.Value);
                    if (account != null)
                    {
                        if (decision == ApprovalDecision.Approve)
                        {
                            // Apply pending changes
                            if (!string.IsNullOrEmpty(approval.PendingChanges))
                            {
                                var pendingChanges = JsonSerializer.Deserialize<UpdateAccountDto>(approval.PendingChanges);
                                if (pendingChanges != null)
                                {
                                    account.CustomerName = pendingChanges.CustomerName;
                                    account.CustomerId = pendingChanges.CustomerId;
                                    account.AccountType = (AccountType)pendingChanges.AccountType;
                                    account.Status = (AccountStatus)pendingChanges.Status;

                                    await _accountRepository.UpdateAsync(account);
                                }
                            }
                        }
                        // If rejected, just update the approval record - no changes to account
                    }
                }
                break;
            default:
                // If there are other types, handle them here or throw if unknown
                break;
        }

        // Delete notification related to this approval (for AccountCreation and AccountUpdate)
        if (decision != ApprovalDecision.Pending)
        {
            await _notificationService.DeleteNotificationByApprovalIdAsync(approvalId);
        }

        return await _approvalRepository.UpdateAsync(approval);
    }

    private async Task UpdateAccountBalancesForTransaction(Transaction transaction)
    {
        var sourceAccount = await _accountRepository.GetByIdAsync(transaction.AccountId);
        if (sourceAccount == null)
        {
            throw new InvalidOperationException($"Source account {transaction.AccountId} not found.");
        }

        // Parse transaction type
        switch (transaction.Type)
        {
            case "Deposit":
                sourceAccount.Balance += transaction.Amount;
                await _accountRepository.UpdateAsync(sourceAccount);
                break;

            case "Withdrawal":
                // Validate balance before withdrawal
                if (sourceAccount.Balance < transaction.Amount)
                {
                    throw new InvalidOperationException($"Insufficient balance for withdrawal. Account {transaction.AccountId} balance: ₹{sourceAccount.Balance:N2}, Transaction amount: ₹{transaction.Amount:N2}");
                }
                sourceAccount.Balance -= transaction.Amount;
                await _accountRepository.UpdateAsync(sourceAccount);
                break;

            case "Transfer":
                if (transaction.TargetAccountId == null)
                {
                    throw new InvalidOperationException("Target account is required for transfers.");
                }

                var targetAccount = await _accountRepository.GetByIdAsync(transaction.TargetAccountId.Value);
                if (targetAccount == null)
                {
                    throw new InvalidOperationException($"Target account {transaction.TargetAccountId} not found.");
                }

                // Validate balance before transfer
                if (sourceAccount.Balance < transaction.Amount)
                {
                    throw new InvalidOperationException($"Insufficient balance for transfer. Account {transaction.AccountId} balance: ₹{sourceAccount.Balance:N2}, Transaction amount: ₹{transaction.Amount:N2}");
                }

                // Deduct from source and add to target
                sourceAccount.Balance -= transaction.Amount;
                targetAccount.Balance += transaction.Amount;

                await _accountRepository.UpdateAsync(sourceAccount);
                await _accountRepository.UpdateAsync(targetAccount);
                break;

            default:
                throw new InvalidOperationException($"Unknown transaction type: {transaction.Type}");
        }
    }

    private async Task HandleAccountCreationApproval(Approval approval, ApprovalDecision decision)
    {
        if (approval.AccountId.HasValue)
        {
            var account = await _accountRepository.GetByIdAsync(approval.AccountId.Value);
            if (account != null)
            {
                if (decision == ApprovalDecision.Approve)
                {
                    account.Status = AccountStatus.Active;
                    await _accountRepository.UpdateAsync(account);
                }
                else if (decision == ApprovalDecision.Reject)
                {
                    account.Status = AccountStatus.Closed;
                    await _accountRepository.UpdateAsync(account);
                }
                // If Pending, do nothing
            }
        }
    }

    public async Task<UserApi.DTOs.PagedResult<ApprovalDetailsDto>> GetAllApprovalDetailsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? decision = null,
        string? type = null)
    {
        var approvals = await _approvalRepository.GetAllWithDetailsAsync();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(decision))
        {
            approvals = approvals.Where(a =>
                a.Decision.ToString().Equals(decision, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            approvals = approvals.Where(a =>
                a.Type.ToString().Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        var approvalDetailsList = approvals.Select(a => new ApprovalDetailsDto
        {
            ApprovalId = a.ApprovalId,
            Type = a.Type.ToString(),
            AccountId = a.AccountId,
            CustomerName = a.Account?.CustomerName,
            ReviewerId = a.ReviewerId,
            Decision = a.Decision.ToString(),
            PendingChanges = a.PendingChanges,
            ApprovalDate = a.ApprovalDate,
            Comments = a.Comments
        });

        // Apply pagination
        var helperResult = UserApi.Helpers.PaginationHelper.CreatePagedResult(approvalDetailsList, pageNumber, pageSize);

        // Convert to DTOs.PagedResult
        return new UserApi.DTOs.PagedResult<ApprovalDetailsDto>
        {
            Items = helperResult.Items,
            PageNumber = helperResult.PageNumber,
            PageSize = helperResult.PageSize,
            TotalCount = helperResult.TotalCount,
            TotalPages = helperResult.TotalPages
        };
    }
}