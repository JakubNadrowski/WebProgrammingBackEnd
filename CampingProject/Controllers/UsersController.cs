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
    public class UsersController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public string GetUser()
        {
            MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString());
            MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM user", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            List<User> usersList = new List<User>();
            Response response = new Response();
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    User user = new User();
                    user.Id = Convert.ToInt32(dt.Rows[i]["iduser"]);
                    user.fName = Convert.ToString(dt.Rows[i]["fname"]);
                    user.lName = Convert.ToString(dt.Rows[i]["lname"]);
                    user.isOwner = Convert.ToInt32(dt.Rows[i]["owner"]);
                    user.email = Convert.ToString(dt.Rows[i]["email"]);
                    user.password = Convert.ToString(dt.Rows[i]["password"]);
                    usersList.Add(user);
                }
            }
            if (usersList.Count > 0)
            {
                return JsonSerializer.Serialize(usersList);
            }
            else
            {
                response.StatusCode = 100;
                response.ErrorMessage = "no data found";
                return JsonSerializer.Serialize(response);
            }
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> AddUser(User newUser)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString()))
                {
                    await con.OpenAsync();

                    string query = "INSERT INTO user (fname, lname, owner, email, password) VALUES (@fName, @lName,@isOwner,@Email,@Password)";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        
                        cmd.Parameters.AddWithValue("@fName", newUser.fName);
                        cmd.Parameters.AddWithValue("@lName", newUser.lName);
                        cmd.Parameters.AddWithValue("@isOwner", newUser.isOwner);
                        cmd.Parameters.AddWithValue("@Email", newUser.email);
                        cmd.Parameters.AddWithValue("@Password", newUser.password);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return Ok("User added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(UserLoginRequest loginRequest)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString()))
                {
                    await con.OpenAsync();

                    string query = "SELECT * FROM user WHERE email = @Email AND password = @Password";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", loginRequest.email);
                        cmd.Parameters.AddWithValue("@Password", loginRequest.password);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                // Authentication successful
                                return Ok(new { success = true });
                            }
                            else
                            {
                                // Authentication failed
                                return Ok(new { success = false });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
