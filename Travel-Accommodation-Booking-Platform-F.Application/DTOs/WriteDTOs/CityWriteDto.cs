using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class CityWriteDto
{
    [Required] [MinLength(3)] public string Name { get; set; }
    [Required] [MinLength(3)] public string Country { get; set; }
    [Required] [MinLength(3)] public string PostOffice { get; set; }
    [Required] [Range(1, int.MaxValue)] public int NumberOfHotels { get; set; }
    [Required] [DataType(DataType.Date)] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required] [DataType(DataType.Date)] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}