using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.StrategyPattern;

namespace Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.FactoryPattern;

public interface IOtpSenderFactory
{
    IOtpSenderStrategy Factory(OtpChannel channel);
}