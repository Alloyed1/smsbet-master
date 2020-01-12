using Dapper;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Service;
using Smsbet.Web.Models;
using Smsbet.Web.ViewModels;

namespace Smsbet.Web.Repository
{
    public interface IAccountRepository
    {
        Task<string> GeneratePassword();
        Task<bool> CheckPassword(string password, string phone);
        Task<bool> SendSms(string code, string phone);
        Task<string> ResendPassword(string phone);
        Task<User> GetUser(string phone);
        Task<MainPageViewModel> GetMainPageView();

        Task<int> AddOrder(string userName, int sum, string paymentId);
        Task CompleteOrder(string paymentId);

        Task<string> GetInfoBar();
        Task SetInfoBar(string text);
        Task AddToBasket(string userName, int forecastId);
        Task<string> GetUserId(string userName);

        Task<CardViewModel> GetCardViewModel(string userName, string itemsCookie);
        Task<int> CheckFreePromocode(string promocode, string userName);
        Task DeleteFromForecast(string userName, int id);
        Task<int> GetSumOrder(string orderId);
        Task<bool> UserWriteFreePromocode(string userName);
        Task SetWhatUserUserFreePromocode(string userName);
        Task<SettingsViewModel> GetSettingsViewModel(string userEmail);
        Task CompletePayBalance(decimal sum, string userName);
    }
    public class AccountRepository : IAccountRepository
    {
        private string connectionString;
        private readonly IMessagePusher _smsPusher;
        
        public AccountRepository(IConfiguration configuration,
            IMessagePusher smsPusher)
        {
            _smsPusher = smsPusher;
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task CompletePayBalance(decimal sum, string userName)
        {
            using (var db = new DbNorthwind())
            {
                var user = await db.Users.FirstOrDefaultAsync(f => f.Email == userName);
                user.Balance += sum;

                await db.Users
                    .Where(w => w.Email == userName)
                    .Set(s => s.Balance, user.Balance)
                    .UpdateAsync();
            }
        }

        public async Task<SettingsViewModel> GetSettingsViewModel(string userEmail)
        {
            using (var db = new DbNorthwind())
            {
                var user = await db.Users
                    .FirstOrDefaultAsync(f => f.UserName == userEmail);

                var model = new SettingsViewModel()
                {
                    Email = user.UserEmail,
                    Phone = user.PhoneNumber,
                    IsEmailPushing = user.IsEmailPushing,
                    IsSmsPushing = user.IsSmsPushing
                };

                return model;
            }
        }

        public async Task<string> GetUserId(string userName)
        {
            using(var db = new SqlConnection(connectionString))
            {
                return await db.QueryFirstOrDefaultAsync<string>("SELECT Id FROM AspNetUsers WHERE Email = @name", new { name = userName });
            }
        }

        public async Task SetWhatUserUserFreePromocode(string userName)
        {
            using(var db = new SqlConnection(connectionString))
            {
                await db.ExecuteAsync("UPDATE AspNetUsers SET FreePromocode = @tr WHERE Email = @userName", new { tr = true, userName = userName });
            }
        }

        public async Task<bool> UserWriteFreePromocode(string userName)
        {
            using (var db = new SqlConnection(connectionString))
            {
                bool active = await db.QueryFirstOrDefaultAsync<bool>("SELECT FreePromocodeWrite FROM AspNetUsers WHERE Email = @userName", new { userName = userName });
                bool used = await db.QueryFirstOrDefaultAsync<bool>("SELECT FreePromocode FROM AspNetUsers WHERE Email = @userName", new { userName = userName });
                
                if (active && !used)
                {
                    return true;
                }
                return false;
            }
        }

        public async Task DeleteFromForecast(string userName, int id)
        {
            using (var db = new SqlConnection(connectionString))
            {
                string userId = await GetUserId(userName);
                await db.ExecuteAsync(@"DECLARE @basketId INT;
                                    SET @basketId = (SELECT Id FROM Baskets WHERE UserId = @userId);
                                     DELETE FROM BasketItems WHERE BasketId = @basketId AND ForecastId = @idForecast", new { idForecast = id, userId = userId });
            }
        }
        public async Task<int> GetSumOrder(string userName)
        {
            using(var db = new SqlConnection(connectionString))
            {
                string userId = await GetUserId(userName);
                var forecastsIds = (await db.QueryAsync<int>("SELECT f.Id AS Id FROM BasketItems i INNER JOIN Baskets b ON i.BasketId = b.Id INNER JOIN Forecasts f ON f.Id = i.ForecastId WHERE f.Status = 'Sale' AND b.UserId = @id ", new { id = userId })).ToList();
                return forecastsIds.Count() * 99;
            }
        }

        public async Task<int> AddOrder(string userName, int sum, string paymentId)
        {
            using(var db = new SqlConnection(connectionString))
            {
                string userId = await GetUserId(userName);

                int orderId = await db.QueryFirstOrDefaultAsync<int>("INSERT INTO Orders (UserId, Status, CreateDate, Sum, PayCode) VALUES(@id, @status, @date, @sum, @paymentId); SELECT SCOPE_IDENTITY();", new { id = userId, status = "pending", date = DateTime.Now, sum = sum, paymentId = paymentId });
                var forecastsIds = (await db.QueryAsync<int>("SELECT f.Id AS Id FROM BasketItems i INNER JOIN Baskets b ON i.BasketId = b.Id INNER JOIN Forecasts f ON f.Id = i.ForecastId WHERE f.Status = 'Sale' AND b.UserId = @id ", new { id = userId })).ToList();
                foreach(var forecast in forecastsIds)
                {
                    await db.ExecuteAsync("INSERT INTO OrderForecasts (OrderId, ForecastId) VALUES(@orderId, @forecastId )", new { orderId = orderId, forecastId = forecast });
                }
                return orderId;


            }
        }
        public async Task CompleteOrder(string paymentId)
        {
            using(var db = new SqlConnection(connectionString))
            {
                string userId = await db.QueryFirstOrDefaultAsync<string>("UPDATE Orders SET Status = 'success' WHERE PayCode = @paymentId;SELECT UserId FROM Orders WHERE PayCode = @paymentId", new { paymentId = paymentId });
                string userPhone = await db.QueryFirstOrDefaultAsync<string>("SELECT PhoneNumber FROM AspNetUsers WHERE Id = @userId", new { userId = userId });

                _ = _smsPusher.Send(userPhone, new List<string>(),
                    "Вы успешно сделали покупку на сайте smsbet.ru" + Environment.NewLine +
                    "За 15 минут до начала события вы получите сообщение с прогнозом. ");
                
                
                int orderId = await db.QueryFirstOrDefaultAsync<int>("SELECT Id FROM Orders WHERE PayCode = @paymentId", new { paymentId });
                var forecasts = await db.QueryAsync<int>("SELECT ForecastId FROM OrderForecasts WHERE OrderId = @orderId", new { orderId = orderId });
                foreach(var forecast in forecasts)
                {
                    await db.ExecuteAsync("INSERT INTO ForecastPhonesBuys (ForecastId, UserId, UserPhone) VALUES (@forecast, @userId, @userPhone)", new { forecast, userId, userPhone });
                }
                await db.ExecuteAsync("DELETE FROM Baskets WHERE UserId = @userId", new { userId });


            }
        }

        public async Task<int> CheckFreePromocode(string promocode, string userName)
        {
            using(var db = new SqlConnection(connectionString))
            {
                var used = await db.QueryFirstOrDefaultAsync<bool>("SELECT FreePromocode FROM AspNetUsers WHERE Email = @userName", new { userName = userName });
                if (used)
                {
                    return 3;
                }
                string promo = await db.QueryFirstOrDefaultAsync<string>("SELECT Value FROM AppSettings WHERE Keys = 'FreePromocode'");
                if(promocode != promo)
                {
                    return 0;
                }
                else
                {
                    string id = await GetUserId(userName);
                    int count = await db.QueryFirstOrDefaultAsync<int>(@"DECLARE @id INT, @count INT;
                                                                SET @id = (SELECT Id FROM Baskets WHERE UserId = @userId);
                                                                    SET @count = (SELECT Count(Id) FROM BasketItems WHERE BasketId = @id);
                                                                    SELECT @count; ", new { userId = id });
                    if(count != 1)
                    {
                        return 2;
                    }
                    else
                    {
                        await db.ExecuteAsync(@"UPDATE AspNetUsers SET FreePromocodeWrite = @tr WHERE Email = @userName", new { tr = true, userName = userName });
                        return 1;
                    }

                }
                
            }
        }

        public async Task<CardViewModel> GetCardViewModel(string userName, string itemsCookie)
        {
            using (var db = new SqlConnection(connectionString))
            {
				var viewModel = new CardViewModel();
				viewModel.UserCardForecasts = new List<Forecasts>();
				if (userName == null)
				{
					if(itemsCookie != null)
					{
						var itemsList = itemsCookie.Split(',').Where(w => w != "").ToList();
                        var forecastsIds = (await db.QueryAsync<string>("SELECT Id FROM Forecasts")).ToList();
                        foreach (var item in itemsList)
						{
                            if(!forecastsIds.Contains(item)) continue;
                            
							viewModel.UserCardForecasts.Add(await db.QueryFirstOrDefaultAsync<Forecasts>("SELECT StartTime, IntervalKoof, PublicPrognoz, ChampionatName, Id FROM Forecasts WHERE Id = @id", new { id = Convert.ToInt32(item) }));
						}
					}
					
					
					
				}
				else
				{
					if (userName != null && itemsCookie != "" && itemsCookie != null)
					{
						string userId = await GetUserId(userName);
						var itemsList = itemsCookie.Split(',').Where(w => w != "").ToList();
						List<int> idsCardUser = (await db.QueryAsync<int>("SELECT bi.ForecastId AS Id FROM BasketItems bi INNER JOIN Baskets b ON bi.BasketId = b.Id WHERE b.UserId = @userId", new { userId = userId })).ToList();
						itemsList = itemsList.Select(s => Convert.ToInt32(s)).Where(w => !idsCardUser.Contains(Convert.ToInt32(w))).Select(s => s.ToString()).ToList();
						idsCardUser.AddRange(itemsList.Select(s => Convert.ToInt32(s)));
						foreach (var id in idsCardUser)
						{
							await AddToBasket(userName, id);
						}
					}
					viewModel.UserCardForecasts = (await db.QueryAsync<Forecasts>("SELECT f.StartTime, f.IntervalKoof, f.PublicPrognoz, f.ChampionatName, f.Id AS Id FROM BasketItems i INNER JOIN Baskets b ON i.BasketId = b.Id INNER JOIN Forecasts f ON f.Id = i.ForecastId WHERE f.Status = 'Sale' AND b.UserId = @id ", new { id = await GetUserId(userName) })).ToList();
					viewModel.ClearCookie = true;
				}

				viewModel.RandomForecasts = (await db.QueryAsync<Forecasts>("SELECT * FROM Forecasts WHERE Status = 'Sale'")).ToList().Where(w => !viewModel.UserCardForecasts.Select(s => s.Id).Contains(w.Id)).ToList();
				return viewModel;
            }
        }

        public async Task AddToBasket(string userName, int forecastId)
        {
            string userId = await GetUserId(userName);

            using (var db = new SqlConnection(connectionString))
            {
                int? basketId = await db.QueryFirstOrDefaultAsync<int>("SELECT Id FROM Baskets WHERE UserId = @id", new { id = userId });
                if(basketId == 0)
                {
                    basketId =  await db.QueryFirstOrDefaultAsync<int>("INSERT INTO Baskets (UserId) VALUES(@id);SELECT SCOPE_IDENTITY();", new { id = userId });
                }
                int? forecast = await db.QueryFirstOrDefaultAsync<int>("SELECT Id FROM BasketItems WHERE ForecastId = @id AND BasketId = @basket", new { id = forecastId, basket = basketId });
                if(forecast == 0)
                {
                    await db.ExecuteAsync("INSERT INTO BasketItems (BasketId, ForecastId) VALUES(@basket, @forecast)", new { basket = basketId, forecast = forecastId });
                }


            }
        }

        public async Task SmsPlus()
        {
            using (var db = new SqlConnection(connectionString))
            {
                await db.ExecuteAsync("UPDATE AppSettings SET Value = (CONVERT(INT, Value) + 1) WHERE Keys = 'CountSms'");
            }
        }

        public async Task UserPlus()
        {
            using (var db = new SqlConnection(connectionString))
            {
                await db.ExecuteAsync("UPDATE AppSettings SET Value = (CONVERT(INT, Value) + 1) WHERE Keys = 'CountUsers'");
            }
        }

        public async Task<User> GetUser(string phone)
        {
            using(var db = new SqlConnection(connectionString))
            {
                return await db.QueryFirstOrDefaultAsync<User>("SELECT * FROM AspNetUsers WHERE Email = @phone", new { phone });
            }
        }

        public async Task<string> GetInfoBar()
        {

            using (var db = new SqlConnection(connectionString))
            {
                return await db.QueryFirstOrDefaultAsync<string>("SELECT Value FROM AppSettings WHERE Keys = 'InfoBar'");
            }

        }

        public async Task SetInfoBar(string text)
        {
            using (var db = new SqlConnection(connectionString))
            {
                await db.ExecuteAsync("UPDATE AppSettings SET Value = @text WHERE Keys = 'InfoBar'", new { text });
            }
        }

        public async Task<MainPageViewModel> GetMainPageView()
        {
            using(var db = new SqlConnection(connectionString))
            {
                var view = new MainPageViewModel();

                view.CountSms = await db.QueryFirstOrDefaultAsync<string>("SELECT Value FROM AppSettings WHERE Keys = 'CountSms'");
                view.CountSuccessForecast = await db.QueryFirstOrDefaultAsync<string>("SELECT Value FROM AppSettings WHERE Keys = 'CountSuccessForecast'");
                view.CountUsers = await db.QueryFirstOrDefaultAsync<string>("SELECT Value FROM AppSettings WHERE Keys = 'CountUsers'");

                var forList = new List<AddForecastViewModel>();
                forList = (await db.QueryAsync<AddForecastViewModel>("SELECT * FROM Forecasts WHERE Status = 'Sale'")).ToList();
                
                for(int i = 0; i < forList.Count(); i++)
                {
                    forList[i].Bookmaker = String.Join(", ", (await db.QueryAsync<string>("SELECT BookMaker FROM BookmakerForecasts WHERE ForecastId = @i", new { i = forList[i].Id })).ToArray());
                }
                view.Forecasts = forList;
                return view;
            }
        }

        public async Task<string> ResendPassword(string phone)
        {
            using (var client = new WebClient())
            {
                string pass;
                using(var db = new SqlConnection(connectionString))
                {
                    pass = await db.QueryFirstOrDefaultAsync<string>("SELECT PasswordText FROM AspNetUsers WHERE Email = @phone", new { phone });
                }
                var res = await _smsPusher.Send(phone, new List<string>(),
                        "Ваш пароль: " + pass);

                if (res.Contains("OK"))
                {
                    _ = SmsPlus();
                    return "ok";
                }
                return "badsms";
            }
        }

        public async Task<bool> SendSms(string code, string phone)
        {
            
            var res = await _smsPusher.Send(phone, new List<string>(), code);
            if (res.Contains("OK")) 
            { 
                await SmsPlus(); 
                return true;
            }

            return false;
                
        }

        public async Task<bool> CheckPassword(string password, string phone)
        {
            using(var db = new SqlConnection(connectionString))
            {
                string pass = await db.QueryFirstOrDefaultAsync<string>("SELECT PasswordText FROM AspNetUsers WHERE Email = @phone", new { phone });
                if(pass == password)
                {
                    return true;
                }
                return false;
            }
        }

        public async Task<string> GeneratePassword()
        {
            using (var client = new WebClient())
            {
                await UserPlus();
                return await client.DownloadStringTaskAsync("https://www.passwordrandom.com/query?command=password&format=plain&count=1&scheme=CvvCVCv");
            }
        }
    }
}
