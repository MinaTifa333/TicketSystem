
using Microsoft.AspNetCore.SignalR;

namespace TicketSystem.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinSectionGroup(string sectionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Section_{sectionId}");
        }

        public async Task LeaveSectionGroup(string sectionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Section_{sectionId}");
        }
        public async Task SendNotificationToSection(string sectionId, string message)
        {
            await Clients.Group($"Section_{sectionId}").SendAsync("ReceiveNotification", message);
        }


    }
}
