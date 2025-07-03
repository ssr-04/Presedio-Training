using System.ComponentModel.DataAnnotations;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.DTOs.Notifications
{
    public class CreateNotificationDto
    {
        [Required]
        public Guid ReceiverId { get; set; }
        [Required]
        public string Message { get; set; } = string.Empty;
        [Required]
        public NotificationCategory Category { get; set; }
    }
}