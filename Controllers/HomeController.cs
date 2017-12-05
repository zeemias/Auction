using System;
using System.Collections.Generic;
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
            return View();
        }

        public ActionResult Item(int id = 0)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Item()
        {
            return View();
        }

    }
}