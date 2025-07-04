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

public class VerifyOtpIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IAuthRepository _authRepository;

    private readonly string _email = "abdullah@gmail.com";
    private readonly string _otpCode = "123456";

    public VerifyOtpIntegrationTests()
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
    [Trait("IntegrationTests - Auth", "VerifyOtp")]
    public async Task Should_ThrowException_When_OtpRecordDoesNotExist()
    {
        await ClearDatabaseAsync();

        var otpRecord = await _authRepository.GetOtpRecordAsync(_email, _otpCode);
        Assert.Null(otpRecord);
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "VerifyOtp")]
    public async Task Should_ThrowException_When_OtpRecordExpired()
    {
        await ClearDatabaseAsync();

        var userMock = _fixture.Build<User>()
            .With(x => x.Email, _email)
            .Without(x => x.UserId)
            .Create();

        var otpRecordMock = _fixture.Build<OtpRecord>()
            .With(x => x.Email, _email)
            .With(x => x.Code, _otpCode)
            .With(x => x.Expiration, DateTime.UtcNow.AddMinutes(-10))
            .Without(x => x.Id)
            .Create();

        await SeedUsersAsync(userMock);
        await SeedOtpRecordsAsync(otpRecordMock);


        var otpRecord = await _authRepository.GetOtpRecordAsync(_email, _otpCode);
        Assert.NotNull(otpRecord);
        Assert.Equal(otpRecordMock.Email, otpRecord.Email);
        using (var scope = Factory.Services.CreateScope())
        {
            var provider = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = provider.Users.ToList();
            var otpRecords = provider.OtpRecords.ToList();
            Assert.True(users.Count == 1);
            Assert.True(otpRecords.Count == 1);
        }

        Assert.True(otpRecord.Expiration.ToUniversalTime() < DateTime.UtcNow.ToUniversalTime());
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "VerifyOtp")]
    public async Task Should_ThrowException_When_UserIsNotFound()
    {
        await ClearDatabaseAsync();

        var userMock = _fixture.Build<User>()
            .With(x => x.Email, _email)
            .With(x => x.IsEmailConfirmed, true)
            .Without(x => x.UserId)
            .Create();

        var otpRecordMock = _fixture.Build<OtpRecord>()
            .With(x => x.Email, _email)
            .With(x => x.Code, _otpCode)
            .With(x => x.Expiration, DateTime.UtcNow.AddMinutes(+10))
            .Without(x => x.Id)
            .Create();

        await SeedUsersAsync(userMock);
        await SeedOtpRecordsAsync(otpRecordMock);


        var otpRecord = await _authRepository.GetOtpRecordAsync(_email, _otpCode);
        Assert.NotNull(otpRecord);
        Assert.Equal(otpRecordMock.Email, otpRecord.Email);
        using (var scope = Factory.Services.CreateScope())
        {
            var provider = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = provider.Users.ToList();
            var otpRecords = provider.OtpRecords.ToList();
            Assert.True(users.Count == 1);
            Assert.True(otpRecords.Count == 1);
        }

        Assert.True(otpRecord.Expiration.ToUniversalTime() > DateTime.UtcNow.ToUniversalTime());

        await ClearDatabaseAsync();
        var user = await _authRepository.GetUserByEmailAsync(_email);
        Assert.Null(user);
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "VerifyOtp")]
    public async Task Should_OtpVerifiedSuccessfully_When_UserEnterCorrectDetails()
    {
        await ClearDatabaseAsync();

        var userMock = _fixture.Build<User>()
            .With(x => x.Email, _email)
            .With(x => x.IsEmailConfirmed, false)
            .Without(x => x.UserId)
            .Create();

        var otpRecordMock = _fixture.Build<OtpRecord>()
            .With(x => x.Email, _email)
            .With(x => x.Code, _otpCode)
            .With(x => x.Expiration, DateTime.UtcNow.AddMinutes(+10))
            .Without(x => x.Id)
            .Create();

        await SeedUsersAsync(userMock);
        await SeedOtpRecordsAsync(otpRecordMock);


        var otpRecord = await _authRepository.GetOtpRecordAsync(_email, _otpCode);
        Assert.NotNull(otpRecord);
        Assert.Equal(otpRecordMock.Email, otpRecord.Email);
        using (var scope = Factory.Services.CreateScope())
        {
            var provider = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = provider.Users.ToList();
            var otpRecords = provider.OtpRecords.ToList();
            Assert.True(users.Count == 1);
            Assert.True(otpRecords.Count == 1);
        }

        Assert.True(otpRecord.Expiration.ToUniversalTime() > DateTime.UtcNow.ToUniversalTime());

        var user = await _authRepository.GetUserByEmailAsync(_email);
        Assert.NotNull(user);
        Assert.Equal(userMock.Email, user.Email);

        user.IsEmailConfirmed = true;
        await _authRepository.UpdateUserAsync(user);
        using (var scope = Factory.Services.CreateScope())
        {
            var provider = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = provider.Users.ToList();
            Assert.True(users.Count == 1);
            Assert.True(users[0].IsEmailConfirmed);
        }

        await _authRepository.InvalidateOtpAsync(otpRecord);
        using (var scope = Factory.Services.CreateScope())
        {
            var provider = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var otpRecords = provider.OtpRecords.ToList();
            Assert.True(otpRecords.Count == 0);
        }
    }
}