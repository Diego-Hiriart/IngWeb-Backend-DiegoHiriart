using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Diagnostics;
using WebAPI_DiegoHiriart.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebAPI_DiegoHiriart.Settings;

namespace WebAPI_DiegoHiriart.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        //A constructor for this class is needed so that when it is called the config and environment info needed are passed
        public PostsController(IConfiguration config, IWebHostEnvironment env)
        {
            this.config = config;
            this.env = env;
            this.db = new AppSettings(this.config, this.env).DBConn;
        }
        //These configurations and environment info are needed to create a DBConfig instance that has the right connection string depending on whether the app is running on a development or production environment
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;
        private string db;//Connection string

        private readonly string userDataClaim = ClaimTypes.UserData;

        [HttpPost, Authorize]
        public async Task<ActionResult<List<Post>>> CreatePost(Post post)
        {
            string createPost = "INSERT INTO posts(userid, modelid, postdate, purchase, firstissues, innoperative, review) " +
                "VALUES(@0, @1, @2, @3, @4, @5, @6)";
            string checkExisting = "SELECT COUNT(*) FROM posts WHERE modelid = @0 AND userid = @1";//Part of the control to see that each user makes only one post per model
            int postCount = 0;

            Int64 userId = this.TokenUserId(Request);

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())//Get how many posts have been made by the user for the model, if not zero, it cant be posted
                        {
                            cmd.CommandText = checkExisting;
                            cmd.Parameters.AddWithValue("@0", post.ModelId);
                            cmd.Parameters.AddWithValue("@1", userId);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    postCount = reader.GetInt32(0);
                                }
                            }
                            if (postCount > 0)
                            {
                                return StatusCode(400, "User already has a post for that model");
                            }
                        }
                        
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = createPost;
                            cmd.Parameters.AddWithValue("@0", userId);//Replace the parameters
                            cmd.Parameters.AddWithValue("@1", post.ModelId);
                            cmd.Parameters.AddWithValue("@2", post.PostDate);
                            cmd.Parameters.AddWithValue("@3", post.Purchase);
                            if (post.FirstIssues is null)
                            {
                                cmd.Parameters.AddWithValue("@4", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@4", post.FirstIssues);
                            }
                            if (post.Innoperative is null)
                            {
                                cmd.Parameters.AddWithValue("@5", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@5", post.Innoperative);
                            }
                            cmd.Parameters.AddWithValue("@6", post.Review);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
                return Ok(post);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500, post);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Post>>> GetAll()
        {
            List<Post> posts = new List<Post>();
            string getPosts = "SELECT * FROM posts";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getPosts;
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var post = new Post();
                                    post.PostId = reader.GetInt64(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    post.UserId = reader.GetInt64(1);
                                    post.ModelId = reader.GetInt64(2);
                                    post.PostDate = reader.GetDateTime(3);
                                    post.Purchase = reader.GetDateTime(4);
                                    post.FirstIssues = reader[5] as DateTime?;
                                    post.Innoperative = reader[6] as DateTime?;
                                    post.Review = reader[7] as string;
                                    posts.Add(post);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return Ok(posts);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpGet("by-user/{id}"), Authorize]
        public async Task<ActionResult<List<Post>>> GetByUser(Int64 id)
        {
            List<Post> posts = new List<Post>();
            string getPostsByUser = "SELECT * FROM posts WHERE userid = @0";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getPostsByUser;
                            cmd.Parameters.AddWithValue("@0", id);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var post = new Post();
                                    post.PostId = reader.GetInt64(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    post.UserId = reader.GetInt64(1);
                                    post.ModelId = reader.GetInt64(2);
                                    post.PostDate = reader.GetDateTime(3);
                                    post.Purchase = reader.GetDateTime(4);
                                    post.FirstIssues = reader[5] as DateTime?;
                                    post.Innoperative = reader[6] as DateTime?;
                                    post.Review = reader[7] as string;
                                    posts.Add(post);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return Ok(posts);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        //Automatically gets the user's post by getting the ID from the token
        [HttpGet("by-user-auto"), Authorize]
        public async Task<ActionResult<List<Post>>> GetByUserAuto()
        {
            Int64 userId = this.TokenUserId(Request);

            List<Post> posts = new List<Post>();
            string getPostsByUser = "SELECT * FROM posts WHERE userid = @0";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getPostsByUser;
                            cmd.Parameters.AddWithValue("@0", userId);//Use token's userId for the search
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var post = new Post();
                                    post.PostId = reader.GetInt64(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    post.UserId = reader.GetInt64(1);
                                    post.ModelId = reader.GetInt64(2);
                                    post.PostDate = reader.GetDateTime(3);
                                    post.Purchase = reader.GetDateTime(4);
                                    post.FirstIssues = reader[5] as DateTime?;
                                    post.Innoperative = reader[6] as DateTime?;
                                    post.Review = reader[7] as string;
                                    posts.Add(post);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return Ok(posts);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpGet("search/{id}"), Authorize]
        public async Task<ActionResult<List<Post>>> GetById(Int64 id)
        {
            List<Post> posts = new List<Post>();
            string getPostsByUser = "SELECT * FROM posts WHERE postid = @0";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getPostsByUser;
                            cmd.Parameters.AddWithValue("@0", id);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var post = new Post();
                                    post.PostId = reader.GetInt64(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    post.UserId = reader.GetInt64(1);
                                    post.ModelId = reader.GetInt64(2);
                                    post.PostDate = reader.GetDateTime(3);
                                    post.Purchase = reader.GetDateTime(4);
                                    post.FirstIssues = reader[5] as DateTime?;
                                    post.Innoperative = reader[6] as DateTime?;
                                    post.Review = reader[7] as string;
                                    posts.Add(post);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return Ok(posts);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpGet("by-model/{id}")]
        public async Task<ActionResult<List<Post>>> GetByModel(Int64 id)
        {
            List<Post> posts = new List<Post>();
            string getPostsByModel = "SELECT * FROM posts WHERE modelid = @0";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getPostsByModel;
                            cmd.Parameters.AddWithValue("@0", id);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var post = new Post();
                                    post.PostId = reader.GetInt64(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    post.UserId = reader.GetInt64(1);
                                    post.ModelId = reader.GetInt64(2);
                                    post.PostDate = reader.GetDateTime(3);
                                    post.Purchase = reader.GetDateTime(4);
                                    post.FirstIssues = reader[5] as DateTime?;
                                    post.Innoperative = reader[6] as DateTime?;
                                    post.Review = reader[7] as string;
                                    posts.Add(post);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return Ok(posts);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpPut, Authorize]
        public async Task<ActionResult<List<Post>>> UpdatePost(Post post)
        {
            string updatePost = "UPDATE posts SET modelid=@0, postdate=@1, purchase=@2, firstissues=@3, " +
                "innoperative=@4, review=@5 WHERE postid = @6";

            Int64 userId = this.TokenUserId(Request);

            if (userId == post.UserId)//If the author on the DB and the user trying to edit are the same, then it can be edited
            {
                try
                {
                    int affectedRows = 0;
                    using (NpgsqlConnection conn = new NpgsqlConnection(db))
                    {
                        conn.Open();
                        if (conn.State == ConnectionState.Open)
                        {
                            using (NpgsqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = updatePost;
                                cmd.Parameters.AddWithValue("@0", post.ModelId);
                                cmd.Parameters.AddWithValue("@1", post.PostDate);
                                cmd.Parameters.AddWithValue("@2", post.Purchase);
                                if (post.FirstIssues is null)
                                {
                                    cmd.Parameters.AddWithValue("@3", DBNull.Value);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@3", post.FirstIssues);
                                }
                                if (post.Innoperative is null)
                                {
                                    cmd.Parameters.AddWithValue("@4", DBNull.Value);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@4", post.Innoperative);
                                }
                                cmd.Parameters.AddWithValue("@5", post.Review);
                                cmd.Parameters.AddWithValue("@6", post.PostId);
                                affectedRows = cmd.ExecuteNonQuery();
                            }
                        }
                        conn.Close();
                    }
                    if (affectedRows > 0)
                    {
                        return Ok(post);
                    }

                }
                catch (Exception eSql)
                {
                    Debug.WriteLine("Exception: " + eSql.Message);
                    return StatusCode(500);
                }
            }
            else
            {
                return StatusCode(401, "You are not allowed to edit this post");
            }
            return BadRequest("Post not found");
        }

        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeletePost(Int64 id)
        {            
            string findPostAuthor = "SELECT userid FROM posts WHERE postid = @0";
            string deletePost = "DELETE FROM posts WHERE postid = @0";
            Int64 postAuthorId = 0;

            Int64 userId = this.TokenUserId(Request);

            try
            {
                int affectedRows = 0;
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = findPostAuthor;
                            cmd.Parameters.AddWithValue("@0", id);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    postAuthorId = reader.GetInt64(0);//Get int from the first column
                                }
                            }
                        }

                        if (userId == postAuthorId)//If the author on the DB and the user trying to delete are the same, then it can be deleted
                        {
                            using (NpgsqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = deletePost;
                                cmd.Parameters.AddWithValue("@0", id);
                                affectedRows = cmd.ExecuteNonQuery();
                            }
                        }
                        else if(userId != postAuthorId && postAuthorId != 0)
                        {
                            return StatusCode(401, "You are not allowed to delete this post");
                        }
                    }
                    conn.Close();
                }
                if (affectedRows > 0)
                {
                    return Ok();
                }

            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            return BadRequest("Post not found");
        }

        private Int64 TokenUserId(HttpRequest request)
        {
            //This block of code is for getting the user's id from the token
            string plainToken = request.Headers.Authorization.ToString();
            plainToken = plainToken.Replace("bearer ", "");
            JwtSecurityTokenHandler validator = new JwtSecurityTokenHandler();
            JwtSecurityToken token = validator.ReadJwtToken(plainToken);//Reads the string (token variable above) and turns it into an instance of a token that can be read
            //The following Int64 has the ID needed to link the post to a user
            return  Int64.Parse(token.Claims.First(claim => claim.Type == userDataClaim).Value);//Read the token's claims, then get the first one which type matches the type we are looking for and get the value, itll be the UserID
        }
    }
}
