using System.ComponentModel.DataAnnotations;
using UserApi.Models;

namespace UserApi.DTOs
{
    public class UpdateAccountDto
    {
        [Required(ErrorMessage = "Customer name is required.")]
        [MinLength(1, ErrorMessage = "Customer name cannot be empty.")]
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "Customer ID is required.")]
        [MinLength(7, ErrorMessage = "Customer ID must be at least 7 characters long.")]
        public string CustomerId { get; set; } = "";

        [Required(ErrorMessage = "Account type is required.")]
        public AccountType AccountType { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public AccountStatus Status { get; set; }
    }
}
