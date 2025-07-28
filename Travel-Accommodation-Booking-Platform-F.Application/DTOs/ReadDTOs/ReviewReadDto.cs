namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;

public class ReviewReadDto
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }
    public int HotelId { get; set; }

    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}