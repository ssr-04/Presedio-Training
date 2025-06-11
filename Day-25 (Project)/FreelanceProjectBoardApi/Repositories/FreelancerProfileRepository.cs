using System.Runtime.Intrinsics.Arm;
using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceProjectBoardApi.Repositories
{
    public class FreelancerProfileRepository : GenericRepository<FreelancerProfile>, IFreelancerProfileRepository
    {
        public FreelancerProfileRepository(FreelanceContext context) : base(context)
        {

        }
        public async Task<PageResult<FreelancerProfile>> GetAllFreelancerProfilesAsync(FreelancerFilter filter, PaginationParams pagination)
        {
            IQueryable<FreelancerProfile> query = _dbSet.Where(fp => !fp.IsDeleted);

            // Eager loading
            query = query.Include(fp => fp.User) // Include user data like email for search
                         .Include(fp => fp.FreelancerSkills!).ThenInclude(fs => fs.Skill)
                         .Include(fp => fp.ProfilePictureFile); // Maybe just profile picture for listings

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                // Search across profile headline, bio, and associated user email
                query = query.Where(fp => fp.Headline.ToLower().Contains(filter.SearchQuery.ToLower()) ||
                                         fp.Bio!.ToLower().Contains(filter.SearchQuery.ToLower()) ||
                                         fp.User.Email.ToLower().Contains(filter.SearchQuery.ToLower()));
            }
            if (filter.SkillIds != null && filter.SkillIds.Any())
            {
                query = query.Where(fp => fp.FreelancerSkills!.Any(fs => filter.SkillIds.Contains(fs.SkillId)));
            }
            if (!string.IsNullOrEmpty(filter.ExperienceLevel))
            {
                query = query.Where(fp => fp.ExperienceLevel == filter.ExperienceLevel);
            }
            if (filter.IsAvailable.HasValue)
            {
                query = query.Where(fp => fp.IsAvailable == filter.IsAvailable.Value);
            }
            if (filter.MinHourlyRate.HasValue)
            {
                query = query.Where(fp => fp.HourlyRate.HasValue && fp.HourlyRate.Value >= filter.MinHourlyRate.Value);
            }
            if (filter.MaxHourlyRate.HasValue)
            {
                query = query.Where(fp => fp.HourlyRate.HasValue && fp.HourlyRate.Value <= filter.MaxHourlyRate.Value);
            }
            if (filter.MinProjectsCompleted.HasValue)
            {
                query = query.Where(fp => fp.ProjectsCompleted >= filter.MinProjectsCompleted.Value);
            }

            var totalRecords = await query.CountAsync();

            // Apply sorting
            var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Headline",
                "ExperienceLevel",
                "HourlyRate",
                "ProjectsCompleted",
                "IsAvailable",
                "CreatedAt"
            };

            if (!string.IsNullOrEmpty(pagination.SortBy) && allowedSortColumns.Contains(pagination.SortBy))
            {
                query = pagination.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(fp => EF.Property<object>(fp, pagination.SortBy))
                    : query.OrderBy(fp => EF.Property<object>(fp, pagination.SortBy));
            }
            else
            {
                // Default sort: newest profiles first
                query = query.OrderByDescending(fp => fp.CreatedAt);
            }

            // Apply pagination
            var freelancers = await query.Skip((pagination.PageNumber - 1) * pagination.PageSize)
                                    .Take(pagination.PageSize)
                                    .ToListAsync();

            return new PageResult<FreelancerProfile>
            {
                Data = freelancers,
                pagination = new PaginationInfo
                {
                    TotalRecords = totalRecords,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize)
                }
            };

        }

        public async Task<FreelancerProfile?> GetFreelancerProfileDetailsAsync(Guid id)
        {
            var result = await _dbSet
                .Where(fp => !fp.IsDeleted && fp.Id == id)
                .Include(fp => fp.User) // Including the associated User (for email/login details if needed)
                .Include(fp => fp.FreelancerSkills!).ThenInclude(fs => fs.Skill) // Including Skills
                .Include(fp => fp.ResumeFile)
                .Include(fp => fp.ProfilePictureFile)
                .Include(fp => fp.RatingsAsRatee) // Including ratings received by this freelancer
                .FirstOrDefaultAsync();
            return result;
        }
    }
}