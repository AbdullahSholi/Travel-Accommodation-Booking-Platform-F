using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;

namespace Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

public interface IAuthRepository
{
    public Task<User?> GetUserByEmailAsync(string email);
    public Task<User?> GetUserByUsernameAsync(string username);
    public Task DeleteExpiredUnconfirmedUsersAsync();
    public Task<bool> EmailExistsAsync(string email);
    public Task<User?> RegisterUserAsync(User user);
    public Task SaveOtpAsync(OtpRecord otpRecord);
    public Task<OtpRecord?> GetOtpRecordAsync(string email, string otp);
    public Task HashAndSavePasswordAsync(User? user, string newPassword);
    public Task RemoveAndSaveOtpAsync(OtpRecord record);
    public Task UpdateUserAsync(User user);
    public Task InvalidateOtpAsync(OtpRecord otpRecord);
}