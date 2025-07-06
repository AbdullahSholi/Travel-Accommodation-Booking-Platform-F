namespace Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;

public static class CityServiceLogMessages
{
    public const string CreateCityRequestReceived = "Create city request received";
    public const string InvalidCityDataReceived = "Invalid city data received";
    public const string CorrectCityInformationSent = "Correct city information sent";

    public const string FetchingCitiesFromRepository = "Fetching cities from repository";
    public const string FetchedCitiesFromRepositorySuccessfully = "Fetching cities from repository successfully";

    public const string GetCityRequestReceived = "Get city request received for city: {CityId}";
    public const string FailedGetCityFromRepository = "Failed to get city from repository for city: {CityId}";

    public const string FetchedCityFromRepositorySuccessfully =
        "Fetched city from repository successfully for city: {CityId}";

    public const string UpdateCityRequestReceived = "Update city request received for city: {CityId}";
    public const string RetrieveCitySuccessfullyFromRepository = "Retrieve city with id: {CityId} successfully";

    public const string DeleteCityRequestReceived = "Delete city request received for city: {CityId}";
    public const string CityDeletedSuccessfully = "City deleted successfully for city: {CityId}";
}