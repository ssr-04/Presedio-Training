
using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    //Method that client invokes to send message to others
    public async Task SendMessage(string userName, string message)
    {
        System.Console.WriteLine(userName);
        //Server receives the message from client and boadcast to all
        await Clients.All.SendAsync("ReceivedMessage", userName, message);
    }

    //When a user connected it logs
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();

    }
}