namespace Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;

public static class ReviewServiceLogMessages
{
    public const string CreateReviewRequestReceived = "Create review request received";
    public const string InvalidReviewDataReceived = "Invalid review data received";
    public const string CorrectReviewInformationSent = "Correct review information sent";

    public const string FetchingReviewsFromRepository = "Fetching reviews from repository";
    public const string FetchedReviewsFromRepositorySuccessfully = "Fetching reviews from repository successfully";

    public const string GetReviewRequestReceived = "Get review request received for review: {ReviewId}";

    public const string FetchedReviewFromRepositorySuccessfully =
        "Fetched review from repository successfully for review: {ReviewId}";

    public const string UpdateReviewRequestReceived = "Update review request received for review: {ReviewId}";
    public const string RetrieveReviewSuccessfullyFromRepository = "Retrieve review with id: {ReviewId} successfully";

    public const string DeleteReviewRequestReceived = "Delete review request received for review: {ReviewId}";
    public const string ReviewDeletedSuccessfully = "Review deleted successfully for review: {ReviewId}";

    public const string ReturningReviewsFromCache = "Returning reviews from cache";
    public const string ReturningReviewFromCache = "Returning review from cache";
    public const string DeleteCachedData = "Delete cached data";
}