using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class ReviewPatchDto
{
    [Range(1, int.MaxValue)] public int? UserId { get; set; }
    [Range(1, int.MaxValue)] public int? HotelId { get; set; }

    [Range(0, 5)] public int? Rating { get; set; }

    [MinLength(5)] public string? Comment { get; set; } = string.Empty;

    [DataType(DataType.Date)] public DateTime CreatedAt { get; set; }
    [DataType(DataType.Date)] public DateTime? LastUpdated { get; set; }
}