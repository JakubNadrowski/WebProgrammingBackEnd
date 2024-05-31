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
using System.Xml.Linq;

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

                    List<string> imagePaths = GetImagePathsForSpot(con, Convert.ToInt64(dt.Rows[i]["idcampingspot"])); // Using 'id' as the primary key column

                    cs.imagePaths = imagePaths;

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
        private List<string> GetImagePathsForSpot(MySqlConnection con, long spotId)
        {
            List<string> imagePaths = new List<string>();
            try
            {
                con.Open(); // Open the connection

                // Query to retrieve image paths for the spot with given spotId
                string query = "SELECT image_path FROM spot_images WHERE spot_id = @SpotId";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SpotId", spotId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            imagePaths.Add(Convert.ToString(reader["image_path"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error retrieving image paths for spot: " + ex.Message);
            }
            finally
            {
                con.Close(); // Close the connection
            }
            return imagePaths;
        }

        [HttpPost]
        [Route("AddSpot")]
        public async Task<IActionResult> AddSpot([FromForm]CampingSpot spot)
        {
            try
            {
                string uploadFolder = @"C:\Users\Legion\Desktop\FrontEnd\Camping\Project 2\camping2\src\assets";

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

                    long spot_id;
                    string getLastInsertedIdQuery = "SELECT LAST_INSERT_ID()";
                    using (MySqlCommand cmd = new MySqlCommand(getLastInsertedIdQuery, con))
                    {
                        var lastInsertedId = await cmd.ExecuteScalarAsync();
                        spot_id = Convert.ToInt64(lastInsertedId);
                    }

                    if (spot.Images != null && spot.Images.Count > 0)
                    {
                        foreach (var image in spot.Images)
                        {
                            var filePath = Path.Combine(uploadFolder, image.FileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            string insertImageQuery = "INSERT INTO spot_images (spot_id, image_path) VALUES (@SpotId, @ImagePath)";
                            using (MySqlCommand imageCmd = new MySqlCommand(insertImageQuery, con))
                            {
                                imageCmd.Parameters.AddWithValue("@SpotId", spot_id);
                                imageCmd.Parameters.AddWithValue("@ImagePath", image.FileName);

                                await imageCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }



                    return Ok("Spot created successfully.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


    }
}
