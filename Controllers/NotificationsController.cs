using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApi.DTOs;
using UserApi.Services;

namespace UserApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Officer")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // GET: api/notifications
    [HttpGet]
    public async Task<IActionResult> GetAllNotifications(
        [FromQuery] string? type = null,
        [FromQuery] string? status = null)
    {
        try
        {
            var notifications = await _notificationService.GetAllNotificationsAsync(type, status);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET: api/notifications/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotificationById(string id)
    {
        try
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound(new { error = $"Notification with ID {id} not found." });
            }
            return Ok(notification);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // PUT: api/notifications/{id}/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateNotificationStatus(string id, [FromBody] UpdateNotificationStatusDto dto)
    {
        try
        {
            var updatedNotification = await _notificationService.UpdateNotificationStatusAsync(id, dto);
            return Ok(updatedNotification);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // DELETE: api/notifications/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(string id)
    {
        try
        {
            await _notificationService.DeleteNotificationAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // DELETE: api/notifications
    [HttpDelete]
    public async Task<IActionResult> DeleteAllNotifications()
    {
        try
        {
            await _notificationService.DeleteAllNotificationsAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}