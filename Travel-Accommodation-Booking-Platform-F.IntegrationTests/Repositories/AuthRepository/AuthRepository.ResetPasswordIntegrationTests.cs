using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;
using Xunit;

public class ResetPasswordIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IAuthRepository _authRepository;

    public ResetPasswordIntegrationTests()
    {
        _fixture = new Fixture();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        _authRepository = provider.GetRequiredService<IAuthRepository>();
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "ResetPassword")]
    public async Task Should_ResetPassword_When_UserEnteredCorrectDetails()
    {
        await ClearDatabaseAsync();

        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .With(x => x.Email, "abdullah@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullahsholi")
            .With(x => x.IsEmailConfirmed, true)
            .Create();

        var otpRecordMock = new OtpRecord
        {
            Email = "abdullah@gmail.com",
            Expiration = DateTime.UtcNow.AddMinutes(+5),
            Code = _fixture.Create<string>()
        };
        await SeedUsersAsync(userMock);
        await SeedOtpRecordsAsync(otpRecordMock);

        var record = await _authRepository.GetOtpRecordAsync(otpRecordMock.Email, otpRecordMock.Code);
        Assert.NotNull(record);
        Assert.True(record.Expiration.ToUniversalTime() > DateTime.UtcNow.ToUniversalTime());

        var user = await _authRepository.GetUserByEmailAsync("abdullah@gmail.com");
        Assert.NotNull(user);
        Assert.Equal(otpRecordMock.Email, user.Email);

        await _authRepository.HashAndSavePasswordAsync(user, "Abdullah@971");
        using (var scope = Factory.Services.CreateScope())
        {
            var provider = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = provider.Users.ToList();
            Assert.StartsWith("$argon2", users[0].Password);
        }

        await _authRepository.RemoveAndSaveOtpAsync(otpRecordMock);
        using (var scope = Factory.Services.CreateScope())
        {
            var provider = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var otpRecords = provider.OtpRecords.ToList();
            Assert.True(otpRecords.Count == 0);
        }
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "ResetPassword")]
    public async Task Should_ThrowException_When_OtpCodeExpired()
    {
        await ClearDatabaseAsync();

        var otpRecordMock = new OtpRecord
        {
            Email = "abdullah@gmail.com",
            Expiration = DateTime.UtcNow.AddMinutes(-100),
            Code = _fixture.Create<string>()
        };

        await SeedOtpRecordsAsync(otpRecordMock);

        var record = await _authRepository.GetOtpRecordAsync(otpRecordMock.Email, otpRecordMock.Code);
        Assert.NotNull(record);
        Assert.True(record.Expiration.ToUniversalTime() < DateTime.UtcNow.ToUniversalTime());
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "ResetPassword")]
    public async Task Should_ThrowException_When_OtpRecordDoesNotExist()
    {
        await ClearDatabaseAsync();

        var otpRecordMock = new OtpRecord
        {
            Email = "abdullah@gmail.com",
            Expiration = DateTime.UtcNow.AddMinutes(+5),
            Code = _fixture.Create<string>()
        };

        await SeedOtpRecordsAsync(otpRecordMock);

        var record = await _authRepository.GetOtpRecordAsync("abdullah1@gmail.com", otpRecordMock.Code);
        Assert.Null(record);
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "ResetPassword")]
    public async Task Should_ThrowException_When_UserNotFound()
    {
        await ClearDatabaseAsync();

        var otpRecordMock = new OtpRecord
        {
            Email = "abdullah@gmail.com",
            Expiration = DateTime.UtcNow.AddMinutes(+5),
            Code = _fixture.Create<string>()
        };

        await SeedOtpRecordsAsync(otpRecordMock);

        var record = await _authRepository.GetOtpRecordAsync(otpRecordMock.Email, otpRecordMock.Code);
        Assert.NotNull(record);
        Assert.True(record.Expiration.ToUniversalTime() > DateTime.UtcNow.ToUniversalTime());

        var user = await _authRepository.GetUserByEmailAsync("abdullah1@gmail.com");
        Assert.Null(user);
    }
}