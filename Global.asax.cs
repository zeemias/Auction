using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Auction.Controllers;
using System.Threading;
using Auction.Models;
using System.Threading.Tasks;
using System.Data.Entity;
using Exchange = Microsoft.Exchange.WebServices.Data;
using System.Net;

namespace Auction
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            int num = 0;
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //Устанавливаем метод обратного вызова
            TimerCallback tm = new TimerCallback(ReloadItem);
            //Устанавливаем время, через которое должен сработать таймер
            TimeSpan start = AppSettings.Start - DateTime.Now;
            //Устанавливаем таймер
            Timer timer = new Timer(tm, num, start, AppSettings.Period);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            //Отлавливаем ошибки
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;
            Response.Clear();
            Server.ClearError();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = "General";
            routeData.Values["exception"] = exception;
            Response.StatusCode = 500;
            if (httpException != null)
            {
                Response.StatusCode = httpException.GetHttpCode();
                switch (Response.StatusCode)
                {
                    case 403:
                        routeData.Values["action"] = "Http403";
                        break;
                    case 404:
                        routeData.Values["action"] = "Http404";
                        break;
                }
            }
            Response.TrySkipIisCustomErrors = true;
            IController errorsController = new ErrorController();
            HttpContextWrapper wrapper = new HttpContextWrapper(Context);
            var rc = new RequestContext(wrapper, routeData);
            errorsController.Execute(rc);
        }

        public static void ReloadItem(object obj)
        {
            lock(HomeController._padLock)
            using (AuctionContext db = new AuctionContext())
            {
                //Получаем список товаров, которые должны обновляться каждый час
                List<Item> tItems = db.Items.Where(t => t.Reload == "true").ToList();
                if(tItems.Count != 0)
                {
                    //Если такие товары есть, обновляем их
                    foreach (var item in tItems)
                    {
                        if (item.TimeOut <= DateTime.Now)
                        {
                            //Если их время вышло, меняем таймер 
                            item.Timer = "http://megatimer.ru/s/ed67c6f0fab237c3ca2b7335c90a68b2.js";
                        }
                        else if (item.LastBet > item.DefaultBet)
                        {
                            //Если время не вышло и последняя ставка не равна ставке по умолчанию, то вычитаем из количества товара единицу
                            item.Quantity -= 1;
                            //Последнюю ставку приравниваем к ставке по умолчанию
                            item.LastBet = item.DefaultBet;
                            //Устанавливаем пользователя сделавшего последнюю ставку
                            item.LastUser = "Empty";
                            //Добавляем победителя
                            Winner win = new Winner() { ItemId = item.Id, ItemName = item.Name, User = item.LastUser };
                            db.Winners.Add(win);
                            //Получаем его Email
                            string Email = db.Users.FirstOrDefault(t => t.Login == win.User).Email;
                            //Отправляем сообщение победителю
                            SendEmail(Email, item.Name);
                        }
                        else
                        {
                            //Если время не вышло но последняя ставка равна ставке по умолчанию, то продливаем товар на один час
                            item.TimeOut += new TimeSpan(1, 0, 0);
                        }
                        //Изменяем данные товара
                        db.Entry(item).State = EntityState.Modified;
                    }
                    //Сохраняем изменения
                    db.SaveChanges();
                }
                if (AppSettings.End <= DateTime.Now)
                {
                    List<Item> fItems = db.Items.Where(t => t.Reload == "false").ToList();
                    List<Winner> winners = db.Winners.ToList();
                    if (fItems.Count != 0)
                    {
                        foreach (var item in fItems)
                        {
                            if(!winners.Any(t => t.ItemId == item.Id))
                            {
                                //Добавляем победителя
                                Winner win = new Winner() { ItemId = item.Id, ItemName = item.Name, User = item.LastUser };
                                db.Winners.Add(win);
                                //Получаем его Email
                                string Email = db.Users.FirstOrDefault(t => t.Login == win.User).Email;
                                //Отправляем сообщение победителю
                                SendEmail(Email, item.Name);
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
        }

        //Функция отправки сообщений на Email
        public static Task SendEmail(string GetEmail, string itemName)
        {
            return Task.Run(() =>
            {
                Exchange.ExchangeService service = new Exchange.ExchangeService();
                service.Credentials = new NetworkCredential(AppSettings.SendEmail, AppSettings.SendEmailPassword);
                service.AutodiscoverUrl(AppSettings.SendEmail);
                Exchange.EmailMessage emailMessage = new Exchange.EmailMessage(service);
                emailMessage.Subject = "Аукцион #ProКачка - Вы выиграли приз!";
                emailMessage.Body = new Exchange.MessageBody("Поздравляем, вы выиграли " + itemName + " на нашем аукционе #ProКачка.");
                emailMessage.ToRecipients.Add(GetEmail);
                emailMessage.Send();
            });
        }
    }
}
