using Auction.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
                ViewBag.Coints = db.Users.FirstOrDefault(t => t.Email == User.Identity.Name).Coints;
            }
            return View();
        }

        public ActionResult Item(int id = 1)
        {
            ViewBag.Id = id;
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                ViewBag.Coints = db.Users.FirstOrDefault(t => t.Email == User.Identity.Name).Coints;
                Item item = db.Items.FirstOrDefault(t => t.Id == id);
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

        [HttpPost]
        public ActionResult Item(int id, string a)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                ApplicationUser user = db.Users.FirstOrDefault(t => t.Email == User.Identity.Name);
                Item item = db.Items.FirstOrDefault(t => t.Id == id);
                ApplicationUser userLast = db.Users.FirstOrDefault(t => t.Email == item.LastUser);
                if (user.Coints >= item.LastBet + item.Step)
                {
                    user.Coints -= item.LastBet + item.Step;
                    userLast.Coints += item.LastBet;
                    item.LastBet = item.LastBet + item.Step;
                    item.LastBetTime = DateTime.Now;
                    item.LastUser = user.Email;
                }
                ViewBag.Coints = user.Coints;
                ViewBag.Name = item.Name;
                ViewBag.Description = item.Description;
                ViewBag.Image = item.Image;
                ViewBag.LastBet = item.LastBet;
                ViewBag.LastBetTime = item.LastBetTime;
                ViewBag.LastUser = item.LastUser;
                ViewBag.Step = item.Step;
                db.Entry(userLast).State = EntityState.Modified;
                db.Entry(user).State = EntityState.Modified;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }
            return View();
        }

    }
}