using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class RefreshTokenWriteDto
{
    [Required] public string RefreshToken { get; set; }
}