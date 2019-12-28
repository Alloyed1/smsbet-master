using System.Collections.Generic;
using System.Threading.Tasks;

namespace smsbet.Services
{
    public interface IMessagePusher
    {
        Task<int> Send(string phone, IEnumerable<string> phones, string text);
    }
}