using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Smsbet.Web.Models
{
    [Table(Name = "Forecasts")]
    public class Forecasts
    {
        [Key]
        [Column(Name = "Id")]
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column(Name = "StartTime")]
        public DateTime StartTime { get; set; } 
        [Column(Name = "ChampionatName")]
        public string ChampionatName { get; set; }
        [Column(Name = "IntervalKoof")]
        public string IntervalKoof { get; set; }
        [Column(Name = "Game")]

        public string Game { get; set; }
        [Column(Name = "PublicPrognoz")]

        public string PublicPrognoz { get; set; }
        [Column(Name = "KoofProxoda")]

        public int KoofProxoda { get; set; }
        [Column(Name = "PercentReturn")]

        public int PercentReturn { get; set; }
        [Column(Name = "Status")]
        public string Status { get; set; }
        [Column(Name = "Result")]
        public string Result { get; set; }
        [Column(Name = "ForecastText")]

        public string ForecastText { get; set; }
    }
}
