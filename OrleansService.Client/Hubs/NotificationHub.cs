using Microsoft.AspNetCore.SignalR;

namespace OrleansService.Client.Hubs
{
    public interface INotificationHub
    {
        Task PublishMessageToHub(string message);
    }

    public class NotificationHub:Hub<INotificationHub>
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogWarning("Client Connected {ClientId}",base.Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }
}
