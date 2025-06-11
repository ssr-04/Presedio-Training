using System.Linq.Expressions;
using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceProjectBoardApi.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(FreelanceContext context) : base(context)
        {
            
        }

        public new async Task<User?> GetByIdAsync(Guid id, bool includeDeleted = false)
        {
            IQueryable<User> query = _dbSet.Where(u => !u.IsDeleted && u.Id == id)
                                        .Include(u => u.ClientProfile)
                                        .Include(u => u.FreelancerProfile);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<PageResult<User>> GetAllUsersAsync(UserFilter filter, PaginationParams pagination, bool includeProfiles = false)
        {
            IQueryable<User> query = _dbSet;

            if (filter.IncludeDeleted.HasValue && filter.IncludeDeleted.Value == true)
            {
                // Just have all users
            }
            else
            {
                query = query.Where(u => !u.IsDeleted);
            }
            if (includeProfiles)
            {
                query = query.Include(u => u.ClientProfile)
                            .Include(u => u.FreelancerProfile);
            }

            //Applying filters
            if (filter.UserType != null)
            {
                query = query.Where(u => u.Type.ToString() == filter.UserType);
            }
            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                query = query.Where(u => u.Email.ToLower().Contains(filter.SearchQuery.ToLower()) ||
                                    (u.ClientProfile != null & u.ClientProfile!.CompanyName.ToLower().Contains(filter.SearchQuery.ToLower())) ||
                                    (u.FreelancerProfile != null & u.FreelancerProfile!.Headline.ToLower().Contains(filter.SearchQuery.ToLower()))
                                    );

            }


            // Getting total records
            var totalrecords = await query.CountAsync(); // for pagination

            if (totalrecords == 0)
            {
                return new PageResult<User>
                {
                    Data = new List<User>(),
                    pagination = new PaginationInfo
                    {
                        TotalRecords = 0,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize,
                        TotalPages = 0
                    }
                };
            }

            // Applying sorting
            var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Email",
                "Type",
                "CreatedAt"
            };

            if (!string.IsNullOrEmpty(pagination.SortBy) && allowedSortColumns.Contains(pagination.SortBy))
            {
                query = pagination.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(u => EF.Property<object>(u, pagination.SortBy))
                    : query.OrderBy(u => EF.Property<object>(u, pagination.SortBy));
            }
            else
            {
                query = query.OrderBy(u => u.CreatedAt);
            }


            // Applying pagination
            var users = await query.Skip((pagination.PageNumber - 1) * pagination.PageSize)
                        .Take(pagination.PageSize).ToListAsync();

            return new PageResult<User>
            {
                Data = users,
                pagination = new PaginationInfo
                {
                    TotalRecords = totalrecords,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalrecords / pagination.PageSize)
                }
            };
        }

        public async Task<User?> GetUserByEmailAsync(string email, bool includeProfiles = false)
        {
            IQueryable<User> query = _dbSet.Where(u => !u.IsDeleted && u.Email.ToLower() == email.ToLower());

            if (includeProfiles)
            {
                query = query.Include(u => u.ClientProfile)
                                .Include(u => u.FreelancerProfile);
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}