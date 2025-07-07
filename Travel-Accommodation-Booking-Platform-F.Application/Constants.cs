namespace Travel_Accommodation_Booking_Platform_F.Application;

public static class Constants
{
    public const int OtpExpirationMinutes = 5;
    public const int AbsoluteExpirationForRetrieveUsersMinutes = 20;
    public const int AbsoluteExpirationForRetrieveRoomsMinutes = 20;
    public const int AbsoluteExpirationForRetrieveHotelsMinutes = 40;
    public const int AbsoluteExpirationForRetrieveCitiesMinutes = 60;
    public const int AbsoluteExpirationForRetrieveReviewsMinutes = 60;

    public const int SlidingExpirationMinutes = 60;
    public const int CachingUnitSize = 1;
}