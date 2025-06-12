using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Helpers
{
    public class UserFilter
    {
        public string? SearchQuery { get; set; } //Like email/name
        public string? UserType { get; set; } // Filter by client,freelancer,admin
        public bool? IncludeDeleted { get; set; }
    }
}