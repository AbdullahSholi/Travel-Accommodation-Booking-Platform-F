using Travel_Accommodation_Booking_Platform_F.Domain.Entities;

namespace Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Observer;

public interface INotifyUsersObserver
{
    public Task SendHotelAnnouncementAsync(Hotel hotel);
}