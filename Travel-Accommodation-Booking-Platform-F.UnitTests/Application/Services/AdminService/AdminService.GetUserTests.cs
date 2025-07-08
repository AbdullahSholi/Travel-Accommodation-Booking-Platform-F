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
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class GetUserTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAdminRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AdminService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly AdminService _sut;

    public GetUserTests()
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
    [Trait("UnitTests - Admin", "GetUser")]
    public async Task Should_ReturnedDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var email = "abdullah.sholi@gmail.com";
        var cachedUser = _fixture.Build<UserReadDto>()
            .With(x => x.UserId, 1)
            .With(x => x.Email, email)
            .Create();

        object cachedObject = cachedUser;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(true);

        // Act
        var sut = await _sut.GetUserAsync(cachedUser.UserId);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(email, sut.Email);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cachedObject), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Admin", "GetUser")]
    public async Task Should_ReturnNull_When_WeTryRetrieveInvalidUser()
    {
        // Arrange
        var userId = 1;
        var email = "abdullah.sholi@gmail.com";
        var cachedUser = _fixture.Build<UserReadDto>().With(x => x.Email, email).Create();

        object cachedObject = cachedUser;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null!);

        // Act
        var sut = await _sut.GetUserAsync(userId);

        // Assert
        Assert.Null(sut);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Admin", "GetUser")]
    public async Task Should_ReturnedDataFromDatabase_When_ThereIsNoCachedData()
    {
        // Arrange
        var userId = 1;
        var email = "abdullah.sholi@gmail.com";
        var cachedUser = _fixture.Build<UserReadDto>()
            .With(x => x.Email, email)
            .Create();

        var user = _fixture.Build<User>()
            .With(x => x.Email, email)
            .Create();

        object cachedObject = cachedUser;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
        _mockMapper.Setup(x => x.Map<UserReadDto>(It.IsAny<User>())).Returns(cachedUser);

        // Act
        var sut = await _sut.GetUserAsync(userId);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(user.Email, sut.Email);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
    }
}