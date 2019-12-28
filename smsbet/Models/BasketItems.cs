using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Smsbet.Web.Models
{
    [Table(Name = "BasketItems")]
    public class BasketItems
    {
        [Key]
        [Column(Name = "Id")]
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column(Name = "BasketId")]
        public int BasketId { get; set; }
        public Basket Basket { get; set; }
        [Column(Name = "ForecastId")]
        public int ForecastId {get;set;}
        public Forecasts Forecast { get; set; }
    }
}
