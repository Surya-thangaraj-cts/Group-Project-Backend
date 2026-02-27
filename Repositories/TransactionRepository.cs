//using UserApi.Data;
using UserApi.Models;
using Microsoft.EntityFrameworkCore;
using UserApi.Repositories;
using UserApprovalApi.Data;
using UserApi.Helpers;

namespace UserApi.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await _context.Transactions.ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(string id)
    {
        return await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == id);
    }

    public async Task<bool> TransactionIdExistsAsync(string transactionId)
    {
        return await _context.Transactions.AnyAsync(t => t.TransactionId == transactionId);
    }

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        // Generate unique Transaction ID
        string newTransactionId;
        do
        {
            newTransactionId = IdGenerator.GenerateTransactionId();
        } while (await TransactionIdExistsAsync(newTransactionId));

        transaction.TransactionId = newTransactionId;

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction> UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var transaction = await GetByIdAsync(id);
        if (transaction == null)
            return false;

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return true;
    }
}