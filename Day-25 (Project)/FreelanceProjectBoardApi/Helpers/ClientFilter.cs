using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Helpers
{
    public class ClientFilter
    {
        public string? SearchQuery { get; set; } //Like email/name
        public bool? IncludeDeleted { get; set; }
    }
}