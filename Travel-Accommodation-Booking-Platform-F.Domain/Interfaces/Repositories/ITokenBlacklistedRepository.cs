using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;

namespace Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

public interface ITokenBlacklistedRepository
{
    public Task<bool> CheckIfTokenBlacklistedAsync(string jti);
    public Task AddTokenToBlacklistAsync(BlacklistedToken token);
}