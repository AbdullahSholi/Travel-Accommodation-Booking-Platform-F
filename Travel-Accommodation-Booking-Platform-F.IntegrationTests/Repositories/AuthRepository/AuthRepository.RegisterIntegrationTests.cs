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
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
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
        await ClearDatabaseAsync();

        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .Without(x => x.OtpRecords)
            .With(x => x.Email, "abdullah@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullahsholi")
            .With(x => x.IsEmailConfirmed, true)
            .Create();

        var userMock1 = _fixture.Build<User>()
            .Without(x => x.UserId)
            .Without(x => x.OtpRecords)
            .With(x => x.Email, "abdullah1@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullah1sholi")
            .With(x => x.IsEmailConfirmed, false)
            .Create();

        var userMock2 = _fixture.Build<User>()
            .Without(x => x.UserId)
            .Without(x => x.OtpRecords)
            .With(x => x.Email, "abdullah2@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullah2sholi")
            .With(x => x.IsEmailConfirmed, true)
            .Create();

        await SeedUsersAsync(userMock, userMock1, userMock2);

        var savedUser1 = await _authRepository.GetUserByEmailAsync("abdullah@gmail.com");
        var savedUser2 = await _authRepository.GetUserByEmailAsync("abdullah1@gmail.com");

        Assert.NotNull(savedUser1);
        Assert.NotNull(savedUser2);

        var otpRecord1 = new OtpRecord
        {
            UserId = savedUser1.UserId,
            Email = savedUser1.Email,
            Expiration = DateTime.UtcNow.AddMinutes(-5),
            Code = _fixture.Create<string>()
        };

        var otpRecord2 = new OtpRecord
        {
            UserId = savedUser2.UserId,
            Email = savedUser2.Email,
            Expiration = DateTime.UtcNow.AddMinutes(-5),
            Code = _fixture.Create<string>()
        };

        await SeedOtpRecordsAsync(otpRecord1, otpRecord2);

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
        await ClearDatabaseAsync();

        var userMock = _fixture.Build<User>()
            .Without(x => x.UserId)
            .Without(x => x.OtpRecords)
            .With(x => x.Email, "abdullah@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullahsholi")
            .With(x => x.IsEmailConfirmed, true)
            .Create();

        var userMock1 = _fixture.Build<User>()
            .Without(x => x.UserId)
            .Without(x => x.OtpRecords)
            .With(x => x.Email, "abdullah1@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullah1sholi")
            .With(x => x.IsEmailConfirmed, false)
            .Create();

        var userMock2 = _fixture.Build<User>()
            .Without(x => x.UserId)
            .Without(x => x.OtpRecords)
            .With(x => x.Email, "abdullah2@gmail.com")
            .With(x => x.Password, "Sholi@971")
            .With(x => x.Username, "abdullah2sholi")
            .With(x => x.IsEmailConfirmed, false)
            .Create();

        await SeedUsersAsync(userMock, userMock1, userMock2);

        var savedUser1 = await _authRepository.GetUserByEmailAsync("abdullah@gmail.com");
        var savedUser2 = await _authRepository.GetUserByEmailAsync("abdullah1@gmail.com");
        var savedUser3 = await _authRepository.GetUserByEmailAsync("abdullah2@gmail.com");
        Assert.NotNull(savedUser1);
        Assert.NotNull(savedUser2);
        Assert.NotNull(savedUser3);

        await SeedOtpRecordsAsync(new OtpRecord
        {
            UserId = userMock.UserId,
            Email = userMock.Email,
            Expiration = DateTime.UtcNow.AddMinutes(+5),
            Code = _fixture.Create<string>()
        });

        var otpRecord1 = new OtpRecord
        {
            UserId = savedUser1.UserId,
            Email = savedUser1.Email,
            Expiration = DateTime.UtcNow.AddMinutes(+5),
            Code = _fixture.Create<string>()
        };

        var otpRecord2 = new OtpRecord
        {
            UserId = savedUser2.UserId,
            Email = savedUser2.Email,
            Expiration = DateTime.UtcNow.AddMinutes(-5),
            Code = _fixture.Create<string>()
        };

        await SeedOtpRecordsAsync(otpRecord1, otpRecord2);

        await _authRepository.DeleteExpiredUnconfirmedUsersAsync();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = context.Users.ToList();
            Assert.True(users.Count == 2);
        }

        var result = await _authRepository.EmailExistsAsync("abdullah1@gmail.com");
        Assert.False(result);

        var newOtpRecord = new OtpRecord
        {
            UserId = savedUser3.UserId,
            Email = savedUser3.Email,
            Expiration = DateTime.UtcNow.AddMinutes(+5),
            Code = _fixture.Create<string>()
        };

        await _authRepository.SaveOtpAsync(newOtpRecord);
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var otpRecords = context.OtpRecords.ToList();
            Assert.True(otpRecords.Count >= 1);

            var strategy = _otpSenderFactory.Factory(OtpChannel.Email);
            Assert.IsType<OtpEmailSenderStrategy>(strategy);

            await strategy.SendOtpAsync(otpRecords[0].Email, otpRecords[0].Code);
        }
    }
}