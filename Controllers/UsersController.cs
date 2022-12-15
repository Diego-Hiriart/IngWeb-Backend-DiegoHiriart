//Diego Hiriart Leon
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Diagnostics;
using WebAPI_DiegoHiriart.Models;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using WebAPI_DiegoHiriart.Settings;
using System.Security.Claims;

namespace WebAPI_DiegoHiriart.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        //A constructor for this class is needed so that when it is called the config and environment info needed are passed
        public UsersController(IConfiguration config, IWebHostEnvironment env)
        {
            this.config = config;
            this.env = env;
            this.db = new AppSettings(this.config, this.env).DBConn;
        }

        //These configurations and environment info are needed to create a DBConfig instance that has the right connection string depending on whether the app is running on a development or production environment
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;
        private string db;//Connection string

        [HttpPost]//Maps method to Post request
        public async Task<ActionResult<List<UserDto>>> CreateUser(UserDto user)
        {
            string createUser = "INSERT INTO users(email, username, passwordhash, passwordsalt) VALUES(@0, @1, @2, @3)";
            string getCreatedUserId = "SELECT userid FROM users WHERE email = @0 AND username = @1 AND passwordhash = @2 AND passwordsalt = @3";
            Int64 newID = 0;
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password)
                || string.IsNullOrEmpty(user.Username))//Do no create if data not complete
            {
                return BadRequest("Incomplete data");
            }
            Utils utils = new Utils(this.config, this.env);
            List<byte[]> passwordHashes = utils.CreatePasswordHash(user.Password);
            User userDb = new User(user.UserID, user.Email, user.Username,
                passwordHashes[0], passwordHashes[1]);
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = createUser;
                            cmd.Parameters.AddWithValue("@0", userDb.Email);//Replace the parameters of the string
                            cmd.Parameters.AddWithValue("@1", userDb.Username);
                            cmd.Parameters.AddWithValue("@2", userDb.PasswordHash);
                            cmd.Parameters.AddWithValue("@3", userDb.PasswordSalt);
                            cmd.ExecuteNonQuery();
                        }

                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getCreatedUserId;
                            cmd.Parameters.AddWithValue("@0", userDb.Email);//Replace the parameters of the string
                            cmd.Parameters.AddWithValue("@1", userDb.Username);
                            cmd.Parameters.AddWithValue("@2", userDb.PasswordHash);
                            cmd.Parameters.AddWithValue("@3", userDb.PasswordSalt);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    newID = reader.GetInt64(0);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                //Create a basic profile when a user is created
                Profile basicProfile = new Profile(newID, "", "", "", false, true);
                ProfilesController profileController = new ProfilesController(config, env);
                await profileController.CreateProfile(basicProfile);
                return Ok(user);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500, user);
            }
        }

        [HttpPost("auth0-user")]
        public async Task<ActionResult<UserDto>> CreateAuth0User(UserDto user)
        {
            var name = user.Username;
            var email = user.Email;
            string createUser = "INSERT INTO users(email, username, passwordhash, passwordsalt) VALUES(@0, @1, @2, @3)";
            string getCreatedUserId = "SELECT userid FROM users WHERE email = @0 AND username = @1 AND passwordhash = @2 AND passwordsalt = @3";
            Int64 newID = 0;
            Utils utils = new Utils(this.config, this.env);
            List<byte[]> passwordHashes = utils.CreatePasswordHash("");
            User userDb = new User(newID, email, name,
                passwordHashes[0], passwordHashes[1]);
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = createUser;
                            cmd.Parameters.AddWithValue("@0", userDb.Email);//Replace the parameters of the string
                            cmd.Parameters.AddWithValue("@1", userDb.Username);
                            cmd.Parameters.AddWithValue("@2", userDb.PasswordHash);
                            cmd.Parameters.AddWithValue("@3", userDb.PasswordSalt);
                            cmd.ExecuteNonQuery();
                        }

                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getCreatedUserId;
                            cmd.Parameters.AddWithValue("@0", userDb.Email);//Replace the parameters of the string
                            cmd.Parameters.AddWithValue("@1", userDb.Username);
                            cmd.Parameters.AddWithValue("@2", userDb.PasswordHash);
                            cmd.Parameters.AddWithValue("@3", userDb.PasswordSalt);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    newID = reader.GetInt64(0);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                //Create a basic profile when a user is created
                Profile basicProfile = new Profile(newID, "", "", "", false, false);
                ProfilesController profileController = new ProfilesController(config, env);
                await profileController.CreateProfile(basicProfile);
                return Ok(new UserDto(userDb.UserID, userDb.Email, userDb.Username, ""));
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500, new UserDto(userDb.UserID, userDb.Email, userDb.Username, ""));
            }
        }


        [HttpGet, Authorize(Roles = "admin")]//Maps this method to the GET request (read), only users with the role Admin can call this method. Returns 403 if wrong role, 401 if no token 
        public async Task<ActionResult<List<UserDto>>> ReadUsers()
        {
            List<UserDto> users = new List<UserDto>();
            string readUsers = "SELECT userid, email, username FROM users";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readUsers;
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
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
                    conn.Close();
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
            string readUsers = "SELECT userid, email, username FROM users WHERE email = @0";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readUsers;
                            cmd.Parameters.AddWithValue("@0", email);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
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
                    conn.Close();
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
            string readUsers = "SELECT userid, email, username FROM users WHERE email LIKE @0";//WHERE string LIKE %substring%
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readUsers;
                            cmd.Parameters.AddWithValue("@0", "%" + email + "%");//%substring%
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
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
                    conn.Close();
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
            string updateUser = "UPDATE users SET email=@0, username=@1, passwordhash=@2, passwordsalt=@3 WHERE userid = @4";
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password)
                || string.IsNullOrEmpty(user.Username))//Do no alter if data not complete
            {
                return BadRequest("Incomplete data or non-existent user");
            }
            Utils utils = new Utils(this.config, this.env);
            List<byte[]> passwordHashes = utils.CreatePasswordHash(user.Password);
            User userDb = new User(user.UserID, user.Email, user.Username,
                passwordHashes[0], passwordHashes[1]);
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
                            cmd.CommandText = updateUser;
                            cmd.Parameters.AddWithValue("@0", userDb.Email);
                            cmd.Parameters.AddWithValue("@1", userDb.Username);
                            cmd.Parameters.AddWithValue("@2", userDb.PasswordHash);
                            cmd.Parameters.AddWithValue("@3", userDb.PasswordSalt);
                            cmd.Parameters.AddWithValue("@4", userDb.UserID);
                            affectedRows = cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
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
        public async Task<IActionResult> DeleteUser(Int64 id)//Deletes the user profile too, it is configured like that in the DB
        {
            string deleteUser = "DELETE FROM users WHERE userid = @0";
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
                            cmd.CommandText = deleteUser;
                            cmd.Parameters.AddWithValue("@0", id);
                            affectedRows = cmd.ExecuteNonQuery();
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
            return BadRequest("User not found");
        }
    }

}
