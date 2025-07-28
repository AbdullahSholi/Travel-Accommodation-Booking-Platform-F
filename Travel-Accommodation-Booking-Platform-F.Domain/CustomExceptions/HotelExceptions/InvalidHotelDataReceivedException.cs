namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.HotelExceptions;

public class InvalidHotelDataReceivedException : Exception
{
    public InvalidHotelDataReceivedException(string message) : base(message)
    {
    }
}