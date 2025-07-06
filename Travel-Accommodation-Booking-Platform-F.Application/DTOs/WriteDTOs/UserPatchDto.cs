namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class UserPatchDto
{
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? DriverLicense { get; set; }
    public string? Role { get; set; }
}