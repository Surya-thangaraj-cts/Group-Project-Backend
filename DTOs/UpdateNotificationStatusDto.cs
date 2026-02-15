using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs;

public class UpdateNotificationStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    [Range(0, 1, ErrorMessage = "Status must be either 0 (Unread) or 1 (Read).")]
    public int Status { get; set; } // 0 = Unread, 1 = Read
}