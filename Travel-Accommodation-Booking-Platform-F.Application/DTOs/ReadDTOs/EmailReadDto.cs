using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;

public class EmailReadDto
{
    [Required] public string Email { get; set; } = string.Empty;
}