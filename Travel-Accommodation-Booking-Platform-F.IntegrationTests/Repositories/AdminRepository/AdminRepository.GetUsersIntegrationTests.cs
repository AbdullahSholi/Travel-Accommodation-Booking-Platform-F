using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AdminService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class GetUsersIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IAdminService _adminService;
    private IMemoryCache _memoryCache;

    public GetUsersIntegrationTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        _adminService = provider.GetRequiredService<IAdminService>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Admin", "GetUsers")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var usersCacheKey = "users-list";

        await ClearDatabaseAsync();

        var user =
            _fixture.Build<User>()
                .Without(x => x.UserId)
                .Without(x => x.OtpRecords)
                .Without(x => x.Bookings)
                .Without(x => x.Reviews)
                .Create();
        await SeedUsersAsync(user);

        // Act
        var users = await _adminService.GetUsersAsync();

        // Assert
        Assert.NotNull(users);
        Assert.Single(users);

        var cacheHit = _memoryCache.TryGetValue(usersCacheKey, out List<UserReadDto> cachedUsers);

        Assert.True(cacheHit);
        Assert.Equal(users.Count, cachedUsers.Count);
    }

    [Fact]
    [Trait("IntegrationTests - Admin", "GetUsers")]
    public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache()
    {
        // Arrange
        var usersCacheKey = "users-list";
        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .Without(x => x.OtpRecords)
            .Without(x => x.Bookings)
            .Without(x => x.Reviews)
            .Create();

        _memoryCache.Remove(usersCacheKey);
        await ClearDatabaseAsync();
        await SeedUsersAsync(userMock);


        var cacheHit = _memoryCache.TryGetValue(usersCacheKey, out List<UserReadDto> cachedUsers);
        Assert.False(cacheHit);

        // Act
        var users = await _adminService.GetUsersAsync();

        // Assert
        Assert.NotNull(users);

        cacheHit = _memoryCache.TryGetValue(usersCacheKey, out List<UserReadDto> cachedUsers1);
        Assert.True(cacheHit);
        Assert.True(cachedUsers1.Count == 1);
    }
}