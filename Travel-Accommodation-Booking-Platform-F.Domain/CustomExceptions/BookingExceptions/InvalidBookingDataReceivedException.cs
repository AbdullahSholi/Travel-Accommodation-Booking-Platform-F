namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.BookingExceptions;

public class InvalidBookingDataReceivedException : Exception
{
    public InvalidBookingDataReceivedException(string message) : base(message)
    {
    }
}