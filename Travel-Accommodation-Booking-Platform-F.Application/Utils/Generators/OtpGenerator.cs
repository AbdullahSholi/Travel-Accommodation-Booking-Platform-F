using System.Text;

namespace Travel_Accommodation_Booking_Platform_F.Application.Utils.Generators;

public static class OtpGenerator
{
    public static string GenerateOtp(int length = 6)
    {
        var rng = new Random();
        var otp = new StringBuilder();
        for (var i = 0; i < length; i++) otp.Append(rng.Next(0, 10));
        return otp.ToString();
    }
}