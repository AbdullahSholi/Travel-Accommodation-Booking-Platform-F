using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.ReviewService;

public interface IReviewService
{
    public Task<ReviewReadDto?> CreateReviewAsync(ReviewWriteDto dto);
    public Task<List<ReviewReadDto>?> GetReviewsAsync();
    public Task<ReviewReadDto?> GetReviewAsync(int reviewId);
    public Task<ReviewReadDto?> UpdateReviewAsync(int id, ReviewPatchDto dto);
    public Task DeleteReviewAsync(int reviewId);
}