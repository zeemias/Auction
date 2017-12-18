using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Models
{
    public class Winner
    {
        //id
        public int Id { get; set; }
        //id товара
        public int ItemId { get; set; }
        //Названи товара
        public string ItemName { get; set; }
        //Пользователь выигравший товар
        public string User { get; set; }
    }
}