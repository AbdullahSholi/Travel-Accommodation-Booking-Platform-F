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
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var room = await _context.Rooms
                .FromSqlInterpolated($"SELECT * FROM Rooms WITH (UPDLOCK) WHERE RoomId = {booking.RoomId}")
                .FirstOrDefaultAsync();
            if (room == null)
                throw new InvalidOperationException(CustomMessages.CustomMessages.RoomNotFound);

            if (booking.CheckOutDate <= booking.CheckInDate)
                throw new InvalidOperationException(CustomMessages.CustomMessages.CheckoutMessage);

            var isOverlapping = await _context.Bookings.AnyAsync(r =>
                r.RoomId == booking.RoomId &&
                booking.CheckInDate < r.CheckOutDate &&
                booking.CheckOutDate > r.CheckInDate
            );

            if (isOverlapping)
                throw new InvalidOperationException(CustomMessages.CustomMessages.RoomAlreadyBooked);

            var totalDays = (int)(booking.CheckOutDate - booking.CheckInDate).TotalDays;
            booking.TotalPrice = room.PricePerNight * totalDays;
            booking.LastUpdated = DateTime.UtcNow;

            _context.Bookings.Add(booking);
            room.IsAvailable = false;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(Booking booking)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var room = await _context.Rooms
                .FromSqlInterpolated($"SELECT * FROM Rooms WITH (UPDLOCK) WHERE RoomId = {booking.RoomId}")
                .FirstOrDefaultAsync();

            if (room == null)
                throw new InvalidOperationException(CustomMessages.CustomMessages.RoomNotFound);

            var isOverlapping = await _context.Bookings.AnyAsync(r =>
                r.RoomId == booking.RoomId &&
                booking.CheckInDate < r.CheckOutDate &&
                booking.CheckOutDate > r.CheckInDate
            );

            if (isOverlapping)
                throw new InvalidOperationException(CustomMessages.CustomMessages.RoomAlreadyBooked);

            var totalDays = (int)(booking.CheckOutDate - booking.CheckInDate).TotalDays;
            booking.TotalPrice = room.PricePerNight * totalDays;
            booking.LastUpdated = DateTime.UtcNow;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(Booking booking)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var room = await _context.Rooms
                .FromSqlInterpolated($"SELECT * FROM Rooms WITH (UPDLOCK) WHERE RoomId = {booking.RoomId}")
                .FirstOrDefaultAsync();

            if (room == null)
                throw new InvalidOperationException(CustomMessages.CustomMessages.RoomNotFound);

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}