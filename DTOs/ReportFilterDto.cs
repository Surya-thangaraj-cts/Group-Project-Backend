using System;

namespace UserApprovalApi.DTOs
{
    public class ReportFilterDto
    {
        public string? ReportType { get; set; }
        public int? AccountId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
