namespace Travel_Accommodation_Booking_Platform_F.Utils.Auth;

public static class LogMessages
{
    public const string LoginRequestReceived = "Login request received for user: {Email}";
    public const string LoginSuccessful = "Login successful for user: {Email}";
    public const string LoginValidationFailed = "Validation failed during login for user: {Email}";
    public const string LoginUserNotFound = "User not found during login attempt: {Email}";
    public const string LoginUnexpectedError = "Unexpected error during login for user: {Email}";

    public const string RegisterRequestReceived = "Registration request received for user: {Email}";
    public const string RegisterSuccessful = "User registered successfully: {Email}";
    public const string RegisterFailed = "Registration failed for user: {Email}";
    public const string RegisterUnexpectedError = "Unexpected error during registration for user: {Email}";

    public const string ForgotPasswordRequestReceived = "Forgot password request received for user: {Email}";
    public const string ForgotPasswordOtpSent = "OTP sent successfully for password reset to user: {Email}";
    public const string ForgotPasswordFailedToSent = "OTP sent failed for password reset to user: {Email}";
    public const string ForgotPasswordUnexpectedError = "Unexpected error during forgot password for user: {Email}";

    public const string ResetPasswordRequestReceived = "Reset password request received for user: {Email}";
    public const string ResetPasswordSuccessful = "Password reset successfully for user: {Email}";
    public const string ResetPasswordFailed = "Failed to reset password for user: {Email}";
    public const string ResetPasswordUnexpectedError = "Unexpected error during password reset for user: {Email}";

    public const string VerifyOtpRequestReceived = "OTP verification request received for user: {Email}";
    public const string VerifyOtpSuccessful = "OTP verified successfully for user: {Email}";
    public const string VerifyOtpFailed = "OTP verification failed for user: {Email}";
    public const string VerifyOtpUnexpectedError = "Unexpected error during OTP verification for user: {Email}";

    public const string LogoutSuccessful = "User logged out successfully";
    public const string LogoutInvalidToken = "Logout attempt with invalid token";
    public const string LogoutUnexpectedError = "Unexpected error during logout";
}