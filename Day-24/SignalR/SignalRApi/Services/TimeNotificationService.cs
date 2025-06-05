

using Microsoft.AspNetCore.SignalR;

public class TimeNotificationService : BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private Timer? _timer = null;

    public TimeNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Timer will call DoWork every 30 seconds after an initial 0-second delay
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

        Console.WriteLine("TimeNotificationService started.");
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
        {
            var currentTime = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"Sending time notification: {currentTime}");

            // Send notification to ALL connected clients
            // "ReceiveTimeNotification" is the method name clients will listen for
            _hubContext.Clients.All.SendAsync("ReceiveTimeNotification", $"Server Time: {currentTime}");
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("TimeNotificationService is stopping.");
            _timer?.Change(Timeout.Infinite, 0); // Stop the timer
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _timer?.Dispose(); // Dispose the timer when the service is disposed
            base.Dispose();
        }

}