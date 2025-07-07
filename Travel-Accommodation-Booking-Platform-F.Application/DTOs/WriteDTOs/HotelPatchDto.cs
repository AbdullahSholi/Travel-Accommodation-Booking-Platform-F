using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class HotelPatchDto
{
    [MaxLength(100)] [MinLength(3)] public string? HotelName { get; set; }
    [MaxLength(50)] [MinLength(3)] public string? OwnerName { get; set; }
    [MinLength(5)] public string? Location { get; set; }
    [MinLength(11)] public string? Description { get; set; }

    public int? CityId { get; set; }
}