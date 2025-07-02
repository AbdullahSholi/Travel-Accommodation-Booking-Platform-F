using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.Hashing;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;
    private readonly EmailSettings _emailSettings;

    public AuthRepository(ApplicationDbContext context, IOptions<EmailSettings> emailSettings)
    {
        _context = context;
        _emailSettings = emailSettings.Value;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

        return user;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);

        return user;
    }

    public async Task DeleteExpiredUnconfirmedUsersAsync()
    {
        var expirationLimit = DateTime.UtcNow;
        var usersToDelete = await _context.Users
            .Where(u => !u.IsEmailConfirmed)
            .Join(_context.OtpRecords,
                user => user.Email,
                otp => otp.Email,
                (user, otp) => new { user, otp })
            .Where(x => x.otp.Expiration < expirationLimit)
            .Select(x => x.user)
            .ToListAsync();

        if (usersToDelete.Any())
        {
            _context.Users.RemoveRange(usersToDelete);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }

    public async Task<User?> RegisterUserAsync(User user)
    {
        user.Password = PasswordHasher.HashPassword(user.Password);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task SaveOtpAsync(OtpRecord otpRecord)
    {
        await _context.OtpRecords.AddAsync(otpRecord);
        await _context.SaveChangesAsync();
    }
    
    public async Task<OtpRecord?> GetOtpRecordAsync(string email, string otp)
    {
        var record = await _context.OtpRecords
            .Where(r => r.Email == email && r.Code == otp)
            .OrderByDescending(r => r.Expiration)
            .FirstOrDefaultAsync();

        return record;
    }

    public async Task HashAndSavePasswordAsync(User? user, string newPassword)
    {
        user.Password = PasswordHasher.HashPassword(newPassword);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAndSaveOtpAsync(OtpRecord record)
    {
        _context.OtpRecords.Remove(record);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task InvalidateOtpAsync(OtpRecord otpRecord)
    {
        _context.OtpRecords.Remove(otpRecord);
        await _context.SaveChangesAsync();
    }
}