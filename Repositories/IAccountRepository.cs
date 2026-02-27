using UserApi.DTOs;
using UserApi.Models;

namespace UserApi.Repositories
{
    public interface IAccountRepository
    {
        Task<PagedResult<Account>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? status = null,
        string? accountType = null);
        Task<Account?> GetByIdAsync(string id);
        Task<Account?> GetByCustomerIdAsync(string customerId);
        Task<bool> CustomerIdExistsAsync(string customerId);
        Task<bool> AccountIdExistsAsync(string accountId);
        Task<Account> AddAsync(Account account);
        Task<Account> UpdateAsync(Account account);
        Task<bool> DeleteAsync(string id);
        Task FindAsync(string id);
    }
}
