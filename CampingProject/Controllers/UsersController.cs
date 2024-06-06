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
using BCrypt.Net;

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
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginRequest)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString()))
                {
                    await con.OpenAsync();

                    string query = "SELECT * FROM user WHERE email = @Email";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", loginRequest.email);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                string storedHash = Convert.ToString(reader["password"]);

                                if (BCrypt.Net.BCrypt.Verify(loginRequest.password, storedHash))
                                {
                                    User user = new User
                                    {
                                        Id = Convert.ToInt32(reader["iduser"]),
                                        fName = Convert.ToString(reader["fname"]),
                                        lName = Convert.ToString(reader["lname"]),
                                        isOwner = Convert.ToInt32(reader["owner"]),
                                        email = Convert.ToString(reader["email"]),
                                        // Don't send the password back to the client
                                    };

                                    // Authentication successful
                                    return Ok(new { success = true, user });
                                }
                                else
                                {
                                    // Authentication failed
                                    return Ok(new { success = false });
                                }
                            }
                            else
                            {
                                // No user found with the provided email
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

        [HttpPut]
        [Route("UpdateUser")]
        public string UpdateSpot([FromBody] User user)
        {
            Response response = new Response();

            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString()))
                {
                    con.Open();

                    string query = "UPDATE user SET fname = @Fname, lname = @Lname, email = @Email, password = @Password WHERE iduser = @Id";
                    MySqlCommand cmd = new MySqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Id", user.Id);
                    cmd.Parameters.AddWithValue("@Fname", user.fName);
                    cmd.Parameters.AddWithValue("@Lname", user.lName);
                    cmd.Parameters.AddWithValue("@Email", user.email);
                    cmd.Parameters.AddWithValue("@Password", user.password);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        response.StatusCode = 200;
                        response.ErrorMessage = "User data updated successfully";
                    }
                    else
                    {
                        response.StatusCode = 100;
                        response.ErrorMessage = "No user found with the provided ID";
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
    }
}
