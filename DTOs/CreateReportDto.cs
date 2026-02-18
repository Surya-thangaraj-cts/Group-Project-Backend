using System;
using System.ComponentModel.DataAnnotations;

namespace UserApprovalApi.DTOs
{
    public class CreateReportDto
    {
        [Required]
        [StringLength(50)]
        public string ReportType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public int? AccountId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TransactionStatus { get; set; }
        public string? TransactionType { get; set; }
    }
}
