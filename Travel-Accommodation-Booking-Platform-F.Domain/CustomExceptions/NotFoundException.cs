namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}