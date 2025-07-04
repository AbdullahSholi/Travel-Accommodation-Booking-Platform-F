namespace Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;

public static class AuthServiceLogMessages
{
    // Login-related
    public const string LoginRequestReceived = "Login request received.";
    public const string LoginMissingEmailAndUsername = "Login failed: both email and username are missing.";
    public const string FetchingUserFromRepository = "Fetching user from repository using {CredentialType}.";
    public const string UserNotFound = "Login failed: no user found with provided {Credential}.";
    public const string InvalidPasswordAttempt = "Invalid password attempt for userId: {UserId}.";
    public const string PasswordVerified = "Password verified successfully for userId: {UserId}.";
    public const string TokenGenerated = "JWT token generated for userId: {UserId}.";

    // Registration-related
    public const string RegistrationRequestReceived = "Registration request received for email: {Email}.";
    public const string DeletedExpiredUnconfirmedUsers = "Expired unconfirmed users deleted.";
    public const string EmailAlreadyExists = "Registration failed: email {Email} already exists.";
    public const string UserRegistered = "User registered successfully with email: {Email}.";
    public const string OtpSaved = "OTP record saved for email: {Email}.";
    public const string OtpSent = "OTP sent to email: {Email}.";
    public const string RegistrationCompleted = "Registration completed for email: {Email}.";

    // OTP-related
    public const string SendOtpRequestReceived = "OTP send request received for email: {Email}.";
    public const string UserNotFoundForOtp = "OTP send failed: no user found with email: {Email}.";
    public const string VerifyOtpRequestReceived = "OTP verification request received for email: {Email}.";
    public const string OtpNotFound = "OTP verification failed: OTP not found for email: {Email}.";
    public const string ExpiredOtpCodeUsed = "OTP verification failed: OTP expired for email: {Email}.";
    public const string InvalidOtpUsed = "OTP verification failed: OTP does not match for email: {Email}.";
    public const string UserNotFoundForOtpVerification = "OTP verification failed: user not found for email: {Email}.";
    public const string UserEmailConfirmed = "Email confirmed successfully for user: {Email}.";
    public const string OtpInvalidated = "OTP invalidated after successful verification for email: {Email}.";

    // Password reset-related
    public const string ResetPasswordRequestReceived = "Reset password request received for email: {Email}.";

    public const string InvalidOrExpiredOtpAttempt =
        "Password reset failed: invalid or expired OTP for email: {Email}.";
    
    public const string UserNotFoundForReset = "Password reset failed: user not found for email: {Email}.";
    public const string PasswordResetSuccessfully = "Password reset successfully for email: {Email}.";
    public const string OtpRemovedAfterReset = "OTP removed after password reset for email: {Email}.";
}