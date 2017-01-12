using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealtimeTestApp.Models;

namespace RealtimeTestApp.Search
{
    public class CompositeQueryStrategy : QueryStrategy
    {
        public List<QueryStrategy> QueryStrategies { get; set; }

        public CompositeQueryStrategy(params QueryStrategy[] queryStrategies)
        {
            QueryStrategies = queryStrategies.ToList();
        }

        public void AddQueryStrategy(QueryStrategy queryStrategy)
        {
            QueryStrategies.Add(queryStrategy);
        }

        public override IEnumerable<Auction> Query(IEnumerable<Auction> auctions)
        {
            IEnumerable<Auction> tempAuctions = auctions;

            foreach (var queryStrategy in QueryStrategies)
            {
                tempAuctions = queryStrategy.Query(tempAuctions);
            }

            return tempAuctions;
        }
    }
}