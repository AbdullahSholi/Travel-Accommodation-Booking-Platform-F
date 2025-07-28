using Travel_Accommodation_Booking_Platform_F.Domain.Enums;

namespace Travel_Accommodation_Booking_Platform_F.Domain.QueryDTOs;

public class RoomQueryDto
{
    public RoomType? RoomType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public bool? IsAvailable { get; set; }
    public int? AdultCapacity { get; set; }
    public int? ChildrenCapacity { get; set; }
    public DateTime? CreatedAt { get; set; }
}