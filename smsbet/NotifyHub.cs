using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Smsbet.Web
{
    public class NotifyHub : Hub
    {
        public async Task Send(string message)
        {
            await Clients.All.SendAsync("Send", message);
        }
    }
}