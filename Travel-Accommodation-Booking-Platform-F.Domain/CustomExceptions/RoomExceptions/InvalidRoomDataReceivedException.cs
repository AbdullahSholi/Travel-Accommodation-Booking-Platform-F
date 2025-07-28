namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.RoomExceptions;

public class InvalidRoomDataReceivedException : Exception
{
    public InvalidRoomDataReceivedException(string message) : base(message)
    {
    }
}