using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class BookingWriteDto
{
    [Required] [Range(1, int.MaxValue)] public int UserId { get; set; }
    [Required] [Range(1, int.MaxValue)] public int RoomId { get; set; }
    
    public DateTime CheckOutDate { get; set; } = DateTime.Now + TimeSpan.FromDays(1);

    public DateTime CreatedAt { get; set; }
    public decimal TotalPrice { get; set; }

    public DateTime LastUpdated { get; set; }
}