namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.ReviewExceptions;

public class FailedToFetchReviewsException : Exception
{
    public FailedToFetchReviewsException(string message) : base(message)
    {
    }
}