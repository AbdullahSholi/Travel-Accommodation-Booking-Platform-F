using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;

public interface IAuthService
{
    public Task<LoginReadDto?> LoginAsync(LoginWriteDto dto);
    public Task<UserReadDto?> RegisterAsync(UserWriteDto userDto);
    public Task<bool> SendOtpAsync(string toEmail);
    public Task<bool> ResetPasswordAsync(ResetPasswordReadDto readDto);
    public Task<bool> VerifyOtpAsync(string email, string otpCode);
}