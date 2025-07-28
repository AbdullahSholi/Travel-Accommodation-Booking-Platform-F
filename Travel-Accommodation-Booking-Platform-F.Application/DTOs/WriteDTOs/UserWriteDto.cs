using System.ComponentModel.DataAnnotations;

namespace Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

public class UserWriteDto
{
    [Required] public string Username { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"[A-Z][a-zA-Z\s]*$")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [RegularExpression(@"[A-Z][a-zA-Z\s]*$")]
    public string LastName { get; set; } = string.Empty;

    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;

    [Required] [MinLength(8)] public string Password { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [Compare("Password", ErrorMessage = "Password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(30)]
    [RegularExpression(@"^\+?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Invalid phone number format.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required] [DataType(DataType.Date)] public DateTime DateOfBirth { get; set; }

    [Required] [StringLength(50)] public string Address1 { get; set; } = string.Empty;

    [StringLength(50)] public string Address2 { get; set; } = string.Empty;

    [Required] [StringLength(100)] public string City { get; set; } = string.Empty;

    [Required] [StringLength(100)] public string Country { get; set; } = string.Empty;

    [Required] [StringLength(100)] public string DriverLicense { get; set; } = string.Empty;

    [Required] [StringLength(30)] public string Role { get; set; } = "User";
}