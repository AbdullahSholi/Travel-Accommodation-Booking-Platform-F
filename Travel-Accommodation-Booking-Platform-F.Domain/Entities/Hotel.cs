namespace Travel_Accommodation_Booking_Platform_F.Domain.Entities;

public class Hotel
{
    public int HotelId { get; set; }
    public string HotelName { get; set; }
    public string OwnerName { get; set; }
    public double StarRating { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }

    public int CityId { get; set; }
    public City? City { get; set; }
    public List<Room>? Rooms { get; set; }
    public List<Review>? Reviews { get; set; }

    public DateTime LastUpdated { get; set; }
}