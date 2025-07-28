namespace Travel_Accommodation_Booking_Platform_F.Domain.Configurations;

public class BlacklistedToken
{
    public int Id { get; set; }
    public string Jti { get; set; }
    public DateTime Expiration { get; set; }
}