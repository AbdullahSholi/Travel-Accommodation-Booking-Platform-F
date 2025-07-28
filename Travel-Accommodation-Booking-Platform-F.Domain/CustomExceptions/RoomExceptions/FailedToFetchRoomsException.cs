namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.RoomExceptions;

public class FailedToFetchRoomsException : Exception
{
    public FailedToFetchRoomsException(string message) : base(message)
    {
    }
}