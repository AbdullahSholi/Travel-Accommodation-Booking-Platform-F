using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class UserPatchDto
{
    public string? Username { get; set; }

    [StringLength(50)]
    [RegularExpression(@"[A-Z][a-zA-Z\s]*$")]
    public string? FirstName { get; set; }

    [StringLength(50)]
    [RegularExpression(@"[A-Z][a-zA-Z\s]*$")]
    public string? LastName { get; set; }

    [EmailAddress] public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }

    [MinLength(8)]
    [MaxLength(30)]
    [RegularExpression(@"^\+?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    [DataType(DataType.Date)] public DateTime? DateOfBirth { get; set; }
    [StringLength(50)] public string? Address1 { get; set; }
    [StringLength(50)] public string? Address2 { get; set; }
    [StringLength(100)] public string? City { get; set; }
    [StringLength(100)] public string? Country { get; set; }
    [StringLength(100)] public string? DriverLicense { get; set; }
    public string? Role { get; set; }
}