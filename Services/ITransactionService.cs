using UserApi.Models;
using UserApi.DTOs;

namespace AccountTrack.Api.Services;

public interface ITransactionService
{
    Task<PagedResult<Transaction>> GetAllTransactionsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        int? accountId = null,
        string? type = null,
        string? status = null,
        string? flag = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    Task<Transaction?> GetTransactionByIdAsync(int id);
    Task<Transaction> CreateTransactionAsync(CreateTransactionDto dto);
    Task<Transaction> UpdateTransactionAsync(int id, Transaction transaction);
    Task<bool> DeleteTransactionAsync(int id);
}