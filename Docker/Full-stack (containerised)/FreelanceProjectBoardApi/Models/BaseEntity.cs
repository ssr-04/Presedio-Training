namespace FreelanceProjectBoardApi.Models
{
    public abstract class BaseEntity // All models inherit it (allows for audit amd also abstract common stuff)
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Audit columns (used for tracking changes - whom and when)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "System"; // By default system (later we can make use of Auth to find the person)

        // same for updates
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } // Based on Auth

        // Soft delete
        public bool IsDeleted { get; set; } = false;
    }
}