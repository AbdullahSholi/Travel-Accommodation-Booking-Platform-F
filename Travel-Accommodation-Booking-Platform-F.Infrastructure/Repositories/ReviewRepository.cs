using Microsoft.EntityFrameworkCore;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _context;

    public ReviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ReviewId == id);

        return review;
    }

    public async Task<List<Review>> GetAllAsync()
    {
        var reviews = await _context.Reviews
            .ToListAsync();

        return reviews;
    }

    public async Task AddAsync(Review review)
    {
        review.LastUpdated = DateTime.UtcNow;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Review review)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Review review)
    {
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }
}