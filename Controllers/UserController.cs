//Diego Hiriart Leon
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;
using WebAPI_DiegoHiriart.Models;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI_DiegoHiriart.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        [HttpPost]//Maps method to Post request
        public async Task<ActionResult<List<UserDto>>> CreateUser(UserDto user)
        {
            string db = APIConfig.ConnectionString;
            string createUser = "INSERT INTO Users(Email, Username, PasswordHash, PasswordSalt) values(@0, @1, @2, @3)";
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password)
                || string.IsNullOrEmpty(user.Username))//Do no create if data not complete
            {
                return BadRequest("Incomplete data");
            }
            List<byte[]> passwordHashes = CreatePasswordHash(user.Password);
            User userDb = new User(user.UserID, user.Email, user.Username,
                passwordHashes[0], passwordHashes[1]);
            try
            {
                using (SqlConnection conn = new SqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = createUser;
                            cmd.Parameters.AddWithValue("@0", userDb.Email);//Replace the parameteres of the string
                            cmd.Parameters.AddWithValue("@1", userDb.Username);
                            cmd.Parameters.AddWithValue("@2", userDb.PasswordHash);
                            cmd.Parameters.AddWithValue("@3", userDb.PasswordSalt);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return Ok(user);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500, user);
            }
        }


        [HttpGet, Authorize(Roles = "admin")]//Maps this method to the GET request (read), only users with the role Admin can call this method. Returns 403 if wrong role, 401 if no token 
        public async Task<ActionResult<List<UserDto>>> ReadUsers()
        {
            List<UserDto> users = new List<UserDto>();
            string db = APIConfig.ConnectionString;
            string readUsers = "SELECT UserID, Email, Username FROM Users";
            try
            {
                using (SqlConnection conn = new SqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readUsers;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var user = new UserDto();
                                    user.UserID = reader.GetInt64(0);//Get a long int from the first column
                                    //Use castings so that nulls get created if needed
                                    user.Email = reader[1] as string;
                                    user.Username = reader[2] as string;
                                    users.Add(user);//Add user to list
                                }
                            }
                        }
                    }
                }
                return Ok(users);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpGet("full-match/{email}"), Authorize]//Maps this method to the GET request (read) for a specific email
        public async Task<ActionResult<List<UserDto>>> ReadUserEmailFullMatch(string email)
        {
            List<UserDto> users = new List<UserDto>();
            string db = APIConfig.ConnectionString;
            string readUsers = "SELECT UserId, Email, Username FROM Users WHERE Email = @0";
            try
            {
                using (SqlConnection conn = new SqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readUsers;
                            cmd.Parameters.AddWithValue("@0", email);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var user = new UserDto();
                                    user.UserID = reader.GetInt64(0);//Get a long int from the first column
                                    //Use castings so that nulls get created if needed
                                    user.Email = reader[1] as string;
                                    user.Username = reader[2] as string;
                                    users.Add(user);//Add user to list
                                }
                            }
                        }
                    }
                }
                if (users.Count > 0)
                {
                    return Ok(users);
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            return BadRequest("User not found");
        }

        [HttpGet("partial-match/{email}"), Authorize]//Maps this method to the GET request (read) for a partial email
        public async Task<ActionResult<List<UserDto>>> ReadUserEmailPartialMatch(string email)
        {
            List<UserDto> users = new List<UserDto>();
            string db = APIConfig.ConnectionString;
            string readUsers = "SELECT UserId, Email, Username FROM Users WHERE Email LIKE @0";//WHERE string LIKE %substring%
            try
            {
                using (SqlConnection conn = new SqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readUsers;
                            cmd.Parameters.AddWithValue("@0", "%"+email+"%");//%substring%
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var user = new UserDto();
                                    user.UserID = reader.GetInt64(0);//Get a long int from the first column
                                    //Use castings so that nulls get created if needed
                                    user.Email = reader[1] as string;
                                    user.Username = reader[2] as string;
                                    users.Add(user);//Add user to list
                                }
                            }
                        }
                    }
                }
                if (users.Count > 0)
                {
                    return Ok(users);
                }               
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            return BadRequest("User not found");
        }

        [HttpPut, Authorize]//Maps the method to PUT, that is update a User
        public async Task<IActionResult> UpdateUser(UserDto user)
        {
            string db = APIConfig.ConnectionString;
            string updateUser = "UPDATE Users SET Email=@0, Username=@1, PasswordHash=@2, PasswordSalt=@3 WHERE UserID = @4";
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password)
                || string.IsNullOrEmpty(user.Username))//Do no alter if data not complete
            {
                return BadRequest("Incomplete data or non-existent user");
            }
            List<byte[]> passwordHashes = CreatePasswordHash(user.Password);
            User userDb = new User(user.UserID, user.Email, user.Username, 
                passwordHashes[0], passwordHashes[1]);
            try
            {
                int affectedRows = 0;
                using (SqlConnection conn = new SqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = updateUser;
                            cmd.Parameters.AddWithValue("@0", userDb.Email);
                            cmd.Parameters.AddWithValue("@1", userDb.Username);
                            cmd.Parameters.AddWithValue("@2", userDb.PasswordHash);
                            cmd.Parameters.AddWithValue("@3", userDb.PasswordSalt);
                            cmd.Parameters.AddWithValue("@4", userDb.UserID);
                            affectedRows = cmd.ExecuteNonQuery();                         
                        }
                    }
                }
                if (affectedRows > 0)
                {
                    return Ok(user);
                }
                
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            return BadRequest("User not found");
        }

        [HttpDelete("{id}"), Authorize(Roles = "admin")]//Maps the method to DELETE by id
        public async Task<IActionResult> DeleteUser(int id)
        {
            string db = APIConfig.ConnectionString;
            string deleteUser = "DELETE FROM Users WHERE UserID = @0";
            try
            {
                int affectedRows = 0;
                using (SqlConnection conn = new SqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = deleteUser;
                            cmd.Parameters.AddWithValue("@0", id);
                            affectedRows = cmd.ExecuteNonQuery();
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
            return BadRequest("User not found");
        }

        private List<byte[]> CreatePasswordHash(string password)
        {
            byte[] passwordHash;
            byte[] passwordSalt;
            using (var hmac = new HMACSHA512())//hmac is a cryptography algorithm it uses any key here, SHA512 hash is 64 bytes and secret key 128, database must store the same sizes or the trailing zeros change the value
            {
                passwordSalt = hmac.Key;//Random key for cryptography
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
            return new List<byte[]> { passwordHash, passwordSalt };
        }
    }

}
