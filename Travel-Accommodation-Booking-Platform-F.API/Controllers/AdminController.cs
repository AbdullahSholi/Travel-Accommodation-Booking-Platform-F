using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AdminService;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.AdminExceptions;
using Travel_Accommodation_Booking_Platform_F.Utils.Admin;
using CustomMessages = Travel_Accommodation_Booking_Platform_F.Utils.Admin.CustomMessages;

namespace Travel_Accommodation_Booking_Platform_F.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AdminsController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminsController> _logger;

    public AdminsController(IAdminService adminService, ILogger<AdminsController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] UserWriteDto dto)
    {
        _logger.LogInformation(LogMessages.CreateUserRequestReceived, dto.Email);
        try
        {
            var user = await _adminService.CreateUserAsync(dto);
            if (user == null)
            {
                _logger.LogWarning(LogMessages.CreateUserFailed, dto.Email);
                return BadRequest(new { Message = CustomMessages.InvalidUserCreationCredentials });
            }

            _logger.LogInformation(LogMessages.CreateUserSuccess, dto.Email);
            return CreatedAtAction(nameof(GetUserById), new { Id = user.UserId }, user);
        }
        catch (InvalidUserDataReceivedException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (DuplicatedEmailException ex)
        {
            _logger.LogError(ex, ex.Message);
            return Conflict(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.CreateUserFailed, dto.Email);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        _logger.LogInformation(LogMessages.GetUsersRequestReceived);

        try
        {
            var users = await _adminService.GetUsersAsync();
            if (users == null)
            {
                _logger.LogWarning(LogMessages.GetUsersFailed);
                return NotFound(new { Message = CustomMessages.ListOfUsersIsNotFound });
            }

            _logger.LogInformation(LogMessages.GetUsersSuccess);
            return Ok(users);
        }
        catch (FailedToFetchUsersException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetUsersFailed);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUserById([FromRoute] int id)
    {
        _logger.LogInformation(LogMessages.GetUserRequestReceived, id);

        try
        {
            var user = await _adminService.GetUserAsync(id);
            if (user == null)
            {
                _logger.LogWarning(LogMessages.GetUserFailed, id);
                return NotFound(new { Message = CustomMessages.UserNotFound });
            }

            _logger.LogInformation(LogMessages.GetUserSuccess, id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetUserFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("users/{id:int}")]
    public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] UserPatchDto dto)
    {
        try
        {
            _logger.LogInformation(LogMessages.UpdateUserRequestReceived, id);
            var updatedUser = await _adminService.UpdateUserAsync(id, dto);
            if (updatedUser == null)
            {
                _logger.LogWarning(LogMessages.UpdateUserFailed, id);
                return NotFound(new { Message = CustomMessages.UserNotFound });
            }

            _logger.LogInformation(LogMessages.UserUpdatedSuccessfully, id);
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.UpdateUserFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser([FromRoute] int id)
    {
        try
        {
            _logger.LogInformation(LogMessages.DeleteUserRequestReceived, id);
            await _adminService.DeleteUserAsync(id);

            _logger.LogInformation(LogMessages.UserUpdatedSuccessfully, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.DeleteUserFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }
}