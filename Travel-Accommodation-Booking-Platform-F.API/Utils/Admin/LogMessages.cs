namespace Travel_Accommodation_Booking_Platform_F.Utils.Admin;

public static class LogMessages
{
    public const string CreateUserRequestReceived = "Create User request received for User: {Email}";
    public const string CreateUserFailed = "Create User failed for User: {Email}";
    public const string CreateUserSuccess = "Registered User success for User: {Email}";

    public const string GetUsersRequestReceived = "Get Users request received";
    public const string GetUsersFailed = "Get Users failed";
    public const string GetUsersSuccess = "Get Users success";

    public const string GetUserRequestReceived = "Get User request received for User with id: {UserId}";
    public const string GetUserFailed = "Get User failed for User with id: {UserId}";
    public const string GetUserSuccess = "Get User success for User with id: {UserId}";

    public const string UpdateUserRequestReceived = "Update User request received for User with id: {UserId}";
    public const string UpdateUserFailed = "Update User failed for User with id: {UserId}";
    public const string UserUpdatedSuccessfully = "Updated successfully, for user with id: {UserId}";

    public const string DeleteUserRequestReceived = "Delete User request received for User with id: {UserId}";
    public const string DeleteUserFailed = "Delete User failed for User with id: {UserId}";

    public const string CheckIfListOfUsersNotUpdatedRecently = "Check if list of users not updated recently";
    public const string RetrievedDataFromBrowserCache = "Retrieved data from browser cache";

    public const string SendETagToClientWhenListOfUsersUpdatedRecently =
        "Send ETag to client if list of users updated recently";

    public const string CheckIfUserIsNotUpdatedRecently = "Check if user is not updated recently";
    public const string SendETagToClientWhenUserUpdatedRecently = "Send ETag to client if user updated recently";

    public const string CheckIfUserTryUpdateTheLastVersionOfData =
        "Check if user try to update the last version of data";

    public const string UserTryUpdateOldVersionOfData = "User try to update the old version of data";
    public const string GetTopVisitedCitiesRequestReceived = "Get Top Visited Cities request received";
    public const string GetTopVisitedCitiesFailed = "Get Top Visited Cities failed";
    public const string GetTopVisitedCitiesSuccess = "Get Top Visited Cities success";

    public const string SendETagToClientWhenListOfTopVisitedCitiesUpdatedRecently =
        "Send ETag to client if list of top visited cities updated recently";

    public const string CheckIfListOfTopVisitedCitiesNotUpdatedRecently =
        "Check if list of top visited cities not updated";

    public const string SearchAboutRoomsRequestReceived = "Search about rooms request received";
    public const string SearchRoomsFailed = "Search rooms failed";
    public const string SearchRoomsSuccess = "Search rooms success";
}