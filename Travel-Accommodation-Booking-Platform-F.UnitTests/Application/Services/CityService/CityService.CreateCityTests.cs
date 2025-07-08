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
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.AdminExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class CreateUserTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAdminRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AdminService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly AdminService _sut;

    public CreateUserTests()
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
    [Trait("UnitTests - Admin", "CreateUser")]
    public async Task Should_InvalidUserDataReceivedException_When_EmailOrUsernameAreMissing()
    {
        var dto = new Fixture().Build<UserWriteDto>()
            .With(x => x.Email, "")
            .With(x => x.Username, "")
            .Create();

        var ex = await Assert.ThrowsAsync<InvalidUserDataReceivedException>(() => _sut.CreateUserAsync(dto));
        Assert.Equal(AdminServiceCustomMessages.InvalidUserDataReceived, ex.Message);
    }

    [Fact]
    [Trait("UnitTests - Admin", "CreateUser")]
    public async Task Should_DuplicatedEmailException_When_TryAddingExistsUser()
    {
        var dto = new Fixture().Build<UserWriteDto>()
            .Create();

        _mockRepo.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<DuplicatedEmailException>(() => _sut.CreateUserAsync(dto));
        Assert.Equal(AdminServiceCustomMessages.DuplicatedEmails, ex.Message);
    }

    [Fact]
    [Trait("UnitTests - Admin", "CreateUser")]
    public async Task Should_AddedUserSuccessfully_When_ValidDataProvided()
    {
        // Arrange
        var writeDto = _fixture.Build<UserWriteDto>()
            .With(x => x.Email, "abdullah.sholi@gmail.com")
            .With(x => x.Username, "abdullah")
            .With(x => x.Role, "User")
            .Create();

        var user = _fixture.Build<User>()
            .With(x => x.Email, writeDto.Email)
            .With(x => x.Username, writeDto.Username)
            .With(x => x.Role, writeDto.Role)
            .Create();

        var readDto = _fixture.Build<UserReadDto>()
            .With(x => x.Email, writeDto.Email)
            .With(x => x.Username, writeDto.Username)
            .With(x => x.Role, writeDto.Role)
            .Create();

        _mockRepo.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _mockCache.Setup(x => x.Remove(It.IsAny<string>()));
        _mockMapper.Setup(x => x.Map<User>(It.IsAny<UserWriteDto>())).Returns(user);
        _mockMapper.Setup(x => x.Map<UserReadDto>(user)).Returns(readDto);

        // Act
        var sut = await _sut.CreateUserAsync(writeDto);

        // Assert
        Assert.NotNull(sut);
        Assert.Equal(writeDto.Email, sut.Email);
        Assert.Equal(writeDto.Username, sut.Username);
        Assert.Equal(writeDto.Role, sut.Role);

        _mockRepo.Verify(x => x.EmailExistsAsync(It.IsAny<string>()), Times.Once);
        _mockRepo.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
        _mockMapper.Verify(x => x.Map<User>(It.IsAny<UserWriteDto>()), Times.Once);
        _mockMapper.Verify(x => x.Map<UserReadDto>(It.IsAny<User>()), Times.Once);
    }
}