using Microsoft.EntityFrameworkCore;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;

public class CityRepository : ICityRepository
{
    private readonly ApplicationDbContext _context;

    public CityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<City?> GetByIdAsync(int id)
    {
        var city = await _context.Cities
            .FirstOrDefaultAsync(h => h.CityId == id);

        return city;
    }

    public async Task<List<City>> GetAllAsync()
    {
        var cities = await _context.Cities
            .Include(h => h.Hotels)
            .AsNoTracking()
            .ToListAsync();

        return cities;
    }

    public async Task AddAsync(City city)
    {
        city.LastUpdated = DateTime.UtcNow;
        _context.Cities.Add(city);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(City city)
    {
        _context.Cities.Update(city);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(City city)
    {
        _context.Cities.Remove(city);
        await _context.SaveChangesAsync();
    }
}