using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;

namespace Travel_Accommodation_Booking_Platform_F.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Address1 { get; set; } = string.Empty;

    public string Address2 { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string DriverLicense { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; } = false;

    public string Role { get; set; } = "User";

    public List<OtpRecord>? OtpRecords { get; set; }
    public List<Booking>? Bookings { get; set; }
}