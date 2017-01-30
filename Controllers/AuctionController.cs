using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
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

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AuctionController()
        {
            ApplicationDbContext = new ApplicationDbContext();
            UserManager =
                System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public ActionResult Auction(long? id)
        {
            var auction = ApplicationDbContext.Auctions.FirstOrDefault(item => item.Id == id);

            return View(auction);
        }

        public ActionResult Auctions()
        {
            return View();
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
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return new HttpUnauthorizedResult("You need admin privileges for this action.");
            }


            ModelState.Remove("Id");
            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("Image", "No file selected.");
            }

            try
            {
                if (file != null)
                {
                    var bitmap = Image.FromStream(file.InputStream);
                }
            }
            catch
            {
                ModelState.AddModelError("Image", "Uploaded file should be an image.");
            }

            if (!ModelState.IsValid)
            {
                return View("NewAuctionForm", auction);
            }


            if (file?.ContentLength > 0)
            {
                var folderPath = Server.MapPath("~/Content/Images");
                Directory.CreateDirectory(folderPath);
                var path = Path.Combine(folderPath, file.FileName);
                file.SaveAs(path);
                auction.Image = "/Content/Images/" + file.FileName;
            }

            AuctionTicker.Instance.AddNewAuction(auction);

            Log.Info("Auction " + (auction.Id == 0 ? "Created" : auction.Id + " Edited"));

            return RedirectToAction("Auctions", "Auction");
        }

        public ActionResult NewAuctionForm()
        {
            return View();
        }


        public ActionResult SearchAuction(int? startingPrice, int? endingPrice, AuctionState? auctionStatus, bool priceQuery,
           bool statusQuery, String searchQuery)
        {
            var queryStrategy = new CompositeQueryStrategy();

            if (statusQuery && auctionStatus.HasValue)
                queryStrategy.AddQueryStrategy(new StatusQueryStrategy(auctionStatus.Value));
            queryStrategy.AddQueryStrategy(new ProductNameQueryStrategy(searchQuery));
            if (priceQuery && startingPrice.HasValue && endingPrice.HasValue)
                queryStrategy.AddQueryStrategy(new PriceRangeQueryStrategy(startingPrice.Value, endingPrice.Value));

            return Json(queryStrategy.Query(ApplicationDbContext.Auctions));
        }

        public ActionResult Delete(long id)
        {
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return new HttpUnauthorizedResult("You need admin privileges for this action.");
            }

            var auction = ApplicationDbContext.Auctions.First(item => item.Id == id);
            auction.State = AuctionState.Draft;
            auction.ClosingDateTime = DateTime.Now;

            ApplicationDbContext.SaveChanges();

            AuctionTicker.Instance.UpdateAuction(auction);
            Log.Info("Auction " + auction.Id + " Deleted");

            return RedirectToAction("Auctions");
        }

        public ActionResult EditForm(long id)
        {
            var auction = ApplicationDbContext.Auctions.First(item => item.Id == id);

            return View("NewAuctionForm", auction);
        }

        public ActionResult Open(long id)
        {
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return new HttpUnauthorizedResult("You need admin privileges for this action.");
            }

            var auction = ApplicationDbContext.Auctions.First(item => item.Id == id);
            auction.State = AuctionState.Open;
            auction.OpeningDateTime = DateTime.Now;

            ApplicationDbContext.SaveChanges();

            AuctionTicker.Instance.UpdateAuction(auction);

            Log.Info("Auction " + auction.Id + " Opened");


            return RedirectToAction("Auction", new {id = id});
        }
    }
}