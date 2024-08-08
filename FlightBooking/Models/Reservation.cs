using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlightBooking.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Flight Flight { get; set; }
        public int PassengerCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }

}