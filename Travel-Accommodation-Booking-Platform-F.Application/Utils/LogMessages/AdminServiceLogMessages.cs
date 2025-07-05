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
    public const string FailedGetUserFromRepository = "Failed to get user from repository for user: {UserId}";
    public const string FetchedUserFromRepositorySuccessfully = "Fetched user from repository successfully for user: {UserId}";
    
    public const string UpdateUserRequestReceived = "Update user request received for user: {UserId}";
    public const string RetrieveUserSuccessfullyFromRepository = "Retrieve user with id: {UserId} successfully";
    
    public const string DeleteUserRequestReceived = "Delete user request received for user: {UserId}";
    public const string UserDeletedSuccessfully = "User deleted successfully for user: {UserId}";
}