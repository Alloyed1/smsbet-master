using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smsbet.Web.Models;

namespace Smsbet.Web.ViewModels
{
    public class InfoViewModel
    {
        public int CountViews { get; set; }
        public Forecasts Forecast { get; set; }
    }
}
