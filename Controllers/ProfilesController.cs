using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using Npgsql;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAPI_DiegoHiriart.Models;
using System.Text.Json;

namespace WebAPI_DiegoHiriart.Controllers
{
    [Route("api/profiles")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        [HttpGet("search/{id}"), Authorize(Roles = "admin")]//Maps this method to the GET request (read), only users with the role Admin can call this method. Returns 403 if wrong role, 401 if no token 
        public async Task<ActionResult<List<Profile>>> SearchProfile(Int64 id)
        {
            List<Profile> profiles = new List<Profile>();
            string db = APIConfig.ConnectionString;
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
                                    profiles.Add(profile);//Add user to list
                                }
                            }
                        }
                    }
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
            string db = APIConfig.ConnectionString;
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
