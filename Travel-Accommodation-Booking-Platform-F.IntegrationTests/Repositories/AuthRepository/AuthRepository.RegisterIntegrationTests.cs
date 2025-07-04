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

public class RegisterIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IAuthRepository _authRepository;
    private IOtpSenderFactory _otpSenderFactory;

    public RegisterIntegrationTests()
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
    [Trait("IntegrationTests - Auth", "Register")]
    public async Task Should_ThrowException_When_UserAlreadyExists()
    {
        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .With(x => x.Email, "abdullah@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullahsholi")
            .With(x => x.IsEmailConfirmed, true)
            .Create();

        var userMock1 = _fixture.Build<User>()
            .Without(x => x.UserId)
            .With(x => x.Email, "abdullah1@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullah1sholi")
            .With(x => x.IsEmailConfirmed, false)
            .Create();

        var userMock2 = _fixture.Build<User>()
            .Without(x => x.UserId)
            .With(x => x.Email, "abdullah@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullahsholi")
            .With(x => x.IsEmailConfirmed, true)
            .Create();

        await SeedUsersAsync(userMock);
        await SeedUsersAsync(userMock1);
        await SeedUsersAsync(userMock2);

        await SeedOtpRecordsAsync(new OtpRecord
        {
            Email = userMock.Email,
            Expiration = DateTime.UtcNow.AddMinutes(-5),
            Code = _fixture.Create<string>()
        });

        await SeedOtpRecordsAsync(new OtpRecord
        {
            Email = userMock1.Email,
            Expiration = DateTime.UtcNow.AddMinutes(-5),
            Code = _fixture.Create<string>()
        });

        await _authRepository.DeleteExpiredUnconfirmedUsersAsync();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = context.Users.ToList();
            Assert.True(users.Count == 2);
        }

        var result = await _authRepository.EmailExistsAsync("abdullah@gmail.com");

        Assert.True(result);
    }

    [Fact]
    [Trait("IntegrationTests - Auth", "Register")]
    public async Task Should_RegisterUserSuccessfully_When_ValidDataProvided()
    {
        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .With(x => x.Email, "abdullah@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullahsholi")
            .With(x => x.IsEmailConfirmed, true)
            .Create();

        var userMock1 = _fixture.Build<User>()
            .Without(x => x.UserId)
            .With(x => x.Email, "abdullah1@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullah1sholi")
            .With(x => x.IsEmailConfirmed, false)
            .Create();

        var userMock2 = _fixture.Build<User>()
            .Without(x => x.UserId)
            .With(x => x.Email, "abdullah@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullahsholi")
            .With(x => x.IsEmailConfirmed, false)
            .Create();

        await SeedUsersAsync(userMock);
        await SeedUsersAsync(userMock1);
        await SeedUsersAsync(userMock2);

        await SeedOtpRecordsAsync(new OtpRecord
        {
            Email = userMock.Email,
            Expiration = DateTime.UtcNow.AddMinutes(+5),
            Code = _fixture.Create<string>()
        });

        await SeedOtpRecordsAsync(new OtpRecord
        {
            Email = userMock1.Email,
            Expiration = DateTime.UtcNow.AddMinutes(-5),
            Code = _fixture.Create<string>()
        });

        var otpRecord = new OtpRecord
        {
            Email = userMock2.Email,
            Expiration = DateTime.UtcNow.AddMinutes(+5),
            Code = _fixture.Create<string>()
        };

        await _authRepository.DeleteExpiredUnconfirmedUsersAsync();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = context.Users.ToList();
            Assert.True(users.Count == 2);
        }

        var result = await _authRepository.EmailExistsAsync("abdullah1@gmail.com");
        Assert.False(result);

        await _authRepository.SaveOtpAsync(otpRecord);
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var otpRecords = context.OtpRecords.ToList();
            Assert.True(otpRecords.Count == 3);
            Assert.True(otpRecords[0].Email == userMock.Email);

            var strategy = _otpSenderFactory.Factory(OtpChannel.Email);
            Assert.IsType<OtpEmailSenderStrategy>(strategy);

            await strategy.SendOtpAsync(userMock.Email, otpRecords[0].Code);
        }
    }
}