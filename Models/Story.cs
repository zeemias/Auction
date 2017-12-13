using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Models
{
    public class Story
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string User { get; set; }
        public int UserId { get; set; }
        public int LastBet { get; set; }
        public int NewBet { get; set; }
        public DateTime Time { get; set; }
    }
}