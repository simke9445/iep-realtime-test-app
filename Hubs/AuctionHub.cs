using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using RealtimeTestApp.Models;
using Microsoft.AspNet.Identity.Owin;

namespace RealtimeTestApp.Hubs
{
    [HubName("auctionTicker")]
    public class AuctionHub : Hub
    {
        private readonly AuctionTicker _auctionTicker;

        public AuctionHub() : this(AuctionTicker.Instance)
        {
        }

        public AuctionHub(AuctionTicker auctionTicker)
        {
            _auctionTicker = auctionTicker;
        }

        public IEnumerable<Auction> GetAllAuctions()
        {
            return _auctionTicker.GetAllAuctions();
        }

        public IEnumerable<Bid> GetAllBids(long auctionId)
        {
            return _auctionTicker.GetAllBids(auctionId);
        }

        public void Bid(long auctionId)
        {
            _auctionTicker.Bid(auctionId, Context.User.Identity.GetUserId());
        }

    }
}