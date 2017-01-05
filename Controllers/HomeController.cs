using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RealtimeTestApp.Hubs;
using RealtimeTestApp.Models;

namespace RealtimeTestApp.Controllers
{
    public class HomeController : Controller
    {
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
        public ActionResult ExtendAuction(string name)
        {
            Console.WriteLine("ExtendAuction " + name);
            AuctionTicker.Instance.ExtendAuction(name);
            
            return Json(new {status="Success", message="Success"});
        }
    }
}