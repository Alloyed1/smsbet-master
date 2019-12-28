using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smsbet.Web.ViewModels
{
    public class MainPageViewModel
    {
        public string CountUsers { get; set; }
        public string CountSms { get; set; }
        public string CountSuccessForecast { get; set; }

        public List<AddForecastViewModel> Forecasts { get; set; }
    }
}
