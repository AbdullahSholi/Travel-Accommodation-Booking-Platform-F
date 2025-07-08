using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AdminService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class UpdateUserTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAdminRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AdminService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly AdminService _sut;

    public UpdateUserTests()
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
    [Trait("UnitTests - Admin", "UpdateUser")]
    public async Task Should_ReturnNull_When_WeRequestInvalidUser()
    {
        // Arrange
        var userId = 1;
        var email = "abdullah.sholi@gmail.com";
        var userPatchDto = _fixture.Build<UserPatchDto>().With(x => x.Email, email).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null!);

        // Act
        var sut = await _sut.UpdateUserAsync(userId, userPatchDto);

        // Assert 
        Assert.Null(sut);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Admin", "UpdateUser")]
    public async Task Should_UpdateUserSuccessfully_When_WeEnterValidDetails()
    {
        // Arrange
        var email = "abdullah.sholi@gmail.com";
        var userId = 1;
        var userPatchDto = _fixture.Build<UserPatchDto>().With(x => x.Email, email).Create();
        var userReadDto = _fixture.Build<UserReadDto>().With(x => x.Email, email).Create();
        var user = _fixture.Build<User>().With(x => x.Email, email).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
        _mockRepo.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _mockCache.Setup(x => x.Remove(It.IsAny<string>()));
        _mockMapper.Setup(x => x.Map<UserReadDto>(It.IsAny<User>())).Returns(userReadDto);

        // Act
        var sut = await _sut.UpdateUserAsync(userId, userPatchDto);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(userReadDto.Email, sut.Email);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
        _mockMapper.Verify(x => x.Map<UserReadDto>(It.IsAny<User>()), Times.Once);
    }
}