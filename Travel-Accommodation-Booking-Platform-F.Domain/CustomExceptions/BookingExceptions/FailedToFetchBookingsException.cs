namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.BookingExceptions;

public class FailedToFetchBookingsException : Exception
{
    public FailedToFetchBookingsException(string message) : base(message)
    {
    }
}