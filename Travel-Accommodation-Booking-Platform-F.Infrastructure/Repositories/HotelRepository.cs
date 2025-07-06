using Microsoft.EntityFrameworkCore;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;

public class HotelRepository : IHotelRepository
{
    private readonly ApplicationDbContext _context;

    public HotelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Hotel?> GetByIdAsync(int id)
    {
        var hotel = await _context.Hotels
            .FirstOrDefaultAsync(h => h.HotelId == id);

        return hotel;
    }

    public async Task<List<Hotel>> GetAllAsync()
    {
        var hotels = await _context.Hotels
            .Include(h => h.Rooms)
            .Include(h => h.City)
            .ToListAsync();

        return hotels;
    }

    public async Task AddAsync(Hotel hotel)
    {
        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Hotel hotel)
    {
        _context.Hotels.Update(hotel);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Hotel hotel)
    {
        _context.Hotels.Remove(hotel);
        await _context.SaveChangesAsync();
    }
}