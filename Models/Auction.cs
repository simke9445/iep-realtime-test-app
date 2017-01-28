using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace RealtimeTestApp.Models
{
    public enum AuctionState
    {
        Draft,
        Ready,
        Open,
        Sold,
        Expired
    }

    public class Auction
    {
        public long Id { get; set; }

        [Required]
        [DisplayName("Product Name")]
        [StringLength(20, ErrorMessage = "The {0} must be at most {1} characters long.")]
        public String ProductName { get; set; }

        [Required]
        [DisplayName("Duration(in seconds)")]
        [Range(0, 3600, ErrorMessage = "{0} must be an integer in {1} - {2} range.")]
        public int Time { get; set; }


        [Required]
        [DisplayName("Starting Price")]
        [Range(0, Int32.MaxValue, ErrorMessage = "The {0} Must be a positive integer.")]
        public int StartingPrice { get; set; }
        public DateTime? CreationDateTime { get; set; }
        public DateTime? OpeningDateTime { get; set; }
        public DateTime? ClosingDateTime { get; set; }
        public AuctionState State { get; set; }

        [DisplayName("Auction Image")]
        public string Image { get; set; }

        public ApplicationUser LastBidUser { get; set; }
        public string LastBidUserId { get; set; }
        public string LastBidUserUserName { get; set; }
    }
}