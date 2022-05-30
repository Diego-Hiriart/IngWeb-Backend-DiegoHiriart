using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Diagnostics;
using WebAPI_DiegoHiriart.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebAPI_DiegoHiriart.Controllers
{
    [Route("api/issues")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        private string userDataClaim = ClaimTypes.UserData;

        [HttpPost, Authorize]
        public async Task<ActionResult<List<Issue>>> CreateIssue(Issue issue)
        {
            string db = APIConfig.ConnectionString;
            string createIssue = "INSERT INTO issues(postid, componentid, issuedate, isfixable, description) " +
                "VALUES(@0, @1, @2, @3, @4)";

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = createIssue;
                            cmd.Parameters.AddWithValue("@0", issue.PostId);//Replace the parameters
                            cmd.Parameters.AddWithValue("@1", issue.ComponentId);
                            cmd.Parameters.AddWithValue("@2", issue.IssueDate);
                            cmd.Parameters.AddWithValue("@3", issue.IsFixable);
                            cmd.Parameters.AddWithValue("@4", issue.Description);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return Ok(issue);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500, issue);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Issue>>> GetAll()
        {
            List<Issue> issues = new List<Issue>();
            string db = APIConfig.ConnectionString;
            string getIssues = "SELECT * FROM issues";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getIssues;
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var issue = new Issue();
                                    issue.IssueId = reader.GetInt64(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    issue.PostId = reader.GetInt64(1);
                                    issue.ComponentId = reader.GetInt32(2);
                                    issue.IssueDate = reader.GetDateTime(3);
                                    issue.IsFixable = reader.GetBoolean(4);
                                    issue.Description = reader[5] as string;
                                    issues.Add(issue);
                                }
                            }
                        }
                    }
                }
                return Ok(issues);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpGet("by-post/{id}"), Authorize]
        public async Task<ActionResult<List<Issue>>> GetByPost(Int64 id)
        {
            List<Issue> issues = new List<Issue>();
            string db = APIConfig.ConnectionString;
            string getIssues = "SELECT * FROM issues WHERE postid = @0";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getIssues;
                            cmd.Parameters.AddWithValue("@0", id);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var issue = new Issue();
                                    issue.IssueId = reader.GetInt64(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    issue.PostId = reader.GetInt64(1);
                                    issue.ComponentId = reader.GetInt32(2);
                                    issue.IssueDate = reader.GetDateTime(3);
                                    issue.IsFixable = reader.GetBoolean(4);
                                    issue.Description = reader[5] as string;
                                    issues.Add(issue);
                                }
                            }
                        }
                    }
                }
                return Ok(issues);
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
            List<Issue> issues = new List<Issue>();
            string db = APIConfig.ConnectionString;
            string getIssues = "SELECT i.* FROM issues i INNER JOIN posts p ON p.postid = i.postid WHERE p.modelid = @0";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getIssues;
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var issue = new Issue();
                                    issue.IssueId = reader.GetInt64(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    issue.PostId = reader.GetInt64(1);
                                    issue.ComponentId = reader.GetInt32(2);
                                    issue.IssueDate = reader.GetDateTime(3);
                                    issue.IsFixable = reader.GetBoolean(4);
                                    issue.Description = reader[5] as string;
                                    issues.Add(issue);
                                }
                            }
                        }
                    }
                }
                return Ok(issues);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpPut, Authorize]
        public async Task<ActionResult<List<Issue>>> UpdateIssue(Issue issue)
        {
            string db = APIConfig.ConnectionString;
            string updateIssue = "UPDATE issues SET componentid=@0, issuedate=@1, isfixable=@2, " +
                "description=@3 WHERE issueid = @4";
            string checkAuthor = "SELECT p.userid FROM issues i INNER JOIN posts p ON p.postid = i.postid WHERE i.issueid = @0";
            Int64 issueAuthor = 0;

            //This block of code is for getting the user's id from the token
            string plainToken = Request.Headers.Authorization.ToString();
            plainToken = plainToken.Replace("bearer ", "");
            JwtSecurityTokenHandler validator = new JwtSecurityTokenHandler();
            JwtSecurityToken token = validator.ReadJwtToken(plainToken);//Reads the string (token variable above) and turns it into an instance of a token that can be read
            //The following userData has the ID needed no link the post to a user
            Int64 userId = Int64.Parse(token.Claims.First(claim => claim.Type == userDataClaim).Value);//Read the token's claims, then get the first one which type matches the type we are looking for and get the value, itll be the UserID

            using (NpgsqlConnection conn = new NpgsqlConnection(db))//Get the issue's author, in order to compare it with the current user
            {
                conn.Open();
                if (conn.State == ConnectionState.Open)
                {
                    using (NpgsqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = checkAuthor;
                        cmd.Parameters.AddWithValue("@0", issue.IssueId);//Replace the parameters
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                issueAuthor = reader.GetInt64(0);//Get int from the first column
                            }
                        }
                    }
                }
            }

            if (userId == issueAuthor)//If the author on the DB and the user trying to edit are the same, then it can be edited
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
                                cmd.CommandText = updateIssue;
                                cmd.Parameters.AddWithValue("@0", issue.ComponentId);//Replace the parameters
                                cmd.Parameters.AddWithValue("@1", issue.IssueDate);
                                cmd.Parameters.AddWithValue("@2", issue.IsFixable);
                                cmd.Parameters.AddWithValue("@3", issue.Description);
                                cmd.Parameters.AddWithValue("@4", issue.IssueId);
                                affectedRows = cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    if (affectedRows > 0)
                    {
                        return Ok(issue);
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
                return StatusCode(401, "You are not allowed to edit this issue");
            }
            return BadRequest("Post not found");
        }

        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeleteIssue(Int64 id)
        {
            string db = APIConfig.ConnectionString;
            string deleteIssue = "DELETE FROM issues WHERE issueid = @0";
            string checkAuthor = "SELECT p.userid FROM issues i INNER JOIN posts p ON p.postid = i.postid WHERE i.issueid = @0";
            Int64 issueAuthor = 0;

            //This block of code is for getting the user's id from the token
            string plainToken = Request.Headers.Authorization.ToString();
            plainToken = plainToken.Replace("bearer ", "");
            JwtSecurityTokenHandler validator = new JwtSecurityTokenHandler();
            JwtSecurityToken token = validator.ReadJwtToken(plainToken);//Reads the string (token variable above) and turns it into an instance of a token that can be read
            //The following userData has the ID needed no link the post to a user
            Int64 userId = Int64.Parse(token.Claims.First(claim => claim.Type == userDataClaim).Value);//Read the token's claims, then get the first one which type matches the type we are looking for and get the value, itll be the UserID

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
                            cmd.CommandText = checkAuthor;
                            cmd.Parameters.AddWithValue("@0", id);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    issueAuthor = reader.GetInt64(0);//Get int from the first column
                                }
                            }
                        }

                        if (userId == issueAuthor)//If the author on the DB and the user trying to delete are the same, then it can be deleted
                        {
                            using (NpgsqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = deleteIssue;
                                cmd.Parameters.AddWithValue("@0", id);
                                affectedRows = cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            return StatusCode(401, "You are not allowed to delete this issue");
                        }
                    }
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
            return BadRequest("Issue not found");
        }
    }
}
