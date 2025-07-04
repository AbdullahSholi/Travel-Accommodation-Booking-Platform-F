using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.FactoryPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.ExternalServices.OtpSender;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;
using Xunit;

public class SendOtpIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IAuthRepository _authRepository;
    private IOtpSenderFactory _otpSenderFactory;

    public SendOtpIntegrationTests()
    {
        _fixture = new Fixture();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        _authRepository = provider.GetRequiredService<IAuthRepository>();
        _otpSenderFactory = provider.GetRequiredService<IOtpSenderFactory>();
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "SendOtp")]
    public async Task Should_SendOtpSuccessfully_When_InsertCorrectDetails()
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
            Email = userMock.Email,
            Expiration = DateTime.UtcNow.AddMinutes(+5),
            Code = _fixture.Create<string>()
        };

        await SeedUsersAsync(userMock);

        var user = await _authRepository.GetUserByEmailAsync(userMock.Email);
        Assert.NotNull(user);
        Assert.Equal(userMock.Email, user.Email);
        Assert.Equal(userMock.Password, user.Password);
        Assert.Equal(userMock.Username, user.Username);

        otpRecordMock.Id = 0;
        await _authRepository.SaveOtpAsync(otpRecordMock);

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = context.Users.ToList();
            var otpRecords = context.OtpRecords.ToList();
            Assert.True(users.Count == 1);
            Assert.True(otpRecords.Count == 1);
        }

        var strategy = _otpSenderFactory.Factory(OtpChannel.Email);
        Assert.NotNull(strategy);
        Assert.IsType<OtpEmailSenderStrategy>(strategy);

        await strategy.SendOtpAsync(user.Email, otpRecordMock.Code);
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "SendOtp")]
    public async Task Should_ThrowException_When_DoesNotExist()
    {
        await ClearDatabaseAsync();

        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .With(x => x.Email, "abdullah@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullahsholi")
            .With(x => x.IsEmailConfirmed, true)
            .Create();

        await SeedUsersAsync(userMock);

        var user = await _authRepository.GetUserByEmailAsync("abdullah1@gmail.com");
        Assert.Null(user);
    }
}