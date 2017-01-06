using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RealtimeTestApp.Hubs;
using RealtimeTestApp.Models;

namespace RealtimeTestApp.Controllers
{
    public class HomeController : Controller
    {
        protected ApplicationDbContext ApplicationDbContext { get; set; }
        protected UserManager<ApplicationUser> UserManager { get; set; }

        public HomeController()
        {
            ApplicationDbContext = new ApplicationDbContext();
            UserManager =
                new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ApplicationDbContext));
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Auctions()
        {

            return View();
        }

        [HttpPost]
        public ActionResult NewAuction(Auction auction)
        {
            AuctionTicker.Instance.AddNewAuction(auction);

            return RedirectToAction("Auctions", "Home");
        }

        public ActionResult NewAuctionForm()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ExtendAuction(long id)
        {
            AuctionTicker.Instance.ExtendAuction(id, UserManager.FindById(User.Identity.GetUserId()), ApplicationDbContext);
            
            return Json(new {status="Success", message="Success"});
        }
    }
}