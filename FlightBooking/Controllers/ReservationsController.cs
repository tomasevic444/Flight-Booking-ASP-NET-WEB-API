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
    [RoutePrefix("api/reservations")]
    public class ReservationsController : ApiController
    {
        private string flightsFilePath = HttpContext.Current.Server.MapPath("~/Data/flights.json");
        private string usersFilePath = HttpContext.Current.Server.MapPath("~/Data/users.json");

        [HttpPost]
        [Route("create")]
        public IHttpActionResult CreateReservation([FromBody] Reservation newReservation)
        {
            if (newReservation == null)
            {
                return BadRequest("Invalid reservation data.");
            }

            // Read existing users
            var users = JsonFileHelper.ReadJsonFile<User>(usersFilePath);
            var user = users.FirstOrDefault(u => u.Username.Equals(newReservation.User.Username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Read flights to update seat availability
            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightsFilePath);
            var flight = flights.FirstOrDefault(f => f.Id == newReservation.Flight.Id);

            if (flight == null || flight.Status != "ACTIVE")
            {
                return BadRequest("Flight not found or not active.");
            }

            if (newReservation.PassengerCount > flight.AvailableSeats)
            {
                return BadRequest("Not enough available seats.");
            }

            // Update flight seat availability
            flight.AvailableSeats -= newReservation.PassengerCount;
            flight.OccupiedSeats += newReservation.PassengerCount;
            JsonFileHelper.WriteJsonFile(flightsFilePath, flights);

            // Generate a unique reservation ID
            int maxReservationId = users.SelectMany(u => u.Reservations ?? new List<Reservation>()).Any() ? users.SelectMany(u => u.Reservations ?? new List<Reservation>()).Max(r => r.Id) : 0;
            newReservation.Id = maxReservationId + 1;

            // Set reservation details
            newReservation.Status = "CREATED";
            newReservation.TotalPrice = flight.Price * newReservation.PassengerCount;

            // Add reservation to the user's reservation list
            user.Reservations.Add(newReservation);
            JsonFileHelper.WriteJsonFile(usersFilePath, users);

            return Ok("Reservation created successfully.");
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetUserReservations(string username, string status = "")
        {
            var users = JsonFileHelper.ReadJsonFile<User>(usersFilePath);
            var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var userReservations = user.Reservations.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                userReservations = userReservations.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(userReservations);
        }
        [HttpPost]
        [Route("cancel")]
        public IHttpActionResult CancelReservation([FromBody] ReservationCancelRequest cancelRequest)
        {
            if (cancelRequest == null)
            {
                return BadRequest("Invalid request data.");
            }

            var users = JsonFileHelper.ReadJsonFile<User>(usersFilePath);
            var user = users.FirstOrDefault(u => u.Reservations != null && u.Reservations.Any(r => r.Id == cancelRequest.ReservationId));

            if (user == null)
            {
                return NotFound();
            }

            var reservation = user.Reservations.FirstOrDefault(r => r.Id == cancelRequest.ReservationId);

            if (reservation == null)
            {
                return NotFound();
            }

            var flights = JsonFileHelper.ReadJsonFile<Flight>(flightsFilePath);
            var flight = flights.FirstOrDefault(f => f.Id == reservation.Flight.Id);

            if (flight == null || flight.Status != "ACTIVE")
            {
                return BadRequest("Flight not found or not active.");
            }

            if (reservation.Status != "CREATED" && reservation.Status != "APPROVED")
            {
                return BadRequest("Reservation cannot be cancelled.");
            }

            var departureTime = reservation.Flight.DepartureDateTime;
            if ((departureTime - DateTime.Now).TotalHours < 24)
            {
                return BadRequest("Cannot cancel reservation within 24 hours of departure.");
            }

            // Update flight seat availability
            flight.AvailableSeats += reservation.PassengerCount;
            flight.OccupiedSeats -= reservation.PassengerCount;
            JsonFileHelper.WriteJsonFile(flightsFilePath, flights);

            // Update reservation status in user data
            reservation.Status = "CANCELLED";
            JsonFileHelper.WriteJsonFile(usersFilePath, users);

            return Ok("Reservation cancelled successfully.");
        }
        [HttpPut]
        [Route("updateStatus")]
        public IHttpActionResult UpdateReservationStatus(int id, string status)
        {
            var users = JsonFileHelper.ReadJsonFile<User>(usersFilePath);
            var user = users.FirstOrDefault(u => u.Reservations != null && u.Reservations.Any(r => r.Id == id));

            if (user == null)
            {
                return NotFound();
            }

            var reservation = user.Reservations.First(r => r.Id == id);

            if (status == "CANCELLED")
            {
                var flights = JsonFileHelper.ReadJsonFile<Flight>(flightsFilePath);
                var flight = flights.FirstOrDefault(f => f.Id == reservation.Flight.Id);
                if (flight != null)
                {
                    var departureTime = flight.DepartureDateTime;
                    if ((departureTime - DateTime.Now).TotalHours < 24)
                    {
                        return BadRequest("Cannot cancel reservation within 24 hours of departure.");
                    }

                    flight.AvailableSeats += reservation.PassengerCount;
                    flight.OccupiedSeats -= reservation.PassengerCount;
                    JsonFileHelper.WriteJsonFile(flightsFilePath, flights);
                }
            }

            reservation.Status = status;
            JsonFileHelper.WriteJsonFile(usersFilePath, users);

            return Ok();
        }
        [HttpGet]
        [Route("all")]
        public IHttpActionResult GetAllReservations()
        {
            var users = JsonFileHelper.ReadJsonFile<User>(usersFilePath);
            var allReservations = users.SelectMany(u => u.Reservations ?? new List<Reservation>()).ToList();
            return Ok(allReservations);
        }
        [HttpGet]
        [Route("flight/{flightId:int}")]
        public IHttpActionResult GetReservationsByFlightId(int flightId)
        {
            var users = JsonFileHelper.ReadJsonFile<User>(usersFilePath);
            var reservations = users.SelectMany(u => u.Reservations ?? new List<Reservation>())
                                    .Where(r => r.Flight.Id == flightId)
                                    .ToList();

            if (!reservations.Any())
            {
                return NotFound();
            }

            return Ok(reservations);
        }


    }
    public class ReservationCancelRequest
    {
        public int ReservationId { get; set; }
    }
}
