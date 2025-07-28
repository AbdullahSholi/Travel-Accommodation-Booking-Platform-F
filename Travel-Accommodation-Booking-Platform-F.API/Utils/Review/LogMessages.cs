namespace Travel_Accommodation_Booking_Platform_F.Utils.Review;

public static class LogMessages
{
    public const string CreateReviewRequestReceived = "Create Review request received";
    public const string CreateReviewFailed = "Create Review failed";
    public const string CreateReviewSuccess = "Registered Review success for Review: {ReviewId}";

    public const string GetReviewsRequestReceived = "Get Reviews request received";
    public const string GetReviewsFailed = "Get Reviews failed";
    public const string GetReviewsSuccess = "Get Reviews success";

    public const string GetReviewRequestReceived = "Get Review request received for Review with idn: {ReviewId}";
    public const string GetReviewFailed = "Get Review failed for Review with id: {ReviewId}";
    public const string GetReviewSuccess = "Get Review success for Review with id: {ReviewId}";

    public const string UpdateReviewRequestReceived = "Update Review request received for Review with id: {ReviewId}";
    public const string UpdateReviewFailed = "Update Review failed for Review with id: {ReviewId}";
    public const string ReviewUpdatedSuccessfully = "Updated successfully, for Review with id: {ReviewId}";

    public const string DeleteReviewRequestReceived = "Delete Review request received for Review with id: {ReviewId}";
    public const string DeleteReviewFailed = "Delete Review failed for Review with id: {ReviewId}";
    public const string ReviewDeletedSuccessfully = "Review deleted successfully for review with id: {ReviewId}";

    public const string CheckIfListOfReviewsNotUpdatedRecently = "Check if list of reviews not updated recently";
    public const string RetrievedDataFromBrowserCache = "Retrieved data from browser cache";

    public const string SendETagToClientWhenListOfReviewsUpdatedRecently =
        "Send ETag to client if list of reviews updated recently";

    public const string CheckIfReviewIsNotUpdatedRecently = "Check if review is not updated recently";
    public const string SendETagToClientWhenReviewUpdatedRecently = "Send ETag to client if review updated recently";

    public const string CheckIfUserTryUpdateTheLastVersionOfData =
        "Check if user try to update the last version of data";

    public const string UserTryUpdateOldVersionOfData = "User try to update the old version of data";
}