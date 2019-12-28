using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Smsbet.Web.Models;
using Smsbet.Web.Repository;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Smsbet.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        IAccountRepository _accountRepository;
        public AccountController(UserManager<User> userManager, IAccountRepository accountRepository, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _accountRepository = accountRepository;
            _signInManager = signInManager;
        }

		[HttpPost]
		public async Task AddToBasket(string userName, int forecastId)
		{
			if (userName == null)
			{

			}

			else
			{
				await _accountRepository.AddToBasket(userName, forecastId);
			}
            
        }

        [HttpGet]
        public async Task<string> Register(string phone)
        {
            string password = await _accountRepository.GeneratePassword();
            User user = new User { Email = phone, PasswordText = password, UserName = phone, PhoneNumber = phone};

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                if(await _accountRepository.SendSms($"Ваш пароль: {user.PasswordText}", phone))
                {
                    return "ok";
                }

                return "badsms";


            }
            
            return "bad";
        }
        [HttpGet]
        public async Task<IActionResult> Test()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<string> ResendPassword(string phone)
        {
            var user = await _accountRepository.GetUser(phone);
            if(user == null)
            {
                return "bad";
            }
            return await _accountRepository.ResendPassword(phone);


        }

        [HttpPost]
        public async Task DeleteFromForecast(string userName, int id)
        {
            await _accountRepository.DeleteFromForecast(userName, id);
        }

        [HttpGet]
        public async Task<int> CheckFreePromocode(string promocode, string userName)
        {
			if(userName == null)
			{
				return 400;
			}
            return await _accountRepository.CheckFreePromocode(promocode, userName);
        }

        [HttpGet]
		public async Task<IActionResult> Card()
		{
			string itemsCookie = Request.Cookies["itemsCard"];
			var model = await _accountRepository.GetCardViewModel(User.Identity.Name, itemsCookie);
			if (model.ClearCookie)
			{
				Response.Cookies.Delete("itemsCard");
			}
			itemsCookie = Request.Cookies["itemsCard"];
			return View(model);
		}

        [HttpPost]
        public async Task<bool> Auth(string phone, string pass)
        {
            var user = await _accountRepository.GetUser(phone);
            if(user != null)
            {
                if (user.PasswordText == pass)
                {
                    await _signInManager.SignInAsync(user, true);
                    return true;
                }
            }
            
            return false;
        }

        [HttpGet]
        public async Task<bool> CheckPassword(string password, string phone)
        { 

            bool passwordCorrect = await _accountRepository.CheckPassword(password, phone);
            if (passwordCorrect)
            {
                var user = await _accountRepository.GetUser(phone);
                await _signInManager.SignInAsync(user, true);
                return true;
            }
            return false;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _accountRepository.GetSettingsViewModel(User.Identity.Name);
            return View(model);
        }
    }
}
