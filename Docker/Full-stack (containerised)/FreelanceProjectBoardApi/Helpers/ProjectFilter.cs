using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Helpers
{
    public class ProjectFilter
    {
        public string? SearchQuery { get; set; } // Like title,description
        public String? Status { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public Guid? ClientId { get; set; }
        public List<string>? SkillNames { get; set; }
        public DateTime? BeforeDeadline { get; set; }
        public DateTime? AfterDeadline { get; set; }
    }
}