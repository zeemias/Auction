﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Models
{
    public class AppSettings
    {
        public static string AdminName { get; set; } = "";

        public static string SendEmail { get; set; } = "";

        public static string SendEmailPassword { get; set; } = "";

        public static string SMTP_Address { get; set; } = "smtp.yandex.ru";

        public static int SMTP_Port { get; set; } = 25;

        public static bool SSL { get; set; } = true;
    }
}