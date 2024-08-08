using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlightBooking.Models
{
    public class Flight
    {
        public int Id { get; set; }
        public string Airline { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public int AvailableSeats { get; set; }
        public int OccupiedSeats { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
    }
}