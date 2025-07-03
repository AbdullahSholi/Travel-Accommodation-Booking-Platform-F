using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.Services.TokenBlacklistService;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class IsTokenBlacklistedTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ITokenBlacklistedRepository> _tokenBlacklistedRepository;
    private readonly Mock<ILogger<TokenBlacklistService>> _logger;
    private readonly TokenBlacklistService _sut;

    public IsTokenBlacklistedTests()
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
    [Trait("TokenBlacklisted", "IsTokenBlacklisted")]
    public async Task Should_ThrowArgumentNullException_When_ArgumentIsNull()
    {
        // Arrange
        var jti = "";
        _tokenBlacklistedRepository.Setup(r => r.CheckIfTokenBlacklistedAsync(jti)).ReturnsAsync(null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.IsTokenBlacklistedAsync(jti));
        Assert.Equal(nameof(jti), exception.ParamName);
    }

    [Fact]
    [Trait("TokenBlacklisted", "IsTokenBlacklisted")]
    public async Task Should_ReturnFalse_When_TokenIsNotBlacklisted()
    {
        // Arrange
        var jtiMock = _fixture.Create<string>();
        _tokenBlacklistedRepository.Setup(r => r.CheckIfTokenBlacklistedAsync(jtiMock)).ReturnsAsync(false);

        // Act
        var result = await _sut.IsTokenBlacklistedAsync(jtiMock);

        // Assert
        Assert.False(result);
        _tokenBlacklistedRepository.Verify(r => r.CheckIfTokenBlacklistedAsync(jtiMock), Times.Once());
    }

    [Fact]
    [Trait("TokenBlacklisted", "IsTokenBlacklisted")]
    public async Task Should_ReturnTrue_When_TokenIsBlacklisted()
    {
        // Arrange
        var jtiMock = _fixture.Create<string>();
        _tokenBlacklistedRepository.Setup(r => r.CheckIfTokenBlacklistedAsync(jtiMock)).ReturnsAsync(true);

        // Act
        var result = await _sut.IsTokenBlacklistedAsync(jtiMock);

        // Assert
        Assert.True(result);
        _tokenBlacklistedRepository.Verify(r => r.CheckIfTokenBlacklistedAsync(jtiMock), Times.Once());
    }
}