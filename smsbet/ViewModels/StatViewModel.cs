using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smsbet.Web.Models;

namespace Smsbet.Web.ViewModels
{
    public class StatViewModel
    {
        public Forecasts Forecast { get; set; }
        public int CountViews { get; set; }
    }
}
