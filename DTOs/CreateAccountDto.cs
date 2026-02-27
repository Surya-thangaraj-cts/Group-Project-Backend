using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs
{
    public class CreateAccountDto
    {
        [Required(ErrorMessage = "Account ID is required.")]
        [RegularExpression(@"^ACC\d{4,}$", ErrorMessage = "Account ID must be in format ACC followed by at least 4 digits (e.g., ACC0001).")]
        public string AccountId { get; set; } = "";

        [Required(ErrorMessage = "Customer name is required.")]
        [MinLength(1, ErrorMessage = "Customer name cannot be empty.")]
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "Customer ID is required.")]
        [MinLength(7, ErrorMessage = "Customer ID must be at least 7 characters long.")]
        public string CustomerId { get; set; } = "";

        [Required(ErrorMessage = "Account type is required.")]
        [Range(0, 1, ErrorMessage = "Account type must be 0 (Savings) or 1 (Current).")]
        public int AccountType { get; set; } // 0 = Savings, 1 = Current
    }
}
