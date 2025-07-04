using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Travel_Accommodation_Booking_Platform_F.Application.Mapping;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;
using Travel_Accommodation_Booking_Platform_F.Application.Services.TokenBlacklistService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.Generators;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.FactoryPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.StrategyPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Utils;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.ExternalServices.OtpSender;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.ExternalServices.OtpSenderFactory;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;
using Travel_Accommodation_Booking_Platform_F.Utils.Auth;

namespace Travel_Accommodation_Booking_Platform_F.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<ITokenBlacklistedRepository, TokenBlacklistedRepository>();
        services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddAutoMapper(typeof(UserProfile).Assembly);
        services.Configure<EmailSettings>(services.BuildServiceProvider().GetRequiredService<IConfiguration>()
            .GetSection("EmailSettings"));
        services.AddScoped<IOtpSenderFactory, OtpSenderFactory>();

        services.AddScoped<IOtpSenderStrategy, OtpEmailSenderStrategy>();
        services.AddScoped<IOtpSenderStrategy, OtpWhatsAppSenderStrategy>();
        services.AddScoped<OtpEmailSenderStrategy>();
        services.AddScoped<OtpWhatsAppSenderStrategy>();
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettingsSection = configuration.GetSection("Jwt");
        services.Configure<JwtSettings>(jwtSettingsSection);
        var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

        var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY") ??
                        throw new InvalidOperationException(CustomMessages.UnSetSecretKey);

        services.AddSingleton<JwtTokenGenerator>(sp =>
        {
            var jwtOptions = sp.GetRequiredService<IOptions<JwtSettings>>();
            return new JwtTokenGenerator(jwtOptions);
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var jti = context.Principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                    var blacklistService = context.HttpContext.RequestServices.GetService<ITokenBlacklistService>();

                    if (jti != null && await blacklistService.IsTokenBlacklistedAsync(jti))
                        context.Fail(CustomMessages.TokenIsBlacklisted);
                }
            };

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            };
        });

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("TRAVEL_ACCOMMODATION_CONNECTION_STRING") ??
                               throw new InvalidOperationException(CustomMessages.UnSetConnectionString);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        return services;
    }

    public static IServiceCollection AddApiVersioningSetup(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.ReportApiVersions = true;
        });
        return services;
    }
}