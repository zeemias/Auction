using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Models
{
    public class Item
    {
        //id товара
        public int Id { get; set; }
        //Название товара
        public string Name { get; set; }
        //Описание товара
        public string Description { get; set; }
        //Картинка товара
        public string Image { get; set; }
        //Ставка по умолчанию
        public int DefaultBet { get; set; }
        //Шаг ставки
        public int Step { get; set; }
        //Последняя ставка
        public int LastBet { get; set; }
        //Пользователь, сделавший последнюю ставку
        public string LastUser { get; set; }
        //Группа товара
        public string Group { get; set; }
        //Время последней ставки
        public DateTime LastBetTime { get; set; }
        //Время конца аукциона
        public DateTime TimeOut { get; set; }
        //Таймер
        public string Timer { get; set; }
        //Количество товара
        public int Quantity { get; set; }
        //Обновление 
        public string Reload { get; set; }
    }
}