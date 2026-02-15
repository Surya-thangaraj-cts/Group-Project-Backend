//using UserApi.Data;
using Microsoft.EntityFrameworkCore;
using UserApi.Models;
using UserApi.Repositories;
using UserApprovalApi.Data;


namespace UserApi.Repositories;

public class ApprovalRepository : IApprovalRepository
{
    private readonly AppDbContext _context;

    public ApprovalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Approval>> GetAllAsync()
    {
        return await _context.Approvals
            .Include(a => a.Transaction)
            .Include(a => a.Account)
            .Include(a => a.Reviewer)
            .ToListAsync();
    }

    public async Task<Approval?> GetByIdAsync(int id)
    {
        return await _context.Approvals
            .Include(a => a.Transaction)
            .Include(a => a.Account)
            .Include(a => a.Reviewer)
            .FirstOrDefaultAsync(a => a.ApprovalId == id);
    }

    public async Task<Approval> AddAsync(Approval approval)
    {
        _context.Approvals.Add(approval);
        await _context.SaveChangesAsync();
        return approval;
    }

    public async Task<Approval> UpdateAsync(Approval approval)
    {
        _context.Approvals.Update(approval);
        await _context.SaveChangesAsync();
        return approval;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var approval = await _context.Approvals.FindAsync(id);
        if (approval == null) return false;

        _context.Approvals.Remove(approval);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Approval?> GetByTransactionIdAsync(int transactionId)
    {
        return await _context.Approvals
            .Include(a => a.Transaction)
            .FirstOrDefaultAsync(a => a.TransactionId == transactionId);
    }

    public async Task<Approval?> GetPendingAccountApprovalAsync(int accountId)
    {
        return await _context.Approvals
            .Include(a => a.Account)
            .FirstOrDefaultAsync(a => a.AccountId == accountId && a.Decision == ApprovalDecision.Pending);
    }

    public async Task<IEnumerable<Approval>> GetAllWithDetailsAsync()
    {
        return await _context.Approvals
            .Include(a => a.Account)
            .Include(a => a.Reviewer)
            .Include(a => a.Transaction)
            .ToListAsync();
    }
}