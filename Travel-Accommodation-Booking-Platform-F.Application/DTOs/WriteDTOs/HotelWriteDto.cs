using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class HotelWriteDto
{
    [Required]
    [MaxLength(100)]
    [MinLength(3)]
    public string HotelName { get; set; }

    [Required]
    [MaxLength(50)]
    [MinLength(3)]
    public string OwnerName { get; set; }

    [Required]
    [Range(0, 5, ErrorMessage = "Star rating must be between 0 and 5")]
    public double StarRating { get; set; }

    [Required] public string Location { get; set; }
    [Required] public string Description { get; set; }
    [Required] public int CityId { get; set; }
}