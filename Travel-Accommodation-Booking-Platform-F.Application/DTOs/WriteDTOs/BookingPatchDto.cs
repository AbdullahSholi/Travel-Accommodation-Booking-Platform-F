using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class BookingPatchDto
{
    [Range(0, int.MaxValue)] public int? RoomId { get; set; }
    [DataType(DataType.Date)] public DateTime? CheckInDate { get; set; }
    [DataType(DataType.Date)] public DateTime? CheckOutDate { get; set; }
    [Range(1, double.MaxValue)] public decimal? TotalPrice { get; set; }
}