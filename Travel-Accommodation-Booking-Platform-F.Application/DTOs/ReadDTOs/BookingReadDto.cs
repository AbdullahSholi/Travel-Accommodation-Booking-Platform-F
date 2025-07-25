namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;

public class BookingReadDto
{
    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int RoomId { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public decimal TotalPrice { get; set; }

    public DateTime LastUpdated { get; set; }
}