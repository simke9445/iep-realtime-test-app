using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealtimeTestApp.Models
{
    public class OrderListViewModel
    {
        public List<Order> Orders { get; set; }
        public Order Order { get; set; }
        public ApplicationUser User { get; set; }
    }
}