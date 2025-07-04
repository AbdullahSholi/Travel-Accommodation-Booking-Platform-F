using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.FactoryPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.StrategyPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Utils;
using Xunit;

public class RegisterTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAuthRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IOtpSenderFactory> _mockOtpSenderFactory;
    private readonly Mock<IOtpSenderStrategy> _mockOtpSenderStrategy;
    private readonly Mock<ITokenGenerator> _mockJwtTokenGenerator;
    private readonly Mock<ILogger<AuthService>> _mockLogger;

    private readonly AuthService _sut;

    public RegisterTests()
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
        _mockOtpSenderStrategy = _fixture.Freeze<Mock<IOtpSenderStrategy>>();

        _sut = new AuthService(
            _mockRepo.Object,
            _mockJwtTokenGenerator.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockOtpSenderFactory.Object
        );
    }

    [Fact]
    [Trait("UnitTests - Auth", "Register")]
    public async Task Should_ThrowException_When_UserAlreadyExists()
    {
        var dto = _fixture.Create<UserWriteDto>();
        _mockRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<Exception>(() => _sut.RegisterAsync(dto));
        Assert.Equal(CustomMessages.DuplicatedEmail, ex.Message);
    }

    [Fact]
    [Trait("UnitTests - Auth", "Register")]
    public async Task Should_RegisterUserSuccessfully_When_ValidDataProvided()
    {
        var dto = _fixture.Build<UserWriteDto>()
            .With(x => x.Email, "abdullah.sholi@gmail.com")
            .With(x => x.Username, "abdullahsholi")
            .With(x => x.FirstName, "Abdullah")
            .With(x => x.LastName, "Sholi")
            .With(x => x.Role, "User")
            .Create();

        var userEntity = _fixture.Build<User>()
            .With(x => x.Email, dto.Email)
            .With(x => x.Username, dto.Username)
            .With(x => x.FirstName, dto.FirstName)
            .With(x => x.LastName, dto.LastName)
            .With(x => x.Role, dto.Role)
            .Create();

        _mockRepo.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.RegisterUserAsync(It.IsAny<User>())).ReturnsAsync(userEntity);
        _mockRepo.Setup(r => r.SaveOtpAsync(It.IsAny<OtpRecord>())).Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<User>(It.IsAny<UserWriteDto>())).Returns(userEntity);
        _mockMapper.Setup(m => m.Map<UserReadDto>(It.IsAny<User>())).Returns((User u) => new UserReadDto
        {
            Username = u.Username,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            Role = u.Role
        });

        _mockOtpSenderFactory.Setup(r => r.Factory(OtpChannel.Email)).Returns(_mockOtpSenderStrategy.Object);
        _mockOtpSenderStrategy.Setup(r => r.SendOtpAsync(dto.Email, It.IsAny<string>())).Returns(Task.CompletedTask);

        var result = await _sut.RegisterAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.Username, result.Username);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(dto.FirstName, result.FirstName);
        Assert.Equal(dto.LastName, result.LastName);
        Assert.Equal(dto.Role, result.Role);

        _mockRepo.Verify(r => r.SaveOtpAsync(It.IsAny<OtpRecord>()), Times.Once);
        _mockOtpSenderFactory.Verify(r => r.Factory(OtpChannel.Email), Times.Once);
        _mockOtpSenderStrategy.Verify(r => r.SendOtpAsync(dto.Email, It.IsAny<string>()), Times.Once);
    }
}