using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FreelanceProjectBoardApi.Hubs
{
    [Authorize] // Ensures only authenticated users can connect to the hub
    public class NotificationHub : Hub
    {
        
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        // Note: We don't need a public method like "SendMessage" here because
        // the notifications will be PUSHED from the server-side service, not
        // sent from one client to another directly through the hub.
    }
}