using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceProjectBoardApi.Repositories
{
    public class ClientProfileRepository : GenericRepository<ClientProfile>, IClientProfileRepository
    {
        public ClientProfileRepository(FreelanceContext context) : base(context)
        {

        }
        public async Task<PageResult<ClientProfile>> GetAllClientProfileAsync(ClientFilter filter, PaginationParams pagination)
        {
            IQueryable<ClientProfile> query = _dbSet;
            if (filter.IncludeDeleted.HasValue && filter.IncludeDeleted.Value == true)
            {
                // Include all profiles
            }
            else
            {
                query = query.Where(cp => !cp.IsDeleted);
            }

            query = query.Include(cp => cp.User);

            //Apply Filters
            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                query = query.Where(cp => cp.CompanyName.ToLower().Contains(filter.SearchQuery.ToLower()) ||
                                         cp.Description!.ToLower().Contains(filter.SearchQuery.ToLower()) ||
                                         cp.Location!.ToLower().Contains(filter.SearchQuery.ToLower()) ||
                                         cp.User.Email.ToLower().Contains(filter.SearchQuery.ToLower()));
            }

            var totalRecords = await query.CountAsync();

            // Apply sorting
            var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CompanyName",
                "Location",
                "ContactPersonName",
                "CreatedAt"
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
            var ClientProfiles = await query.Skip((pagination.PageNumber - 1) * pagination.PageSize)
                                            .Take(pagination.PageSize)
                                            .ToListAsync();

            return new PageResult<ClientProfile>
            {
                Data = ClientProfiles,
                pagination = new PaginationInfo
                {
                    TotalRecords = totalRecords,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalRecords/pagination.PageSize)
                }
            };
        }

        public async Task<ClientProfile?> GetClientProfileDetailsAsync(Guid id)
        {
            return await _dbSet
                .Where(cp => !cp.IsDeleted && cp.Id == id)
                .Include(cp => cp.User) // the associated User
                .Include(cp => cp.PostedProjects) // projects posted by this client
                .FirstOrDefaultAsync();

        }
    }
}