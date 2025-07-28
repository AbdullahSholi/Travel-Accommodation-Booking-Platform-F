namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.BookingExceptions;

public class FailedToFetchBookingException : Exception
{
    public int BookingId { get; }

    public FailedToFetchBookingException(string message, int bookingId) : base(message)
    {
        BookingId = bookingId;
    }
}