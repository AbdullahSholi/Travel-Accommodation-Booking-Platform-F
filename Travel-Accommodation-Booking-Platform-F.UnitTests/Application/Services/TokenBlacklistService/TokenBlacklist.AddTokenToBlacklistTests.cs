using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.Services.TokenBlacklistService;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class AddTokenToBlacklistTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ITokenBlacklistedRepository> _tokenBlacklistedRepository;
    private readonly Mock<ILogger<TokenBlacklistService>> _logger;
    private readonly TokenBlacklistService _sut;

    public AddTokenToBlacklistTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _tokenBlacklistedRepository = _fixture.Freeze<Mock<ITokenBlacklistedRepository>>();
        _logger = _fixture.Freeze<Mock<ILogger<TokenBlacklistService>>>();

        _sut = new TokenBlacklistService(
            _tokenBlacklistedRepository.Object,
            _logger.Object
        );
    }

    [Fact]
    [Trait("TokenBlacklisted", "AddTokenToBlacklist")]
    public async Task Should_ThrowArgumentNullException_When_ArgumentsAreNullOrEmpty()
    {
        // Arrange
        var jti = "";
        var expiration = new DateTime(2020, 01, 01);
        var token = _fixture.Build<BlacklistedToken>()
            .With(x => x.Jti, jti)
            .With(x => x.Expiration, expiration)
            .Create();

        _tokenBlacklistedRepository.Setup(r => r.AddTokenToBlacklistAsync(token)).Returns(Task.CompletedTask);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddTokenToBlacklistAsync(jti, expiration));
        Assert.Equal(nameof(jti), exception.ParamName);
    }

    [Fact]
    [Trait("TokenBlacklisted", "AddTokenToBlacklist")]
    public async Task Should_ThrowArgumentNullException_When_JtiArgumentIsNullOrEmpty()
    {
        // Arrange
        var jti = "";
        var expiration = new DateTime(2320, 01, 01);
        var token = _fixture.Build<BlacklistedToken>()
            .With(x => x.Jti, jti)
            .With(x => x.Expiration, expiration)
            .Create();

        _tokenBlacklistedRepository.Setup(r => r.AddTokenToBlacklistAsync(token)).Returns(Task.CompletedTask);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddTokenToBlacklistAsync(jti, expiration));
        Assert.Equal(nameof(jti), exception.ParamName);
    }

    [Fact]
    [Trait("TokenBlacklisted", "AddTokenToBlacklist")]
    public async Task Should_ThrowArgumentNullException_When_ExpirationArgumentIsInvalid()
    {
        // Arrange
        var jti = "123456";
        var expiration = new DateTime(2020, 01, 01);
        var token = _fixture.Build<BlacklistedToken>()
            .With(x => x.Jti, jti)
            .With(x => x.Expiration, expiration)
            .Create();

        _tokenBlacklistedRepository.Setup(r => r.AddTokenToBlacklistAsync(token)).Returns(Task.CompletedTask);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddTokenToBlacklistAsync(jti, expiration));
        Assert.Equal(nameof(expiration), exception.ParamName);
    }

    [Fact]
    [Trait("TokenBlacklisted", "AddTokenToBlacklist")]
    public async Task Should_AddTokenToBlacklistSuccessfully_When_EnterValidInformation()
    {
        // Arrange
        var jti = "123456";
        var expiration = new DateTime(2320, 01, 01);

        _tokenBlacklistedRepository.Setup(r => r.AddTokenToBlacklistAsync(It.IsAny<BlacklistedToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.AddTokenToBlacklistAsync(jti, expiration);

        // Assert
        _tokenBlacklistedRepository.Verify(r => r.AddTokenToBlacklistAsync(
            It.Is<BlacklistedToken>(t => t.Jti == jti && t.Expiration == expiration)
        ), Times.Once);
    }
}