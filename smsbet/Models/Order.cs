using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Smsbet.Web.Models
{
    [Table(Name = "Oders")]
    public class Order
    {
        [Key]
        [Column(Name = "Id")]
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column(Name = "Status")]
        public string Status { get; set; }
        [Column(Name = "PayCode")]
        public string PayCode { get; set; }
        [Column(Name = "CreateDate")]
        public DateTime CreateDate { get; set; }

        [Column(Name = "Sum")]
        public int Sum { get; set; }
        
        [Column(Name = "UserId")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
