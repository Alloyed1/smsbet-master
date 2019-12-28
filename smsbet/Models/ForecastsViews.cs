using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Smsbet.Web.Models
{
    [Table(Name = "ForecastsViews")]
    public class ForecastsViews
    {
        [Key]
        [Column(Name = "Id")]
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column(Name = "ForecastId")]
        public int ForecastId { get; set; }
        [Column(Name = "UserEmail")]
        public string UserEmail { get; set; }
        [Column(Name = "Time")]

        public DateTime Time { get; set; }
    }
}
