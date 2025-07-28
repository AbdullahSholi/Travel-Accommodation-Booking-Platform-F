using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class ReviewWriteDto
{
    [Required] [Range(1, int.MaxValue)] public int? UserId { get; set; }
    [Required] [Range(1, int.MaxValue)] public int? HotelId { get; set; }

    [Required] [Range(0, 5)] public int? Rating { get; set; }

    [Required] [MinLength(5)] public string? Comment { get; set; } = string.Empty;

    [Required] [DataType(DataType.Date)] public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    [Required] [DataType(DataType.Date)] public DateTime? LastUpdated { get; set; } = DateTime.UtcNow;
}