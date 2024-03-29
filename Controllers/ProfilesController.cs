﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Diagnostics;
using WebAPI_DiegoHiriart.Models;
using System.Text.Json;
using WebAPI_DiegoHiriart.Settings;

namespace WebAPI_DiegoHiriart.Controllers
{
    [Route("api/profiles")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        //A constructor for this class is needed so that when it is called the config and environment info needed are passed
        public ProfilesController(IConfiguration config, IWebHostEnvironment env)
        {
            this.config = config;
            this.env = env;
            this.db = new AppSettings(this.config, this.env).DBConn;
        }
        //These configurations and environment info are needed to create a DBConfig instance that has the right connection string depending on whether the app is running on a development or production environment
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;
        private string db;//Connection string

        [HttpPost, Authorize]
        public async Task<ActionResult<List<Profile>>> CreateProfile(Profile profile)
        {
            string createProfile = "INSERT INTO profiles(userid, firstname, lastname, bio, isadmin) " +
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
                            cmd.CommandText = createProfile;
                            cmd.Parameters.AddWithValue("@0", profile.UserId);
                            cmd.Parameters.AddWithValue("@1", profile.Firstname);
                            cmd.Parameters.AddWithValue("@2", profile.Lastname);
                            cmd.Parameters.AddWithValue("@3", profile.Bio);
                            cmd.Parameters.AddWithValue("@4", profile.IsAdmin);
                            cmd.Parameters.AddWithValue("@5", profile.localAccount);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
                return Ok(profile);

            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }


        [HttpGet("search/{id}"), Authorize(Roles = "admin")]//Maps this method to the GET request (read), only users with the role Admin can call this method. Returns 403 if wrong role, 401 if no token 
        public async Task<ActionResult<List<Profile>>> SearchProfile(Int64 id)
        {
            List<Profile> profiles = new List<Profile>();
            string searchProfile = "SELECT * FROM profiles WHERE userid = @0";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = searchProfile;
                            cmd.Parameters.AddWithValue("@0", id);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var profile = new Profile();
                                    profile.UserId = reader.GetInt64(0);//Get a long int from the first column
                                    //Use castings so that nulls get created if needed
                                    profile.Firstname = reader[1] as string;
                                    profile.Lastname = reader[2] as string;
                                    profile.Bio = reader[3] as string;
                                    profile.IsAdmin = reader.GetBoolean(4);
                                    profile.localAccount = reader.GetBoolean(5);
                                    profiles.Add(profile);//Add user to list
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return Ok(profiles);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpPut("role-control"), Authorize(Roles ="admin")]
        public async Task<ActionResult<List<Object>>> AdminRoleEdit(Int64 id, bool isAdmin)
        {
            string adminRoleUpdate = "UPDATE profiles SET isadmin=@0 WHERE userid = @1";
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
                            cmd.CommandText = adminRoleUpdate;
                            cmd.Parameters.AddWithValue("@0", isAdmin);
                            cmd.Parameters.AddWithValue("@1", id);
                            affectedRows = cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
                if (affectedRows > 0)
                {
                    return Ok(JsonSerializer.Serialize(new List<Object>{id, isAdmin }));
                }

            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            return BadRequest("User's profile not found");
        }
    }
}
