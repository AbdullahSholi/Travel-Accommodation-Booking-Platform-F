using Microsoft.EntityFrameworkCore;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _context;

    public BookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(r => r.BookingId == id);

        return booking;
    }

    public async Task<List<Booking>> GetAllAsync()
    {
        var bookings = await _context.Bookings
            .ToListAsync();

        return bookings;
    }

    public async Task AddAsync(Booking booking)
    {
        booking.LastUpdated = DateTime.UtcNow;
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Booking booking)
    {
        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
    }
}