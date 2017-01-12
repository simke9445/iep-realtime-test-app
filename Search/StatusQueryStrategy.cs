using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealtimeTestApp.Models;

namespace RealtimeTestApp.Search
{
    public class StatusQueryStrategy : QueryStrategy
    {
        public StatusQueryStrategy(AuctionState state)
        {
            this.State = state;
        }

        public AuctionState State { get; set; }

        public override IEnumerable<Auction> Query(IEnumerable<Auction> auctions)
        {
            return auctions.Where(auction => auction.State == State);
        }
    }
}