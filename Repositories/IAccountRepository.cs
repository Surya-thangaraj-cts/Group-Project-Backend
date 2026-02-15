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
        Task<Account?> GetByIdAsync(int id);
        Task<Account> AddAsync(Account account);
        Task<Account> UpdateAsync(Account account);
        Task<bool> DeleteAsync(int id);
        Task FindAsync(int id);
    }
}
