namespace Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.AdminExceptions;

public class DuplicatedEmailException : Exception
{
    public DuplicatedEmailException(string message) : base(message)
    {
    }
}