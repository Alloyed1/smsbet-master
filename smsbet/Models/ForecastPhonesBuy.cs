using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Smsbet.Web.Models
{
    [Table(Name = "ForecastPhonesBuys")]
    public class ForecastPhonesBuy
    {
        [Key]
        [Column(Name = "Id")]
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column(Name = "ForecastId")]
        public int ForecastId { get; set; }
        public Forecasts Forecast { get; set; }
        [Column(Name = "UserPhone")]
        public string UserPhone { get; set; }
        [Column(Name = "UserId")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
