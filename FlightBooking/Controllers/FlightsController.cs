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
    [RoutePrefix("api/flights")]
    public class FlightController : ApiController
    {
        private string flightDataFilePath = HttpContext.Current.Server.MapPath("~/Data/flights.json");
        private string airlineDataFilePath = HttpContext.Current.Server.MapPath("~/Data/airlines.json");
        private string userDataFilePath = HttpContext.Current.Server.MapPath("~/Data/users.json");


        [HttpGet]
        [Route("")]
        public IEnumerable<Flight> GetActiveFlights()
        {
            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightDataFilePath);
            var users = JsonFileHelper.ReadJsonFile<User>(userDataFilePath);
            var currentTime = DateTime.UtcNow;
            var airlines = JsonFileHelper.ReadJsonFile<Airline>(airlineDataFilePath);

            foreach (var flight in flights)
            {
                if (flight.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) && flight.ArrivalDateTime < currentTime)
                {
                    flight.Status = "FINISHED";

                    // Update flight status in airlines.json
                    var airline = airlines.FirstOrDefault(a => a.Name.Equals(flight.Airline, StringComparison.OrdinalIgnoreCase));
                    if (airline != null)
                    {
                        var airlineFlight = airline.Flights.FirstOrDefault(f => f.Id == flight.Id);
                        if (airlineFlight != null)
                        {
                            airlineFlight.Status = "FINISHED";
                        }
                    }

                    // Update flight status in users.json
                    foreach (var user in users)
                    {
                        if (user.Reservations != null)
                        {
                            foreach (var reservation in user.Reservations)
                            {
                                if (reservation.Flight.Id == flight.Id)
                                {
                                    reservation.Flight.Status = "FINISHED";
                                    reservation.Status = "FINISHED";
                                }
                            }
                        }
                    }
                }
            }

            // Save changes back to the files
            JsonFileHelper.WriteJsonFile(flightDataFilePath, flights);
            JsonFileHelper.WriteJsonFile(airlineDataFilePath, airlines);
            JsonFileHelper.WriteJsonFile(userDataFilePath, users);

            return flights.Where(f => f.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase));
        }
        [HttpGet]
        [Route("non-deleted")]
        public IHttpActionResult GetNonDeletedFlights()
        {
            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightDataFilePath);
            var users = JsonFileHelper.ReadJsonFile<User>(userDataFilePath);
            var currentTime = DateTime.UtcNow;
            var airlines = JsonFileHelper.ReadJsonFile<Airline>(airlineDataFilePath);

            foreach (var flight in flights)
            {
                if (flight.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) && flight.ArrivalDateTime < currentTime)
                {
                    flight.Status = "FINISHED";

                    // Update flight status in airlines.json
                    var airline = airlines.FirstOrDefault(a => a.Name.Equals(flight.Airline, StringComparison.OrdinalIgnoreCase));
                    if (airline != null)
                    {
                        var airlineFlight = airline.Flights.FirstOrDefault(f => f.Id == flight.Id);
                        if (airlineFlight != null)
                        {
                            airlineFlight.Status = "FINISHED";
                        }
                    }

                    // Update flight status in users.json
                    foreach (var user in users)
                    {
                        if (user.Reservations != null)
                        {
                            foreach (var reservation in user.Reservations)
                            {
                                if (reservation.Flight.Id == flight.Id)
                                {
                                    reservation.Flight.Status = "FINISHED";
                                    reservation.Status = "FINISHED";
                                }
                            }
                        }
                    }
                }
            }

            // Save changes back to the files
            JsonFileHelper.WriteJsonFile(flightDataFilePath, flights);
            JsonFileHelper.WriteJsonFile(airlineDataFilePath, airlines);
            JsonFileHelper.WriteJsonFile(userDataFilePath, users);
            var nonDeletedFlights = flights.Where(f => !f.Status.Equals("DELETED", StringComparison.OrdinalIgnoreCase)).ToList();
            return Ok(nonDeletedFlights);
        }

        [HttpGet]
        [Route("all")]
        public IHttpActionResult GetAllFlights(string username = null, string status = "ACTIVE")
        {
            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightDataFilePath);
            var users = JsonFileHelper.ReadJsonFile<User>(userDataFilePath);

            if (!string.IsNullOrEmpty(username))
            {
                var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user != null)
                {
                    var userSpecificFlights = new List<Flight>();

                    // Add user reservations with FINISHED or CANCELLED status
                    var userReservations = user.Reservations
                        .Where(r => r.Flight.Status.Equals("FINISHED", StringComparison.OrdinalIgnoreCase) ||
                                    r.Flight.Status.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
                        .Select(r => r.Flight)
                        .ToList();
                    userSpecificFlights.AddRange(userReservations);

                    // Add all ACTIVE flights
                    var activeFlights = flights.Where(f => f.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase)).ToList();
                    userSpecificFlights.AddRange(activeFlights);

                    // Ensure no duplicates
                    var distinctFlights = userSpecificFlights.Distinct().ToList();

                    return Ok(distinctFlights);
                }
            }

            if (!string.IsNullOrEmpty(status) && !status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                flights = flights.Where(f => f.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(flights);
        }

        [HttpGet]
        [Route("search")]
        public IEnumerable<Flight> SearchFlights(string departure = null, string destination = null, DateTime? departureDate = null, DateTime? arrivalDate = null, string airline = null)
        {
            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightDataFilePath);
            return flights.Where(f =>
                (string.IsNullOrEmpty(departure) || f.Departure.Equals(departure, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(destination) || f.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase)) &&
                (!departureDate.HasValue || f.DepartureDateTime.Date == departureDate.Value.Date) &&
                (!arrivalDate.HasValue || f.ArrivalDateTime.Date == arrivalDate.Value.Date) &&
                (string.IsNullOrEmpty(airline) || f.Airline.Equals(airline, StringComparison.OrdinalIgnoreCase)) &&
                f.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase));
        }

        [HttpGet]
        [Route("sort")]
        public IEnumerable<Flight> SortFlights(string sortBy)
        {
            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightDataFilePath);

            if (sortBy == "priceAsc")
            {
                return flights.OrderBy(f => f.Price);
            }
            else if (sortBy == "priceDesc")
            {
                return flights.OrderByDescending(f => f.Price);
            }
            return flights;
        }

        [HttpPost]
        [Route("add")]
        public IHttpActionResult AddFlight(Flight newFlight)
        {
            if (newFlight == null)
            {
                return BadRequest("Invalid flight data.");
            }

            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightDataFilePath);

            // Generate a unique ID for the new flight
            int maxFlightId = flights.Any() ? flights.Max(f => f.Id) : 0;
            newFlight.Id = maxFlightId + 1;

            flights.Add(newFlight);
            JsonFileHelper.WriteJsonFile(flightDataFilePath, flights);

            var airlines = JsonFileHelper.ReadJsonFile<Airline>(airlineDataFilePath);
            var airline = airlines.FirstOrDefault(a => a.Name.Equals(newFlight.Airline, StringComparison.OrdinalIgnoreCase));
            if (airline != null)
            {
                airline.Flights.Add(newFlight);
                JsonFileHelper.WriteJsonFile(airlineDataFilePath, airlines);
            }

            return Ok("Flight added successfully.");
        }

        [HttpPut]
        [Route("edit")]
        public IHttpActionResult EditFlight(Flight updatedFlight)
        {
            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightDataFilePath);
            var flight = flights.FirstOrDefault(f => f.Id == updatedFlight.Id);
            if (flight == null)
            {
                return NotFound();
            }

            // Prevent changing departure and destination
            if (flight.Departure != updatedFlight.Departure || flight.Destination != updatedFlight.Destination)
            {
                return BadRequest("Cannot change departure and destination.");
            }

            var users = JsonFileHelper.ReadJsonFile<User>(userDataFilePath);
            foreach (var user in users)
            {
                if (user.Reservations != null)
                {
                    foreach (var reservation in user.Reservations)
                    {
                        if (reservation.Flight.Id == updatedFlight.Id &&
                            (reservation.Status == "CREATED" || reservation.Status == "APPROVED"))
                        {
                            if (flight.Price != updatedFlight.Price)
                            {
                                return BadRequest("Cannot change the price of a flight with reservations in KREIRANA/ODOBRENA status.");
                            }
                        }
                    }
                }
            }

            // If the airline has changed, update the airlines' flight lists
            if (!flight.Airline.Equals(updatedFlight.Airline, StringComparison.OrdinalIgnoreCase))
            {
                var airlines = JsonFileHelper.ReadJsonFile<Airline>(airlineDataFilePath);

                // Remove the flight from the old airline's list
                var oldAirline = airlines.FirstOrDefault(a => a.Name.Equals(flight.Airline, StringComparison.OrdinalIgnoreCase));
                if (oldAirline != null)
                {
                    oldAirline.Flights.RemoveAll(f => f.Id == updatedFlight.Id);
                }

                // Add the flight to the new airline's list
                var newAirline = airlines.FirstOrDefault(a => a.Name.Equals(updatedFlight.Airline, StringComparison.OrdinalIgnoreCase));
                if (newAirline != null)
                {
                    newAirline.Flights.Add(updatedFlight);
                }

                JsonFileHelper.WriteJsonFile(airlineDataFilePath, airlines);
            }
            else {
                var airlines = JsonFileHelper.ReadJsonFile<Airline>(airlineDataFilePath);
                var airline = airlines.FirstOrDefault(a => a.Name.Equals(flight.Airline, StringComparison.OrdinalIgnoreCase));
                if (airline != null)
                {
                    var airlineFlight = airline.Flights.FirstOrDefault(f => f.Id == updatedFlight.Id);
                    if (airlineFlight != null)
                    {
                        airlineFlight.DepartureDateTime = updatedFlight.DepartureDateTime;
                        airlineFlight.ArrivalDateTime = updatedFlight.ArrivalDateTime;
                        airlineFlight.AvailableSeats = updatedFlight.AvailableSeats;
                        airlineFlight.OccupiedSeats = updatedFlight.OccupiedSeats;
                        airlineFlight.Price = updatedFlight.Price;
                        airlineFlight.Status = updatedFlight.Status;
                    }
                    JsonFileHelper.WriteJsonFile(airlineDataFilePath, airlines);
                }

            }

            // Update the flight details
            flight.Airline = updatedFlight.Airline;
            flight.DepartureDateTime = updatedFlight.DepartureDateTime;
            flight.ArrivalDateTime = updatedFlight.ArrivalDateTime;
            flight.AvailableSeats = updatedFlight.AvailableSeats;
            flight.OccupiedSeats = updatedFlight.OccupiedSeats;
            flight.Price = updatedFlight.Price;
            flight.Status = updatedFlight.Status;

            // Update the flight in users.json
            foreach (var user in users)
            {
                if (user.Reservations != null)
                {
                    foreach (var reservation in user.Reservations)
                    {
                        if (reservation.Flight.Id == updatedFlight.Id)
                        {
                            reservation.Flight.Airline = updatedFlight.Airline;
                            reservation.Flight.DepartureDateTime = updatedFlight.DepartureDateTime;
                            reservation.Flight.ArrivalDateTime = updatedFlight.ArrivalDateTime;
                            reservation.Flight.AvailableSeats = updatedFlight.AvailableSeats;
                            reservation.Flight.OccupiedSeats = updatedFlight.OccupiedSeats;
                            reservation.Flight.Price = updatedFlight.Price;
                            reservation.Flight.Status = updatedFlight.Status;

                            if (updatedFlight.Status == "CANCELLED")
                            {
                                reservation.Status = "CANCELLED";
                            }
                            if (updatedFlight.Status == "FINISHED")
                            {
                                reservation.Status = "FINISHED";
                            }
                        }
                    }
                }
            }
            JsonFileHelper.WriteJsonFile(userDataFilePath, users);

            JsonFileHelper.WriteJsonFile(flightDataFilePath, flights);
            return Ok("Flight updated successfully.");
        }

        [HttpDelete]
        [Route("delete")]
        public IHttpActionResult DeleteFlight(int id)
        {
            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightDataFilePath);
            var flight = flights.FirstOrDefault(f => f.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            var users = JsonFileHelper.ReadJsonFile<User>(userDataFilePath);
            foreach (var user in users)
            {
                if (user.Reservations != null)
                {
                    foreach (var reservation in user.Reservations)
                    {
                        if (reservation.Flight.Id == id && (reservation.Status == "CREATED" || reservation.Status == "APPROVED"))
                        {
                            return BadRequest("Cannot delete flight with reservations having status 'CREATED' or 'APPROVED'.");
                        }
                    }
                }
            }

            // Update the flight status in the airline file
            var airlines = JsonFileHelper.ReadJsonFile<Airline>(airlineDataFilePath);
            foreach (var airline in airlines)
            {
                var airlineFlight = airline.Flights.FirstOrDefault(f => f.Id == id);
                if (airlineFlight != null)
                {
                    airlineFlight.Status = "DELETED";
                    break;
                }
            }

            // Update the flight status in the user reservations
            foreach (var user in users)
            {
                if (user.Reservations != null)
                {
                    foreach (var reservation in user.Reservations)
                    {
                        if (reservation.Flight.Id == id)
                        {
                            reservation.Flight.Status = "DELETED";
                        }
                    }
                }
            }
            foreach (var flightt in flights)
            {
                if (flightt.Id == id)
                {
                    flightt.Status = "DELETED";
                }
            }
            // Save changes to the files
            JsonFileHelper.WriteJsonFile(airlineDataFilePath, airlines);
            JsonFileHelper.WriteJsonFile(userDataFilePath, users);
            JsonFileHelper.WriteJsonFile(flightDataFilePath, flights);



            return Ok();
        }
        [HttpGet]
        [Route("searchFlights")]
        public IHttpActionResult SearchFlights(string departure = null, string destination = null, DateTime? departureDate = null)
        {
            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightDataFilePath);
            var filteredFlights = flights.Where(f =>
                (string.IsNullOrEmpty(departure) || f.Departure.Equals(departure, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(destination) || f.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase)) &&
                (!departureDate.HasValue || f.DepartureDateTime.Date == departureDate.Value.Date)).ToList();

            return Ok(filteredFlights);
        }
    }
}
