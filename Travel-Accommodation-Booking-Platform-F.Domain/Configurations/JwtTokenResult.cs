namespace Travel_Accommodation_Booking_Platform_F.Domain.Configurations;

public class JwtTokenResult
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime AccessTokenExpiration { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    public string AccessTokenJti { get; set; }
    public string RefreshTokenJti { get; set; }
}