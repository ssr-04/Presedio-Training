namespace FreelanceProjectBoardApi.Helpers
{
    public class FreelancerFilter
    {
        public string? SearchQuery { get; set; } //Like headline, bio, email
        public List<Guid>? SkillIds { get; set; }
        public string? ExperienceLevel { get; set; }
        public bool? IsAvailable { get; set; }
        public decimal? MinHourlyRate { get; set; }
        public decimal? MaxHourlyRate { get; set; }
        public int? MinProjectsCompleted { get; set; }
    }
}