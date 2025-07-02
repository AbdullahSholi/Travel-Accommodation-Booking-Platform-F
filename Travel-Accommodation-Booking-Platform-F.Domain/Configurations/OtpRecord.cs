namespace Travel_Accommodation_Booking_Platform_F.Domain.Configurations;

public class OtpRecord
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Code { get; set; }
    public DateTime Expiration { get; set; }
}