namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.AdminExceptions;

public class InvalidUserDataReceivedException : Exception
{
    public InvalidUserDataReceivedException(string message) : base(message)
    {
    }
}