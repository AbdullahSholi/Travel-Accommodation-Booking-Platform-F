using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.FactoryPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Utils;
using Xunit;
using Xunit.Abstractions;

public class VerifyOtpTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAuthRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IOtpSenderFactory> _mockOtpSenderFactory;
    private readonly Mock<ITokenGenerator> _mockJwtTokenGenerator;
    private readonly Mock<ILogger<AuthService>> _mockLogger;

    private readonly AuthService _sut;

    public VerifyOtpTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
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

    private readonly string _email = "abdullah@gmail.com";
    private readonly string _otpCode = "123456";

    [Fact]
    [Trait("Auth", "VerifyOtp")]
    public async Task Should_ThrowException_When_OtpRecordDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetOtpRecordAsync(_email, _otpCode)).ReturnsAsync((OtpRecord)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.VerifyOtpAsync(_email, _otpCode));
        Assert.Equal(CustomMessages.OtpNotFound, exception.Message);
    }

    [Fact]
    [Trait("Auth", "VerifyOtp")]
    public async Task Should_ThrowException_When_OtpRecordExpired()
    {
        // Arrange
        var otpRecordMock = _fixture.Build<OtpRecord>()
            .With(x => x.Email, _email)
            .With(x => x.Expiration, new DateTime(2000, 1, 1))
            .Create();
        _mockRepo.Setup(r => r.GetOtpRecordAsync(_email, _otpCode)).ReturnsAsync(otpRecordMock);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.VerifyOtpAsync(_email, _otpCode));
        Assert.Equal(CustomMessages.ExpiredOtpCode, exception.Message);
    }

    [Fact]
    [Trait("Auth", "VerifyOtp")]
    public async Task Should_ThrowException_When_OtpCodeInvalid()
    {
        // Arrange
        var otpRecordMock = _fixture.Build<OtpRecord>()
            .With(x => x.Email, _email)
            .With(x => x.Expiration, new DateTime(2100, 1, 1))
            .With(x => x.Code, "")
            .Create();
        _mockRepo.Setup(r => r.GetOtpRecordAsync(_email, _otpCode)).ReturnsAsync(otpRecordMock);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.VerifyOtpAsync(_email, _otpCode));
        Assert.Equal(CustomMessages.InvalidOtp, exception.Message);
    }

    [Fact]
    [Trait("Auth", "VerifyOtp")]
    public async Task Should_ThrowException_When_UserIsNotFound()
    {
        // Arrange
        var otpRecordMock = _fixture.Build<OtpRecord>()
            .With(x => x.Email, _email)
            .With(x => x.Expiration, new DateTime(2100, 1, 1))
            .With(x => x.Code, "123456")
            .Create();

        _mockRepo.Setup(r => r.GetOtpRecordAsync(_email, _otpCode)).ReturnsAsync(otpRecordMock);
        _mockRepo.Setup(r => r.GetUserByEmailAsync(_email)).ReturnsAsync((User)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.VerifyOtpAsync(_email, _otpCode));
        Assert.Equal(CustomMessages.UserNotFound, exception.Message);
        _mockRepo.Verify(r => r.GetOtpRecordAsync(_email, _otpCode), Times.Once);
    }

    [Fact]
    [Trait("Auth", "VerifyOtp")]
    public async Task Should_OtpVerifiedSuccessfully_When_UserEnterCorrectDetails()
    {
        // Arrange
        var otpRecordMock = _fixture.Build<OtpRecord>()
            .With(x => x.Email, _email)
            .With(x => x.Expiration, new DateTime(2100, 1, 1))
            .With(x => x.Code, "123456")
            .Create();

        var userMock = _fixture.Build<User>()
            .With(x => x.Email, _email)
            .Create();

        _mockRepo.Setup(r => r.GetOtpRecordAsync(_email, _otpCode)).ReturnsAsync(otpRecordMock);
        _mockRepo.Setup(r => r.GetUserByEmailAsync(_email)).ReturnsAsync(userMock);
        _mockRepo.Setup(r => r.UpdateUserAsync(userMock)).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.InvalidateOtpAsync(otpRecordMock)).Returns(Task.CompletedTask);

        // Act
        var sut = await _sut.VerifyOtpAsync(_email, _otpCode);

        // Assert
        Assert.True(sut);
        _mockRepo.Verify(r => r.GetOtpRecordAsync(_email, _otpCode), Times.Once);
        _mockRepo.Verify(r => r.GetUserByEmailAsync(_email), Times.Once);
        _mockRepo.Verify(r => r.UpdateUserAsync(userMock), Times.Once);
        _mockRepo.Verify(r => r.InvalidateOtpAsync(otpRecordMock), Times.Once);
    }
}