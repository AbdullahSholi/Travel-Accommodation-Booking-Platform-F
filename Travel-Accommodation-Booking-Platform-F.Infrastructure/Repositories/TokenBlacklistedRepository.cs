using Microsoft.EntityFrameworkCore;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;

public class TokenBlacklistedRepository : ITokenBlacklistedRepository
{
    private readonly ApplicationDbContext _context;

    public TokenBlacklistedRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckIfTokenBlacklistedAsync(string jti)
    {
        var exists = await _context.BlacklistedTokens
            .AnyAsync(t => t.Jti == jti && t.Expiration > DateTime.UtcNow);

        return exists;
    }

    public async Task AddTokenToBlacklistAsync(BlacklistedToken token)
    {
        _context.BlacklistedTokens.Add(token);
        await _context.SaveChangesAsync();
    }
}