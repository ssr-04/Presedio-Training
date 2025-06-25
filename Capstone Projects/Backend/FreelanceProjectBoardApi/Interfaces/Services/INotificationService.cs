using FreelanceProjectBoardApi.DTOs.Notifications;

namespace FreelanceProjectBoardApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto?> CreateNotificationAsync(CreateNotificationDto createDto);
        Task<IEnumerable<NotificationDto>> GetNotificationsByReceiverIdAsync(Guid receiverId);
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId); // Pass userId for security
        Task<int> MarkAllAsReadAsync(Guid receiverId);
    }
}