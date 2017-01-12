﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealtimeTestApp.Models
{
    public enum OrderState
    {
        Queued,
        Canceled,
        Confirmed
    }

    public enum TokenPackage
    {
        Silver = 3,
        Gold = 5,
        Platinum = 10
    }

    public class Order
    {
        public long Id { get; set; }
        public TokenPackage TokenAmount { get; set; }
        public int PackagePrice { get; set; }
        public DateTime? CreationDateTime { get; set; }
        public OrderState State { get; set; }

        public ApplicationUser User { get; set; }
        public string UserId { get; set; }

    }
}