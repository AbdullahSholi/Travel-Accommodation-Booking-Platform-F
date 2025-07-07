using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class CityPatchDto
{
    [MinLength(3)] public string? Name { get; set; }
    [MinLength(3)] public string? Country { get; set; }
    [MinLength(3)] public string? PostOffice { get; set; }
    [Range(1, int.MaxValue)] public int? NumberOfHotels { get; set; }
    [DataType(DataType.Date)] public DateTime? UpdatedAt { get; set; }
}