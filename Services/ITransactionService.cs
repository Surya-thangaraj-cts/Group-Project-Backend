using UserApi.Models;
using UserApi.DTOs;

namespace AccountTrack.Api.Services;

public interface ITransactionService
{
    Task<PagedResult<Transaction>> GetAllTransactionsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? accountId = null,
        string? type = null,
        string? status = null,
        string? flag = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    Task<Transaction?> GetTransactionByIdAsync(string id);
    Task<Transaction> CreateTransactionAsync(CreateTransactionDto dto);
    Task<Transaction> UpdateTransactionAsync(string id, Transaction transaction);
    Task<bool> DeleteTransactionAsync(string id);
}