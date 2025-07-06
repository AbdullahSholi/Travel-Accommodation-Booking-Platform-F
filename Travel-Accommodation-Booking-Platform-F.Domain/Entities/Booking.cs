namespace Travel_Accommodation_Booking_Platform_F.Domain.Entities;

public class Booking
{
    public int BookingId { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public decimal TotalPrice { get; set; }
}