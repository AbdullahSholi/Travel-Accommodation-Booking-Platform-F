using System.ComponentModel.DataAnnotations;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class RoomPatchDto
{
    public RoomType? RoomType { get; set; }
    public List<string>? Images { get; set; } = new();
    [MaxLength(10)] public string? Description { get; set; } = string.Empty;
    [Range(1, double.MaxValue)] public decimal? PricePerNight { get; set; }
    public bool? IsAvailable { get; set; } = true;
    [Range(0, int.MaxValue)] public int? AdultCapacity { get; set; } = 2;
    [Range(0, int.MaxValue)] public int? ChildrenCapacity { get; set; } = 0;
    [DataType(DataType.Date)] public DateTime? UpdatedAt { get; set; }
}