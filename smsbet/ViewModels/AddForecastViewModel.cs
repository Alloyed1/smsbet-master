using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smsbet.Web.ViewModels
{
    public class AddForecastViewModel
    {

        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public string ChampionatName { get; set; }
        public string IntervalKoof { get; set; }

        public string PublicPrognoz { get; set; }

        public string Game { get; set; }

        public int KoofProxoda { get; set; }
        public int PercentReturn { get; set; }
        public string ForecastText { get; set; }

		public string Status { get; set; }

        public string Bookmaker { get; set; }
    }
}
