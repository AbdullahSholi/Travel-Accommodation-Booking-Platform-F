namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.RoomExceptions;

public class FailedToFetchRoomException : Exception
{
    public int RoomId { get; }

    public FailedToFetchRoomException(string message, int roomId) : base(message)
    {
        RoomId = roomId;
    }
}