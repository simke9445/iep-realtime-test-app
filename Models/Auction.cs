using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealtimeTestApp.Models
{
    public enum AuctionState
    {
        Draft,
        Ready,
        Sold,
        Expired
    }

    public class Auction
    {
        public long Id { get; set; }
        public String ProductName { get; set; }
        public int Time { get; set; }
        public int StartingPrice { get; set; }
        public DateTime? CreationDateTime { get; set; }
        public DateTime? OpeningDateTime { get; set; }
        public DateTime? ClosingDateTime { get; set; }
        public AuctionState State { get; set; }
        public string Image { get; set; }
        public ApplicationUser LastBidUser { get; set; }
        public string LastBidUserId { get; set; }
    }
}