using CampingProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.IO;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace CampingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public BookingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetAllBookings")]
        public async Task<IActionResult> GetAllBookings()
        {
            List<Booking> bookings = new List<Booking>();

            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString()))
                {
                    await con.OpenAsync();

                    string query = "SELECT idbookings, userid, spotid, startdate, enddate FROM bookings";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                bookings.Add(new Booking
                                {
                                    Id = reader.GetInt32("idbookings"),
                                    userID = reader.GetInt32("userid"),
                                    spotID = reader.GetInt32("spotid"),
                                    startDate = DateOnly.FromDateTime(reader.GetDateTime("startdate")),
                                    endDate = DateOnly.FromDateTime(reader.GetDateTime("enddate"))
                                });
                            }
                        }
                    }
                }

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("AddBooking")]
        public async Task<IActionResult> AddBooking( Booking booking)
        {
            try
            {
                var startDate = DateOnly.ParseExact(booking.startDate.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var endDate = DateOnly.ParseExact(booking.endDate.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture);

                using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString()))
                {
                    await con.OpenAsync();

                    string query = "INSERT INTO bookings (userID, spotID, startDate, endDate) VALUES (@userID, @spotID, @startDate, @endDate)";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userID", booking.userID);
                        cmd.Parameters.AddWithValue("@spotID", booking.spotID);
                        cmd.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));

                        await cmd.ExecuteNonQueryAsync();
                    }

                    return Ok("Booking created successfully.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }




    }
}

