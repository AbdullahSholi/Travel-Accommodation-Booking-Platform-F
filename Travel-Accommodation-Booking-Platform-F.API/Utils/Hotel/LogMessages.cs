namespace Travel_Accommodation_Booking_Platform_F.Utils.Hotel;

public static class LogMessages
{
    public const string CreateHotelRequestReceived = "Create Hotel request received";
    public const string CreateHotelFailed = "Create Hotel failed";
    public const string CreateHotelSuccess = "Registered Hotel success for Hotel: {HotelId}";
    public const string InvalidHotelDataReceivedFromRequest = "Invalid Hotel data received from request";

    public const string GetHotelsRequestReceived = "Get Hotels request received";
    public const string GetHotelsFailed = "Get Hotels failed";
    public const string GetHotelsSuccess = "Get Hotels success";

    public const string GetHotelRequestReceived = "Get Hotel request received for Hotel with idn: {HotelId}";
    public const string GetHotelFailed = "Get Hotel failed for Hotel with id: {HotelId}";
    public const string GetHotelSuccess = "Get Hotel success for Hotel with id: {HotelId}";

    public const string UpdateHotelRequestReceived = "Update Hotel request received for Hotel with id: {HotelId}";
    public const string UpdateHotelFailed = "Update Hotel failed for Hotel with id: {HotelId}";
    public const string HotelUpdatedSuccessfully = "Updated successfully, for Hotel with id: {HotelId}";

    public const string DeleteHotelRequestReceived = "Delete Hotel request received for Hotel with id: {HotelId}";
    public const string DeleteHotelFailed = "Delete Hotel failed for Hotel with id: {HotelId}";
}