using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Smsbet.Web.Models
{
    [Table(Name = "AspNetUsers")]
    public class User : IdentityUser
    {
        [Key]
        [Column(Name = "Id")]
        [PrimaryKey, Identity]
        public override string Id { get; set; }

        [Column(Name = "Email")]
        public override string Email { get; set; }
        [Column(Name = "PhoneNumber")]
        public override string PhoneNumber { get; set; }
        [Column(Name = "UserName")]
        public override string UserName { get; set; }

        [Column(Name = "PasswordText")]
        public string PasswordText { get; set; }
        [Column(Name = "FreePromocode")]
        public bool FreePromocode { get; set; }
        [Column(Name = "FreePromocodeWrite")]
        public bool FreePromocodeWrite { get; set; }
        [Column(Name = "IsSmsPushing")]
        public bool IsSmsPushing { get; set; }
        [Column(Name = "IsEmailPushing")]
        public bool IsEmailPushing { get; set; }
        
        [Column(Name ="Balance")]
        
        
        public decimal Balance { get; set; }
        [Column(Name ="NewConfirmPassword")]
        public string NewConfirmPassword { get; set; }
        [Column(Name ="ConfirmPasswordCode")]
        public string ConfirmPasswordCode { get; set; }
        
        
        [Column(Name ="UserEmail")]
        public string UserEmail { get; set; }
        [Column(Name ="NewUserEmail")]
        public string NewUserEmail { get; set; }
        [Column(Name ="NewUserEmailCode")]
        public string NewUserEmailCode { get; set; }
        
        
        [Column(Name ="NewUserPhone")]
        public string NewUserPhone { get; set; }
        [Column(Name ="NewUserPhoneCode")]
        public string NewUserPhoneCode { get; set; }
        
    }
}
