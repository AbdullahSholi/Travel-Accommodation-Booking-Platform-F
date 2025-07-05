namespace Travel_Accommodation_Booking_Platform_F.Utils.Admin;

public static class LogMessages
{
    public const string CreateUserRequestReceived = "Create User request received for User: {Email}";
    public const string CreateUserFailed = "Create User failed for User: {Email}";
    public const string CreateUserSuccess = "Registered User success for User: {Email}";
    public const string InvalidUserDataReceivedFromRequest = "Invalid User data received from request";
    
    public const string GetUsersRequestReceived = "Get Users request received";
    public const string GetUsersFailed = "Get Users failed";
    public const string GetUsersSuccess = "Get Users success";
    
    public const string GetUserRequestReceived = "Get User request received for User with id: {UserId}";
    public const string GetUserFailed = "Get User failed for User with id: {UserId}";
    public const string GetUserSuccess  = "Get User success for User with id: {UserId}";
    
    public const string UpdateUserRequestReceived = "Update User request received for User with id: {UserId}";
    public const string UpdateUserFailed = "Update User failed for User with id: {UserId}";
    public const string UserUpdatedSuccessfully = "Updated successfully, for user with id: {UserId}";
    
    public const string DeleteUserRequestReceived = "Delete User request received for User with id: {UserId}";
    public const string DeleteUserFailed = "Delete User failed for User with id: {UserId}";
}