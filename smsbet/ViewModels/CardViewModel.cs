using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smsbet.Web.Models;

namespace Smsbet.Web.ViewModels
{
    public class CardViewModel
    {
        public List<Forecasts> UserCardForecasts { get; set; }
        public List<Forecasts> RandomForecasts { get; set; }
		public bool ClearCookie { get; set; }
    }
}
