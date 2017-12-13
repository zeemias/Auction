using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Models
{
    public class AppSettings
    {
        public static string AdminName { get; } = "MHQW-IT31\\Trainee";
        public static string SendEmail { get; } = "DeltaCreditBot@yandex.ru";
        public static string SendEmailPassword { get; } = "";
        public static string SMTP_Address { get; } = "smtp.yandex.ru";
        public static int SMTP_Port { get; } = 25;
        public static bool SSL { get; } = true;
    }
}