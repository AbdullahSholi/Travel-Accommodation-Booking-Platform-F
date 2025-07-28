using Travel_Accommodation_Booking_Platform_F.Domain.Enums;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;

public class RoomReadDto
{
    public int RoomId { get; set; }
    public RoomType RoomType { get; set; }
    public List<string> Images { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int AdultCapacity { get; set; } = 2;
    public int ChildrenCapacity { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}