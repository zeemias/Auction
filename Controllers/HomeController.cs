using Auction.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
        private string photoPath;

        public async Task<ActionResult> Index()
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                ApplicationUser user = await db.Users.FirstOrDefaultAsync(t => t.Email == User.Identity.Name);
                ViewBag.Coints = user.Coints;
                ViewBag.Items = await db.Items.Where(t => t.Group == user.Group || t.Group == "Общая").ToListAsync();
            }
            return View();
        }

        public async Task<ActionResult> Item(int id = 1)
        {
            ViewBag.Id = id;
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                ApplicationUser user = await db.Users.FirstOrDefaultAsync(t => t.Email == User.Identity.Name);
                Item item = await db.Items.FirstOrDefaultAsync(t => t.Id == id);
                List<Story> story = await db.Stories.Where(t => t.ItemId == id).ToListAsync();
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
                ViewBag.Stories = story.Reverse<Story>();
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Item(int id, int lastBet)
        {
            ViewBag.Id = id;
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                ApplicationUser user = await db.Users.FirstOrDefaultAsync(t => t.Email == User.Identity.Name);
                Item item = await db.Items.FirstOrDefaultAsync(t => t.Id == id);
                ApplicationUser userLast = await db.Users.FirstOrDefaultAsync(t => t.Email == item.LastUser);

                if (item.DefaultBet >= item.LastBet)
                {
                    item.LastBet = item.DefaultBet;
                }
                if (lastBet < item.LastBet)
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
                        item.LastUser = user.Email;
                        db.Entry(userLast).State = EntityState.Modified;
                        db.Stories.Add(new Story
                        {
                            ItemId = id,
                            User = user.Email,
                            UserId = user.Id,
                            NewBet = item.LastBet,
                            LastBet = item.LastBet - item.Step,
                            Time = item.LastBetTime
                        });
                        await SendEmailAsync(userLast.Email, "Аукцион ДельтаКредит", "Ваша ставка на " + item.Name + " была повышена другим пользователем.");
                    }
                    else
                    {
                        item.LastBet = item.LastBet + item.Step;
                        item.LastBetTime = DateTime.Now;
                        item.LastUser = user.Email;
                    }
                    db.Entry(user).State = EntityState.Modified;
                    db.Entry(item).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
                List<Story> story = await db.Stories.Where(t => t.ItemId == id).ToListAsync();
                ViewBag.Coints = user.Coints;
                ViewBag.Name = item.Name;
                ViewBag.Description = item.Description;
                ViewBag.Image = item.Image;
                ViewBag.LastBet = item.LastBet;
                ViewBag.LastBetTime = item.LastBetTime;
                ViewBag.LastUser = item.LastUser;
                ViewBag.Step = item.Step;
                ViewBag.Stories = story.Reverse<Story>(); ;
            }
            return View();
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
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                db.Items.Add(item);
                await db.SaveChangesAsync();
            }
            string path = "Item/" + item.Id;
            return RedirectToAction(path, "Home");
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
            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                if (upload != null)
                {
                    string fileName = "/Content/images/" + System.IO.Path.GetFileName(upload.FileName);
                    upload.SaveAs(Server.MapPath(fileName));
                    photoPath = fileName;
                }
            }
            return Json(photoPath);
        }

    }
}