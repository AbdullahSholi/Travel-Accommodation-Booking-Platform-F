namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.ReviewExceptions;

public class InvalidReviewDataReceivedException : Exception
{
    public InvalidReviewDataReceivedException(string message) : base(message)
    {
    }
}