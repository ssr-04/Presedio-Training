using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceProjectBoardApi.Repositories
{
    public class SkillRepository : GenericRepository<Skill>, ISkillRepository
    {
        public SkillRepository(FreelanceContext context) : base(context) { }

        public async Task<Skill?> GetSkillByNameAsync(string name)
        {
            return await _dbSet.Where(s => !s.IsDeleted && s.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<PageResult<Skill>> GetAllSkillsAsync(SkillFilter filter, PaginationParams pagination)
        {
            IQueryable<Skill> query = _dbSet;
            if (filter.IncludeDeleted.HasValue && filter.IncludeDeleted.Value == true)
            {
                // Include all skills
            }
            else
            {
                query = query.Where(cp => !cp.IsDeleted);
            }


            //Apply Filters
            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                query = query.Where(s => s.Name.ToLower().Contains(filter.SearchQuery.ToLower()));
            }

            var totalRecords = await query.CountAsync();

            // Apply sorting
            var allowedSortColumns = new HashSet<string>()
            {
                "Name"
            };

            if (!string.IsNullOrEmpty(pagination.SortBy) && allowedSortColumns.Contains(pagination.SortBy))
            {
                query = pagination.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(cp => EF.Property<object>(cp, pagination.SortBy))
                    : query.OrderBy(cp => EF.Property<object>(cp, pagination.SortBy));
            }
            else
            {
                // Default sort: newest clients first
                query = query.OrderByDescending(cp => cp.CreatedAt);
            }

            // Apply pagination
            var Skills = await query.Skip((pagination.PageNumber - 1) * pagination.PageSize)
                                            .Take(pagination.PageSize)
                                            .ToListAsync();

            return new PageResult<Skill>
            {
                Data = Skills,
                pagination = new PaginationInfo
                {
                    TotalRecords = totalRecords,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalRecords/pagination.PageSize)
                }
            };
        }
    }
}