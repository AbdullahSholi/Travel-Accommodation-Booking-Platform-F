using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AdminService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class DeleteUserTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAdminRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AdminService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly AdminService _sut;

    public DeleteUserTests()
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
    [Trait("UnitTests - Admin", "DeleteUser")]
    public async Task Should_FailedToDeleteUser_When_WeTryDeleteInvalidUser()
    {
        // Arrange
        var userId = 1;

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null!);

        // Act
        await _sut.DeleteUserAsync(userId);

        // Assert 
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.DeleteAsync(It.IsAny<User>()), Times.Never);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("UnitTests - Admin", "DeleteUser")]
    public async Task Should_DeleteUserSuccessfully_When_WeTryDeleteValidUser()
    {
        // Arrange
        var userId = 1;
        var email = "abdullah.sholi@gmail.com";
        var user = _fixture.Build<User>().With(x => x.Email, email).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(user);

        // Act
        await _sut.DeleteUserAsync(userId);

        // Assert 
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.DeleteAsync(It.IsAny<User>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
    }
}