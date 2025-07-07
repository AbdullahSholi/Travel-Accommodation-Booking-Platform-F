namespace Travel_Accommodation_Booking_Platform_F.Utils.City;

public static class LogMessages
{
    public const string CreateCityRequestReceived = "Create City request received";
    public const string CreateCityFailed = "Create City failed";
    public const string CreateCitySuccess = "Registered City success for City: {CityId}";

    public const string GetCitiesRequestReceived = "Get Cities request received";
    public const string GetCitiesFailed = "Get Cities failed";
    public const string GetCitiesSuccess = "Get Cities success";

    public const string GetCityRequestReceived = "Get City request received for City with idn: {CityId}";
    public const string GetCityFailed = "Get City failed for City with id: {CityId}";
    public const string GetCitySuccess = "Get City success for City with id: {CityId}";

    public const string UpdateCityRequestReceived = "Update City request received for City with id: {CityId}";
    public const string UpdateCityFailed = "Update City failed for City with id: {CityId}";
    public const string CityUpdatedSuccessfully = "Updated successfully, for City with id: {CityId}";

    public const string DeleteCityRequestReceived = "Delete City request received for City with id: {CityId}";
    public const string DeleteCityFailed = "Delete City failed for City with id: {CityId}";
    public const string CityDeletedSuccessfully = "City deleted successfully for City with id: {CityId}";


    public const string CheckIfListOfCitiesNotUpdatedRecently = "Check if list of cities not updated recently";
    public const string RetrievedDataFromBrowserCache = "Retrieved data from browser cache";

    public const string SendETagToClientWhenListOfCitiesUpdatedRecently =
        "Send ETag to client if list of cities updated recently";

    public const string CheckIfCityIsNotUpdatedRecently = "Check if city is not updated recently";
    public const string SendETagToClientWhenCityUpdatedRecently = "Send ETag to client if city updated recently";

    public const string CheckIfUserTryUpdateTheLastVersionOfData =
        "Check if user try to update the last version of data";

    public const string UserTryUpdateOldVersionOfData = "User try to update the old version of data";
}