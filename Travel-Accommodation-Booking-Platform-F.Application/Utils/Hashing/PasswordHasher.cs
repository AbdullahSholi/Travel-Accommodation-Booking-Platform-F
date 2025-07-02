using Isopoh.Cryptography.Argon2;

namespace Travel_Accommodation_Booking_Platform_F.Application.Utils.Hashing;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        var config = new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 4,
            MemoryCost = 65536,
            Lanes = 2,
            Threads = Environment.ProcessorCount,
            Password = System.Text.Encoding.UTF8.GetBytes(password),
            Salt = Argon2Generator.GenerateSalt(16),
            HashLength = 32
        };

        using var argon2A = new Argon2(config);
        var hash = argon2A.Hash();
        return config.EncodeString(hash.Buffer);
    }

    public static bool VerifyPassword(string hashEncoded, string inputPassword)
    {
        return Argon2.Verify(hashEncoded, inputPassword);
    }
}