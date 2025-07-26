namespace Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;

public static class AdminServiceLogMessages
{
    public const string CreateUserRequestReceived = "Create user request received";
    public const string InvalidUserDataReceived = "Invalid user data received";
    public const string CorrectUserInformationSent = "Correct user information sent";
    public const string EmailAlreadyExists = "Email already exists with the given email: {Email}";

    public const string FetchingUsersFromRepository = "Fetching users from repository";
    public const string FetchedUsersFromRepositorySuccessfully = "Fetching users from repository successfully";

    public const string GetUserRequestReceived = "Get user request received for user: {UserId}";

    public const string FetchedUserFromRepositorySuccessfully =
        "Fetched user from repository successfully for user: {UserId}";

    public const string UpdateUserRequestReceived = "Update user request received for user: {UserId}";
    public const string RetrieveUserSuccessfullyFromRepository = "Retrieve user with id: {UserId} successfully";

    public const string DeleteUserRequestReceived = "Delete user request received for user: {UserId}";
    public const string UserDeletedSuccessfully = "User deleted successfully for user: {UserId}";

    public const string ReturningUsersFromCache = "Returning users from cache";
    public const string ReturningUserFromCache = "Returning user from cache";
    public const string DeleteCachedData = "Delete cached data";
    
    public const string FetchingCitiesFromRepository = "Fetching cities from repository";
    public const string ReturningCitiesFromCache  = "Returning cities from cache";
    public const string FailedFetchingCitiesFromRepository = "Failed fetching cities from repository";
    public const string FetchedCitiesFromRepositorySuccessfully = $"Fetched cities from repository successfully";
}