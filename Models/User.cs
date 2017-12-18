using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Models
{
    public class User
    {
        //id пользователя
        public int Id { get; set; }
        //Логин пользователя
        public string Login { get; set; }
        //Email пользователя
        public string Email { get; set; }
        //Монеты пользователя
        public int Coints { get; set; }
        //Группа пользователя
        public string Group { get; set; }
    }
}