using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;
using Travel_Accommodation_Booking_Platform_F.Application.Services.TokenBlacklistService;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions;
using Travel_Accommodation_Booking_Platform_F.Utils.Auth;

namespace Travel_Accommodation_Booking_Platform_F.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginWriteDto dto)
    {
        _logger.LogInformation(LogMessages.LoginRequestReceived, dto.Email);
        try
        {
            var result = await _authService.LoginAsync(dto);

            _logger.LogInformation(LogMessages.LoginSuccessful, dto.Email);
            return Ok(new { result });
        }
        catch (ValidationAppException ex)
        {
            _logger.LogWarning(ex, LogMessages.LoginValidationFailed, dto.Email);
            return BadRequest(new { Message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, LogMessages.LoginUserNotFound, dto.Email);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.LoginUnexpectedError, dto.Email);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserWriteDto dto)
    {
        _logger.LogInformation(LogMessages.RegisterRequestReceived, dto.Email);
        try
        {
            var user = await _authService.RegisterAsync(dto);
            if (user == null)
            {
                _logger.LogWarning(LogMessages.RegisterFailed, dto.Email);
                return BadRequest(new { Message = CustomMessages.InvalidCredentials });
            }

            _logger.LogInformation(LogMessages.RegisterSuccessful, dto.Email);
            return Ok(new
            {
                User = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.RegisterUnexpectedError, dto.Email);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] EmailReadDto readDto)
    {
        _logger.LogInformation(LogMessages.ForgotPasswordRequestReceived, readDto.Email);
        try
        {
            var isSent = await _authService.SendOtpAsync(readDto.Email);

            if (isSent)
            {
                _logger.LogInformation(LogMessages.ForgotPasswordOtpSent, readDto.Email);
                return Ok(new { Message = CustomMessages.EmailSentSuccessfully });
            }

            _logger.LogWarning(LogMessages.ForgotPasswordFailedToSent, readDto.Email);
            return BadRequest(new { Message = CustomMessages.EmailSentFailed });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.ForgotPasswordUnexpectedError, readDto.Email);
            return NotFound(new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordReadDto readDto)
    {
        _logger.LogInformation(LogMessages.ResetPasswordRequestReceived, readDto.Email);
        try
        {
            var isSuccess = await _authService.ResetPasswordAsync(readDto);
            if (isSuccess)
            {
                _logger.LogInformation(LogMessages.ResetPasswordSuccessful, readDto.Email);
                return Ok(new { Message = CustomMessages.PasswordResetSuccessfully });
            }

            _logger.LogWarning(LogMessages.ResetPasswordFailed, readDto.Email);
            return BadRequest(new { Message = CustomMessages.FailedToResetPassword });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.ResetPasswordUnexpectedError, readDto.Email);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpWriteDto dto)
    {
        _logger.LogInformation(LogMessages.VerifyOtpRequestReceived, dto.Email);
        try
        {
            var isVerified = await _authService.VerifyOtpAsync(dto.Email, dto.OtpCode);
            if (isVerified)
            {
                _logger.LogInformation(LogMessages.VerifyOtpSuccessful, dto.Email);
                return Ok(new { Message = CustomMessages.EmailVerifiedSuccessfully });
            }

            _logger.LogWarning(LogMessages.VerifyOtpFailed, dto.Email);
            return BadRequest(new { Message = CustomMessages.EmailVerificationFailed });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.VerifyOtpUnexpectedError, dto.Email);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromServices] ITokenBlacklistService blacklistService)
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length) : authHeader;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var exp = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

            if (jti == null || exp == null)
            {
                _logger.LogWarning(LogMessages.LogoutInvalidToken);
                return BadRequest(new { Message = CustomMessages.InvalidToken });
            }

            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).UtcDateTime;

            await blacklistService.AddTokenToBlacklistAsync(jti, expirationTime);

            _logger.LogInformation(LogMessages.LogoutSuccessful);
            return Ok(new { Message = CustomMessages.LoggedOutSuccessfully });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.LogoutUnexpectedError);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }
}