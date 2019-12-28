using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Smsbet.Web.Models
{
    [Table(Name = "LastQuery")]
    public class Basket
    {
        [Key]
        [Column(Name = "Id")]
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column(Name = "UserId")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
