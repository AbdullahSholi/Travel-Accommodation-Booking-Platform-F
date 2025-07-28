using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.Hashing;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.FactoryPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Utils;
using Xunit;

public class LoginTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAuthRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IOtpSenderFactory> _mockOtpSenderFactory;
    private readonly Mock<ITokenGenerator> _mockJwtTokenGenerator;
    private readonly Mock<ILogger<AuthService>> _mockLogger;

    private readonly AuthService _sut;

    public LoginTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _mockRepo = _fixture.Freeze<Mock<IAuthRepository>>();
        _mockJwtTokenGenerator = _fixture.Freeze<Mock<ITokenGenerator>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<AuthService>>>();
        _mockOtpSenderFactory = _fixture.Freeze<Mock<IOtpSenderFactory>>();

        _sut = new AuthService(
            _mockRepo.Object,
            _mockJwtTokenGenerator.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockOtpSenderFactory.Object
        );
    }

    [Fact]
    [Trait("UnitTests - Auth", "Login")]
    public async Task Should_ThrowValidationError_When_EmailAndUsernameAreMissing()
    {
        var dto = new Fixture().Build<LoginWriteDto>()
            .With(x => x.Email, "")
            .With(x => x.Username, "")
            .Create();

        var ex = await Assert.ThrowsAsync<ValidationAppException>(() => _sut.LoginAsync(dto));
        Assert.Equal(AuthServiceCustomMessages.EmailOrUsernameNotFound, ex.Message);
    }

    [Fact]
    [Trait("UnitTests - Auth", "Login")]
    public async Task Should_ThrowNotFoundException_When_UserNotExists()
    {
        var dto = _fixture.Create<LoginWriteDto>();
        _mockRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null!);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.LoginAsync(dto));
        Assert.Equal(AuthServiceCustomMessages.UserNotFound, ex.Message);
        _mockRepo.Verify(r => r.GetUserByEmailAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Auth", "Login")]
    public async Task Should_ThrowValidationError_When_PasswordIsIncorrect()
    {
        var dto = _fixture.Create<LoginWriteDto>();
        var user = _fixture.Create<User>();
        _mockRepo.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<ValidationAppException>(() => _sut.LoginAsync(dto));
        Assert.Equal(AuthServiceCustomMessages.InvalidPassword, ex.Message);
    }

    [Fact]
    [Trait("UnitTests - Auth", "Login")]
    public async Task Should_ReturnToken_When_CredentialsAreValid()
    {
        var dto = _fixture.Create<LoginWriteDto>();
        var user = _fixture.Build<User>()
            .With(x => x.Password, PasswordHasher.HashPassword(dto.Password))
            .Create();

        _mockRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _mockJwtTokenGenerator.Setup(t => t.GenerateToken(user.Email, user.Role))
            .Returns("valid-token");

        var result = await _sut.LoginAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("valid-token", result.Token);
        _mockRepo.Verify(r => r.GetUserByEmailAsync(It.IsAny<string>()), Times.Once);
        _mockJwtTokenGenerator.Verify(r => r.GenerateToken(user.Email, user.Role), Times.Once);
    }
}