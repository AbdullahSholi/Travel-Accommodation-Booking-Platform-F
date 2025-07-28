namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.ReviewExceptions;

public class FailedToFetchReviewException : Exception
{
    public int ReviewId { get; }

    public FailedToFetchReviewException(string message, int reviewId) : base(message)
    {
        ReviewId = reviewId;
    }
}