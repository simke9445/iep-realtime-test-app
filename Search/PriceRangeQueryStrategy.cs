using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealtimeTestApp.Models;

namespace RealtimeTestApp.Search
{
    public class PriceRangeQueryStrategy : QueryStrategy
    {
        public PriceRangeQueryStrategy(int startingPrice, int endingPrice)
        {
            this.StartingPrice = startingPrice;
            this.EndingPrice = endingPrice;
        }

        public int EndingPrice { get; set; }

        public int StartingPrice { get; set; }

        public override IEnumerable<Auction> Query(IEnumerable<Auction> auctions)
        {
            return
                auctions.Where(auction => auction.StartingPrice <= EndingPrice && auction.StartingPrice >= StartingPrice);
        }
    }
}