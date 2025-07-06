namespace Travel_Accommodation_Booking_Platform_F.Domain.Entities;

public class City
{
    public int CityId { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string PostOffice { get; set; }
    public int NumberOfHotels { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Hotel>? Hotels { get; set; }
}