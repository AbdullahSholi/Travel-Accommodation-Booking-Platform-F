using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.BookingService;

public interface IBookingService
{
    public Task<BookingReadDto?> CreateBookingAsync(BookingWriteDto dto);
    public Task<List<BookingReadDto>?> GetBookingsAsync();
    public Task<BookingReadDto?> GetBookingAsync(int bookingId);
    public Task<BookingReadDto?> UpdateBookingAsync(int id, BookingPatchDto dto);
    public Task DeleteBookingAsync(int bookingId);
}