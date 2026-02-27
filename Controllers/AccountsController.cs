using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UserApi.DTOs;
using UserApi.Models;
using UserApi.Repositories;
using UserApi.Services;

namespace UserApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Officer")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IApprovalRepository _approvalRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public AccountsController(
        IAccountRepository accountRepository,
        IApprovalRepository approvalRepository,
        INotificationService notificationService,
        IMapper mapper)
    {
        _accountRepository = accountRepository;
        _approvalRepository = approvalRepository;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            AccountStatus? filterStatus = null;

            if (status.HasValue)
            {
                // Validate the status value (0 = Active, 1 = Closed, 2 = Pending)
                if (status.Value < 0 || status.Value > 2)
                {
                    return BadRequest(new { error = "Invalid status value. Use 0 (Active), 1 (Closed), or 2 (Pending)." });
                }

                filterStatus = (AccountStatus)status.Value;
            }

            // Updated to match repository signature
            string? statusString = filterStatus?.ToString();

            var pagedAccounts = await _accountRepository.GetAllAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                status: statusString,
                accountType: null
            );
            var accountDtos = _mapper.Map<IEnumerable<AccountDto>>(pagedAccounts.Items);
            return Ok(accountDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving accounts.", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id);
            return account is null ? NotFound() : Ok(_mapper.Map<AccountDto>(account));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving the account.", details = ex.Message });
        }
    }

    [HttpGet("check-customer/{customerId}")]
    public async Task<IActionResult> CheckCustomerIdExists(string customerId)
    {
        try
        {
            var exists = await _accountRepository.CustomerIdExistsAsync(customerId);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while checking customer ID.", details = ex.Message });
        }
    }

    [HttpGet("check-account/{accountId}")]
    public async Task<IActionResult> CheckAccountIdExists(string accountId)
    {
        try
        {
            var exists = await _accountRepository.AccountIdExistsAsync(accountId);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while checking account ID.", details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountDto dto)
    {
        try
        {
            // Model validation is automatically handled by ASP.NET Core
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto is null)
                return BadRequest("Request body is required.");

            // Check if Account ID already exists
            if (await _accountRepository.AccountIdExistsAsync(dto.AccountId))
            {
                return BadRequest(new { error = "Account ID already exists. Please use a unique Account ID." });
            }

            // Check if Customer ID already exists
            if (await _accountRepository.CustomerIdExistsAsync(dto.CustomerId))
            {
                return BadRequest(new { error = "Customer ID already exists. Please use a unique Customer ID." });
            }

            // Create account with Pending status and Balance = 0
            // Account ID is provided by the officer
            var account = new Account
            {
                AccountId = dto.AccountId,  // Officer provides the Account ID
                CustomerName = dto.CustomerName,
                CustomerId = dto.CustomerId,
                AccountType = (AccountType)dto.AccountType,
                Balance = 0, // Default balance to 0
                Status = AccountStatus.Pending // Set to Pending initially
            };

            // Save account (Account ID is already set from DTO)
            var createdAccount = await _accountRepository.AddAsync(account);

            // Create approval record for account creation
            var approval = new Approval
            {
                Type = ApprovalType.AccountCreation,
                TransactionId = null,
                AccountId = createdAccount.AccountId,
                ReviewerId = 1, // Random reviewer ID (implement proper logic)
                Decision = ApprovalDecision.Pending,
                Comments = $"New account creation request for customer {dto.CustomerName}",
                ApprovalDate = DateTime.UtcNow,
                PendingChanges = JsonSerializer.Serialize(dto) // Store creation details
            };

            await _approvalRepository.AddAsync(approval);

            // Create notification for pending account creation approval
            await _notificationService.CreateNotificationForApprovalAsync(
                approval.ApprovalId,
                approval.ReviewerId,
                ApprovalType.AccountCreation
            );

            return Ok(new
            {
                message = "Account creation request submitted for approval",
                accountId = createdAccount.AccountId,
                approvalId = approval.ApprovalId,
                status = "Pending"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while creating the account.", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateAccountDto updated)
    {
        try
        {
            // Model validation is automatically handled by ASP.NET Core
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (updated is null)
                return BadRequest("Request body is required.");

            var existing = await _accountRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            // Create approval record for account update
            var approval = new Approval
            {
                Type = ApprovalType.AccountUpdate,
                TransactionId = null,
                AccountId = id,
                ReviewerId = 1, // Random reviewer ID (implement proper logic)
                Decision = ApprovalDecision.Pending,
                Comments = $"Account update request for Account ID {id}",
                ApprovalDate = DateTime.UtcNow,
                PendingChanges = JsonSerializer.Serialize(updated) // Store pending changes as JSON
            };

            await _approvalRepository.AddAsync(approval);

            // Create notification for pending account update approval
            await _notificationService.CreateNotificationForApprovalAsync(
                approval.ApprovalId,
                approval.ReviewerId,
                ApprovalType.AccountUpdate
            );

            return Ok(new
            {
                message = "Account update request submitted for approval",
                approvalId = approval.ApprovalId,
                status = "Pending"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while updating the account.", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var deleted = await _accountRepository.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while deleting the account.", details = ex.Message });
        }
    }
}
