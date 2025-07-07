namespace Travel_Accommodation_Booking_Platform_F.Utils.Hotel;

public static class LogMessages
{
    public const string CreateHotelRequestReceived = "Create Hotel request received";
    public const string CreateHotelFailed = "Create Hotel failed";
    public const string CreateHotelSuccess = "Registered Hotel success for Hotel: {HotelId}";

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
    public const string HotelDeletedSuccessfully = "Hotel deleted successfully for hotel with id: {HotelId}";

    public const string CheckIfListOfHotelsNotUpdatedRecently = "Check if list of hotels not updated recently";
    public const string RetrievedDataFromBrowserCache = "Retrieved data from browser cache";

    public const string SendETagToClientWhenListOfHotelsUpdatedRecently =
        "Send ETag to client if list of hotels updated recently";

    public const string CheckIfHotelIsNotUpdatedRecently = "Check if hotel is not updated recently";
    public const string SendETagToClientWhenHotelUpdatedRecently = "Send ETag to client if hotel updated recently";

    public const string CheckIfUserTryUpdateTheLastVersionOfData =
        "Check if user try to update the last version of data";

    public const string UserTryUpdateOldVersionOfData = "User try to update the old version of data";
}