namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.CustomMessages;

public static class CustomMessages
{
    public const string UnSetAppPassword = "APP_PASSWORD environment variable is not set.";
    public const string YourOtpCode = "Your OTP Code";

    public const string NonSetConnectionStringAtEnvironmentVariables =
        "Environment variable 'TRAVEL_ACCOMMODATION_CONNECTION_STRING' is not set.";

    public const string RoomNotFound = "Room not found.";
    public const string CheckoutMessage = "Check-out date must be after check-in date.";
    public const string RoomAlreadyBooked = "The room is already booked during the selected dates.";
}