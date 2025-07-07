using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Observer;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Subject;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;

public class HotelPublisherSubject : IHotelPublisherSubject
{
    private readonly List<INotifyUsersObserver> _observers = new();

    public void AddObserver(INotifyUsersObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void RemoveObserver(INotifyUsersObserver observer)
    {
        if (_observers.Contains(observer))
            _observers.Remove(observer);
    }

    public async Task NotifyObserversAsync(Hotel hotel)
    {
        foreach (var observer in _observers)
        {
            await observer.SendHotelAnnouncementAsync(hotel);
        }
    }
}