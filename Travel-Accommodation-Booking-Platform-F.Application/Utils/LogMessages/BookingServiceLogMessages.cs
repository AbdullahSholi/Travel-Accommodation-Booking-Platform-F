namespace Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;

public class BookingServiceLogMessages
{
    public const string CreateBookingRequestReceived = "Create booking request received";
    public const string InvalidBookingDataReceived = "Invalid booking data received";
    public const string CorrectBookingInformationSent = "Correct booking information sent";

    public const string FetchingBookingsFromRepository = "Fetching bookings from repository";
    public const string FetchedBookingsFromRepositorySuccessfully = "Fetching bookings from repository successfully";

    public const string GetBookingRequestReceived = "Get booking request received for booking: {BookingId}";

    public const string FetchedBookingFromRepositorySuccessfully =
        "Fetched booking from repository successfully for booking: {BookingId}";

    public const string UpdateBookingRequestReceived = "Update booking request received for booking: {BookingId}";
    public const string RetrieveBookingSuccessfullyFromRepository = "Retrieve booking with id: {BookingId} successfully";

    public const string DeleteBookingRequestReceived = "Delete booking request received for booking: {BookingId}";
    public const string BookingDeletedSuccessfully = "Booking deleted successfully for booking: {BookingId}";

    public const string ReturningBookingsFromCache = "Returning bookings from cache";
    public const string ReturningBookingFromCache = "Returning booking from cache";
    public const string DeleteCachedData = "Delete cached data";
}