using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);
        private static readonly object Lock = new object();
        private static readonly int ExtendPeriod = 10;


        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static AuctionTicker Instance => _instance.Value;
        public IHubConnectionContext<dynamic> Clients { get; set; }

        public AuctionTicker(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;

            _timer = new Timer(Tick, null, _updateInterval, _updateInterval);
        }

        private void Tick(object state)
        {
            lock (Lock)
            {
                using (var context = new ApplicationDbContext())
                {
                    Parallel.ForEach(context.Auctions.Where(item => item.State == AuctionState.Open), item =>
                    {
                        if (item.Time == 0)
                        {
                            item.State = item.LastBidUserId != null ? AuctionState.Sold : AuctionState.Expired;
                            item.ClosingDateTime = DateTime.Now;
                            Log.Info("Auction " + item.Id + " " + item.State);
                            UpdateAuction(item);
                        }
                        else
                        {
                            item.Time--;
                        }
                    });

                    context.SaveChanges();
                    Clients.All.tick();
                }
            }
        }

        public void AddNewAuction(Auction auction)
        {
            using (var context = new ApplicationDbContext())
            {
                lock (Lock)
                {
                    if (auction.Id != 0)
                    {
                        var myAuction = context.Auctions.First(item => item.Id == auction.Id);
                        myAuction.ProductName = auction.ProductName;
                        myAuction.StartingPrice = auction.StartingPrice;
                        myAuction.Time = auction.Time;
                        if (!String.IsNullOrWhiteSpace(auction.Image))
                            myAuction.Image = auction.Image;
                    }
                    else
                    {
                        auction.State = AuctionState.Ready;
                        auction.CreationDateTime = DateTime.Now;
                        auction.OpeningDateTime = DateTime.Now;
                        context.Auctions.Add(auction);
                    }

                    context.SaveChanges();
                }

                Clients.All.updateAuction(auction);
            }

        }

        public void Bid(long auctionId, string userId)
        {
            using (var context = new ApplicationDbContext())
            {
                var user = context.Users.Find(userId);
                
                if(user == null)
                {
                    return;
                }

                if (user.TokenStashSize <= 0)
                {
                    return;
                }

                lock (Lock)
                {
                    Auction auction = context.Auctions.First(item => item.Id == auctionId && item.State == AuctionState.Open);
                    if (auction != null)
                    {
                        user.TokenStashSize--;
                        auction.LastBidUser = user;
                        auction.LastBidUserUserName = user.UserName;
                        auction.Time += auction.Time < 10 ? 10 : 0;
                        auction.StartingPrice++;
                        Bid bid = new Bid
                        {
                            User = user,
                            Auction = auction,
                            CreationDateTime = DateTime.Now,
                            UserName = user.UserName
                        };

                        context.Bids.Add(bid);
                        context.SaveChanges();

                        UpdateAuction(auction);
                        Log.Info("Auction " + auction.Id + " bidded by user " + user.Id);

                    }

                }
            }
        }

        public IEnumerable<Auction> GetAllAuctions()
        {
            using (var context = new ApplicationDbContext())
            {

                return
                    context.Auctions.Where(item => item.State == AuctionState.Open && item.OpeningDateTime != null).OrderByDescending(item => item.OpeningDateTime).Take(5).ToList();
            }
        }

        public IEnumerable<Bid> GetAllBids(long auctionId)
        {
            using (var context = new ApplicationDbContext())
            {
                return
                    context.Bids.Where(bid => bid.AuctionId == auctionId)
                        .OrderBy(bid => bid.CreationDateTime)
                        .Take(10)
                        .ToList();
            }
        }

        public void UpdateAuction(Auction auction)
        {
            Clients.All.updateAuction(auction);
        }
    }
}
