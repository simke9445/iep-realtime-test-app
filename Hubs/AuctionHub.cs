using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using RealtimeTestApp.Models;

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

    }
}