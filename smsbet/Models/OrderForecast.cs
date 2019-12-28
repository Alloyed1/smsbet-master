using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Smsbet.Web.Models
{
    [Table(Name = "OrderForecasts")]
    public class OrderForecast
    {
        [Key]
        [Column(Name = "Id")]
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column(Name = "OrderId")]
        public int OrderId { get; set; }
        public Order Order { get; set; }
        [Column(Name = "ForecastId")]

        public int ForecastId { get; set; }
        public Forecasts Forecast { get; set; }
    }
}
