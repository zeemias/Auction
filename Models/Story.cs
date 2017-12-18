using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Models
{
    public class Story
    {
        //id истории
        public int Id { get; set; }
        //id товара
        public int ItemId { get; set; }
        //Пользователь сделавший ставку
        public string User { get; set; }
        //id пользователя, сделавшего ставку
        public int UserId { get; set; }
        //Предыдущая ставка
        public int LastBet { get; set; }
        //Новая ставка
        public int NewBet { get; set; }
        //Время ставки
        public DateTime Time { get; set; }
    }
}