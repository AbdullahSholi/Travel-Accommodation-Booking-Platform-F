using Microsoft.Extensions.Logging;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.TokenBlacklistService;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ITokenBlacklistedRepository _tokenBlacklistedRepository;
    private readonly ILogger<TokenBlacklistService> _logger;

    public TokenBlacklistService(ITokenBlacklistedRepository tokenBlacklistedRepository,
        ILogger<TokenBlacklistService> logger)
    {
        _tokenBlacklistedRepository = tokenBlacklistedRepository;
        _logger = logger;
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        if (string.IsNullOrEmpty(jti))
        {
            _logger.LogWarning(TokenBlacklistLogMessages.InvalidJtiProvidedForBlacklistCheck);
            throw new ArgumentNullException(nameof(jti));
        }

        _logger.LogInformation(TokenBlacklistLogMessages.CheckTokenBlacklistStatus, jti);

        var isBlacklisted = await _tokenBlacklistedRepository.CheckIfTokenBlacklistedAsync(jti);

        if (isBlacklisted)
            _logger.LogWarning(TokenBlacklistLogMessages.TokenIsBlacklisted, jti);
        else
            _logger.LogInformation(TokenBlacklistLogMessages.TokenIsNotBlacklisted, jti);

        return isBlacklisted;
    }

    public async Task AddTokenToBlacklistAsync(string jti, DateTime expiration)
    {
        if (string.IsNullOrEmpty(jti))
        {
            _logger.LogWarning(TokenBlacklistLogMessages.InvalidJtiProvidedForBlacklistAddition);
            throw new ArgumentNullException(nameof(jti));
        }

        if (expiration < DateTime.Now)
        {
            _logger.LogWarning(TokenBlacklistLogMessages.InvalidExpirationDateProvided, jti, expiration);
            throw new ArgumentNullException(nameof(expiration));
        }

        _logger.LogInformation(TokenBlacklistLogMessages.AddingTokenToBlacklist, jti, expiration);

        var token = new BlacklistedToken
        {
            Jti = jti,
            Expiration = expiration
        };

        await _tokenBlacklistedRepository.AddTokenToBlacklistAsync(token);

        _logger.LogInformation(TokenBlacklistLogMessages.TokenBlacklistedSuccessfully, jti);
    }
}