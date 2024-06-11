using CampingProject.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.Json;

namespace CampingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public CommentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("AddComment")]
        public async Task<IActionResult> AddComment(Comment newComment)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString()))
                {
                    await con.OpenAsync();

                    string query = "INSERT INTO comments (campingspotid, rating, comment) VALUES (@campingSpotId, @rating,@comment)";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {

                        cmd.Parameters.AddWithValue("@campingSpotId", newComment.spotId);
                        cmd.Parameters.AddWithValue("@rating", newComment.rating);
                        cmd.Parameters.AddWithValue("@comment", newComment.comment);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return Ok("Comment added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetAllComments")]
        public string GetComments()
        {
            MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString());
            MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM comments", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            List<Comment> commentsList = new List<Comment>();
            Response response = new Response();
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Comment comment = new Comment();
                    comment.Id = Convert.ToInt32(dt.Rows[i]["idcomments"]);
                    comment.spotId = Convert.ToInt32(dt.Rows[i]["campingspotid"]);
                    comment.rating = Convert.ToInt32(dt.Rows[i]["rating"]);
                    comment.comment = Convert.ToString(dt.Rows[i]["comment"]);
                    commentsList.Add(comment);
                }
            }
            if (commentsList.Count > 0)
            {
                return JsonSerializer.Serialize(commentsList);
            }
            else
            {
                response.StatusCode = 100;
                response.ErrorMessage = "no data found";
                return JsonSerializer.Serialize(response);
            }
        }

        [HttpGet]
        [Route("GetCommentsBySpotId")]
        public string GetCommentsBySpotId(int spotId)
        {
            MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString());
            MySqlDataAdapter da = new MySqlDataAdapter($"SELECT * FROM comments WHERE campingspotid = {spotId}", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            List<Comment> commentsList = new List<Comment>();
            Response response = new Response();

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Comment comment = new Comment();
                    comment.Id = Convert.ToInt32(dt.Rows[i]["idcomments"]);
                    comment.spotId = Convert.ToInt32(dt.Rows[i]["campingspotid"]);
                    comment.rating = Convert.ToInt32(dt.Rows[i]["rating"]);
                    comment.comment = Convert.ToString(dt.Rows[i]["comment"]);
                    commentsList.Add(comment);
                }
                return JsonSerializer.Serialize(commentsList);
            }
            else
            {
                response.StatusCode = 100;
                response.ErrorMessage = "No comments found for this spot ID";
                return JsonSerializer.Serialize(response);
            }
        }
    }


}
