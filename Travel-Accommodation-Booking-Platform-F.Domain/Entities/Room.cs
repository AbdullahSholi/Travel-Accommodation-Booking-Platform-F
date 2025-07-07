using Travel_Accommodation_Booking_Platform_F.Domain.Enums;

namespace Travel_Accommodation_Booking_Platform_F.Domain.Entities;

public class Room
{
    public int RoomId { get; set; }
    public RoomType RoomType { get; set; }
    public List<string> Images { get; set; }
    public string Description { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsAvailable { get; set; }
    public int AdultCapacity { get; set; }
    public int ChildrenCapacity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Booking>? Bookings { get; set; }
    public int HotelId { get; set; }
    public Hotel? Hotel { get; set; }

    public DateTime LastUpdated { get; set; }
}