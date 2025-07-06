namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.HotelExceptions;

public class FailedToFetchHotelsException : Exception
{
    public FailedToFetchHotelsException(string message) : base(message)
    {
    }
}