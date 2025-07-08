using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AdminService;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.AdminExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Utils;
using Xunit;
using Xunit.Abstractions;

public class UpdateUserIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IAdminRepository _adminRepository;
    private IAdminService _adminService;
    private IMapper _mapper;
    private IMemoryCache _memoryCache;
    private readonly ITestOutputHelper _output;

    public UpdateUserIntegrationTests(ITestOutputHelper output)
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
    [Trait("IntegrationTests - Admin", "UpdateUser")]
    public async Task Should_UpdateUserSuccessfully_When_CorrectDataProvided()
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

        var userPatchDto = _fixture.Build<UserPatchDto>()
            .With(x => x.Email, "ali@gmail.com")
            .With(x => x.Username, username)
            .Create();

        // Act
        await _adminService.UpdateUserAsync(userId, userPatchDto);

        // Assert
        var updatedUser = await _adminRepository.GetByIdAsync(userId);

        Assert.NotNull(updatedUser);
        Assert.Equal(userPatchDto.Email, updatedUser.Email);

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