using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace GiamSat.API.Hubs
{
    public class ReportHub : Hub
    {
        // Hub can be empty, we just need IHubContext<ReportHub> to send messages to clients.
        // Clients can join specific groups if needed, or we just send to ConnectionId.
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
