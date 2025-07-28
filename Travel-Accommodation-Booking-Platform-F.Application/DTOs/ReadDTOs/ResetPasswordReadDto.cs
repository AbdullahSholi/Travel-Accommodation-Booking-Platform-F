namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;

public class ResetPasswordReadDto
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}