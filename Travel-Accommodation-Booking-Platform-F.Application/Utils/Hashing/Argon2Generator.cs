using System.Security.Cryptography;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Hashing;

public static class Argon2Generator
{
    public static byte[] GenerateSalt(int length)
    {
        var salt = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }
}