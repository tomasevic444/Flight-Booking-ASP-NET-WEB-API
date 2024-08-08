using FlightBooking.Models;
using FlightBooking.Ultities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FlightBooking.Controllers
{
    [RoutePrefix("api/reviews")]
    public class ReviewsController : ApiController
    {
        private string airlinesFilePath = HttpContext.Current.Server.MapPath("~/Data/airlines.json");
        private string usersFilePath = HttpContext.Current.Server.MapPath("~/Data/users.json");
        private string imagesFolderPath = HttpContext.Current.Server.MapPath("~/Data/Images/");

        [HttpPost]
        [Route("create")]
        public IHttpActionResult CreateReview([FromBody] ReviewCreateRequest reviewRequest)
        {
            if (reviewRequest == null || string.IsNullOrEmpty(reviewRequest.Title) ||
                string.IsNullOrEmpty(reviewRequest.Content) || reviewRequest.ReservationId == 0)
            {
                return BadRequest("Invalid review data.");
            }

            var users = JsonFileHelper.ReadJsonFile<User>(usersFilePath);
            if (users == null)
            {
                return InternalServerError(new Exception("Failed to read users data."));
            }

            var user = users.FirstOrDefault(u => u.Reservations != null && u.Reservations.Any(r => r.Id == reviewRequest.ReservationId));
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var reservation = user.Reservations.FirstOrDefault(r => r.Id == reviewRequest.ReservationId);
            if (reservation == null)
            {
                return BadRequest("Reservation not found.");
            }

            var airlines = JsonFileHelper.ReadJsonFile<Airline>(airlinesFilePath);
            if (airlines == null)
            {
                return InternalServerError(new Exception("Failed to read airlines data."));
            }

            var airline = airlines.FirstOrDefault(a => a.Name.Equals(reservation.Flight.Airline, StringComparison.OrdinalIgnoreCase));
            if (airline == null)
            {
                return BadRequest("Airline not found.");
            }

            // Generate a unique review ID
            int maxReviewId = airlines.SelectMany(a => a.Reviews).Any() ? airlines.SelectMany(a => a.Reviews).Max(r => r.Id) : 0;

            string imagePath = null;
            if (!string.IsNullOrEmpty(reviewRequest.Image))
            {
                var base64Data = reviewRequest.Image.Substring(reviewRequest.Image.IndexOf(',') + 1);
                byte[] imageBytes = Convert.FromBase64String(base64Data);
                string fileName = $"{Guid.NewGuid()}.jpg";
                imagePath = Path.Combine(imagesFolderPath, fileName);
                File.WriteAllBytes(imagePath, imageBytes);
                imagePath = "/Data/Images/" + fileName; // Use relative path for saving in JSON
            }

            var newReview = new Review
            {
                Id = maxReviewId + 1,
                Reviewer = user.Username, // Use the username of the user
                AirlineName = airline.Name,
                Title = reviewRequest.Title,
                Content = reviewRequest.Content,
                Image = imagePath,
                Status = "CREATED" // Or "APPROVED" based on your application logic
            };

            airline.Reviews.Add(newReview);
            JsonFileHelper.WriteJsonFile(airlinesFilePath, airlines);

            return Ok("Review submitted successfully.");
        }

        [HttpGet]
        [Route("getAllReviews")]
        public IHttpActionResult GetAllReviews()
        {
            var airlines = JsonFileHelper.ReadJsonFile<Airline>(airlinesFilePath);
            if (airlines == null)
            {
                return InternalServerError(new Exception("Failed to read airlines data."));
            }

            var reviews = airlines.SelectMany(a => a.Reviews).ToList();
            return Ok(reviews);
        }

        [HttpPost]
        [Route("updateStatus")]
        public IHttpActionResult UpdateReviewStatus([FromBody] UpdateReviewStatusRequest request)
        {
            var airlines = JsonFileHelper.ReadJsonFile<Airline>(airlinesFilePath);
            if (airlines == null)
            {
                return InternalServerError(new Exception("Failed to read airlines data."));
            }

            var review = airlines.SelectMany(a => a.Reviews).FirstOrDefault(r => r.Id == request.ReviewId);
            if (review == null)
            {
                return BadRequest("Review not found.");
            }

            review.Status = request.Status;
            JsonFileHelper.WriteJsonFile(airlinesFilePath, airlines);

            return Ok("Review status updated successfully.");
        }
    }

    public class UpdateReviewStatusRequest
    {
        public int ReviewId { get; set; }
        public string Status { get; set; }
    }

    public class ReviewCreateRequest
    {
        public int ReservationId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Image { get; set; } // Base64 encoded image string
    }
}