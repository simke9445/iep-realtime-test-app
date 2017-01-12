using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RealtimeTestApp.Models;

namespace RealtimeTestApp.Controllers
{
    public class UserController : Controller
    {
        protected ApplicationDbContext ApplicationDbContext { get; set; }
        protected UserManager<ApplicationUser> UserManager { get; set; }

        public UserController()
        {
            ApplicationDbContext = new ApplicationDbContext();
            UserManager =
                new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ApplicationDbContext));
        }

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EditForm()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(UserManager.FindById(User.Identity.GetUserId()));
        }

        [HttpPost]
        public ActionResult Edit(ApplicationUser user)
        {
            var userInDb = ApplicationDbContext.Users.Single(u => u.Id == user.Id);
            userInDb.FirstName = user.FirstName;
            userInDb.LastName = user.LastName;

            ApplicationDbContext.SaveChanges();

            return RedirectToAction("Auctions", "Auction");
        }

        public ActionResult TokenOrderForm()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new OrderListViewModel()
            {
                User = UserManager.FindById(User.Identity.GetUserId())

            };
            viewModel.Orders = ApplicationDbContext.Orders.AsParallel().Where(item => item.User == viewModel.User).ToList();

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult TokenOrder(Order order)
        {
            order.User = UserManager.FindById(User.Identity.GetUserId());
            order.CreationDateTime = DateTime.Now;
            order.State = OrderState.Confirmed;
            order.PackagePrice = 10; // default
            order.User.TokenStashSize += (int)order.TokenAmount;

            ApplicationDbContext.Orders.Add(order);
            ApplicationDbContext.SaveChanges();

            return RedirectToAction("Auctions", "Auction");
        }

        public ActionResult OrderDetails(Order order)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(order);
        }
    }
}