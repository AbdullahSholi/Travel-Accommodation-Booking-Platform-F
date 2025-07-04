using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Utils;
using Xunit;

public class LoginIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IAuthRepository _authRepository;
    private ITokenGenerator _jwtTokenGenerator;

    private readonly LoginWriteDto _loginWriteDto = new()
    {
        Email = "abdullah@gmail.com",
        Password = "Sholi@971",
        Username = "abdullahsholi"
    };

    public LoginIntegrationTests()
    {
        _fixture = new Fixture();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        _authRepository = provider.GetRequiredService<IAuthRepository>();
        _jwtTokenGenerator = provider.GetRequiredService<ITokenGenerator>();
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "Login")]
    public async Task Should_LoginSuccessfully_When_UserEnterCorrectCredentials()
    {
        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .With(x => x.Email, _loginWriteDto.Email)
            .With(x => x.Password, _loginWriteDto.Password)
            .With(x => x.Username, _loginWriteDto.Username)
            .Create();

        await SeedUsersAsync(userMock);
        var userEmail = userMock.Email;

        var user = await _authRepository.GetUserByEmailAsync(userEmail);
        var token = _jwtTokenGenerator.GenerateToken(userMock.Email, userMock.Role);

        Assert.NotNull(user);
        Assert.Equal(userEmail, user.Email);
        Assert.NotNull(user);
        Assert.Equal(userMock.Username, user.Username);
        Assert.NotNull(user);
        Assert.Equal(userMock.Role, user.Role);
        Assert.False(string.IsNullOrEmpty(token));

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                         ?? jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        Assert.Equal(userMock.Email, emailClaim);
        Assert.Equal(userMock.Role, roleClaim);
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "Login")]
    public async Task Should_LoginFailed_When_UserNotFound()
    {
        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .With(x => x.Email, "ahmad@gmail.com")
            .With(x => x.Password, _loginWriteDto.Password)
            .With(x => x.Username, _loginWriteDto.Username)
            .Create();

        await SeedUsersAsync(userMock);

        var user = await _authRepository.GetUserByEmailAsync(_loginWriteDto.Email);

        Assert.Null(user);
    }
}