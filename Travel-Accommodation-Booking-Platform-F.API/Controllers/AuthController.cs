using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;
using Travel_Accommodation_Booking_Platform_F.Application.Services.TokenBlacklistService;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions;

namespace Travel_Accommodation_Booking_Platform_F.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginWriteDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(new { result });
        }
        catch (ValidationAppException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = CustomMessages.CustomMessages.InternalServerError });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserWriteDto dto)
    {
        try
        {
            var user = await _authService.RegisterAsync(dto);
            if (user == null)
                return BadRequest(new { Message = CustomMessages.CustomMessages.InvalidCredentials });

            return Ok(new
            {
                User = user
            });
        }
        catch (Exception e)
        {
            return BadRequest(new { e.Message });
        }
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] EmailReadDto readDto)
    {
        try
        {
            var isSent = await _authService.SendOtpAsync(readDto.Email);

            return Ok(new { Message = CustomMessages.CustomMessages.EmailSentSuccessfully });
        }
        catch (Exception e)
        {
            return NotFound( new { Message = e.Message });
        }
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordReadDto readDto)
    {
        try
        {
            var success = await _authService.ResetPasswordAsync(readDto);
            return Ok(new { Message = CustomMessages.CustomMessages.PasswordResetSuccessfully });
        }
        catch (Exception e)
        {
            return BadRequest(new { Message = e.Message } );
        }
    }
    
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpWriteDto dto)
    {
        try
        {
            var result = await _authService.VerifyOtpAsync(dto.Email, dto.OtpCode);
            if (result)
                return Ok(new { Message = "Email verified successfully." });

            return BadRequest(new { Message = "Failed to verify email." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromServices] ITokenBlacklistService blacklistService)
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var exp = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

        if (jti == null || exp == null)
            return BadRequest(new { Message = CustomMessages.CustomMessages.InvalidToken });

        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).UtcDateTime;

        await blacklistService.AddTokenToBlacklistAsync(jti, expirationTime);

        return Ok(new { Message = CustomMessages.CustomMessages.LoggedOutSuccessfully });
    }
}