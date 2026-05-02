using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace GiamSat.API.Hubs
{
    /// <summary>
    /// SignalR hub đẩy log realtime cho UI. Server gọi qua IHubContext&lt;LogsHub&gt; trong SignalRLogSink.
    /// Phương thức "LogEntry" được client subscribe ở UI.
    /// </summary>
    [Authorize(Policy = AppPermissions.System_Logs_View)]
    public class LogsHub : Hub
    {
        public const string HubPath = "/hubs/logs";

        public Task Ping() => Clients.Caller.SendAsync("Pong");
    }
}
