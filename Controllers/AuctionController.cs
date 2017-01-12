using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RealtimeTestApp.Hubs;
using RealtimeTestApp.Models;
using RealtimeTestApp.Search;
using WebGrease.Activities;
using WebGrease.Css.ImageAssemblyAnalysis;

namespace RealtimeTestApp.Controllers
{
    public class AuctionController : Controller
    {
        protected ApplicationDbContext ApplicationDbContext { get; set; }
        protected UserManager<ApplicationUser> UserManager { get; set; }

        public AuctionController()
        {
            ApplicationDbContext = new ApplicationDbContext();
            UserManager =
                new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ApplicationDbContext));
        }

        // GET: Auction
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Auction(long? id)
        {
            var auction = ApplicationDbContext.Auctions.FirstOrDefault(item => item.Id == id);

            return View(auction);
        }

        public ActionResult Auctions()
        {
            return View(UserManager.FindById(User.Identity.GetUserId()));
        }

        public ActionResult WonAuctions()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var id = User.Identity.GetUserId();
            var auctions =
                ApplicationDbContext.Auctions.Where(
                    auction => auction.LastBidUserId == id && auction.State == AuctionState.Sold).ToList();

            return View(auctions);
        }

        [HttpPost]
        public ActionResult NewAuction(Auction auction, HttpPostedFileBase file)
        {
            if(file?.ContentLength > 0)
            {
                var folderPath = Server.MapPath("~/Content/Images");
                Directory.CreateDirectory(folderPath);
                var path = Path.Combine(folderPath, file.FileName);
                file.SaveAs(path);
                auction.Image = "/Content/Images/" + file.FileName;
            }

            AuctionTicker.Instance.AddNewAuction(auction);

            return RedirectToAction("Auctions", "Auction");
        }

        public ActionResult NewAuctionForm()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ExtendAuction(long id)
        {
            AuctionTicker.Instance.ExtendAuction(id, UserManager.FindById(User.Identity.GetUserId()), ApplicationDbContext);

            return Json(new { status = "Success", message = "Success" });
        }

        public ActionResult SearchAuction(int? startingPrice, int? endingPrice, AuctionState? auctionStatus, bool priceQuery,
           bool statusQuery, String searchQuery)
        {
            var queryStrategy = new CompositeQueryStrategy();

            if (statusQuery)
                queryStrategy.AddQueryStrategy(new StatusQueryStrategy(auctionStatus.Value));
            queryStrategy.AddQueryStrategy(new ProductNameQueryStrategy(searchQuery));
            if (priceQuery)
                queryStrategy.AddQueryStrategy(new PriceRangeQueryStrategy(startingPrice.Value, endingPrice.Value));

            return Json(queryStrategy.Query(ApplicationDbContext.Auctions));
        }

        public ActionResult Delete(long id)
        {
            var auction = ApplicationDbContext.Auctions.First(item => item.Id == id);
            auction.State = AuctionState.Draft;
            auction.ClosingDateTime = DateTime.Now;

            ApplicationDbContext.SaveChanges();

            return RedirectToAction("Auctions");
        }

        public ActionResult EditForm(long id)
        {
            var auction = ApplicationDbContext.Auctions.First(item => item.Id == id);

            return View("NewAuctionForm", auction);
        }

        public ActionResult Open(long id)
        {
            var auction = ApplicationDbContext.Auctions.First(item => item.Id == id);
            auction.State = AuctionState.Open;
            auction.OpeningDateTime = DateTime.Now;

            ApplicationDbContext.SaveChanges();

            return RedirectToAction("Auction", new {id = id});
        }
    }
}