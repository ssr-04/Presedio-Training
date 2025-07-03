using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceProjectBoardApi.Repositories
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        public ProjectRepository(FreelanceContext context) : base(context)
        { }

        public async Task<PageResult<Project>> GetAllProjectsAsync(ProjectFilter filter, PaginationParams pagination)
        {
            IQueryable<Project> query = _dbSet.Where(p => !p.IsDeleted);

            // Eager loading for common listing needs
            query = query.Include(p => p.Client).ThenInclude(c => c.ClientProfile)
                         .Include(p => p.AssignedFreelancer).ThenInclude(f => f!.FreelancerProfile)
                         .Include(p => p.ProjectSkills!).ThenInclude(ps => ps.Skill);

            // Applying filters
            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                query = query.Where(p => p.Title.ToLower().Contains(filter.SearchQuery.ToLower()) || p.Description.ToLower().Contains(filter.SearchQuery.ToLower())
                || (p.ProjectSkills != null && p.ProjectSkills.Any(ps => ps.Skill.Name.ToLower() == filter.SearchQuery.ToLower())));
            }

            if (filter.Status != null && Enum.TryParse<ProjectStatus>(filter.Status, true, out var statusValue))
            {
                query = query.Where(p => p.Status == statusValue);
            }
            if (filter.MinBudget.HasValue)
            {
                query = query.Where(p => p.Budget >= filter.MinBudget.Value);
            }
            if (filter.MaxBudget.HasValue)
            {
                query = query.Where(p => p.Budget <= filter.MaxBudget.Value);
            }
            if (filter.SkillNames != null && filter.SkillNames.Any())
            {
                filter.SkillNames = filter.SkillNames.Select(s => s.ToLower()).ToList();
                query = query.Where(p => p.ProjectSkills != null && p.ProjectSkills.Any(ps => filter.SkillNames.Contains(ps.Skill.Name.ToLower())));
            }
            if (filter.ClientId.HasValue)
            {
                query = query.Where(p => p.ClientId == filter.ClientId.Value);
            }
            if (filter.BeforeDeadline.HasValue)
            {
                query = query.Where(p => p.Deadline.HasValue && p.Deadline.Value <= filter.BeforeDeadline.Value);
            }
            if (filter.AfterDeadline.HasValue)
            {
                query = query.Where(p => p.Deadline.HasValue && p.Deadline.Value >= filter.AfterDeadline.Value);
            }

            // Total count
            var totalRecords = await query.CountAsync();

            // Applying sorting
            var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Title",
                "Budget",
                "Deadline",
                "Status",
                "CreatedAt"
            };

            if (!string.IsNullOrEmpty(pagination.SortBy) && allowedSortColumns.Contains(pagination.SortBy))
            {
                query = pagination.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => EF.Property<object>(p, pagination.SortBy))
                    : query.OrderBy(p => EF.Property<object>(p, pagination.SortBy));
            }
            else
            {
                // Default sort: newest projects first
                query = query.OrderByDescending(p => p.CreatedAt);
            }


            //Applying pagination
            var projects = await query.Skip((pagination.PageNumber - 1) * pagination.PageSize)
                            .Take(pagination.PageSize)
                            .ToListAsync();

            return new PageResult<Project>
            {
                Data = projects,
                pagination = new PaginationInfo
                {
                    TotalRecords = totalRecords,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize)
                }
            };
        }

        public async Task<Project?> GetProjectDetailsAsync(Guid id)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.Id == id)
                .Include(p => p.ProjectSkills!).ThenInclude(ps => ps.Skill)
                .Include(p => p.Client).ThenInclude(c => c.ClientProfile) //client with profile
                .Include(p => p.AssignedFreelancer).ThenInclude(f => f!.FreelancerProfile)
                .Include(p => p.Proposals)
                .Include(p => p.ProjectSkills)
                .Include(p => p.Attachments)
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByUser(Guid userId)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.ClientId == userId)
                .Include(p => p.ProjectSkills!).ThenInclude(ps => ps.Skill)
                .Include(p => p.Client).ThenInclude(c => c.ClientProfile) //client with profile
                .Include(p => p.AssignedFreelancer).ThenInclude(f => f!.FreelancerProfile)
                .Include(p => p.Proposals)
                .Include(p => p.ProjectSkills)
                .Include(p => p.Attachments)
                .Include(p => p.Ratings)
                .ToListAsync();
        }
    }
}