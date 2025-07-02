using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class VerifyOtpWriteDto
{
    [Required] [EmailAddress] public string Email { get; set; }

    [Required] public string OtpCode { get; set; }
}