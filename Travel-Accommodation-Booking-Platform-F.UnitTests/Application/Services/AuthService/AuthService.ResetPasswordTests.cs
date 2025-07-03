using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.FactoryPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Utils;
using Xunit;

public class ResetPasswordTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAuthRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IOtpSenderFactory> _mockOtpSenderFactory;
    private readonly Mock<ITokenGenerator> _mockJwtTokenGenerator;
    private readonly Mock<ILogger<AuthService>> _mockLogger;

    private readonly AuthService _sut;

    public ResetPasswordTests()
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

    [Fact]
    [Trait("UnitTests - Auth", "ResetPassword")]
    public async Task Should_ThrowException_When_OtpRecordDoesNotExist()
    {
        // Arrange
        var dtoMock = _fixture.Build<ResetPasswordReadDto>()
            .With(x => x.Email, "abdullah@gmail.com")
            .Create();

        _mockRepo.Setup(r => r.GetOtpRecordAsync(dtoMock.Email, dtoMock.Otp)).ReturnsAsync((OtpRecord)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.ResetPasswordAsync(dtoMock));
        Assert.Equal(CustomMessages.InvalidOrExpiredOtpCode, exception.Message);
    }

    [Fact]
    [Trait("UnitTests - Auth", "ResetPassword")]
    public async Task Should_ThrowException_When_OtpCodeExpired()
    {
        // Arrange
        var dtoMock = _fixture.Build<ResetPasswordReadDto>()
            .With(x => x.Email, "abdullah@gmail.com")
            .Create();
        var otpRecord = _fixture.Build<OtpRecord>()
            .With(x => x.Email, "abdullah@gmail.com")
            .With(x => x.Expiration, new DateTime(2023, 1, 1, 10, 0, 0))
            .Create();

        _mockRepo.Setup(r => r.GetOtpRecordAsync(dtoMock.Email, dtoMock.Otp)).ReturnsAsync(otpRecord);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.ResetPasswordAsync(dtoMock));
        Assert.Equal(CustomMessages.ExpiredOtpCode, exception.Message);
        _mockRepo.Verify(r => r.GetOtpRecordAsync(dtoMock.Email, dtoMock.Otp), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Auth", "ResetPassword")]
    public async Task Should_ThrowException_When_UserNotFound()
    {
        // Arrange
        var dtoMock = _fixture.Build<ResetPasswordReadDto>()
            .With(x => x.Email, "abdullah@gmail.com")
            .Create();
        var otpRecord = _fixture.Build<OtpRecord>()
            .With(x => x.Email, "abdullah@gmail.com")
            .With(x => x.Expiration, new DateTime(2100, 1, 1, 10, 0, 0))
            .Create();

        _mockRepo.Setup(r => r.GetOtpRecordAsync(dtoMock.Email, dtoMock.Otp)).ReturnsAsync(otpRecord);
        _mockRepo.Setup(r => r.GetUserByEmailAsync(dtoMock.Email)).ReturnsAsync((User)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.ResetPasswordAsync(dtoMock));
        Assert.Equal(CustomMessages.UserNotFound, exception.Message);
        _mockRepo.Verify(r => r.GetOtpRecordAsync(dtoMock.Email, dtoMock.Otp), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Auth", "ResetPassword")]
    public async Task Should_ResetPassword_When_UserEnteredCorrectDetails()
    {
        // Arrange
        var dtoMock = _fixture.Build<ResetPasswordReadDto>()
            .With(x => x.Email, "abdullah@gmail.com")
            .Create();
        var otpRecordMock = _fixture.Build<OtpRecord>()
            .With(x => x.Email, "abdullah@gmail.com")
            .With(x => x.Expiration, new DateTime(2100, 1, 1, 10, 0, 0))
            .Create();
        var userMock = _fixture.Build<User>()
            .With(x => x.Email, "abdullah@gmail.com")
            .Create();

        _mockRepo.Setup(r => r.GetOtpRecordAsync(dtoMock.Email, dtoMock.Otp)).ReturnsAsync(otpRecordMock);
        _mockRepo.Setup(r => r.GetUserByEmailAsync(dtoMock.Email)).ReturnsAsync(userMock);
        _mockRepo.Setup(r => r.HashAndSavePasswordAsync(userMock, dtoMock.NewPassword)).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.RemoveAndSaveOtpAsync(otpRecordMock)).Returns(Task.CompletedTask);

        // Act
        var sut = await _sut.ResetPasswordAsync(dtoMock);

        // Assert
        Assert.True(sut);
        _mockRepo.Verify(r => r.GetOtpRecordAsync(dtoMock.Email, dtoMock.Otp), Times.Once);
        _mockRepo.Verify(r => r.GetUserByEmailAsync(dtoMock.Email), Times.Once);
        _mockRepo.Verify(r => r.HashAndSavePasswordAsync(userMock, dtoMock.NewPassword), Times.Once);
        _mockRepo.Verify(r => r.RemoveAndSaveOtpAsync(otpRecordMock), Times.Once);
    }
}