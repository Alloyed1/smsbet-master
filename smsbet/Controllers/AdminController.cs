using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Smsbet.Web.Models;
using Smsbet.Web.Repository;
using Smsbet.Web.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Smsbet.Web.Controllers
{
    public class AdminController : Controller
    {
        IAdminRepository _adminRepository;
        public AdminController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task StopBuy(int forecastId)
        {
            await _adminRepository.StopBuy(forecastId);
        }

        [HttpGet]
        public async Task SendSmsForAll(int forecastId)
        {
            await _adminRepository.SendSmsForAll(forecastId);
        }
        [HttpGet]
        public async Task SendSmsAboutStatus(int forecastId, string status )
        {
            await _adminRepository.SendSmsAboutStatus(forecastId, status);
        }

        [HttpGet]
        public IActionResult AddForecast()
        {
            return View();
        }

        [HttpGet]
        public async Task<string> GetPercentPlus()
        {
            return await _adminRepository.GetPercentPlus();
        }
        [HttpGet]
        public IActionResult SetResult(int id, string status)
        {
            ViewBag.Status = status;
            return View(id);
        }
        [HttpPost]
        public async Task<IActionResult> SetResult(int id, string result, string status)
        {
            await _adminRepository.ChangeStatus(id, status, result);
            await _adminRepository.SendSmsAboutStatus(id, status);

            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public async Task<string> GetEmails()
        {
            return await _adminRepository.GetEmails();
        }

        [HttpPost]
        public async Task SetPercentWin(string percent)
        {
            await _adminRepository.SetPercentWin(percent);
        }

        [HttpGet]
        public async Task<IActionResult> SmsAndEmailSender()
        {
            return View();
        }

        [HttpPost]
        public async Task SendSMSAll(string body)
        {
            await _adminRepository.SendSMSPushing(body);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeForecast(int forecastId)
        {
            Forecasts f =  await _adminRepository.GetForecastById(forecastId);


            return View(new AddForecastViewModel { Id = f.Id ,ChampionatName = f.ChampionatName, ForecastText = f.ForecastText, Game = f.Game, IntervalKoof = f.IntervalKoof, KoofProxoda = f.KoofProxoda, PercentReturn = f.PercentReturn, PublicPrognoz = f.PublicPrognoz, StartTime = f.StartTime, Status = f.Status});
        }
        [HttpPost]
        public async Task<IActionResult> ChangeForecast(AddForecastViewModel model)
        {
            var forecast = new Forecasts { Id = model.Id , ChampionatName = model.ChampionatName, ForecastText = model.ForecastText, Game = model.Game, IntervalKoof = model.IntervalKoof, KoofProxoda = model.KoofProxoda, PercentReturn = model.PercentReturn, PublicPrognoz = model.PublicPrognoz, StartTime = model.StartTime, Status = model.Status };
            await _adminRepository.UpdateForecast(forecast);
            return RedirectToAction("Forecasts", "Admin");
        }



		//[HttpPost]
		// async Task ChangeStatus(int id, string status)
		//{
			//await _adminRepository.ChangeStatus(id, status);
            //await _adminRepository.SendSmsAboutStatus(id, status);
		//}

        [HttpPost]
        public async Task<IActionResult> AddForecast(AddForecastViewModel model)
        {
            await _adminRepository.AddForecast(model);
            return RedirectToAction("Index", "Admin");
        }

        public async Task<IActionResult> Promocodes()
        {
            return View(await _adminRepository.GetPromocodeViewModel());
        }

        [HttpPost]
        public async Task SetFreePromocode(string code)
        {
            await _adminRepository.SetFreePromocode(code);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            return View(await _adminRepository.GetUsers());
        }

        [HttpGet]
        public async Task<IActionResult> DeleteForecast(int id)
        {
            await _adminRepository.DeleteForecast(id);
            return RedirectToAction("Forecasts", "Admin");

        }

        [HttpGet]
        public async Task<IActionResult> Forecasts()
        {
            var model = await _adminRepository.GetForecastsListViewModel();
            return View(model);
        }
    }
}
