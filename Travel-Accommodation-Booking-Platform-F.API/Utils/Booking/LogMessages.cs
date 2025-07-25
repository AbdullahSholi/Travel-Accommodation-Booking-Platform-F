namespace Travel_Accommodation_Booking_Platform_F.Utils.Booking;

public static class LogMessages
{
    public const string CreateBookingRequestReceived = "Create Booking request received";
    public const string CreateBookingFailed = "Create Booking failed";
    public const string CreateBookingSuccess = "Registered Booking success for Booking: {BookingId}";

    public const string GetBookingsRequestReceived = "Get Bookings request received";
    public const string GetBookingsFailed = "Get Bookings failed";
    public const string GetBookingsSuccess = "Get Bookings success";

    public const string GetBookingRequestReceived = "Get Booking request received for Booking with idn: {BookingId}";
    public const string GetBookingFailed = "Get Booking failed for Booking with id: {BookingId}";
    public const string GetBookingSuccess = "Get Booking success for Booking with id: {BookingId}";

    public const string UpdateBookingRequestReceived = "Update Booking request received for Booking with id: {BookingId}";
    public const string UpdateBookingFailed = "Update Booking failed for Booking with id: {BookingId}";
    public const string BookingUpdatedSuccessfully = "Updated successfully, for Booking with id: {BookingId}";

    public const string DeleteBookingRequestReceived = "Delete Booking request received for Booking with id: {BookingId}";
    public const string DeleteBookingFailed = "Delete Booking failed for Booking with id: {BookingId}";
    public const string BookingDeletedSuccessfully = "Booking deleted successfully for booking with id: {BookingId}";

    public const string CheckIfListOfBookingsNotUpdatedRecently = "Check if list of bookings not updated recently";
    public const string RetrievedDataFromBrowserCache = "Retrieved data from browser cache";

    public const string SendETagToClientWhenListOfBookingsUpdatedRecently =
        "Send ETag to client if list of bookings updated recently";

    public const string CheckIfBookingIsNotUpdatedRecently = "Check if booking is not updated recently";
    public const string SendETagToClientWhenBookingUpdatedRecently = "Send ETag to client if booking updated recently";

    public const string CheckIfUserTryUpdateTheLastVersionOfData =
        "Check if user try to update the last version of data";

    public const string UserTryUpdateOldVersionOfData = "User try to update the old version of data";
}