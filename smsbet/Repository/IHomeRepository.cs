using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LinqToDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Smsbet.Web.Models;
using Smsbet.Web.ViewModels;

namespace Smsbet.Web.Repository
{
    public interface IHomeRepository
    {
        Task<InfoViewModel> GetInfoViewModel(int forecastId);
        Task AddViewForecastView(string userEmail, int forecastId);
        Task<int> CalcSumWin(int sum);
        Task UpdatePushing(string isType, bool value, string userName);
        Task<List<StatViewModel>> GetForecastForStat(int month, int day, string sortBy);
        Task<List<string>> GetUserInfo(string userName);
        Task<string> CheckCodeNewPassword(string code, string userName);
        Task<string> CheckCodeNewEmail(string code, string userName);
        Task<string> CheckCodeNewPhone(string code, string userName, UserManager<User> userManager);
        Task<bool> CheckBalancePay(string userName, int sum, IAccountRepository repository);
    }
    public class HomeRepository : IHomeRepository
    {
        private readonly string connectionString;
        public HomeRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task UpdatePushing(string isType, bool value, string userName)
        {
            using (var db = new DbNorthwind())
            {
                if (isType == "sms")
                {
                    await db.Users
                        .Where(w => w.Email == userName)
                        .Set(s => s.IsSmsPushing, value)
                        .UpdateAsync();
                }
                else if (isType == "email")
                {
                    await db.Users
                        .Where(w => w.Email == userName)
                        .Set(s => s.IsEmailPushing, value)
                        .UpdateAsync();
                }
            }
        }

        public async Task<string> CheckCodeNewPassword(string code, string userName)
        {
            using (var db = new DbNorthwind())
            {
                var user = await db.Users.FirstOrDefaultAsync(f => f.UserName == userName);
                if (user.ConfirmPasswordCode != code)
                {
                    return "Неверный код";
                }

                return "success";
            }
        }

        public async Task<bool> CheckBalancePay(string userName, int sum, IAccountRepository repository)
        {
            using (var db = new DbNorthwind())
            {
                var user = await db.Users
                    .FirstOrDefaultAsync(f => f.Email == userName);

                if (user.Balance >= sum)
                {
                    var guid = Guid.NewGuid().ToString();
                    await repository.AddOrder(userName, sum, guid);

                    user.Balance = user.Balance - sum;
                    await db.UpdateAsync(user);
                    _ = repository.CompleteOrder(guid);

                    return true;
                }
                return false;

            }
        }

        public async Task<string> CheckCodeNewEmail(string code, string userName)
        {
            using (var db = new DbNorthwind())
            {
                var user = await db.Users.FirstOrDefaultAsync(f => f.UserName == userName);
                if (user.NewUserEmailCode != code)
                {
                    return "Неверный код";
                }

                await db.Users.Where(w => w.UserName == userName)
                    .Set(s => s.UserEmail, user.NewUserEmail)
                    .Set(s => s.NewUserEmail, String.Empty)
                    .Set(s => s.NewUserEmailCode, String.Empty)
                    .UpdateAsync();

                return "success";
            }
        }
        public async Task<string> CheckCodeNewPhone(string code, string userName, UserManager<User> userManager)
        {
            using (var db = new DbNorthwind())
            {
                var user = await db.Users.FirstOrDefaultAsync(f => f.UserName == userName);
                if (user.NewUserPhoneCode != code)
                {
                    return "Неверный код";
                }

                var token = await userManager.GenerateChangeEmailTokenAsync(user, user.NewUserPhone);
                _ = userManager.ChangeEmailAsync(user, user.NewUserPhone, token);

                _ = db.Users.Where(w => w.UserName == userName)
                    .Set(s => s.PhoneNumber, user.NewUserPhone)
                    .Set(s => s.NewUserPhone, String.Empty)
                    .Set(s => s.NewUserPhoneCode, String.Empty)
                    .UpdateAsync();
                
                

                return "success";
            }
        }

        public async Task AddViewForecastView(string userEmail, int forecastId)
        {
            using (var db = new SqlConnection(connectionString))
            {
                var forecastsView = new ForecastsViews { ForecastId = forecastId, Time = DateTime.Now, UserEmail = userEmail };
                await db.ExecuteAsync("INSERT INTO ForecastsViews (ForecastId, UserEmail, Time) VALUES(@id, @email, @time)", new { id = forecastsView.ForecastId, email = forecastsView.UserEmail, time = forecastsView.Time });
            }
        }

        public async Task<List<string>> GetUserInfo(string userName)
        {
            using (var db = new DbNorthwind())
            {
                var user = await db.Users
                    .FirstOrDefaultAsync(f => f.Email == userName);
                

                return new List<string>
                {
                    user.PhoneNumber, 
                    user.UserEmail, 
                    user.IsSmsPushing.ToString(), 
                    user.IsEmailPushing.ToString(), 
                    ((int)user.Balance).ToString()
                };
            }
        }



        public async Task<List<StatViewModel>> GetForecastForStat(int month, int day, string sortBy)
        {
            
            using(var db = new SqlConnection(connectionString))
            {
                var forecasts = await db.QueryAsync<Forecasts>("SELECT * FROM Forecasts WHERE Status = 'Success' OR Status = 'Fail'");
                var successForecast = forecasts.Where(w => w.Status == "Success");
                var failForecast = forecasts.Where(w => w.Status == "Fail");

                if (sortBy == null)
                {
                    var forecast = successForecast.Where(w => w.StartTime.Month == (month + 1) && w.StartTime.Day == day).OrderByDescending(o => o.Status).ToList();
                    forecast.AddRange(failForecast.Where(w => w.StartTime.Month == (month + 1) && w.StartTime.Day == day).ToList());
                    var modelList2 = new List<StatViewModel>();
                    foreach(var forec in forecast)
                    {
                        var model = new StatViewModel();
                        model.Forecast = forec;
                        model.CountViews = await db.QueryFirstOrDefaultAsync<int>("SELECT Count(Id) FROM ForecastsViews WHERE ForecastId = @id", new { id = forec.Id });
                        modelList2.Add(model);
                    }
                    return modelList2;
                }
                else if(sortBy == "popular")
                {
                    Dictionary<int, int> counts = new Dictionary<int, int>();
                    foreach(var forecast in successForecast.Where(w => w.StartTime.Month == (month + 1) && w.StartTime.Day == day).ToList())
                    {
                        var timeMinus = DateTime.Now.AddHours(-1);
                        int count = await db.QueryFirstOrDefaultAsync<int>("SELECT COUNT(DISTINCT UserEmail) FROM ForecastsViews WHERE ForecastId = @id", new { id = forecast.Id });
                        counts.Add(forecast.Id, count);
                    }
                    var items = counts.OrderByDescending(j => j.Value);
                    var forec = new List<Forecasts>();
                    foreach(var fore in items)
                    {
                        forec.Add(successForecast.FirstOrDefault(w => w.Id == fore.Key));
                    }
                    forec.AddRange(failForecast.Where(w => w.StartTime.Month == (month + 1) && w.StartTime.Day == day).ToList());
                    var modelList3 = new List<StatViewModel>();
                    foreach (var fore in forec)
                    {
                        var model = new StatViewModel();
                        model.Forecast = fore;
                        model.CountViews = await db.QueryFirstOrDefaultAsync<int>("SELECT Count(Id) FROM ForecastsViews WHERE ForecastId = @id", new { id = fore.Id });
                        modelList3.Add(model);
                    }
                    return modelList3;

                }
                else if(sortBy == "price")
                {
                    var dfdfg = successForecast.Where(w => w.StartTime.Month == (month + 1) && w.StartTime.Day == day).OrderByDescending(o => Convert.ToDouble(o.IntervalKoof.Split("-")[1].Replace(".", ","))).ToList();
                    dfdfg.AddRange(failForecast.Where(w => w.StartTime.Month == (month + 1) && w.StartTime.Day == day).ToList());
                    var modelList2 = new List<StatViewModel>();
                    foreach (var fore in dfdfg)
                    {
                        var model = new StatViewModel();
                        model.Forecast = fore;
                        model.CountViews = await db.QueryFirstOrDefaultAsync<int>("SELECT Count(Id) FROM ForecastsViews WHERE ForecastId = @id", new { id = fore.Id });
                        modelList2.Add(model);
                    }
                    return modelList2;
                }

                forecasts = forecasts.Where(w => w.StartTime.Month == (month + 1) && w.StartTime.Day == day).OrderByDescending(o => o.Status).ToList();
                var modelList = new List<StatViewModel>();
                foreach (var fore in forecasts)
                {
                    var model = new StatViewModel();
                    model.Forecast = fore;
                    model.CountViews = await db.QueryFirstOrDefaultAsync<int>("SELECT Count(Id) FROM ForecastsViews WHERE ForecastId = @id", new { id = fore.Id });
                    modelList.Add(model);
                }
                return modelList;

            }
        }

        public async Task<int> CalcSumWin(int sum)
        {
            using(var db = new SqlConnection(connectionString))
            {
                int percent;
                int.TryParse(await db.QueryFirstOrDefaultAsync<string>("SELECT Value FROM AppSettings WHERE Keys = 'PercentPlus'"), out percent);
                return Convert.ToInt32(Convert.ToDouble(percent) / Convert.ToDouble(100) * Convert.ToDouble(sum));
            }
        }

        public async Task<InfoViewModel> GetInfoViewModel(int forecastId)
        {
            using (var db = new SqlConnection(connectionString))
            {
                var model =  new InfoViewModel();
                var timeMinus = DateTime.Now.AddHours(-1);
                model.CountViews = await db.QueryFirstOrDefaultAsync<int>("SELECT COUNT(DISTINCT UserEmail) FROM ForecastsViews WHERE ForecastId = @id AND Time > @time", new { id = forecastId, time = timeMinus });
                model.Forecast = await db.QueryFirstOrDefaultAsync<Forecasts>("SELECT * FROM Forecasts WHERE Id = @id", new { id = forecastId });

                return model;
            }
        }
    }
}
