namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class HotelPatchDto
{
    public string? HotelName { get; set; }
    public string? OwnerName { get; set; }
    public string? Location { get; set; }

    public int? CityId { get; set; }
}