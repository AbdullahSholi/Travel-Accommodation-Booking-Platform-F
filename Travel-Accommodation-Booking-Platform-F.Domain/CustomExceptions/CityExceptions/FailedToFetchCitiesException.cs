namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.CityExceptions;

public class FailedToFetchCitiesException : Exception
{
    public FailedToFetchCitiesException(string message) : base(message)
    {
    }
}