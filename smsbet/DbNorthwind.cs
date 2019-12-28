using LinqToDB;
using Smsbet.Web.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smsbet.Web.Models;

namespace Smsbet.Web
{

        public class DbNorthwind : LinqToDB.Data.DataConnection
        {
            public DbNorthwind() : base(ProviderName.SqlServer, @"Data Source=wpl19.hosting.reg.ru;Initial Catalog=u0831016_smsbedb;User Id=u0831016_smsbetuser;Password=cw42pu!QAZ;") { }

            public ITable<AppSettings> AppSettins => GetTable<AppSettings>();
            public ITable<User> Users => GetTable<User>();
            public ITable<Basket> Baskets => GetTable<Basket>();
            public ITable<BasketItems> BasketItems => GetTable<BasketItems>();
            public ITable<BookmakerForecasts> BookmakerForecasts => GetTable<BookmakerForecasts>();
            public ITable<ForecastPhonesBuy> ForecastPhonesBuy => GetTable<ForecastPhonesBuy>();
            public ITable<Forecasts> Forecasts => GetTable<Forecasts>();
            public ITable<ForecastsViews> ForecastsViews => GetTable<ForecastsViews>();
            public ITable<Order> Orders => GetTable<Order>();
            public ITable<OrderForecast> OrderForecasts => GetTable<OrderForecast>();

    }
}
