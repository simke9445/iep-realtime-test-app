using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RealtimeTestApp.Models
{
    public class Bid
    {
        public long Id { get; set; }
        public DateTime? CreationDateTime { get; set; }

        public Auction Auction { get; set; }
        public long AuctionId { get; set; }

        public ApplicationUser User { get; set; }
        public string UserId { get; set; }
    }
}