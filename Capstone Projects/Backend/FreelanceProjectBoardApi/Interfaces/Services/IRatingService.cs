using FreelanceProjectBoardApi.DTOs.Ratings;
using System.Collections.Generic;

namespace FreelanceProjectBoardApi.Services.Interfaces
{
    public interface IRatingService
    {
        Task<RatingResponseDto?> CreateRatingAsync(Guid raterId, CreateRatingDto createDto);
        Task<RatingResponseDto?> GetRatingByIdAsync(Guid id);
        Task<IEnumerable<RatingResponseDto>> GetRatingsReceivedByUserIdAsync(Guid userId);
        Task<IEnumerable<RatingResponseDto>> GetRatingsGivenByUserIdAsync(Guid userId);
        Task<double> GetAverageRatingForUserAsync(Guid userId); // For a specific user as a Ratee
        Task<bool> DeleteRatingAsync(Guid id);
    }
}
