using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AdminService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.AdminExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class CreateUserIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IAdminRepository _adminRepository;
    private IAdminService _adminService;
    private IMapper _mapper;
    private IMemoryCache _memoryCache;
    private readonly ITestOutputHelper _output;

    public CreateUserIntegrationTests(ITestOutputHelper output)
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
    [Trait("IntegrationTests - Admin", "CreateUser")]
    public async Task Should_ThrowDuplicatedEmailException_When_UserTryAddExistingUser()
    {
        // Arrange
        await ClearDatabaseAsync();
        var email = "abdullah.sholi@gmail.com";
        var username = "abdullahsholi";
        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .Without(x => x.OtpRecords)
            .Without(x => x.Bookings)
            .Without(x => x.Reviews)
            .With(x => x.Email, email)
            .With(x => x.Username, username)
            .With(x => x.IsEmailConfirmed, true)
            .Create();

        var userWriteDto = _fixture.Build<UserWriteDto>()
            .With(x => x.Email, email)
            .With(x => x.Username, username)
            .Create();

        await SeedUsersAsync(userMock);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<DuplicatedEmailException>(() => _adminService.CreateUserAsync(userWriteDto));
        Assert.Equal(AdminServiceCustomMessages.DuplicatedEmails, exception.Message);
    }

    [Fact]
    [Trait("IntegrationTests - Admin", "CreateUser")]
    public async Task Should_AddNewUserCorrectly_When_CorrectCredentialsAreProvided()
    {
        // Arrange
        await ClearDatabaseAsync();
        var email = "abdullah.sholi@gmail.com";
        var email1 = "abdullah.sholi1@gmail.com";
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

        var isExist = await _adminRepository.EmailExistsAsync(email1);
        Assert.False(isExist);

        var userWriteDto = _mapper.Map<UserWriteDto>(userMock);

        // Act
        await _adminService.CreateUserAsync(userWriteDto);

        // Assert
        var user = (await _adminRepository.GetAllAsync()).First();
        var userId = user.UserId;

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