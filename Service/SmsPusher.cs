using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;

namespace Service
{
    public class SmsPusher : IMessagePusher
    {
        public async Task<string> Send(string phone, List<string> phones, string text)
        {
            if (phone == null && phones == null) return "badsms";
            
            var client = new RestClient("https://smsc.ru");
            var request = new RestRequest("sys/send.php");
            
            request.AddQueryParameter("login", "smsbet");
            request.AddQueryParameter("psw", "Ujklujkl87");
            request.AddQueryParameter("mes", text);

            if (phone == null && phones.Any())
            {
                string phonesString = "+" + String.Join(";+", phones);
                request.AddQueryParameter("phones", phonesString);
            }
            else
            {
                request.AddQueryParameter("phones", "+" + phone);
            }
                
            
            var res = client.Execute(request,Method.GET);
            
            return res.Content;

        }
    }
}