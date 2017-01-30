using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public UserController()
        {
            ApplicationDbContext = new ApplicationDbContext();
            UserManager =
                new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ApplicationDbContext));
        }

        public ActionResult EditForm()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(UserManager.FindById(User.Identity.GetUserId()));
        }

        public ActionResult Details()
        {
            
            return View(UserManager.FindById(User.Identity.GetUserId()));
        }

        [HttpPost]
        public ActionResult Edit(ApplicationUser user)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }


            if (user.Id != User.Identity.GetUserId())
            {
                RedirectToAction("EditForm", "Auction");
            }

            var userInDb = ApplicationDbContext.Users.Single(u => u.Id == user.Id);
            userInDb.FirstName = user.FirstName;
            userInDb.LastName = user.LastName;

            ApplicationDbContext.SaveChanges();

            Log.Info("Edited User " + user.Id);

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

        //[HttpPost]
        //public ActionResult TokenOrder(Order order)
        //{
        //    order.User = UserManager.FindById(User.Identity.GetUserId());
        //    order.CreationDateTime = DateTime.Now;
        //    order.State = OrderState.Confirmed;
        //    order.PackagePrice = 10; // default
        //    order.User.TokenStashSize += (int)order.TokenAmount;

        //    ApplicationDbContext.Orders.Add(order);
        //    ApplicationDbContext.SaveChanges();

        //    return RedirectToAction("Auctions", "Auction");
        //}


        public ActionResult TokenOrderConfirmation(string clientid, int amount, string status, double enduserprice)
        {
            if (UserManager.FindById(clientid) == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotAcceptable);
            }
            var user = UserManager.FindById(clientid);
            Order order = new Order()
            {
                User = user,
                TokenAmount = (TokenPackage)amount,
                CreationDateTime = DateTime.Now,
                PackagePrice = (int)enduserprice,
                State = status.Equals("success") ? OrderState.Confirmed : OrderState.Canceled
            };

            order.User.TokenStashSize += (int) order.TokenAmount;

            ApplicationDbContext.Orders.Add(order);
            ApplicationDbContext.SaveChanges();


            MailMessage mail = new MailMessage("simke9445@gmail.com", user.Email);
            SmtpClient client = new SmtpClient
            {
                Port = 25,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("simke9445@gmail.com", "cnhhbkfzrcutarvx"),
                EnableSsl = true,
                Host = "smtp.gmail.com"
            };
            mail.Subject = "[TokenOrder]";
            mail.Body = "Token Order " + order.State + " for token package " + order.TokenAmount;
            client.Send(mail);


            Log.Info("Token Order[package = " + order.TokenAmount + "] for user "+ user.Id + order.State);



            return new HttpStatusCodeResult(HttpStatusCode.Accepted);
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