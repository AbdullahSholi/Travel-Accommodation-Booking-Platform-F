using Microsoft.EntityFrameworkCore;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Domain.QueryDTOs;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly ApplicationDbContext _context;

    public AdminRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        var user = await _context.Users
            .Where(u => u.UserId == id).FirstOrDefaultAsync();

        return user;
    }

    public async Task<List<User>> GetAllAsync()
    {
        var users = await _context.Users
            .AsNoTracking()
            .ToListAsync();

        return users;
    }

    public async Task AddAsync(User user)
    {
        user.LastUpdated = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var isExist = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());

        return isExist;
    }

    public async Task<List<City>> GetTopVisitedCitiesAsync()
    {
        var topCityIds = await _context.Bookings
            .GroupBy(b => b.Room.Hotel.CityId)
            .Select(g => new
            {
                CityId = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .Select(g => g.CityId)
            .ToListAsync();

        var cities = await _context.Cities
            .Where(c => topCityIds.Contains(c.CityId))
            .AsNoTracking()
            .ToListAsync();

        cities = cities.OrderBy(c => topCityIds.IndexOf(c.CityId)).ToList();

        return cities;
    }

    public async Task<List<Room>> SearchRoomAsync(RoomQueryDto dto)
    {
        var query = _context.Rooms.AsQueryable();

        if (dto.RoomType.HasValue)
            query = query.Where(c => c.RoomType == dto.RoomType.Value);

        if (dto.MinPrice.HasValue)
            query = query.Where(c => c.PricePerNight >= dto.MinPrice.Value);

        if (dto.MaxPrice.HasValue)
            query = query.Where(c => c.PricePerNight <= dto.MaxPrice.Value);


        if (dto.IsAvailable.HasValue)
            query = query.Where(c => c.IsAvailable == dto.IsAvailable.Value);

        if (dto.AdultCapacity.HasValue)
            query = query.Where(c => c.AdultCapacity == dto.AdultCapacity.Value);

        if (dto.ChildrenCapacity.HasValue)
            query = query.Where(c => c.ChildrenCapacity == dto.ChildrenCapacity.Value);

        if (dto.CreatedAt.HasValue)
            query = query.Where(c => c.CreatedAt == dto.CreatedAt.Value);

        var rooms = await query.ToListAsync();

        return rooms;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}