namespace Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.StrategyPattern;

public interface IOtpSenderStrategy
{
    public Task SendOtpAsync(string to, string otp);
}