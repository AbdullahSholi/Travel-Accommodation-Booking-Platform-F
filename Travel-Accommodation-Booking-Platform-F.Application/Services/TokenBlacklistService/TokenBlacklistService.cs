using Travel_Accommodation_Booking_Platform_F.Application.Services.TokenBlacklistService;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ITokenBlacklistedRepository _tokenBlacklistedRepository;

    public TokenBlacklistService(ITokenBlacklistedRepository tokenBlacklistedRepository)
    {
        _tokenBlacklistedRepository = tokenBlacklistedRepository;
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        return await _tokenBlacklistedRepository.CheckIfTokenBlacklistedAsync(jti);
    }

    public async Task AddTokenToBlacklistAsync(string jti, DateTime expiration)
    {
        var token = new BlacklistedToken
        {
            Jti = jti,
            Expiration = expiration
        };

        await _tokenBlacklistedRepository.AddTokenToBlacklistAsync(token);
    }
}