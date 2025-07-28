namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.CityExceptions;

public class FailedToFetchCityException : Exception
{
    public int CityId { get; }

    public FailedToFetchCityException(string message, int cityId) : base(message)
    {
        CityId = cityId;
    }
}