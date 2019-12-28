using Dapper;
using LinqToDB;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Service;
using Smsbet.Web.Models;
using Smsbet.Web.ViewModels;

namespace Smsbet.Web.Repository
{
    public interface IAdminRepository
    {
        Task AddForecast(AddForecastViewModel model);
        Task<List<ForecastListViewModel>> GetForecastsListViewModel();

        Task DeleteForecast(int id);
        Task<List<User>> GetUsers();
        Task<string> GetPercentPlus();
        Task SendSmsForAll(int forecastId);
        Task SetFreePromocode(string code);
        Task<PromocodeViewModel> GetPromocodeViewModel();
		Task ChangeStatus(int id, string status, string result);
        Task SetPercentWin(string percent);
        Task SendSmsAboutStatus(int forecastId, string status);
        Task<Forecasts> GetForecastById(int forecastId);
        Task UpdateForecast(Forecasts forecast);
        Task StopBuy(int forecastId);
        Task DeleteFromBasket(int forecastId);
        Task SendSMSPushing(string body);
        Task<string> GetEmails();
    }
    public class AdminRepository : IAdminRepository
    {
        private readonly string connectionString;
        private readonly IMessagePusher _smsPusher;
        public AdminRepository(IConfiguration configuration,
            IMessagePusher smsPusher)
        {
            _smsPusher = smsPusher;
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<string> GetEmails()
        {
            using (var db = new DbNorthwind())
            {
                return String.Join("\n", await db.Users
                    .Where(w => w.IsEmailPushing && w.Email != null)
                    .Select(s => s.UserEmail)
                    .ToListAsync());
            }
        }

        public async Task StopBuy(int forecastId)
        {
            using (var db = new DbNorthwind())
            {
                var forecastStatus = (await db.Forecasts
                    .FirstOrDefaultAsync(w => w.Id == forecastId)).Status;

                if (forecastStatus == "Sale")
                {
                    await db.Forecasts
                        .Where(w => w.Id == forecastId)
                        .Set(s => s.Status, "Complete")
                        .UpdateAsync();
                }
                
                _ = DeleteFromBasket(forecastId);
            }
        }

        public async Task SendSMSPushing(string body)
        {
            using (var db = new DbNorthwind())
            {
                var phonesList = await db.Users
                    .Where(w => w.IsSmsPushing)
                    .Select(s => s.PhoneNumber)
                    .ToListAsync();
                
                if(phonesList.Count() > 1)
                {
                    _ = _smsPusher.Send(null, phonesList, body);
                }
                else
                {
                    _ = _smsPusher.Send(phonesList.First(), new List<string>(), body);
                }

                _ = SmsPlus(phonesList.Count);
                
            }
        }

        public async Task DeleteFromBasket(int forecastId)
        {
            using (var db = new DbNorthwind())
            {
                await db.BasketItems
                    .Where(w => w.ForecastId == forecastId)
                    .DeleteAsync();
            }
        }

        public async Task SendSmsForAll(int forecastId)
        {
            using(var db = new SqlConnection(connectionString))
            {
                List<string> phonesList = (await db.QueryAsync<string>("SELECT UserPhone FROM ForecastPhonesBuys WHERE ForecastId = @forecastId", new { forecastId })).ToList();
                var forecast = await db.QueryFirstOrDefaultAsync<Forecasts>("SELECT * FROM Forecasts WHERE Id = @forecastId", new { forecastId });
                if(phonesList.Count() > 1)
                {

                    _ = _smsPusher.Send(null, phonesList,
                        $"Рады сообщить, что для вас готова ставка:{forecast.Game}. {forecast.ChampionatName}. {forecast.ForecastText}. {forecast.PublicPrognoz} ");
                }
                else
                {
                    _ =_smsPusher.Send(phonesList.First(), new List<string>(), 
                        $"Рады сообщить, что для вас готова ставка:{forecast.Game}. {forecast.ChampionatName}. {forecast.ForecastText}. {forecast.PublicPrognoz} ");
                }
                await db.ExecuteAsync("UPDATE Forecasts SET Status = 'Complete' WHERE Id = @forecastId", new { forecastId });
                _ = SmsPlus(phonesList.Count);

            }
        }

        public async Task<Forecasts> GetForecastById(int forecastId)
        {
            using(var db = new DbNorthwind())
            {
                var query = from s in db.Forecasts
                            where s.Id == forecastId
                            select s;
                Forecasts appSettings = await query.FirstOrDefaultAsync();
                return appSettings;
            }
        }

        public async Task UpdateForecast(Forecasts forecast)
        {
            using (var db = new DbNorthwind())
            {
                await db.Forecasts.Where(w => w.Id == forecast.Id)
                    .Set(s => s.Game, forecast.Game)
                    .Set(s => s.IntervalKoof, forecast.IntervalKoof)
                    .Set(s => s.KoofProxoda, forecast.KoofProxoda)
                    .Set(s => s.StartTime, forecast.StartTime)
                    .Set(s => s.ChampionatName, forecast.ChampionatName)
                    .Set(s => s.PercentReturn, forecast.PercentReturn)
                    .Set(s => s.PublicPrognoz, forecast.PublicPrognoz)
                    .Set(s => s.ForecastText, forecast.ForecastText)
                    .UpdateAsync();
            }
        }

        public async Task SendSmsAboutStatus(int forecastId, string status)
        {
            using(var db = new SqlConnection(connectionString))
            {
                List<string> phonesList = (await db.QueryAsync<string>("SELECT UserPhone FROM ForecastPhonesBuys WHERE ForecastId = @forecastId", new { forecastId })).ToList();
                if (phonesList.Count() > 1)
                {

                    if (status == "Success")
                        _ =_smsPusher.Send(null, phonesList,
                            "Поздравляем!" + Environment.NewLine + "Ваша ставка прошла." + Environment.NewLine +
                            "Оставьте положительный отзыв перейдя по ссылке https://bitly.su/KLuhVrAT и получите 200 рублей на счет.");

                    else if (status == "Fail")
                        _ =_smsPusher.Send(null, phonesList,
                            "К сожалению, сегодня удача не на нашей стороне. " + Environment.NewLine +
                            "Возврат средств по купленному прогнозу переведен в личный кабинет: Smsbet.Web.ru/lk");



                }
                else if(phonesList.Any())
                {
                    if (status == "Success")
                        _ =_smsPusher.Send(phonesList.First(),new List<string>() , 
                            "Поздравляем!" + Environment.NewLine + "Ваша ставка прошла." + Environment.NewLine +
                            "Оставьте положительный отзыв перейдя по ссылке https://bitly.su/KLuhVrAT и получите 200 рублей на счет.");
                    
                    else if (status == "Fail")
                        _ =_smsPusher.Send(phonesList.First(), new List<string>(), 
                            "К сожалению, сегодня удача не на нашей стороне. " + Environment.NewLine +
                            "Возврат средств по купленному прогнозу переведен в личный кабинет: Smsbet.Web.ru/lk");
                    
                    
                }
                _ =SmsPlus(phonesList.Count);
            }
            
        }


        public async Task DeleteForecast(int id)
        {
            using (var db = new SqlConnection(connectionString))
            {
                await db.ExecuteAsync("DELETE FROM Forecasts WHERE Id = @id; DELETE FROM  BookmakerForecasts WHERE ForecastId = @id ", new { id = id });
                await db.ExecuteAsync("DELETE FROM BasketItems WHERE ForecastId = @id", new { id = id });
            }
        }

        public async Task SetPercentWin(string percent)
        {
            using(var db = new SqlConnection(connectionString))
            {
                await db.ExecuteAsync("UPDATE AppSettings SET Value = @percent Where Keys = 'PercentPlus'", new { percent = percent });
            }
        }

        public async Task<PromocodeViewModel> GetPromocodeViewModel()
        {
            using(var db = new SqlConnection(connectionString))
            {
                var viewModel = new PromocodeViewModel();
                viewModel.FreePromocode = await db.QueryFirstOrDefaultAsync<string>("SELECT Value FROM AppSettings WHERE Keys = 'FreePromocode'");
                return viewModel;
            }
        }

		public async Task ChangeStatus(int id, string status, string result)
		{
			using (var db = new SqlConnection(connectionString))
			{
				await db.ExecuteAsync("UPDATE Forecasts SET Status = @status, Result = @result WHERE Id = @id", new { status = status ,id = id, result = result });
				if(status == "Success")
				{
					await db.ExecuteAsync("UPDATE AppSettings SET Value = (CONVERT(INT, Value) + 1) WHERE Keys = 'CountSuccessForecast'");
				}

                await DeleteFromBasket(id);
            }
		}

		public async Task SetFreePromocode(string code)
        {
            using(var db = new SqlConnection(connectionString))
            {
                await db.ExecuteAsync("UPDATE AppSettings SET Value = @code WHERE Keys = 'FreePromocode'", new { code = code });
            }
        }

        public async Task<string> GetPercentPlus()
        {
            using (var db = new SqlConnection(connectionString))
            {
                return await db.QueryFirstOrDefaultAsync<string>("SELECT Value FROM AppSettings WHERE Keys = 'PercentPlus'");
            }
        }

        public async Task<List<User>> GetUsers()
        {
            using(var db = new SqlConnection(connectionString))
            {
                return (await db.QueryAsync<User>("SELECT Email FROM AspNetUsers")).ToList();
            }
        }

        public async Task<List<ForecastListViewModel>> GetForecastsListViewModel()
        {
            using (var db = new SqlConnection(connectionString))
            {
                var returnList = new List<ForecastListViewModel>();
                var forecasts = (await db.QueryAsync<Forecasts>("SELECT * FROM Forecasts")).ToList();
                foreach(var fore in forecasts)
                {
                    var viewModel = new ForecastListViewModel();

                    viewModel.Forecast = fore;
                    List<string> bookMakersList = (await db.QueryAsync<string>("SELECT BookMaker FROM BookmakerForecasts WHERE ForecastId = @id", new { id = fore.Id })).ToList();
                    viewModel.Bookmakers = bookMakersList;

                    returnList.Add(viewModel);
                }
                return returnList;
            }
        }

        public async Task SmsPlus(int count)
        {
            using (var db = new SqlConnection(connectionString))
            {
                await db.ExecuteAsync("UPDATE AppSettings SET Value = (CONVERT(INT, Value) + @count) WHERE Keys = 'CountSms'", new { count });
            }
        }

        public async Task AddForecast(AddForecastViewModel model)
        {
            using(var db = new SqlConnection(connectionString))
            {
                int te = await db.QueryFirstAsync<int>("INSERT INTO Forecasts (StartTime, ChampionatName, IntervalKoof, Game, KoofProxoda, PercentReturn, ForecastText, Status, PublicPrognoz) VALUES(@StartTime, @ChampionatName, @IntervalKoof, @Game, @KoofProxoda, @PercentReturn, @ForecastText, @Status, @PublicPrognoz); SELECT SCOPE_IDENTITY();", new {StartTime = model.StartTime, ChampionatName = model.ChampionatName, IntervalKoof = model.IntervalKoof, Game = model.Game, KoofProxoda = model.KoofProxoda, PercentReturn = model.PercentReturn, ForecastText = model.ForecastText, Status = "Sale", PublicPrognoz = model.PublicPrognoz });
                if (model.Bookmaker.Contains(","))
                {
                    if (model.Bookmaker.Contains("Все"))
                    {
                        var massBookmakers = new List<string>(){ "Фонбет", "Bwin", "Лига ставок", "Олимп", "Пари матч", "Bet City", "1xBet", "Балт Бет", "Tennisi Bet", "Leon", "Winline", "Marathon Bet"};
                        var bookmakersList = new List<BookmakerForecasts>();
                        foreach (string str in massBookmakers)
                        {
                            bookmakersList.Add(new BookmakerForecasts()
                            {
                                ForecastId = te,
                                BookMaker = str
                            });
                            //await db.ExecuteAsync("INSERT INTO BookmakerForecasts (ForecastId, BookMaker) VALUES(@id, @bookmaker)", new { id = te, bookmaker = str });
                        }
                        using (var linq = new DbNorthwind())
                        {
                            linq.BookmakerForecasts.BulkCopy(bookmakersList);
                        }
                    }
                    else
                    {
                        foreach (string str in model.Bookmaker.Split(new char[] { ',' }))
                        {
                            await db.ExecuteAsync("INSERT INTO BookmakerForecasts (ForecastId, BookMaker) VALUES(@id, @bookmaker)", new { id = te, bookmaker = str });
                        }
                    }
                    
                }
                else
                {
                    if(model.Bookmaker == "Все")
                    {
                        var massBookmakers = new List<string>() { "Фонбет", "Bwin", "Лига ставок", "Олимп", "Пари матч", "Bet City", "1xBet", "Балт Бет", "Tennisi Bet", "Leon", "Winline", "Marathon Bet" };
                        var bookmakersList = new List<BookmakerForecasts>();
                        foreach (string str in massBookmakers)
                        {
                            bookmakersList.Add(new BookmakerForecasts()
                            {
                                ForecastId = te,
                                BookMaker = str
                            });
                            //await db.ExecuteAsync("INSERT INTO BookmakerForecasts (ForecastId, BookMaker) VALUES(@id, @bookmaker)", new { id = te, bookmaker = str });
                        }
                        using (var linq = new DbNorthwind())
                        {
                            linq.BookmakerForecasts.BulkCopy(bookmakersList);
                        }
                        
                    }
                    else
                    {
                        await db.ExecuteAsync("INSERT INTO BookmakerForecasts (ForecastId, BookMaker) VALUES(@id, @bookmaker)", new { id = te, bookmaker = model.Bookmaker });
                    }
                }
                    
            }
        }
    }
}
