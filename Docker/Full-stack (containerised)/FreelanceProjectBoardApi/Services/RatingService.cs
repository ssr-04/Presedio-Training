using AutoMapper;
using FreelanceProjectBoardApi.DTOs.Notifications;
using FreelanceProjectBoardApi.DTOs.Ratings;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Services.Interfaces;

namespace FreelanceProjectBoardApi.Services.Implementations
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IProjectRepository _projectRepository; // To verify project
        private readonly IUserRepository _userRepository; // To verify rater/ratee
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public RatingService(
            IRatingRepository ratingRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            IMapper mapper)
        {
            _ratingRepository = ratingRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<RatingResponseDto?> CreateRatingAsync(Guid raterId, CreateRatingDto createDto)
        {
            // Verify rater and ratee exist and are valid users
            var rater = await _userRepository.GetByIdAsync(raterId);
            var ratee = await _userRepository.GetByIdAsync(createDto.RateeId);
            var project = await _projectRepository.GetByIdAsync(createDto.ProjectId);

            if (rater == null || ratee == null || project == null)
            {
                return null; // Invalid rater, ratee, or project
            }

            // - Rater should be the client, and ratee should be the assigned freelancer, OR
            // - Rater should be the assigned freelancer, and ratee should be the client.
            // - Rating only allowed if project is completed.
            if (project.Status != ProjectStatus.Completed)
            {
                return null; // Can only rate completed projects
            }

            // Ensures rater is part of the project and rating the correct person
            bool isValidRatingScenario = false;
            if (rater.Id == project.ClientId && ratee.Id == project.AssignedFreelancerId) // Client rating Freelancer
            {
                isValidRatingScenario = true;
            }
            else if (rater.Id == project.AssignedFreelancerId && ratee.Id == project.ClientId) // Freelancer rating Client
            {
                isValidRatingScenario = true;
            }

            if (!isValidRatingScenario)
            {
                return null; // Invalid rating scenario (e.g., client rating client, or random users rating)
            }

            // Check if this specific rating has already been given (e.g., client can rate freelancer once per project)
            var existingRating = await _ratingRepository.GetAllAsync(
                r => r.ProjectId == createDto.ProjectId && r.RaterId == raterId && r.RateeId == createDto.RateeId
            );
            if (existingRating.Any())
            {
                return null; // Rating already submitted
            }

            var rating = _mapper.Map<Rating>(createDto);
            rating.RaterId = raterId; // Ensure rater ID is from authenticated user

            await _ratingRepository.AddAsync(rating);
            await _ratingRepository.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                ReceiverId = rating.RateeId,
                Category = NotificationCategory.NewRating,
                Message = $"{project.Client.Name} has rated you for the project {project.Title}"
            });

            var createdRating = await _ratingRepository.GetByIdAsync(rating.Id); // Reload to ensure full object
            return _mapper.Map<RatingResponseDto>(createdRating);
        }

        public async Task<RatingResponseDto?> GetRatingByIdAsync(Guid id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            return rating == null ? null : _mapper.Map<RatingResponseDto>(rating);
        }

        public async Task<IEnumerable<RatingResponseDto>> GetRatingsReceivedByUserIdAsync(Guid userId)
        {
            var ratings = await _ratingRepository.GetRatingsReceivedByUserAsync(userId, includeRater: true);
            return _mapper.Map<IEnumerable<RatingResponseDto>>(ratings);
        }

        public async Task<IEnumerable<RatingResponseDto>> GetRatingsGivenByUserIdAsync(Guid userId)
        {
            var ratings = await _ratingRepository.GetRatingsGivenByUserAsync(userId, includeRatee: true);
            return _mapper.Map<IEnumerable<RatingResponseDto>>(ratings);
        }

        public async Task<double> GetAverageRatingForUserAsync(Guid userId)
        {
            return await _ratingRepository.GetAverageRatingForUserAsync(userId);
        }

        public async Task<bool> DeleteRatingAsync(Guid id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            if (rating == null)
            {
                return false;
            }
            // Add authorization check here: Only the rater or an admin can delete a rating.
            await _ratingRepository.DeleteAsync(id); // Soft delete
            await _ratingRepository.SaveChangesAsync();
            return true;
        }
    }
}
