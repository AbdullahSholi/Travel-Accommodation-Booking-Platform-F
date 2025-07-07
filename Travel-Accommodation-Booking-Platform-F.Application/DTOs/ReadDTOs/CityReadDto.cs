namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;

public class CityReadDto
{
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostOffice { get; set; } = string.Empty;
    public int NumberOfHotels { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; }
}