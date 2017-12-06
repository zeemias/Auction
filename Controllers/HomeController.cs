using Auction.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Auction.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                ApplicationUser user = db.Users.FirstOrDefault(t => t.Email == User.Identity.Name);
                ViewBag.Coints = user.Coints;
                ViewBag.Items = db.Items.Where(t => t.Group == user.Group || t.Group == "Общая").ToList();
            }
            return View();
        }

        public ActionResult Item(int id = 1)
        {
            ViewBag.Id = id;
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                ApplicationUser user = db.Users.FirstOrDefault(t => t.Email == User.Identity.Name);
                Item item = db.Items.FirstOrDefault(t => t.Id == id);
                if (item.Group != "Общая" && item.Group != user.Group)
                {
                    return RedirectToAction("Index", "Home");
                }
                ViewBag.Coints = user.Coints;
                if (item.DefaultBet >= item.LastBet)
                {
                    ViewBag.Name = item.Name;
                    ViewBag.Description = item.Description;
                    ViewBag.Image = item.Image;
                    ViewBag.LastBet = item.DefaultBet;
                    ViewBag.LastBetTime = "";
                    ViewBag.LastUser = "";
                    ViewBag.Step = item.Step;
                }
                else
                {
                    ViewBag.Name = item.Name;
                    ViewBag.Description = item.Description;
                    ViewBag.Image = item.Image;
                    ViewBag.LastBet = item.LastBet;
                    ViewBag.LastBetTime = item.LastBetTime;
                    ViewBag.LastUser = item.LastUser;
                    ViewBag.Step = item.Step;
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult Item(int id, int lastBet)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                ApplicationUser user = db.Users.FirstOrDefault(t => t.Email == User.Identity.Name);
                Item item = db.Items.FirstOrDefault(t => t.Id == id);
                ApplicationUser userLast = db.Users.FirstOrDefault(t => t.Email == item.LastUser);
                if(lastBet < item.LastBet)
                {
                    ViewBag.Error = "Ставка была повышена другим пользователем.";
                }
                else if (user.Coints >= item.LastBet + item.Step)
                {
                    user.Coints -= item.LastBet + item.Step;
                    userLast.Coints += item.LastBet;
                    item.LastBet = item.LastBet + item.Step;
                    item.LastBetTime = DateTime.Now;
                    item.LastUser = user.Email;
                    db.Entry(userLast).State = EntityState.Modified;
                    db.Entry(user).State = EntityState.Modified;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    SendEmailAsync(user.Email, "Аукцион ДельтаКредит", "Ваша ставка на " + item.Name + " была повышена другим пользователем.");
                }
                ViewBag.Coints = user.Coints;
                ViewBag.Name = item.Name;
                ViewBag.Description = item.Description;
                ViewBag.Image = item.Image;
                ViewBag.LastBet = item.LastBet;
                ViewBag.LastBetTime = item.LastBetTime;
                ViewBag.LastUser = item.LastUser;
                ViewBag.Step = item.Step;
            }
            return View();
        }

        public void SendEmailAsync(string GetEmail, string mailSubject, string mailBody)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("DeltaCreditBot@yandex.ru");
                mail.To.Add(GetEmail);
                mail.Subject = mailSubject;
                mail.Body = mailBody;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 25))
                {
                    smtp.Credentials = new NetworkCredential("DeltaCreditBot@yandex.ru", "");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
            catch
            {
            }
        }

    }
}