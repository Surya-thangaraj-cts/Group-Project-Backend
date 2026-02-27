using Microsoft.EntityFrameworkCore;
using UserApi.Helpers;
using UserApi.Models;
using UserApprovalApi.Data;

namespace UserApi.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserApi.DTOs.PagedResult<Account>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? accountType = null)
        {
            var accounts = await _context.Accounts.ToListAsync();

            // Apply filters
            IEnumerable<Account> filteredAccounts = accounts;

            if (!string.IsNullOrWhiteSpace(status))
            {
                filteredAccounts = filteredAccounts.Where(a =>
                    a.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(accountType))
            {
                filteredAccounts = filteredAccounts.Where(a =>
                    a.AccountType.ToString().Equals(accountType, StringComparison.OrdinalIgnoreCase));
            }

            // Apply pagination
            var pagedResult = PaginationHelper.CreatePagedResult(filteredAccounts, pageNumber, pageSize);

            // Map Helpers.PagedResult<Account> to DTOs.PagedResult<Account>
            return new UserApi.DTOs.PagedResult<Account>
            {
                Items = pagedResult.Items,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount
            };
        }

        public async Task<Account?> GetByIdAsync(string id)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == id);
        }

        public async Task<Account?> GetByCustomerIdAsync(string customerId)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.CustomerId == customerId);
        }

        public async Task<bool> CustomerIdExistsAsync(string customerId)
        {
            return await _context.Accounts.AnyAsync(a => a.CustomerId == customerId);
        }

        public async Task<bool> AccountIdExistsAsync(string accountId)
        {
            return await _context.Accounts.AnyAsync(a => a.AccountId == accountId);
        }

        public async Task FindAsync(string id)
        {
            await _context.Accounts.FindAsync(id);
        }

        public async Task<Account> AddAsync(Account account)
        {
            // Account ID is provided by the officer - no auto-generation
            // Validation is done in the controller
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<Account> UpdateAsync(Account account)
        {
            // Ensure the entity is being tracked
            var existingAccount = await _context.Accounts.FindAsync(account.AccountId);
            if (existingAccount == null)
            {
                throw new InvalidOperationException($"Account with ID {account.AccountId} not found.");
            }

            // Update properties explicitly
            _context.Entry(existingAccount).CurrentValues.SetValues(account);

            await _context.SaveChangesAsync();
            return existingAccount;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return false;

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
