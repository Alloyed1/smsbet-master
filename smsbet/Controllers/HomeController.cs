using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using LinqToDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using Smsbet.Web.Models;
using Smsbet.Web.Repository;
using Smsbet.Web.ViewModels;
using Yandex.Checkout.V3;

namespace Smsbet.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IHomeRepository _homeRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<User> _userManager;
        static readonly Client _client = new Client("636277", "test_0MYGrT71SMVbgqqLI4LepYRutPbKwTrmjBicjA7_96A");

        public HomeController(IAccountRepository accountRepository, IHomeRepository homeRepository,
            IHttpContextAccessor accessor, UserManager<User> context)
        {
            _accountRepository = accountRepository;
            _homeRepository = homeRepository;
            _accessor = accessor;
            _userManager = context;
        }

        [HttpGet]
        public async Task<int> CalcSumWin(int sum)
        {
            return await _homeRepository.CalcSumWin(sum);
        }

        [HttpGet]
        public async Task<List<StatViewModel>> GetForecastForStat(int month, int day, string sortBy)
        {
            return await _homeRepository.GetForecastForStat(month, day, sortBy);
        }

        [HttpGet]
        public async Task<string> GetInfoBar()
        {
            return await _accountRepository.GetInfoBar();
        }

        [HttpGet]
        public async Task<string> PayBalance(int sum)
        {
            var newPayment = new NewPayment
            {
                Amount = new Amount {Value = sum, Currency = "RUB"},
                Confirmation = new Confirmation
                {
                    Type = ConfirmationType.Redirect,
                    ReturnUrl = $"https://smsbet.ru",

                },
                Description = "balance:" + User.Identity.Name
            };

            Payment payment = _client.CreatePayment(newPayment);
            return payment.Confirmation.ConfirmationUrl;
        }

        [HttpGet]
        public async Task<IActionResult> Pay()
        {
            int sum = await _accountRepository.GetSumOrder(User.Identity.Name);

            if (sum == 0)
            {
                return RedirectToAction("Card", "Account");
            }
            

            if (sum == 99 && await _accountRepository.UserWriteFreePromocode(User.Identity.Name))
            {
                await _accountRepository.AddOrder(User.Identity.Name, 0, User.Identity.Name);
                await _accountRepository.CompleteOrder(User.Identity.Name);
                await _accountRepository.SetWhatUserUserFreePromocode(User.Identity.Name);
                return RedirectToAction("Index", "Home");
            }
            
            if (_homeRepository.CheckBalancePay(User.Identity.Name, sum, _accountRepository).Result)
            {
                return RedirectToAction("Index", "Home");
            }
            
            else
            {
                var newPayment = new NewPayment
                {
                    Amount = new Amount {Value = sum, Currency = "RUB"},
                    Confirmation = new Confirmation
                    {
                        Type = ConfirmationType.Redirect,
                        ReturnUrl = $"https://smsbet.ru",

                    },
                };
                Payment payment = _client.CreatePayment(newPayment);

                int orderId = await _accountRepository.AddOrder(User.Identity.Name, sum, payment.Id);

                string url = payment.Confirmation.ConfirmationUrl;
                return Redirect(url);
            }
            

        }

        public class Object
        {
            public string id;
            public string status;
            public string description;
            public amount Amount;
        }

        public class objectClass
        {
            public string type;
            public Object Object;
        }

        public class amount
        {
            public decimal value;
        }

        [HttpGet]
        public async Task<string> CheckPasswordCorrect(string password)
        {
            if (password.Length < 6)
            {
                return "Мин. длина пароля 6 символов";
            }
            else
            {
                using (var db = new DbNorthwind())
                {
                    var user = await db.Users.FirstOrDefaultAsync(f => f.UserName == User.Identity.Name);
                    string code = await _accountRepository.GeneratePassword();
                    await _accountRepository.SendSms($"Код: {code}", user.PhoneNumber);


                    await db.Users
                        .Where(w => w.Id == user.Id)
                        .Set(s => s.NewConfirmPassword, password)
                        .Set(s => s.ConfirmPasswordCode, code)
                        .UpdateAsync();


                }

                return "success";
            }
        }

        [HttpGet]
        public async Task<string> CheckEmailCorrect(string email)
        {

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            
            if (match.Success)
            {
                using (var db = new DbNorthwind())
                {
                    var anyUser = await db.Users.FirstOrDefaultAsync(f => f.UserEmail == email);
                    if (anyUser != null) return "Email занят";
                    
                    var user = await db.Users.FirstOrDefaultAsync(f => f.UserName == User.Identity.Name);
                    string code = await _accountRepository.GeneratePassword();
                    await _accountRepository.SendSms($"Код: {code}", user.PhoneNumber);


                    await db.Users
                        .Where(w => w.Id == user.Id)
                        .Set(s => s.NewUserEmail, email)
                        .Set(s => s.NewUserEmailCode, code)
                        .UpdateAsync();


                }

                return "success";
            }

            return "Невалидный Email";
        }
        [HttpGet]
        public async Task<string> CheckPhoneCorrect(string phone)
        {

            if (phone.Length != 10)
            {
                return "Невалидный номер телефона";
            }
            using (var db = new DbNorthwind())
            {
                var anyUser = await db.Users.FirstOrDefaultAsync(f => f.PhoneNumber == phone);
                if (anyUser != null) return "Телефон занят";
                
                var user = await db.Users.FirstOrDefaultAsync(f => f.UserName == User.Identity.Name);
                string code = await _accountRepository.GeneratePassword();
                await _accountRepository.SendSms($"Код: {code}", user.PhoneNumber);


                await db.Users
                    .Where(w => w.Id == user.Id)
                    .Set(s => s.NewUserPhone, phone)
                    .Set(s => s.NewUserPhoneCode, code)
                    .UpdateAsync();

                return "success";


            }
            
            
        }

        [HttpGet]
        public async Task<string> CheckCodeNewPassword(string code)
        {
            var res = await _homeRepository.CheckCodeNewPassword(code, User.Identity.Name);
            if (res == "success")
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                await _userManager.ChangePasswordAsync(user, user.PasswordText, user.NewConfirmPassword);
                
                user.PasswordText = user.NewConfirmPassword;
                await _userManager.UpdateAsync(user);
            }

            return res;
        }
        [HttpGet]
        public async Task<string> CheckCodeNewEmail(string code)
        {
            var res = await _homeRepository.CheckCodeNewEmail(code, User.Identity.Name);

            return res;
        }
        [HttpGet]
        public async Task<string> CheckCodeNewPhone(string code)
        {
            var res = await _homeRepository.CheckCodeNewPhone(code, User.Identity.Name, _userManager);

            return res;
        }

        [HttpPost]
        public async Task<IActionResult> Complete([FromBody]objectClass request)
        {
            string remoteIpAddress = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var listIps = new List<string> { "185.71.76", "185.71.77", "77.75.153", "77.75.154", "2a02:5180" };
            if(remoteIpAddress.Contains(listIps[0]) || remoteIpAddress.Contains(listIps[1]) || remoteIpAddress.Contains(listIps[2]) || remoteIpAddress.Contains(listIps[3]) || remoteIpAddress.Contains(listIps[4]))
            {
                if(request.Object.status == "waiting_for_capture")
                {
                    _client.CapturePayment(request.Object.id);
                }
                else if(request.Object.status == "succeeded")
                {
                    var client = new RestClient("https://payment.yandex.net/api/v3/payments/");
                    var requestHttp = new RestRequest($"{request.Object.id}/", Method.GET);
                    client.Authenticator = new HttpBasicAuthenticator("636277", "test_0MYGrT71SMVbgqqLI4LepYRutPbKwTrmjBicjA7_96A");

                    IRestResponse response = client.Execute(requestHttp);
                    var obj = JsonConvert.DeserializeObject<Object>(response.Content);
                    if(obj.status == "succeeded")
                    {
                        if (obj.description.Contains("balance:"))
                        {
                            var userName = obj.description.Replace("balance:", "");
                            await _accountRepository.CompletePayBalance(obj.Amount.value, userName);
                        }
                        else
                        {
                            await _accountRepository.CompleteOrder(request.Object.id);
                        }
                        return StatusCode(200);
                        
                    }
                    else
                    {
                        return StatusCode(404);
                    }

                    
                }
            }
            else
            {
                return StatusCode(404);
            }
            
            return StatusCode(200);
        }

        [HttpGet]
        public async Task<List<string>> GetUserInfo()
        {
            return await _homeRepository.GetUserInfo(User.Identity.Name);
        }

        [HttpGet]
        public async Task UpdatePushing(string isType, bool value)
        {
            string userName = User.Identity.Name;
            await _homeRepository.UpdatePushing(isType, value, userName);
        }
        

        [HttpGet]
        public async Task<IActionResult> Stat(int? month, int? day, string sortBy)
        {
            if(month == null || day == null)
            {
                var date = DateTime.Now;
                month = date.Month - 1;
                day = date.Day - 1;
            }
            if(sortBy == null)
            {
                sortBy = "price";
            }
            ViewBag.Month = month;
            ViewBag.Day = day;
            var list = await _homeRepository.GetForecastForStat((int)month, (int)day, sortBy);

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Info(int forecastId, string userEmail)
        {
			if(userEmail == null)
			{
				userEmail = Request.Cookies["_ym_uid"];
			}
            await _homeRepository.AddViewForecastView(userEmail, forecastId);
            return View(await _homeRepository.GetInfoViewModel(forecastId));
        }

        [HttpPost]
        public async Task SetInfoBar(string text)
        {
            await _accountRepository.SetInfoBar(text);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _accountRepository.GetMainPageView());
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
