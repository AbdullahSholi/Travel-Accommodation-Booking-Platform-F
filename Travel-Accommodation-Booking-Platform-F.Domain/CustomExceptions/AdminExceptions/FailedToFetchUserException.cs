namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.AdminExceptions;

public class FailedToFetchUserException : Exception
{
    public int UserId { get; }
    public FailedToFetchUserException(string message, int userId) : base(message)
    {
        UserId = userId;
    }
}