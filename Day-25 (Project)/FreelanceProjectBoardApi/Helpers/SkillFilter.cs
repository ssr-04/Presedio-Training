using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Helpers
{
    public class SkillFilter
    {
        public string? SearchQuery { get; set; } //Like email/name
        public bool? IncludeDeleted { get; set; }
    }
}