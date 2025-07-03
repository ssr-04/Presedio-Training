using AutoMapper;
using FreelanceProjectBoardApi.DTOs.Notifications;
using FreelanceProjectBoardApi.Hubs;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FreelanceProjectBoardApi.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(INotificationRepository notificationRepository, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<NotificationDto?> CreateNotificationAsync(CreateNotificationDto createDto)
        {
            var notification = _mapper.Map<Notification>(createDto);
            
            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();

            var notificationDto = _mapper.Map<NotificationDto>(notification);

            // **THE REAL-TIME PART**
            // Push the notification to the specific user via SignalR.
            // The UserId is used to target the specific client connection(s).
            await _hubContext.Clients.User(notification.ReceiverId.ToString())
                                     .SendAsync("ReceiveNotification", notificationDto);

            return notificationDto;
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsByReceiverIdAsync(Guid receiverId)
        {
            var notifications = await _notificationRepository.GetAllAsync(
                n => n.ReceiverId == receiverId,
                orderBy: q => q.OrderByDescending(n => n.CreatedAt)
            );
            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null || notification.ReceiverId != userId)
            {
                return false; // Not found or user doesn't own this notification
            }

            if (notification.IsRead) return true; // Already read

            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
            await _notificationRepository.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAllAsReadAsync(Guid receiverId)
        {
            return await _notificationRepository.MarkAllAsReadAsync(receiverId);
        }
    }
}