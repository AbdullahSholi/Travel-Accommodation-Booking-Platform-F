using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.FactoryPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.StrategyPattern;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.ExternalServices.OtpSender;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.ExternalServices.OtpSenderFactory;

public class OtpSenderFactory : IOtpSenderFactory
{
    private readonly OtpEmailSenderStrategy _emailSender;
    private readonly OtpWhatsAppSenderStrategy _whatsAppSender;
    
    public OtpSenderFactory(
        OtpEmailSenderStrategy emailSender,
        OtpWhatsAppSenderStrategy whatsAppSender)
    {
        _emailSender = emailSender;
        _whatsAppSender = whatsAppSender;
    }
    
    public IOtpSenderStrategy Factory(OtpChannel channel)
    {
        return channel switch
        {
            OtpChannel.Email => _emailSender,
            OtpChannel.WhatsApp => _whatsAppSender,
            _ => throw new NotImplementedException(),
        };
    }
}