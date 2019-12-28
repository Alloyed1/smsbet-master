using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Smsbet.Web.Models
{
    [Table(Name = "AppSettings")]
    public class AppSettings
    {
        [Key]
        [Column(Name = "Id")]
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column(Name = "Keys")]
        public string Keys { get; set; }
        [Column(Name = "Value")]
        public string Value { get; set; }
    }
}
