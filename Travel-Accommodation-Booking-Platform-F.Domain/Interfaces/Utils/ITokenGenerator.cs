namespace Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Utils;

public interface ITokenGenerator
{
    string GenerateToken(string username, string role);
}