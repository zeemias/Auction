using Auction.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Auction.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        public async Task<ActionResult> Index()
        {
            using (AuctionContext db = new AuctionContext())
            {
                User user = await db.Users.FirstOrDefaultAsync(t => t.Login == User.Identity.Name);
                ViewBag.Coints = user.Coints;
                ViewBag.Items = await db.Items.Where(t => t.Group == user.Group || t.Group == "Общая").ToListAsync();
            }
            return View();
        }

        public async Task<ActionResult> Item(int id = 1)
        {
            using (AuctionContext db = new AuctionContext())
            {
                User user = await db.Users.FirstOrDefaultAsync(t => t.Login == User.Identity.Name);
                Item item = await db.Items.FirstOrDefaultAsync(t => t.Id == id);
                List<Story> story = await db.Stories.Where(t => t.ItemId == id).ToListAsync();
                if (item.Group != "Общая" && item.Group != user.Group)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (item.DefaultBet >= item.LastBet)
                {
                    item.LastBet = item.DefaultBet;
                }
                ViewBag.Coints = user.Coints;
                ViewBag.Stories = story.Reverse<Story>();
                return View(item);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Item(int id, int lastBet)
        {
            using (AuctionContext db = new AuctionContext())
            {
                User user = await db.Users.FirstOrDefaultAsync(t => t.Login == User.Identity.Name);
                Item item = await db.Items.FirstOrDefaultAsync(t => t.Id == id);
                User userLast = await db.Users.FirstOrDefaultAsync(t => t.Login == item.LastUser);

                if (item.DefaultBet >= item.LastBet)
                {
                    item.LastBet = item.DefaultBet;
                }
                if (item.TimeOut < DateTime.Now)
                {
                    ViewBag.Error = "Аукцион завершен!";
                }
                else if (lastBet < item.LastBet)
                {
                    ViewBag.Error = "Ставка была повышена другим пользователем.";
                }
                else if (user.Coints >= item.LastBet + item.Step)
                {
                    user.Coints -= item.LastBet + item.Step;
                    if(item.LastBet != item.DefaultBet)
                    {
                        userLast.Coints += item.LastBet;
                        item.LastBet = item.LastBet + item.Step;
                        item.LastBetTime = DateTime.Now;
                        item.LastUser = user.Login;
                        db.Entry(userLast).State = EntityState.Modified;
                        await SendEmailAsync(userLast.Email, "Аукцион ДельтаКредит", "Ваша ставка на " + item.Name + " была повышена другим пользователем.");
                    }
                    else
                    {
                        item.LastBet = item.LastBet + item.Step;
                        item.LastBetTime = DateTime.Now;
                        item.LastUser = user.Login;
                    }
                    db.Stories.Add(new Story
                    {
                        ItemId = id,
                        User = user.Login,
                        UserId = user.Id,
                        NewBet = item.LastBet,
                        LastBet = item.LastBet - item.Step,
                        Time = item.LastBetTime
                    });
                    db.Entry(user).State = EntityState.Modified;
                    db.Entry(item).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
                List<Story> story = await db.Stories.Where(t => t.ItemId == id).ToListAsync();
                ViewBag.Stories = story.Reverse<Story>();
                return View(item);
            }
        }

        public ActionResult AddItem()
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddItem(Item item)
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                return RedirectToAction("Index", "Home");
            }
            item.LastBet = 0;
            item.LastBetTime = DateTime.Now;
            item.LastUser = "Empty";
            using (AuctionContext db = new AuctionContext())
            {
                db.Items.Add(item);
                await db.SaveChangesAsync();
            }
            string path = "Item/" + item.Id;
            return RedirectToAction(path, "Home");
        }

        public ActionResult Register()
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(string a)
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                string pathGroup1 = Request["Group1"];
                string pathGroup2 = Request["Group2"];
                if (pathGroup1 != "")
                {
                    await RegisterAll(pathGroup1, "Группа 1");
                    ViewBag.RegisterSuccess = true;
                }
                if (pathGroup2 != "")
                {
                    await RegisterAll(pathGroup2, "Группа 2");
                    ViewBag.RegisterSuccess = true;
                }
            }
            return View();
        }

        public ActionResult SendRegistrationMessage()
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> SendRegistrationMessage(string a)
        {
            if (User.Identity.Name != AppSettings.AdminName)
            {
                return RedirectToAction("Index", "Home");
            }
            using (AuctionContext db = new AuctionContext())
            {
                List<User> users = await db.Users.ToListAsync();
                if (users.Count != 0)
                {
                    foreach (var user in users)
                    {
                        string mailBody = "Для входа на аукцион #PROКАЧКА используйте Вашу учетную запись DeltaCredit.";
                        await SendEmailAsync(user.Email, "Аукцион ДельтаКредит", mailBody);
                    }
                }
            }
            ViewBag.SendSuccess = true;
            return View();
        }

        private async Task RegisterAll(string Path, string GroupName)
        {
            string[] Group = System.IO.File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory.ToString() + Path);
            foreach (string str in Group)
            {
                string[] text = str.Split(':');
                using (AuctionContext db = new AuctionContext())
                {
                    string[] email = text[0].Split('\\');
                    db.Users.Add(new User { Login = text[0], Email = email[1] + "@deltacredit.ru", Group = GroupName, Coints = Convert.ToInt32(text[1]) });
                    await db.SaveChangesAsync();
                }
            }
        }

        public static async Task SendEmailAsync(string GetEmail, string mailSubject, string mailBody)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(AppSettings.SendEmail);
            mail.To.Add(GetEmail);
            mail.Subject = mailSubject;
            mail.Body = mailBody;
            mail.IsBodyHtml = true;

            using (SmtpClient smtp = new SmtpClient(AppSettings.SMTP_Address, AppSettings.SMTP_Port))
            {
                smtp.Credentials = new NetworkCredential(AppSettings.SendEmail, AppSettings.SendEmailPassword);
                smtp.EnableSsl = AppSettings.SSL;
                await smtp.SendMailAsync(mail);
            }
        }

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
            return Json(filePath);
        }

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
            return Json(filePath);
        }
    }
}