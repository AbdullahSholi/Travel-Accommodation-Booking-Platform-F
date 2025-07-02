namespace Travel_Accommodation_Booking_Platform_F.Application.Services.TokenBlacklistService;

public interface ITokenBlacklistService
{
    Task<bool> IsTokenBlacklistedAsync(string jti);
    Task AddTokenToBlacklistAsync(string jti, DateTime expiration);
}