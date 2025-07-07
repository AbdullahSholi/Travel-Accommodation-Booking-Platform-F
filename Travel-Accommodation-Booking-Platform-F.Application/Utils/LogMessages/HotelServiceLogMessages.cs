namespace Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;

public static class HotelServiceLogMessages
{
    public const string CreateHotelRequestReceived = "Create hotel request received";
    public const string InvalidHotelDataReceived = "Invalid hotel data received";
    public const string CorrectHotelInformationSent = "Correct hotel information sent";

    public const string FetchingHotelsFromRepository = "Fetching hotels from repository";
    public const string FetchedHotelsFromRepositorySuccessfully = "Fetching hotels from repository successfully";

    public const string GetHotelRequestReceived = "Get hotel request received for hotel: {HotelId}";

    public const string FetchedHotelFromRepositorySuccessfully =
        "Fetched hotel from repository successfully for hotel: {HotelId}";

    public const string UpdateHotelRequestReceived = "Update hotel request received for hotel: {HotelId}";
    public const string RetrieveHotelSuccessfullyFromRepository = "Retrieve hotel with id: {HotelId} successfully";

    public const string DeleteHotelRequestReceived = "Delete hotel request received for hotel: {HotelId}";
    public const string HotelDeletedSuccessfully = "Hotel deleted successfully for hotel: {HotelId}";

    public const string ReturningHotelsFromCache = "Returning hotels from cache";
    public const string ReturningHotelFromCache = "Returning hotel from cache";
}