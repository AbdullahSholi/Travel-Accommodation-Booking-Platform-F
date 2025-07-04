using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.FactoryPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.StrategyPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Utils;
using Xunit;

public class SendOtpTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAuthRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IOtpSenderFactory> _mockOtpSenderFactory;
    private readonly Mock<ITokenGenerator> _mockJwtTokenGenerator;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly Mock<IOtpSenderStrategy> _mockOtpSenderStrategy;

    private readonly AuthService _sut;

    public SendOtpTests()
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
    [Trait("UnitTests - Auth", "SendOtp")]
    public async Task Should_ThrowException_When_UserDoesNotExist()
    {
        // Arrange
        var emailMock = _fixture.Create<string>();
        _mockRepo.Setup(r => r.GetUserByEmailAsync(emailMock))
            .ReturnsAsync((User)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.SendOtpAsync(emailMock));
        Assert.Equal(CustomMessages.UserNotFound, exception.Message);
    }

    [Fact]
    [Trait("UnitTests - Auth", "SendOtp")]
    public async Task Should_ThrowValidationAppException_When_InsertInvalidOtpSendingStrategy()
    {
        // Arrange
        var emailMock = "abdullah@gmail.com";
        var userMock = _fixture.Build<User>()
            .With(x => x.Email, emailMock)
            .Create();

        _mockRepo.Setup(r => r.GetUserByEmailAsync(emailMock))
            .ReturnsAsync(userMock);

        _mockRepo.Setup(r => r.SaveOtpAsync(It.IsAny<OtpRecord>())).Returns(Task.CompletedTask);

        _mockOtpSenderFactory.Setup(r => r.Factory(OtpChannel.Email)).Returns((IOtpSenderStrategy)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationAppException>(() => _sut.SendOtpAsync(emailMock));
        Assert.Equal(CustomMessages.InvalidStrategy, exception.Message);
        _mockRepo.Verify(r => r.GetUserByEmailAsync(emailMock), Times.Once);
        _mockOtpSenderFactory.Verify(r => r.Factory(OtpChannel.Email), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Auth", "SendOtp")]
    public async Task Should_SendOtpSuccessfully_When_InsertCorrectDetails()
    {
        // Arrange
        var emailMock = "abdullah@gmail.com";
        var userMock = _fixture.Build<User>()
            .With(x => x.Email, emailMock)
            .Create();

        _mockRepo.Setup(r => r.GetUserByEmailAsync(emailMock))
            .ReturnsAsync(userMock);

        _mockRepo.Setup(r => r.SaveOtpAsync(It.IsAny<OtpRecord>())).Returns(Task.CompletedTask);

        _mockOtpSenderFactory.Setup(r => r.Factory(OtpChannel.Email)).Returns(_mockOtpSenderStrategy.Object);

        _mockOtpSenderStrategy.Setup(r => r.SendOtpAsync(emailMock, It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        var sut = await _sut.SendOtpAsync(emailMock);

        // Assert
        Assert.True(sut);
        _mockRepo.Verify(r => r.GetUserByEmailAsync(emailMock), Times.Once);
        _mockRepo.Verify(r => r.SaveOtpAsync(It.IsAny<OtpRecord>()), Times.Once);
        _mockOtpSenderFactory.Verify(r => r.Factory(OtpChannel.Email), Times.Once);
        _mockOtpSenderStrategy.Verify(r => r.SendOtpAsync(emailMock, It.IsAny<string>()), Times.Once);
    }
}