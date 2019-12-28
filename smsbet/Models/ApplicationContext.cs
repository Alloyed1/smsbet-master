using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smsbet.Web.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //builder.Entity<Favorite>().HasKey(k => new { k.Id, k.UserId });
        }
        public DbSet<Forecasts> Forecasts { get; set; }
        public DbSet<BookmakerForecasts> BookmakerForecasts { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }

        public DbSet<ForecastsViews> ForecastsViews { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItems> BasketItems { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderForecast> OrderForecasts { get; set; }
        public DbSet<ForecastPhonesBuy> ForecastPhonesBuys { get; set; }
    }
}
