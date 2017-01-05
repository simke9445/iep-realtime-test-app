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
        private readonly ConcurrentDictionary<string, Auction> _auctions = new ConcurrentDictionary<string, Auction>();
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);
        private static readonly object Lock = new object();
        private static readonly int ExtendPeriod = 10;

        public static AuctionTicker Instance => _instance.Value;
        public IHubConnectionContext<dynamic> Clients { get; set; }

        public AuctionTicker(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;

            _auctions.Clear();

            _auctions.TryAdd("1", new Auction() { Name = "1", Time = 1000 });
            _auctions.TryAdd("2", new Auction() { Name = "2", Time = 100 });
            _auctions.TryAdd("3", new Auction() { Name = "3", Time = 10 });

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
                if (_auctions.TryAdd(auction.Name, auction))
                {
                    Clients.All.newAuction(auction);
                }
            }
            
        }

        public void ExtendAuction(string name)
        {
            lock (Lock)
            {
                Auction auction = _auctions.Values.FirstOrDefault(item => item.Name == name);
                if (auction != null)
                {
                    auction.Time += ExtendPeriod;
                    Clients.All.extendAuction(auction, ExtendPeriod);
                }
            }

        }

        public void RemoveAuction(Auction auction)
        {
            Auction placeholderAuction;
            if (_auctions.TryRemove(auction.Name, out placeholderAuction))
            {
                Clients.All.removeAuction(auction);
            }
        }

        public IEnumerable<Auction> GetAllAuctions()
        {
            return _auctions.Values;
        }
    }
}