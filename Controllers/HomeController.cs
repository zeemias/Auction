using Auction.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Exchange = Microsoft.Exchange.WebServices.Data;

namespace Auction.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public static object _padLock = new object();

        //Главная страница аукциона
        public ActionResult Index()
        {
            lock(_padLock)
            using (AuctionContext db = new AuctionContext())
            {
                //Находим текущего пользователя в БД
                User user = db.Users.FirstOrDefault(t => t.Login == User.Identity.Name);
                if (user == null)
                {
                    //Если пользователь не найден, значит он не зарегистрирован, отправляем на страницу "Ошибка авторизации"
                    return RedirectToAction("RegisterError","Home");
                }
                else
                {
                    //Если пользователь найден, то получаем количество его монет 
                    ViewBag.Coints = user.Coints;
                    //Получаем доступные для его группы товары
                    ViewBag.Items = db.Items.Where(t => t.Group == user.Group || t.Group == "Общая").OrderByDescending(t => t.LastBet).ToList();
                }
            }
            return View();
        }

        //Страница товара аукциона
        public ActionResult Item(int id = 9)
        {
            lock(_padLock)
            using (AuctionContext db = new AuctionContext())
            {
                //Находим текущего пользователя в БД
                User user = db.Users.FirstOrDefault(t => t.Login == User.Identity.Name);
                if (user == null)
                {
                    //Если пользователь не найден, значит он не зарегистрирован, отправляем на страницу "Ошибка авторизации"
                    return RedirectToAction("RegisterError", "Home");
                }
                else
                {
                    //Если пользователь найден, то получаем товар, на который он перешел
                    Models.Item item = db.Items.FirstOrDefault(t => t.Id == id);
                    if (item.Group != "Общая" && item.Group != user.Group)
                    {
                        //Если группа товара не соответствует группе пользователя, то отправляем его на главную страницу аукциона
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        //Если группа товара соответствует группе пользователя, то получаем количество монет пользователя
                        ViewBag.Coints = user.Coints;
                        //Получаем победителей данного товара
                        List<Winner> winners = db.Winners.Where(t => t.ItemId == item.Id).ToList();
                        if(winners.Count != 0)
                        {
                            //Если список не пустой, проверяем, есть ли там текущий пользователь
                            foreach(var winner in winners)
                            {
                                if(winner.User == user.Login)
                                {
                                    //Если уже есть, то запрещаем делать ставку
                                    ViewBag.Winner = true;
                                }
                            }
                        }
                        //Получаем историю со ставками данного товара
                        List<Story> story = db.Stories.Where(t => t.ItemId == id).ToList();
                        //Делаем реверс истории, чтобы ставки отображались по дате - от большей к меньшей
                        ViewBag.Stories = story.Reverse<Story>();
                        return View(item);
                    }
                }
            }
        }

        //Повышение ставки на товар
        [HttpPost]
        public ActionResult Item(int id, int lastBet)
        {
            lock(_padLock)
            using (AuctionContext db = new AuctionContext())
            {
                //Получаем текущего пользователя
                User user = db.Users.FirstOrDefault(t => t.Login == User.Identity.Name);
                //Получаем товар, на который он хочет повысить ставку
                Models.Item item = db.Items.FirstOrDefault(t => t.Id == id);

                if (item.TimeOut <= DateTime.Now || item.Quantity == 0)
                {
                    //Если текущая дата больше даты конца аукциона или количество товара равно 0, то аукцион завершен
                    ViewBag.Error = "Аукцион завершен!";
                }
                else if (lastBet < item.LastBet)
                {
                    //Если текущая ставка товара больше, чем отображенная ставка у пользователя, выводим сообщение об ошибке
                    ViewBag.Error = "Ставка была повышена другим пользователем.";
                }
                else if (item.LastUser == user.Login)
                {
                    //Если текущий пользователь это пльзователь, сделавший ставку последним, запрещаем ему делать ставку
                    ViewBag.Error = "Нельзя повысить свою ставку!";
                }
                else if (user.Coints >= item.LastBet + item.Step)
                {
                    //Если у пользователя достаточно монет, чтобы сделать ставку, он делает ставку
                    //Вычитаем монеты со счета пользователя
                    user.Coints -= item.LastBet + item.Step;
                    if(item.LastBet != item.DefaultBet)
                    {
                        //Если текущая ставка товара не равна ставке по умолчанию, то находим пользователя, который делал ставку последним
                        User userLast =  db.Users.FirstOrDefault(t => t.Login == item.LastUser);
                        //Возвращаем ему поставленные деньги
                        userLast.Coints += item.LastBet;
                        //Повышаем ставку
                        item.LastBet = item.LastBet + item.Step;
                        //Устанавливаем время повышения ставки
                        item.LastBetTime = DateTime.Now;
                        //Делаем пользователя последним, кто сделал ставку
                        item.LastUser = user.Login;
                        //Изменяем данные предыдущего пользователя
                        db.Entry(userLast).State = EntityState.Modified;
                        //Отправляем предыдущему пользователю сообщение, что его ставка была повышена другим пользователем
                        SendEmail(userLast.Email, "Аукцион ДельтаКредит", "Ваша ставка на " + item.Name + " была повышена другим пользователем.");
                    }
                    else
                    {
                        //Если текущая ставка товара равна ставке по умолчанию, то повышаем ставку
                        item.LastBet = item.LastBet + item.Step;
                        //Устанавливаем время повышения ставки
                        item.LastBetTime = DateTime.Now;
                        //Делаем пользователя последним, кто сделал ставку
                        item.LastUser = user.Login;
                    }
                    //Добавляем в историю, что ставка была повышена
                    db.Stories.Add(new Story
                    {
                        ItemId = id,
                        User = user.Login,
                        UserId = user.Id,
                        NewBet = item.LastBet,
                        LastBet = item.LastBet - item.Step,
                        Time = item.LastBetTime
                    });
                    //Изменяем данные текущего пользователя
                    db.Entry(user).State = EntityState.Modified;
                    //Изменяем данные товара
                    db.Entry(item).State = EntityState.Modified;
                    //Сохраняем изменения
                    db.SaveChanges();
                }
                //Получаем историю со ставками данного товара
                List<Story> story = db.Stories.Where(t => t.ItemId == id).ToList();
                //Делаем реверс истории, чтобы ставки отображались по дате - от большей к меньшей
                ViewBag.Stories = story.Reverse<Story>();
                //Отображаем количество монет пользователя
                ViewBag.Coints = user.Coints;
                return View(item);
            }
        }

        //Страница добавления товара
        public ActionResult AddItem()
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                //Если текущий пользователь не является администратором аукциона, отпрвляем его на главную страницу
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        //Добавление товара
        [HttpPost]
        public ActionResult AddItem(Models.Item item)
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                //Если текущий пользователь не является администратором аукциона, отпрвляем его на главную страницу
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (item.Reload == "true")
                {
                    //Если товар должен обновляться каждый час
                    item.Timer = "http://megatimer.ru/s/df6c9a2b958964ea26e52c3458237d24.js";
                }
                else
                {
                    //Если товар не должен обновляться каждый час
                    item.Timer = "http://megatimer.ru/s/a11003fdf036516ac368e7824d9c07e7.js";
                }
                //Устанавливаем последнюю ставку равной ставке по умолчанию
                item.LastBet = item.DefaultBet;
                //Устанавливаем время последней ставки равной текущему времени
                item.LastBetTime = DateTime.Now;
                //Устанавливаем пользователя сделавшего последнюю ставку
                item.LastUser = "Empty";
                lock(_padLock)
                using (AuctionContext db = new AuctionContext())
                {
                    //Добавляем товар в БД
                    db.Items.Add(item);
                    //Сохраняем изменения
                    db.SaveChanges();
                    //Товар добавлен
                    ViewBag.AddSuccess = true;
                    return View();
                }
            }
        }

        //Страница регистрации пользователей
        public ActionResult Register()
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                //Если текущий пользователь не является администратором аукциона, отпрвляем его на главную страницу
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        //Регистрация пользователей
        [HttpPost]
        public ActionResult Register(string a)
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                //Если текущий пользователь не является администратором аукциона, отпрвляем его на главную страницу
                return RedirectToAction("Index", "Home");
            }
            else
            {
                //Получаем расположение списка пользователей первой и второй групп
                string pathGroup1 = Request["Group1"];
                string pathGroup2 = Request["Group2"];
                if (pathGroup1 != "")
                {
                    //Если есть расположение списка, регистрируем пользователей первой группы
                    RegisterAll(pathGroup1, "Группа 1");
                    //Пользователи зарегистрированы
                    ViewBag.RegisterSuccess = true;
                }
                if (pathGroup2 != "")
                {
                    //Если есть расположение списка, регистрируем пользователей второй группы
                    RegisterAll(pathGroup2, "Группа 2");
                    //Пользователи зарегистрированы
                    ViewBag.RegisterSuccess = true;
                }
                return View();
            }
        }

        //Страница с отправкой сообщений о регистрации пользователям 
        public ActionResult SendRegistrationMessage()
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                //Если текущий пользователь не является администратором аукциона, отпрвляем его на главную страницу
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        //Отправка сообщений о регистрации пользователям
        [HttpPost]
        public ActionResult SendRegistrationMessage(string a)
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                //Если текущий пользователь не является администратором аукциона, отпрвляем его на главную страницу
                return RedirectToAction("Index", "Home");
            }
            else
            {
                lock(_padLock)
                using (AuctionContext db = new AuctionContext())
                {
                    //Получаем список пользователей
                    List<User> users = db.Users.ToList();
                    if (users.Count != 0)
                    {
                        //Если список не пустой, начинаем отправлять сообщения
                        foreach (var user in users)
                        {
                            //Текст сообщения
                            string mailBody = "Для входа на аукцион #PROКАЧКА используйте Вашу учетную запись DeltaCredit.";
                            //Отпрака сообщения
                            SendEmail(user.Email, "Аукцион ДельтаКредит", mailBody);
                        }
                    }
                    //Все письма отправлены
                    ViewBag.SendSuccess = true;
                    return View();
                }
            }
        }

        //Страница ошибка регистрации
        public ActionResult RegisterError()
        {
            return View();
        }

        //Страница админ панели
        public ActionResult AdminPanel()
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                //Если текущий пользователь не является администратором аукциона, отпрвляем его на главную страницу
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        //Старница со всеми лотами
        public ActionResult AllItems()
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                //Если текущий пользователь не является администратором аукциона, отпрвляем его на главную страницу
                return RedirectToAction("Index", "Home");
            }
            else
            {
                lock(_padLock)
                using (AuctionContext db = new AuctionContext())
                {
                   //Получаем все лоты аукциона
                   ViewBag.Items = db.Items.ToList();
                   return View();
                }
            }
        }

        //Функция для регистрации пользователей
        public Task RegisterAll(string Path, string GroupName)
        {
            return Task.Run(() =>
            {
                //Получем массив пользователей группы с их монетами
                string[] Group = System.IO.File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory.ToString() + Path);
                if (Group.Length != 0)
                {
                    //Если массив не пустой, начинаем регистрировать каждого пользователя
                    foreach (string str in Group)
                    {
                        //Разбиваем строку на логин пользователя и количество монет
                        string[] text = str.Split(':');
                        //Разбиваем логин пользователя на имя компьютера и имя самого пользователя, чтобы потом сделать из него Email
                        string[] email = text[0].Split('\\');
                        lock(_padLock)
                        using (AuctionContext db = new AuctionContext())
                        {
                            //Добавляем нового пользователя в БД
                            db.Users.Add(new User { Login = text[0], Email = email[1] + "@deltacredit.ru", Group = GroupName, Coints = Convert.ToInt32(text[1]) });
                            //Сохраняем изменения
                            db.SaveChanges();
                        }
                    }
                }
            });
        }

        //Функция отправки сообщений на Email
        public Task SendEmail(string GetEmail, string mailSubject, string mailBody)
        {
            return Task.Run(() =>
            {
                Exchange.ExchangeService service = new Exchange.ExchangeService();
                service.Credentials = new NetworkCredential(AppSettings.SendEmail, AppSettings.SendEmailPassword);
                service.AutodiscoverUrl(AppSettings.SendEmail);
                Exchange.EmailMessage emailMessage = new Exchange.EmailMessage(service);
                emailMessage.Body = new Exchange.MessageBody(mailBody);
                emailMessage.ToRecipients.Add(GetEmail);
                emailMessage.Send();
            });
        }

        //Функция загрузки фото
        public JsonResult UploadPhoto()
        {
            string filePath = "";
            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                if (upload != null)
                {
                    string fileName = "/Content/images/" + System.IO.Path.GetFileName(upload.FileName);
                    upload.SaveAs(Server.MapPath(fileName));
                    filePath = fileName;
                }
            }
            //Возвращаем расположение фото
            return Json(filePath);
        }

        //Функция загрузки групп
        public JsonResult UploadGroup()
        {
            string filePath = "";
            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                if (upload != null)
                {
                    string fileName = "/Register/" + System.IO.Path.GetFileName(upload.FileName);
                    upload.SaveAs(Server.MapPath(fileName));
                    filePath = fileName;
                }
            }
            //Возвращаем расположение списка группы
            return Json(filePath);
        }
    }
}