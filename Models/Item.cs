using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int DefaultBet { get; set; }
        public int Step { get; set; }
        public int LastBet { get; set; }
        public string LastUser { get; set; }
        public string Group { get; set; }
        public DateTime LastBetTime { get; set; }
        public DateTime TimeOut { get; set; }
    }
}