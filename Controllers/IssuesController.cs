﻿using Microsoft.AspNetCore.Authorization;
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
    [Route("api/issues")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        //A constructor for this class is needed so that when it is called the config and environment info needed are passed
        public IssuesController(IConfiguration config, IWebHostEnvironment env)
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
        public async Task<ActionResult<List<Issue>>> CreateIssue(Issue issue)
        {
            string createIssue = "INSERT INTO issues(postid, componentid, issuedate, isfixable, description) " +
                "VALUES(@0, @1, @2, @3, @4)";
            string checkAuthor = "SELECT userid FROM posts WHERE postid = @0";//To compare the current user with the one that made the post to which an issue is to be added
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
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = checkAuthor;
                            cmd.Parameters.AddWithValue("@0", issue.PostId);

                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    issueAuthor = reader.GetInt64(0);
                                }
                            }
                        }

                        if (userId == issueAuthor)//Is the current user is the author of the post that an issue is being added to, allow the issue to be posted
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
                        else
                        {
                            return StatusCode(401, "You are not allowed to add an issue to this post");
                        }
                    }
                    conn.Close();
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
                    conn.Close();
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
                    conn.Close();
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
                    conn.Close();
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
                conn.Close();
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
                        conn.Close();
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
            return BadRequest("Issue not found");
        }
    }
}
