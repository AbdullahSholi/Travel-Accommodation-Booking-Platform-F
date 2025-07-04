using Travel_Accommodation_Booking_Platform_F.Domain.Entities;

namespace Travel_Accommodation_Booking_Platform_F.Domain.Configurations;

public class OtpRecord
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Code { get; set; }
    public DateTime Expiration { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
}