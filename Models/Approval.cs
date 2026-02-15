using UserApi.Models;

namespace UserApi.Models;

public class Approval
{
    public int ApprovalId { get; set; }

    // Type of approval request
    public ApprovalType Type { get; set; }

    // For transaction approvals (high-value transactions)
    public int? TransactionId { get; set; }
    public Transaction? Transaction { get; set; }

    // For account update/creation approvals
    public int? AccountId { get; set; }
    public Account? Account { get; set; }

    // Pending changes for account updates/creation (stored as JSON)
    public string? PendingChanges { get; set; }

    public int ReviewerId { get; set; }
    public User? Reviewer { get; set; }

    public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;
    public string Comments { get; set; } = "";
    public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
}
