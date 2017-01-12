using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealtimeTestApp.Models;

namespace RealtimeTestApp.Search
{
    public class ProductNameQueryStrategy : QueryStrategy
    {
        public String ProductName { get; set; }

        public ProductNameQueryStrategy(String productName)
        {
            ProductName = productName;
        }

        public override IEnumerable<Auction> Query(IEnumerable<Auction> auctions)
        {
            return auctions.Where(delegate(Auction auction)
            {
                if (String.IsNullOrWhiteSpace(ProductName))
                {
                    return true;
                }

                String[] productNameParts = ProductName.ToLower().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                String[] currentAuctionProductNameParts = auction.ProductName.ToLower().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                int queryIndex = 0, currentAuctionQueryIndex = 0, matchedParts = 0;

                while (currentAuctionProductNameParts.Length - currentAuctionQueryIndex >=
                       productNameParts.Length - queryIndex && matchedParts < productNameParts.Length)
                {
                    if (currentAuctionProductNameParts[currentAuctionQueryIndex].Contains(productNameParts[queryIndex]))
                    {
                        queryIndex++;
                        currentAuctionQueryIndex++;
                        matchedParts++;
                    }
                    else if (queryIndex != 0)
                    {
                        queryIndex = 0;
                        matchedParts = 0;
                    }
                    else
                    {
                        currentAuctionQueryIndex++;
                    }
                }

                return matchedParts == productNameParts.Length;
            });
        }
    }
}