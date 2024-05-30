using CampingProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.IO;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CampingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampingSpotController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public CampingSpotController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetAllSpots")]
        public string GetSpots()
        {
            MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString());
            MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM campingspot", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            List<CampingSpot> spotsList = new List<CampingSpot>();
            Response response = new Response();
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    CampingSpot cs = new CampingSpot();
                    cs.id = Convert.ToInt32(dt.Rows[i]["idcampingspot"]);
                    cs.owner = Convert.ToInt32(dt.Rows[i]["owner"]);
                    cs.name = Convert.ToString(dt.Rows[i]["name"]);
                    cs.location = Convert.ToString(dt.Rows[i]["location"]);
                    cs.description = Convert.ToString(dt.Rows[i]["description"]);
                    cs.capacity = Convert.ToInt32(dt.Rows[i]["capacity"]);
                    cs.price = Convert.ToInt32(dt.Rows[i]["price"]);

                    spotsList.Add(cs);
                }
            }
            if (spotsList.Count > 0)
            {
                return JsonSerializer.Serialize(spotsList);
            }
            else
            {
                response.StatusCode = 100;
                response.ErrorMessage = "no data found";
                return JsonSerializer.Serialize(response);
            }
        }

        [HttpPost]
        [Route("AddSpot")]
        public async Task<IActionResult> AddSpot(CampingSpot spot)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString()))
                {
                    await con.OpenAsync();

                    string query = "INSERT INTO campingspot (owner, name, location, description, capacity, price) VALUES (@owner, @name,@location,@description,@capacity,@price)";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {

                        cmd.Parameters.AddWithValue("@owner", spot.owner);
                        cmd.Parameters.AddWithValue("@name", spot.name);
                        cmd.Parameters.AddWithValue("@location", spot.location);
                        cmd.Parameters.AddWithValue("@description", spot.description);
                        cmd.Parameters.AddWithValue("@capacity", spot.capacity);
                        cmd.Parameters.AddWithValue("@price", spot.price);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return Ok("Spot created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
