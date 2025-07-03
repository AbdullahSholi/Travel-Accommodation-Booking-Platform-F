using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.StrategyPattern;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.ExternalServices.OtpSender;

public class OtpEmailSenderStrategy : IOtpSenderStrategy
{
    private readonly EmailSettings _emailSettings;

    public OtpEmailSenderStrategy(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendOtpAsync(string to, string otp)
    {
        var appPassword = Environment.GetEnvironmentVariable("APP_PASSWORD") ??
                          throw new InvalidOperationException(CustomMessages.CustomMessages.UnSetAppPassword);
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = CustomMessages.CustomMessages.YourOtpCode;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            { Text = $"<h3>Your OTP Code is: <b>{otp}</b></h3><p>This code expires in 5 minutes.</p>" };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port,
            SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_emailSettings.Username, appPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}