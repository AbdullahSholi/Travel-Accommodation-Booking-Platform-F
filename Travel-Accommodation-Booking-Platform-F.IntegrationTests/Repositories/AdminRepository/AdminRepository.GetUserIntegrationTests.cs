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

public class GetUserIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IAdminRepository _adminRepository;
    private IAdminService _adminService;
    private IMemoryCache _memoryCache;

    public GetUserIntegrationTests()
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

        _adminRepository = provider.GetRequiredService<IAdminRepository>();
        _adminService = provider.GetRequiredService<IAdminService>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Admin", "GetUser")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .Without(x => x.OtpRecords)
            .Without(x => x.Bookings)
            .Without(x => x.Reviews)
            .Create();

        await ClearDatabaseAsync();
        await SeedUsersAsync(userMock);

        var existingUser = (await _adminRepository.GetAllAsync()).FirstOrDefault(u => u.Email == userMock.Email);
        Assert.NotNull(existingUser);

        var userId = existingUser.UserId;

        // Act
        var user = await _adminService.GetUserAsync(userId);

        // Assert
        Assert.NotNull(user);

        var userCacheKey = GetUserCacheKey(userId);

        var cacheHit = _memoryCache.TryGetValue(userCacheKey, out UserReadDto cachedUser);
        Assert.True(cacheHit);
        Assert.NotNull(cachedUser);
        Assert.Equal(user.UserId, cachedUser.UserId);
    }

    [Fact]
    [Trait("IntegrationTests - Admin", "GetUser")]
    public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache()
    {
        // Arrange
        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .Without(x => x.OtpRecords)
            .Without(x => x.Bookings)
            .Without(x => x.Reviews)
            .Create();

        await ClearDatabaseAsync();
        await SeedUsersAsync(userMock);

        var existingUser = (await _adminRepository.GetAllAsync()).First();
        var userId = existingUser.UserId;
        var userCacheKey = GetUserCacheKey(userId);

        var cacheHit = _memoryCache.TryGetValue(userCacheKey, out UserReadDto cachedUser);
        Assert.False(cacheHit);

        // Act
        var user = await _adminService.GetUserAsync(userId);

        // Assert
        Assert.NotNull(user);

        cacheHit = _memoryCache.TryGetValue(userCacheKey, out UserReadDto cachedUser1);
        Assert.True(cacheHit);
        Assert.NotNull(cachedUser1);
    }

    private string GetUserCacheKey(int userId)
    {
        return $"user_{userId}";
    }
}