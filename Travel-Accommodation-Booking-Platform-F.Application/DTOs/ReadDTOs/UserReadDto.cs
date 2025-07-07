namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;

public class UserReadDto
{
    public int UserId { get; set; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; init; } = "User";
    public DateTime LastUpdated { get; set; }
}