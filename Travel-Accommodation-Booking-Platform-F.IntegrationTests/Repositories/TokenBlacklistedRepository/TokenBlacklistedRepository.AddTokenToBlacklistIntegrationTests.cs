using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;
using Xunit;

public class AddTokenToBlacklistIntegrationTests : IntegrationTestBase
{
    private ITokenBlacklistedRepository _tokenBlacklistedRepository;
    
    public AddTokenToBlacklistIntegrationTests()
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        _tokenBlacklistedRepository = provider.GetRequiredService<ITokenBlacklistedRepository>();
    }
    
    [Fact]
    [Trait("IntegrationTests - TokenBlacklisted", "AddTokenToBlacklist")]
    public async Task Should_AddTokenToBlacklistSuccessfully_When_EnterValidInformation()
    {
        await ClearDatabaseAsync();
        
        var jti = "123456";
        var expiration = DateTime.Today.AddHours(1);
        var token = new BlacklistedToken
        {
            Jti = jti,
            Expiration = expiration
        };

        await _tokenBlacklistedRepository.AddTokenToBlacklistAsync(token);
        using (var scope = Factory.Services.CreateScope())
        {
            var provider = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var blacklistedTokens = provider.BlacklistedTokens.ToList();
            Assert.True(blacklistedTokens.Count == 1);
        }
    }
}