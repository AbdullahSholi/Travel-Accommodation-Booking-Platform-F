using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class TokenBlacklistIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private ITokenBlacklistedRepository _tokenBlacklistedRepository;
    
    public TokenBlacklistIntegrationTests()
    {
        _fixture = new Fixture();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        _tokenBlacklistedRepository = provider.GetRequiredService<ITokenBlacklistedRepository>();
    }

    [Fact]
    [Trait("IntegrationTests - TokenBlacklisted", "IsTokenBlacklisted")]
    public void Should_ThrowArgumentNullException_When_ArgumentIsNull()
    {
        var jti = ""; 
        Assert.Empty(jti);
    }
    
    [Fact]
    [Trait("IntegrationTests - TokenBlacklisted", "IsTokenBlacklisted")]
    public async Task Should_ReturnFalse_When_TokenIsNotBlacklisted()
    {
        await ClearDatabaseAsync();
        
        var jti = _fixture.Create<string>();
        var isBlacklisted = await _tokenBlacklistedRepository.CheckIfTokenBlacklistedAsync(jti);
        Assert.False(isBlacklisted);
    }

    [Fact]
    [Trait("IntegrationTests - TokenBlacklisted", "IsTokenBlacklisted")]
    public async Task Should_ReturnTrue_When_TokenIsBlacklisted()
    {
        await ClearDatabaseAsync();
        
        var jti = "123456";
        var blacklistedTokenMock = _fixture.Build<BlacklistedToken>()
            .Without(x => x.Id)
            .With(x => x.Jti, jti)
            .With(x => x.Expiration, DateTime.UtcNow.AddMinutes(+10))
            .Create();

        await SeedBlacklistedTokensAsync(blacklistedTokenMock);
        
        var isBlacklisted = await _tokenBlacklistedRepository.CheckIfTokenBlacklistedAsync(jti);
        Assert.True(isBlacklisted);
    }
}