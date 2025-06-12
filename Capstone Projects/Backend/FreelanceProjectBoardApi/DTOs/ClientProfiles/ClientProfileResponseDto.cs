namespace FreelanceProjectBoardApi.DTOs.ClientProfiles
{
    public class ClientProfileResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? ContactPersonName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
