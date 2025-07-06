using Travel_Accommodation_Booking_Platform_F.Domain.Enums;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class RoomPatchDto
{
    public RoomType? RoomType { get; set; }
    public List<string>? Images { get; set; } = new();
    public string? Description { get; set; } = string.Empty;
    public decimal? PricePerNight { get; set; }
    public bool? IsAvailable { get; set; } = true;
    public int? AdultCapacity { get; set; } = 2;
    public int? ChildrenCapacity { get; set; } = 0;
    public DateTime? UpdatedAt { get; set; }
}