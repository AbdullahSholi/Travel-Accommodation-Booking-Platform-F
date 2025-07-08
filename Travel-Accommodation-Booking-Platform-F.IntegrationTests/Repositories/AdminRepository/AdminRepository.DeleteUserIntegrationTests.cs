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

public class DeleteUserIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IAdminRepository _adminRepository;
    private IAdminService _adminService;
    private IMapper _mapper;
    private IMemoryCache _memoryCache;
    private readonly ITestOutputHelper _output;

    public DeleteUserIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
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
        _mapper = provider.GetRequiredService<IMapper>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Admin", "DeleteUser")]
    public async Task Should_DeleteUserSuccessfully_When_CorrectDataProvided()
    {
        // Arrange
        await ClearDatabaseAsync();
        var email = "abdullah.sholi@gmail.com";
        var username = "abdullahsholi";
        var cacheKey = "users-list";

        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .Without(x => x.OtpRecords)
            .Without(x => x.Bookings)
            .Without(x => x.Reviews)
            .With(x => x.Email, email)
            .With(x => x.Username, username)
            .With(x => x.IsEmailConfirmed, true)
            .Create();

        await SeedUsersAsync(userMock);

        var existingUser = (await _adminRepository.GetAllAsync()).First();
        var userId = existingUser.UserId;

        var user = await _adminRepository.GetByIdAsync(userId);
        Assert.NotNull(user);

        // Act
        await _adminService.DeleteUserAsync(userId);

        // Assert
        var users = await _adminRepository.GetAllAsync();
        Assert.Empty(users);

        var cacheHit1 = _memoryCache.TryGetValue(cacheKey, out List<UserReadDto> cachedUsers);
        var cacheHit2 = _memoryCache.TryGetValue(GetUserCacheKey(userId), out UserReadDto cachedUser);

        Assert.False(cacheHit1);
        Assert.False(cacheHit2);
    }

    private string GetUserCacheKey(int userId)
    {
        return $"user_{userId}";
    }
}