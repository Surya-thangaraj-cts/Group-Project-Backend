using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs;

public class UpdateApprovalDto
{
    [Required(ErrorMessage = "Decision is required.")]
    [Range(1, 2, ErrorMessage = "Decision must be either 1 (Approve) or 2 (Reject).")]
    public int Decision { get; set; } // 1 = Approve, 2 = Reject

    public string Comments { get; set; } = "";
}