namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.AdminExceptions;

public class FailedToFetchUsersException : Exception
{
    public FailedToFetchUsersException(string message) : base(message)
    {
        
    }
}