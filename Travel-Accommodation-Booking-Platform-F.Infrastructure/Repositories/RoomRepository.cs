using Microsoft.EntityFrameworkCore;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _context;

    public RoomRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(h => h.RoomId == id);

        return room;
    }

    public async Task<List<Room>> GetAllAsync()
    {
        var rooms = await _context.Rooms
            .AsNoTracking()
            .ToListAsync();

        return rooms;
    }

    public async Task AddAsync(Room room)
    {
        room.LastUpdated = DateTime.UtcNow;
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Room room)
    {
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }
}