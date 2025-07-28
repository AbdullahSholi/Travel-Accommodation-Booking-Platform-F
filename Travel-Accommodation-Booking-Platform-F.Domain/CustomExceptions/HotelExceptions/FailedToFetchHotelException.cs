namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.HotelExceptions;

public class FailedToFetchHotelException : Exception
{
    public int HotelId { get; }

    public FailedToFetchHotelException(string message, int hotelId) : base(message)
    {
        HotelId = hotelId;
    }
}