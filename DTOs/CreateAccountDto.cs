using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs
{
    public class CreateAccountDto
    {
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
