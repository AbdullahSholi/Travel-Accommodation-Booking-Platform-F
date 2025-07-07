using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Observer;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.ExternalServices.NotifyUsers;

public class NotifyUsersEmailObserver : INotifyUsersObserver
{
    private readonly IAdminRepository _adminRepository;
    private readonly EmailSettings _emailSettings;

    public NotifyUsersEmailObserver(IAdminRepository adminRepository, IOptions<EmailSettings> emailSettings)
    {
        _adminRepository = adminRepository;
        _emailSettings = emailSettings.Value;
    }
    
    public async Task SendHotelAnnouncementAsync(Hotel hotel)
    {
        var appPassword = Environment.GetEnvironmentVariable("APP_PASSWORD") ??
                          throw new InvalidOperationException(CustomMessages.CustomMessages.UnSetAppPassword);
        
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));

        var users = await _adminRepository.GetAllAsync();
        if (users == null || !users.Any()) return;
        
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_emailSettings.Username, appPassword);
        
        foreach (var user in users)
        {
            email.To.Add(MailboxAddress.Parse(user.Email));
            email.Subject = $"Exciting News! New Hotel Opened: {hotel.HotelName}";

            var body = $@"
        <div style='font-family: Arial, sans-serif; color: #333;'>
            <h2 style='color: #2E86C1;'>🌍 We’re Expanding Around the World!</h2>
            <p>We are thrilled to announce the opening of our brand-new hotel: <strong>{hotel.HotelName}</strong> located in <strong>{hotel.Location}</strong>.</p>
            <p>Whether you're planning your next getaway or a business trip, our new location offers top-notch services, luxurious rooms, and unforgettable experiences.</p>
            <p style='margin-top: 20px;'>Thank you for being a valued member of our community. We can't wait to welcome you!</p>
            <p style='margin-top: 30px;'>Best regards,<br><strong>{_emailSettings.SenderName}</strong></p>
        </div>";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };
            
            await smtp.SendAsync(email);
        }
        
        await smtp.DisconnectAsync(true);
    }
}