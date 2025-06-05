using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using NotifyAPI.DTOs;

namespace NotifyService.Hubs
{
    [Authorize] 
    public class NotificationHub : Hub
    {
        // To send notification to all connected clients
        public async Task SendDocumentNotification(DocumentMetadataDto document)
        {
            Console.WriteLine($"Broadcasting new document notification: {document.Title}");
            await Clients.All.SendAsync("ReceiveDocumentNotification", document);
        }

    }
}