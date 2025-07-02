namespace Travel_Accommodation_Booking_Platform_F.Utils;

public static class CustomMessages
{
    public const string UnSetSecretKey = "SECRET_KEY environment variable is not set.";

    public const string UnSetConnectionString =
        "TRAVEL_ACCOMMODATION_CONNECTION_STRING environment variable is not set.";

    public const string TokenIsBlacklisted = "Token is blacklisted.";
    public const string HelloMessage = "Hello from Travel Accommodation API!";
    public const string InvalidCredentials = "Invalid credentials.";
    public const string EmailSentSuccessfully = "Email sent successfully.";
    public const string PasswordResetSuccessfully = "Password reset successfully.";
    public const string InvalidToken = "Invalid token";
    public const string LoggedOutSuccessfully = "Logged out successfully";
    public const string InternalServerError = "Internal Server Error";
    public const string EmailVerifiedSuccessfully = "Email verified successfully";
    public const string EmailVerificationFailed = "Email verification failed";
    public const string FailedToResetPassword = "Failed to reset password";
}