using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlightBooking.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Reviewer { get; set; } // username of the user
        public string AirlineName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public string Status { get; set; }
    }
}