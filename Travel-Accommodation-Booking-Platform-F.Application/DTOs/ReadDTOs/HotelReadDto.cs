namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;

public class HotelReadDto
{
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public double StarRating { get; set; } = 3;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int CityId { get; set; }
}