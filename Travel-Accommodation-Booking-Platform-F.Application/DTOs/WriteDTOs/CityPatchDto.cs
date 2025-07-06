namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class CityPatchDto
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? PostOffice { get; set; }
    public int? NumberOfHotels { get; set; }
    public DateTime? UpdatedAt { get; set; }
}