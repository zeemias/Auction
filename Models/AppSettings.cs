using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Models
{
    public class AppSettings
    {
        //Дата старта аукциона
        public static DateTime Start { get; } = new DateTime(2017, 12, 18, 22, 40, 00);
        //Дата конца аукциона
        public static DateTime End { get; } = new DateTime(2017, 12, 25, 10, 00, 00);
        //Время обновления товара
        public static TimeSpan Period { get; } = new TimeSpan(1, 0, 0);
        //Логин администратора аукциона
        public static string AdminName { get; } = "PCZEEMI\\Фаузат"; //MHQW-IT31\\Trainee
        //Email для отправки писем
        public static string SendEmail { get; } = "DeltaCreditBot@yandex.ru";
        //Пароль от Email
        public static string SendEmailPassword { get; } = "Gcr8UcxdDPa";
        //SMTP сервер
        public static string SMTP_Address { get; } = "smtp.yandex.ru";
        //SMTP порт
        public static int SMTP_Port { get; } = 25;
        //SSL
        public static bool SSL { get; } = true;
    }
}