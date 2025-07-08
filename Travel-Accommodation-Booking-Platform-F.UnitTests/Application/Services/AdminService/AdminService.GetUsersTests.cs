using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AdminService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.AdminExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class GetUsersTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAdminRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AdminService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly AdminService _sut;

    public GetUsersTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _mockRepo = _fixture.Freeze<Mock<IAdminRepository>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<AdminService>>>();
        _mockCache = _fixture.Freeze<Mock<IMemoryCache>>();

        _sut = new AdminService(
            _mockRepo.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCache.Object
        );
    }

    [Fact]
    [Trait("UnitTests - Admin", "GetUsers")]
    public async Task Should_ReturnedDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var email = "abdullah.sholi@gmail.com";
        var cachedUsers = new List<UserReadDto>
        {
            _fixture.Build<UserReadDto>().With(x => x.Email, email).Create(),
            _fixture.Create<UserReadDto>(),
            _fixture.Create<UserReadDto>()
        };

        object cachedObject = cachedUsers;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(true);

        // Act
        var sut = await _sut.GetUsersAsync();

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(email, sut[0].Email);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cachedObject), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Admin", "GetUsers")]
    public async Task Should_FailedToFetchUsersException_When_ThereIsNoUsersListCommingFromDatabase()
    {
        // Arrange
        var email = "abdullah.sholi@gmail.com";
        var cachedUsers = new List<UserReadDto>
        {
            _fixture.Build<UserReadDto>().With(x => x.Email, email).Create(),
            _fixture.Create<UserReadDto>(),
            _fixture.Create<UserReadDto>()
        };

        object cachedObject = cachedUsers;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync((List<User>)null!);

        // Act & Assert 
        var exception = await Assert.ThrowsAsync<FailedToFetchUsersException>(() => _sut.GetUsersAsync());
        Assert.Equal(AdminServiceCustomMessages.FailedFetchingUsersFromRepository, exception.Message);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Admin", "GetUsers")]
    public async Task Should_ReturnedDataFromDatabase_When_ThereIsNoCachedData()
    {
        // Arrange
        var email = "abdullah.sholi@gmail.com";
        var cachedUsers = new List<UserReadDto>
        {
            _fixture.Build<UserReadDto>().With(x => x.Email, email).Create(),
            _fixture.Create<UserReadDto>(),
            _fixture.Create<UserReadDto>()
        };

        var users = new List<User>
        {
            _fixture.Build<User>().With(x => x.Email, email).Create(),
            _fixture.Create<User>(),
            _fixture.Create<User>()
        };

        object cachedObject = cachedUsers;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(users);
        _mockMapper.Setup(x => x.Map<List<UserReadDto>>(It.IsAny<List<User>>())).Returns(cachedUsers);

        // Act
        var sut = await _sut.GetUsersAsync();

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(users[0].Email, sut[0].Email);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
        _mockRepo.Verify(x => x.GetAllAsync(), Times.Once);
    }
}