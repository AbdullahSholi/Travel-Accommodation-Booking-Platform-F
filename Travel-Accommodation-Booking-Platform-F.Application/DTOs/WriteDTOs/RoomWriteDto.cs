using System.ComponentModel.DataAnnotations;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class RoomWriteDto
{
    [Required] public RoomType RoomType { get; set; }
    [Required] public List<string> Images { get; set; } = new();
    [Required] [MaxLength(10)] public string Description { get; set; } = string.Empty;
    [Range(1, double.MaxValue)] public decimal PricePerNight { get; set; }
    [Required] public bool IsAvailable { get; set; } = true;
    [Required] [Range(0, int.MaxValue)] public int AdultCapacity { get; set; } = 2;
    [Required] [Range(0, int.MaxValue)] public int ChildrenCapacity { get; set; } = 0;
    [Required] public DateTime CreatedAt { get; set; }
    [Required] public DateTime UpdatedAt { get; set; }
    [Required] public int HotelId { get; set; }
}