namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions;

public class ValidationAppException : Exception
{
    public ValidationAppException(string message) : base(message) { }
}