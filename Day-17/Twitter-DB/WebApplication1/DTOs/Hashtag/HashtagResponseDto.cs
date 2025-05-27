namespace FirstTwitterApp.DTOs.Hashtag
{
    public class HashtagResponseDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}