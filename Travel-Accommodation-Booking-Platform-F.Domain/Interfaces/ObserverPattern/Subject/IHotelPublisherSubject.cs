using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Observer;

namespace Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Subject;

public interface IHotelPublisherSubject
{
    void AddObserver(INotifyUsersObserver observer);
    void RemoveObserver(INotifyUsersObserver observer);
    Task NotifyObserversAsync(Hotel hotel);
}