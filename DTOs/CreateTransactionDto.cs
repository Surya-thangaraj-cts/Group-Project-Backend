using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs;

public class CreateTransactionDto : IValidatableObject
{
    [Required(ErrorMessage = "AccountId is required.")]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "TransactionType is required.")]
    [Range(1, 3, ErrorMessage = "TransactionType must be 1 (Deposit), 2 (Withdrawal), or 3 (Transfer).")]
    public int TransactionType { get; set; } // 1 = Deposit, 2 = Withdrawal, 3 = Transfer

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    public string Narrative { get; set; } = string.Empty;

    public int? ToAccountId { get; set; } // Only required when TransactionType = 3 (Transfer)

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Custom validation: ToAccountId is required when TransactionType is 3 (Transfer)
        if (TransactionType == 3 && (!ToAccountId.HasValue || ToAccountId.Value <= 0))
        {
            yield return new ValidationResult(
                "ToAccountId is required for Transfer transactions.",
                new[] { nameof(ToAccountId) });
        }
    }
}