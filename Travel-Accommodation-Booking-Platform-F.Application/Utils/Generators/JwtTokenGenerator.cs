using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Utils;

namespace Travel_Accommodation_Booking_Platform_F.Application.Utils.Generators;

public class JwtTokenGenerator : ITokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    private readonly string _secretKey;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings, string secretKey)
    {
        _jwtSettings = jwtSettings.Value;
        _secretKey = secretKey;
    }

    public string GenerateToken(string username, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_jwtSettings.ExpiresInMinutes.ToString())),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}