using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace Auction.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public ActionResult General(Exception exception)
        {
            string Path = AppDomain.CurrentDomain.BaseDirectory + "/ErrorLogs/" + DateTime.Now.ToString().Replace(".", string.Empty).Replace(" ", string.Empty).Replace(":", string.Empty) + ".txt";
            using (StreamWriter sw = new StreamWriter(Path, true, System.Text.Encoding.Default))
            {
                sw.WriteLine("Error Message: " + exception.Message + "\nStackTrace: " + exception.StackTrace);
            }
            return View();
        }

        public ActionResult Http404()
        {
            return View();
        }

        public ActionResult Http403()
        {
            return View();
        }
    }
}