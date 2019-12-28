using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
    public interface IMessagePusher
    {
        Task<string> Send(string phone, IEnumerable<string> phones, string text);
    }
}