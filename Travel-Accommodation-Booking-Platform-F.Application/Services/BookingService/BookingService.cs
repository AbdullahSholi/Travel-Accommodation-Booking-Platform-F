using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.BookingExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.BookingService;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;
    private readonly IMemoryCache _memoryCache;

    private const string BookingsCacheKey = "bookings-list";

    public BookingService(IBookingRepository bookingRepository, IMapper mapper, ILogger<BookingService> logger,
        IMemoryCache memoryCache)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<BookingReadDto?> CreateBookingAsync(BookingWriteDto dto)
    {
        _logger.LogInformation(BookingServiceLogMessages.CreateBookingRequestReceived);

        if (dto == null)
        {
            _logger.LogWarning(BookingServiceLogMessages.InvalidBookingDataReceived);
            throw new InvalidBookingDataReceivedException(BookingServiceCustomMessages.InvalidBookingDataReceived);
        }

        _logger.LogInformation(BookingServiceLogMessages.CorrectBookingInformationSent);

        var booking = _mapper.Map<Booking>(dto);
        
        booking.CheckInDate = DateTime.Now;
        await _bookingRepository.AddAsync(booking);

        _logger.LogInformation(BookingServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(BookingsCacheKey);
        _memoryCache.Remove(GetBookingCacheKey(booking.BookingId));

        var bookingReadDto = _mapper.Map<BookingReadDto>(booking);
        return bookingReadDto;
    }

    public async Task<List<BookingReadDto>?> GetBookingsAsync()
    {
        _logger.LogInformation(BookingServiceLogMessages.FetchingBookingsFromRepository);

        if (_memoryCache.TryGetValue(BookingsCacheKey, out List<BookingReadDto> cachedBookings))
        {
            _logger.LogInformation(BookingServiceLogMessages.ReturningBookingsFromCache);
            return cachedBookings;
        }

        var bookings = await _bookingRepository.GetAllAsync();
        if (bookings == null)
        {
            _logger.LogWarning(BookingServiceCustomMessages.FailedFetchingBookingsFromRepository);
            throw new FailedToFetchBookingsException(BookingServiceCustomMessages.FailedFetchingBookingsFromRepository);
        }

        _logger.LogInformation(BookingServiceLogMessages.FetchedBookingsFromRepositorySuccessfully);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(Constants.AbsoluteExpirationForRetrieveBookingsMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(Constants.SlidingExpirationMinutes))
            .SetSize(Constants.CachingUnitSize);

        var bookingsReadDto = _mapper.Map<List<BookingReadDto>>(bookings);

        _memoryCache.Set(BookingsCacheKey, bookingsReadDto, cacheEntryOptions);
        return bookingsReadDto;
    }

    public async Task<BookingReadDto?> GetBookingAsync(int bookingId)
    {
        _logger.LogInformation(BookingServiceLogMessages.GetBookingRequestReceived, bookingId);

        var bookingCacheKey = GetBookingCacheKey(bookingId);

        if (_memoryCache.TryGetValue(bookingCacheKey, out BookingReadDto cachedBooking))
        {
            _logger.LogInformation(BookingServiceLogMessages.ReturningBookingFromCache);
            return cachedBooking;
        }

        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null) return null;

        _logger.LogInformation(BookingServiceLogMessages.FetchedBookingFromRepositorySuccessfully, bookingId);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(Constants.AbsoluteExpirationForRetrieveBookingsMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(Constants.SlidingExpirationMinutes))
            .SetSize(Constants.CachingUnitSize);

        var bookingReadDto = _mapper.Map<BookingReadDto>(booking);

        _memoryCache.Set(bookingCacheKey, bookingReadDto, cacheEntryOptions);
        return bookingReadDto;
    }

    public async Task<BookingReadDto?> UpdateBookingAsync(int bookingId, BookingPatchDto dto)
    {
        _logger.LogInformation(BookingServiceLogMessages.UpdateBookingRequestReceived, bookingId);

        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null) return null;

        _logger.LogInformation(BookingServiceLogMessages.RetrieveBookingSuccessfullyFromRepository, bookingId);

        booking.RoomId = dto.RoomId ?? booking.RoomId;
        booking.CheckInDate = dto.CheckInDate ?? booking.CheckInDate;
        booking.CheckOutDate = dto.CheckOutDate ?? booking.CheckOutDate;
        booking.TotalPrice = dto.TotalPrice ?? booking.TotalPrice;
        booking.LastUpdated = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking);

        _logger.LogInformation(BookingServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(BookingsCacheKey);
        _memoryCache.Remove(GetBookingCacheKey(bookingId));

        var bookingReadDto = _mapper.Map<BookingReadDto>(booking);
        return bookingReadDto;
    }

    public async Task DeleteBookingAsync(int bookingId)
    {
        _logger.LogInformation(BookingServiceLogMessages.DeleteBookingRequestReceived, bookingId);

        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null) return;
        _logger.LogInformation(BookingServiceLogMessages.RetrieveBookingSuccessfullyFromRepository, bookingId);

        await _bookingRepository.DeleteAsync(booking);
        _logger.LogInformation(BookingServiceLogMessages.BookingDeletedSuccessfully, bookingId);

        _logger.LogInformation(CityServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(BookingsCacheKey);
        _memoryCache.Remove(GetBookingCacheKey(bookingId));
    }

    private string GetBookingCacheKey(int bookingId)
    {
        return $"booking_{bookingId}";
    }
}