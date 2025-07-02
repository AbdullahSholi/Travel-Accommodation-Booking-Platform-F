using AspNetCoreRateLimit;
using Travel_Accommodation_Booking_Platform_F.Extensions;
using Travel_Accommodation_Booking_Platform_F.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddLogging();

builder.Services.AddLogging();

builder.Services.AddApplicationServices();

builder.Services.AddApiVersioningSetup();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddDatabase();

builder.Services.AddRateLimiting(builder.Configuration);

builder.Services.AddCors();

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