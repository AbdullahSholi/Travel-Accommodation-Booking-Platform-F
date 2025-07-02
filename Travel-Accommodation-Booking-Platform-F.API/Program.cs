using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Travel_Accommodation_Booking_Platform_F.Application.Mapping;
using Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;
using Travel_Accommodation_Booking_Platform_F.Application.Services.TokenBlacklistService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.Generators;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;
using Travel_Accommodation_Booking_Platform_F.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddLogging();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ITokenBlacklistedRepository, TokenBlacklistedRepository>();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

var jwtSettingsSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY") ??
                throw new InvalidOperationException(CustomMessages.UnSetSecretKey);

builder.Services.AddSingleton<JwtTokenGenerator>(
    sp =>
    {
        var jwtSettings = sp.GetRequiredService<IOptions<JwtSettings>>();
        return new JwtTokenGenerator(jwtSettings, secretKey);
    });

var connectionString = Environment.GetEnvironmentVariable("TRAVEL_ACCOMMODATION_CONNECTION_STRING") ??
                       throw new InvalidOperationException(CustomMessages.UnSetConnectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async (context) =>
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
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddCors();

builder.Services.AddControllers();

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.Urls.Clear();
    app.Urls.Add(Constants.LocalUrl);
    app.MapOpenApi();
}
else
{
    app.Urls.Clear();
    app.Urls.Add(Constants.DockerSwarmUrl);
}

app.MapGet("/", () => CustomMessages.HelloMessage);

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors(policy =>
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseHsts();
app.UseXContentTypeOptions();
app.UseReferrerPolicy(opts => opts.NoReferrer());
app.UseXXssProtection(options => options.EnabledWithBlockMode());
app.UseXfo(options => options.Deny());
app.UseIpRateLimiting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();