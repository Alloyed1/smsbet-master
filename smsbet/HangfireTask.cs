﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LinqToDB;
using Smsbet.Web.Migrations;

namespace Smsbet.Web
{
    public static class HangfireTask
    {
        public async static Task CheckMathes()
        {
            using(var db = new DbNorthwind())
            {
                var forecasts = await db.Forecasts
                    .Where(w => w.Status == "Sale" && w.StartTime.AddMinutes(-15) <= DateTime.Now)
                    .ToListAsync();
                
                
                
                

                foreach (var item in forecasts)
                {
                    var phonesList = await db.ForecastPhonesBuy
                        .Where(w => w.ForecastId == item.Id)
                        .Select(s => s.UserPhone)
                        .ToListAsync();

                    if (phonesList.Count() > 1)
                    {
                        string phones = "+7" + String.Join(";+7", phonesList);

                        using (var client = new WebClient())
                        {
                            await client.DownloadStringTaskAsync(
                                "https://smsc.ru/sys/send.php?login=Smsbet.Web&psw=Ujklujkl87&phones=" + phones + "&mes=" +
                                $"Рады сообщить, что для вас готова ставка : {item.Game}. {item.ChampionatName}. {item.ForecastText}. {item.PublicPrognoz}");
                        }
                    }
                    else if(phonesList.Any())
                    {
                        using (var client = new WebClient())
                        {


                            await client.DownloadStringTaskAsync(
                                "https://smsc.ru/sys/send.php?login=Smsbet.Web&psw=Ujklujkl87&phones=" + "+7" +
                                phonesList[0] + "&mes=" +
                                $"Рады сообщить, что для вас готова ставка : {item.Game}. {item.ChampionatName}. {item.ForecastText}. {item.PublicPrognoz}");

                        }
                    }

                    var countSms = int.Parse((await db.AppSettins.FirstOrDefaultAsync(f => f.Keys == "CountSms")).Value);
                    
                    await db.AppSettins
                        .Where(w => w.Keys == "CountSms")
                        .Set(s => s.Value, (countSms + phonesList.Count).ToString())
                        .UpdateAsync();
                    
                    await db.BasketItems
                        .Where(w => w.ForecastId == item.Id)
                        .DeleteAsync();
                    

                    await db.Forecasts
                        .Where(f => f.Id == item.Id)
                        .Set(s => s.Status, "Complete")
                        .UpdateAsync();
                    
                }
                
                
                
            }
        }
    }
}