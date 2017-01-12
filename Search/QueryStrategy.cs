using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealtimeTestApp.Models;

namespace RealtimeTestApp.Search
{
    public abstract class QueryStrategy
    {
        public abstract IEnumerable<Auction> Query(IEnumerable<Auction> auctions);
    }
}