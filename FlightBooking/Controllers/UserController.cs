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
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private string dataFilePath = HttpContext.Current.Server.MapPath("~/Data/users.json");

        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(User user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user data.");
            }

            var users = JsonFileHelper.ReadJsonFile<User>(dataFilePath);
            if (users.Any(u => u.Username == user.Username))
            {
                return BadRequest("Username already exists.");
            }

            users.Add(user);
            JsonFileHelper.WriteJsonFile(dataFilePath, users);

            return Ok("User registered successfully.");
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginRequest loginRequest)
        {
            if (loginRequest == null)
            {
                return BadRequest("Invalid login data.");
            }

            var users = JsonFileHelper.ReadJsonFile<User>(dataFilePath);
            var user = users.FirstOrDefault(u => u.Username == loginRequest.Username && u.Password == loginRequest.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            // Store user information in session
            HttpContext.Current.Session["Username"] = user.Username;
            HttpContext.Current.Session["UserType"] = user.UserType;

            return Ok(new { userType = user.UserType, username = user.Username });
        }

        [HttpGet]
        [Route("profile")]
        public IHttpActionResult GetProfile(string username)
        {
            var users = JsonFileHelper.ReadJsonFile<User>(dataFilePath);
            var user = users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        [Route("updateProfile")]
        public IHttpActionResult UpdateProfile(User updatedUser)
        {
            if (updatedUser == null)
            {
                return BadRequest("Invalid user data.");
            }

            var users = JsonFileHelper.ReadJsonFile<User>(dataFilePath);
            var userIndex = users.FindIndex(u => u.Username == updatedUser.Username);

            if (userIndex == -1)
            {
                return NotFound();
            }

            var existingUser = users[userIndex];

            // Preserve the existing password if it's not provided in the update
            if (string.IsNullOrEmpty(updatedUser.Password))
            {
                updatedUser.Password = existingUser.Password;
            }
            // Preserve the existing UserType if it's not provided in the update
            if (string.IsNullOrEmpty(updatedUser.UserType))
            {
                updatedUser.UserType = existingUser.UserType;
            }
            if (updatedUser.Reservations == null)
            {
                updatedUser.Reservations = existingUser.Reservations;
            }


            users[userIndex] = updatedUser;
            JsonFileHelper.WriteJsonFile(dataFilePath, users);

            return Ok("Profile updated successfully.");
        }
        [HttpPost]
        [Route("logout")]
        public IHttpActionResult Logout()
        {
            HttpContext.Current.Session.Clear();
            return Ok("Logged out successfully.");
        }
        [HttpGet]
        [Route("isAuthenticated")]
        public IHttpActionResult IsAuthenticated()
        {
            if (HttpContext.Current.Session["Username"] != null)
            {
                return Ok(new
                {
                    isAuthenticated = true,
                    username = HttpContext.Current.Session["Username"].ToString(),
                    userType = HttpContext.Current.Session["UserType"].ToString()
                });
            }
            return Ok(new { isAuthenticated = false });
        }
        [HttpGet]
        [Route("getAllUsers")]
        public IHttpActionResult GetAllUsers()
        {
            var users = JsonFileHelper.ReadJsonFile<User>(dataFilePath);
            return Ok(users);
        }

        [HttpGet]
        [Route("searchUsers")]
        public IHttpActionResult SearchUsers(string firstName = null, string lastName = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var users = JsonFileHelper.ReadJsonFile<User>(dataFilePath);
            var filteredUsers = users.Where(u =>
                (string.IsNullOrEmpty(firstName) || u.FirstName.Contains(firstName)) &&
                (string.IsNullOrEmpty(lastName) || u.LastName.Contains(lastName)) &&
                (!dateFrom.HasValue || u.DateOfBirth >= dateFrom.Value) &&
                (!dateTo.HasValue || u.DateOfBirth <= dateTo.Value)
            ).ToList();

            return Ok(filteredUsers);
        }

        [HttpGet]
        [Route("sortUsers")]
        public IHttpActionResult SortUsers(string sortBy, bool ascending)
        {
            var users = JsonFileHelper.ReadJsonFile<User>(dataFilePath);
            List<User> sortedUsers;

            switch (sortBy)
            {
                case "FirstName":
                    sortedUsers = ascending ? users.OrderBy(u => u.FirstName).ToList() : users.OrderByDescending(u => u.FirstName).ToList();
                    break;
                case "LastName":
                    sortedUsers = ascending ? users.OrderBy(u => u.LastName).ToList() : users.OrderByDescending(u => u.LastName).ToList();
                    break;
                case "DateOfBirth":
                    sortedUsers = ascending ? users.OrderBy(u => u.DateOfBirth).ToList() : users.OrderByDescending(u => u.DateOfBirth).ToList();
                    break;
                default:
                    sortedUsers = users;
                    break;
            }

            return Ok(sortedUsers);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
