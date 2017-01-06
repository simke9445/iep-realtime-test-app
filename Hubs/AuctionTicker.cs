using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using RealtimeTestApp.Models;
using WebGrease.Css.Extensions;

namespace RealtimeTestApp.Hubs
{
    public class AuctionTicker
    {
        private readonly Timer _timer;
        private static readonly Lazy<AuctionTicker> _instance = new Lazy<AuctionTicker>(() => new AuctionTicker(GlobalHost.ConnectionManager.GetHubContext<AuctionHub>().Clients));
        private readonly ConcurrentDictionary<long, Auction> _auctions = new ConcurrentDictionary<long, Auction>();
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);
        private static readonly object Lock = new object();
        private static readonly int ExtendPeriod = 10;

        public static AuctionTicker Instance => _instance.Value;
        public IHubConnectionContext<dynamic> Clients { get; set; }

        public AuctionTicker(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;

            _auctions.Clear();

            using (var context = new ApplicationDbContext())
            {
                context.Auctions.ToList().AsParallel().ForEach(item =>
                {
                    if (item.State == AuctionState.Ready)
                    {
                        _auctions.TryAdd(item.Id, item);
                    }
                });
            }

            /*_auctions.TryAdd(1, new Auction() { Id = 1, Time = 1000, ProductName = "1"});
            _auctions.TryAdd(2, new Auction() { Id = 2, Time = 100, ProductName = "2"});
            _auctions.TryAdd(3, new Auction() { Id = 3, Time = 10, ProductName = "3"});*/

            _timer = new Timer(Tick, null, _updateInterval, _updateInterval);
        }

        private void Tick(object state)
        {
            lock (Lock)
            {
                _auctions.AsParallel().ForEach(item =>
                {

                    item.Value.Time--;
                    if (item.Value.Time == 0)
                    {
                        RemoveAuction(item.Value);
                    }
                });

                Clients.All.tick();
            }
        }

        public void AddNewAuction(Auction auction)
        {
            lock (Lock)
            {
                if (_auctions.TryAdd(auction.Id, auction))
                {
                    using (var context = new ApplicationDbContext())
                    {   
                        auction.State = AuctionState.Ready;
                        auction.CreationDateTime = DateTime.Now;
                        auction.OpeningDateTime = DateTime.Now;
                        context.Auctions.Add(auction);
                        context.SaveChanges();
                    }

                    Clients.All.newAuction(auction);
                }
            }
            
        }

        public void ExtendAuction(long id, ApplicationUser user, ApplicationDbContext context)
        {
            lock (Lock)
            {
                Auction auction = _auctions.Values.FirstOrDefault(item => item.Id == id);
                if (auction != null)
                {
                    auction.LastBidUser = user;
                    auction.Time += ExtendPeriod;
                    Bid bid = new Bid();
                    bid.User = user;
                    bid.Auction = auction;
                    bid.CreationDateTime = DateTime.Now;

                    context.Bids.Add(bid);
                    context.SaveChanges();

                    
                    Clients.All.extendAuction(auction, ExtendPeriod);
                }
            }

        }

        public void RemoveAuction(Auction auction)
        {
            Auction placeholderAuction;
            if (_auctions.TryRemove(auction.Id, out placeholderAuction))
            {
                using (var context = new ApplicationDbContext())
                {
                    auction = context.Auctions.First(item => item.Id == auction.Id);
                    auction.State = AuctionState.Expired;
                    auction.ClosingDateTime = DateTime.Now;
                    context.SaveChanges();
                }
                Clients.All.removeAuction(auction);
            }
        }

        public IEnumerable<Auction> GetAllAuctions()
        {
            return _auctions.Values;
        }
    }
}