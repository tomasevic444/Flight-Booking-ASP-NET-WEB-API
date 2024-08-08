using FlightBooking.Models;
using FlightBooking.Ultities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;


namespace FlightBooking.Controllers
{
    [RoutePrefix("api/airlines")]
    public class AirlineController : ApiController
    {
        private string dataFilePath = HttpContext.Current.Server.MapPath("~/Data/airlines.json");
        private string flightsFilePath = HttpContext.Current.Server.MapPath("~/Data/flights.json");
        private string usersFilePath = HttpContext.Current.Server.MapPath("~/Data/users.json");

        [HttpGet]
        [Route("")]
        public IEnumerable<Airline> GetAirlines()
        {
            var airlines = JsonFileHelper.ReadJsonFile<Airline>(dataFilePath)
                                         .Where(a => !a.IsDeleted)
                                         .ToList();

            foreach (var airline in airlines)
            {
                if (airline.Flights == null)
                {
                    airline.Flights = new List<Flight>();
                }
                if (airline.Reviews == null)
                {
                    airline.Reviews = new List<Review>();
                }
            }
            return airlines;
        }

        [HttpGet]
        [Route("details")]
        public IHttpActionResult GetAirlineDetails(string name)
        {
            var airlines = JsonFileHelper.ReadJsonFile<Airline>(dataFilePath);
            var airline = airlines.FirstOrDefault(a => a.Name == name);
            if (airline == null)
            {
                return NotFound();
            }

            if (airline.Flights == null)
            {
                airline.Flights = new List<Flight>();
            }
            if (airline.Reviews == null)
            {
                airline.Reviews = new List<Review>();
            }

            var approvedReviews = airline.Reviews.Where(r => r.Status == "APPROVED").ToList();
            return Ok(new { airline, reviews = approvedReviews });
        }

        [HttpPost]
        [Route("add")]
        public IHttpActionResult AddAirline(Airline newAirline)
        {
            if (newAirline.Flights == null)
            {
                newAirline.Flights = new List<Flight>();
            }
            if (newAirline.Reviews == null)
            {
                newAirline.Reviews = new List<Review>();
            }

            var airlines = JsonFileHelper.ReadJsonFile<Airline>(dataFilePath);
            if (airlines.Any(a => a.Name == newAirline.Name))
            {
                return BadRequest("Airline with the same name already exists.");
            }

            airlines.Add(newAirline);
            JsonFileHelper.WriteJsonFile(dataFilePath, airlines);
            return Ok();
        }

        [HttpGet]
        [Route("search")]
        public IEnumerable<Airline> SearchAirlines(string name = null, string address = null, string contact = null)
        {
            var airlines = JsonFileHelper.ReadJsonFile<Airline>(dataFilePath)
                                         .Where(a => !a.IsDeleted)
                                         .ToList();

            if (!string.IsNullOrEmpty(name))
            {
                airlines = airlines.Where(a => a.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            if (!string.IsNullOrEmpty(address))
            {
                airlines = airlines.Where(a => a.Address.IndexOf(address, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            if (!string.IsNullOrEmpty(contact))
            {
                airlines = airlines.Where(a => a.ContactInfo.IndexOf(contact, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            foreach (var airline in airlines)
            {
                if (airline.Flights == null)
                {
                    airline.Flights = new List<Flight>();
                }
                if (airline.Reviews == null)
                {
                    airline.Reviews = new List<Review>();
                }
            }

            return airlines;
        }

        [HttpDelete]
        [Route("delete")]
        public IHttpActionResult DeleteAirline(string name)
        {
            var airlines = JsonFileHelper.ReadJsonFile<Airline>(dataFilePath);
            var airline = airlines.FirstOrDefault(a => a.Name == name);
            if (airline == null)
            {
                return NotFound();
            }

            if (airline.Flights.Any(f => f.Status == "ACTIVE"))
            {
                return BadRequest("Cannot delete airline with active flights.");
            }

            airline.IsDeleted = true; // Logical delete
            JsonFileHelper.WriteJsonFile(dataFilePath, airlines);
            return Ok();
        }
        public class EditAirlineRequest
        {
            public Airline UpdatedAirline { get; set; }
            public string OldName { get; set; }
        }

        [HttpPut]
        [Route("edit")]
        public IHttpActionResult EditAirline(EditAirlineRequest request)
        {
            var airlines = JsonFileHelper.ReadJsonFile<Airline>(dataFilePath);
            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightsFilePath);
            var users = JsonFileHelper.ReadJsonFile<User>(usersFilePath);

            var airline = airlines.FirstOrDefault(a => a.Name == request.OldName);
            if (airline == null || airline.IsDeleted)
            {
                return NotFound();
            }

            // Check if the new name already exists
            if (airlines.Any(a => a.Name == request.UpdatedAirline.Name && a.Name != request.OldName))
            {
                return BadRequest("Airline with the same name already exists.");
            }

            // Update the airline name
            airline.Name = request.UpdatedAirline.Name;
            airline.Address = request.UpdatedAirline.Address;
            airline.ContactInfo = request.UpdatedAirline.ContactInfo;

            // Update the airline name in flights
            foreach (var flight in flights.Where(f => f.Airline == request.OldName))
            {
                flight.Airline = request.UpdatedAirline.Name;
            }

            // Update the airline name in user reservations
            foreach (var user in users)
            {
                foreach (var reservation in user.Reservations.Where(r => r.Flight.Airline == request.OldName))
                {
                    reservation.Flight.Airline = request.UpdatedAirline.Name;
                }
            }
            foreach (var flight in airline.Flights)
            {
                flight.Airline = request.UpdatedAirline.Name;
            }
            foreach (var flight in airline.Reviews)
            {
                flight.AirlineName = request.UpdatedAirline.Name;
            }

            JsonFileHelper.WriteJsonFile(dataFilePath, airlines);
            JsonFileHelper.WriteJsonFile(flightsFilePath, flights);
            JsonFileHelper.WriteJsonFile(usersFilePath, users);

            return Ok();
        }
    }
}
