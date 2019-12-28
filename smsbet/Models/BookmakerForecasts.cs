using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Smsbet.Web.Models
{
    [Table(Name = "BookmakerForecasts")]
    public class BookmakerForecasts
    {
        [Key]
        [Column(Name = "Id")]
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column(Name = "ForecastId")]
        public int ForecastId { get; set; }
        public Forecasts Forecast { get; set; }
        [Column(Name = "BookMaker")]
        public string BookMaker { get; set; }
    }
}
