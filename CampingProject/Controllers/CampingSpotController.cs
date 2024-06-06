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
                string query = "SELECT image_data FROM spot_images WHERE spot_id = @SpotId";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SpotId", spotId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] imageData = (byte[])reader["image_data"];
                            string base64String = Convert.ToBase64String(imageData);
                            imagePaths.Add(base64String);
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

        [HttpPut]
        [Route("UpdateSpot")]
        public string UpdateSpot([FromBody] CampingSpot spot)
        {
            Response response = new Response();

            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString()))
                {
                    con.Open();

                    string query = "UPDATE campingspot SET owner = @Owner, name = @Name, location = @Location, description = @Description, capacity = @Capacity, price = @Price WHERE idcampingspot = @Id";
                    MySqlCommand cmd = new MySqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Id", spot.id);
                    cmd.Parameters.AddWithValue("@Owner", spot.owner);
                    cmd.Parameters.AddWithValue("@Name", spot.name);
                    cmd.Parameters.AddWithValue("@Location", spot.location);
                    cmd.Parameters.AddWithValue("@Description", spot.description);
                    cmd.Parameters.AddWithValue("@Capacity", spot.capacity);
                    cmd.Parameters.AddWithValue("@Price", spot.price);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        response.StatusCode = 200;
                        response.ErrorMessage = "Camping spot updated successfully";
                    }
                    else
                    {
                        response.StatusCode = 100;
                        response.ErrorMessage = "No spot found with the provided ID";
                    }
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.ErrorMessage = $"Error: {ex.Message}";
            }

            return JsonSerializer.Serialize(response);
        }


        [HttpPost]
        [Route("AddSpot")]
        public async Task<IActionResult> AddSpot([FromForm]CampingSpot spot)
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
                            byte[] imageData;
                            using (var ms = new MemoryStream())
                            {
                                await image.CopyToAsync(ms);
                                imageData = ms.ToArray();
                            }

                            string insertImageQuery = "INSERT INTO spot_images (spot_id, image_data) VALUES (@SpotId, @ImageData)";
                            using (MySqlCommand imageCmd = new MySqlCommand(insertImageQuery, con))
                            {
                                imageCmd.Parameters.AddWithValue("@SpotId", spot_id);
                                imageCmd.Parameters.AddWithValue("@ImageData", imageData);

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
