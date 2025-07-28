using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class LoginWriteDto
{
    public string? Email { get; set; }
    public string? Username { get; set; }
    [Required] public string Password { get; set; }
}