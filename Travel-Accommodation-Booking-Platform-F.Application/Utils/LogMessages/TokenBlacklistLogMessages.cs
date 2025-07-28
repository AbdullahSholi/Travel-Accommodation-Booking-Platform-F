namespace Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;

public static class TokenBlacklistLogMessages
{
    public const string CheckTokenBlacklistStatus = "Checking if token with JTI: {Jti} is blacklisted.";
    public const string TokenIsBlacklisted = "Token with JTI: {Jti} is blacklisted.";
    public const string TokenIsNotBlacklisted = "Token with JTI: {Jti} is not blacklisted.";
    public const string AddingTokenToBlacklist = "Adding token with JTI: {Jti} to blacklist, expires at {Expiration}.";
    public const string TokenBlacklistedSuccessfully = "Token with JTI: {Jti} successfully added to blacklist.";
    public const string InvalidJtiProvidedForBlacklistCheck = "Invalid or empty JTI provided for blacklist check.";

    public const string InvalidJtiProvidedForBlacklistAddition =
        "Invalid or empty JTI provided for blacklist addition.";

    public const string InvalidExpirationDateProvided =
        "Invalid expiration date '{1}' provided for token with JTI '{0}'.";
}