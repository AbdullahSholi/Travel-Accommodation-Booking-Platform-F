namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.CityExceptions;

public class InvalidCityDataReceivedException : Exception
{
    public InvalidCityDataReceivedException(string message) : base(message)
    {
    }
}