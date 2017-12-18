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
        //Страница для ошибок кроме 403, 404
        public ActionResult General(Exception exception)
        {
            //Создаем файл с стектрейсом ошибки, где название файла - дата и время, когда произошла ошибка
            string Path = AppDomain.CurrentDomain.BaseDirectory + "/ErrorLogs/" + DateTime.Now.ToString().Replace(".", string.Empty).Replace(" ", string.Empty).Replace(":", string.Empty) + ".txt";
            using (StreamWriter sw = new StreamWriter(Path, true, System.Text.Encoding.Default))
            {
                //Записываем стектрейс ошибки в файл
                sw.WriteLine("Error Message: " + exception.Message + "\nStackTrace: " + exception.StackTrace);
            }
            return View();
        }

        //Страница ошибки 404
        public ActionResult Http404()
        {
            return View();
        }

        //Страница ошибки 403
        public ActionResult Http403()
        {
            return View();
        }
    }
}