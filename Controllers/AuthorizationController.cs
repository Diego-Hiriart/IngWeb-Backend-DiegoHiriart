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
using WebAPI_DiegoHiriart.Settings;

namespace WebAPI_DiegoHiriart.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        //A constructor for this class is needed so that when it is called the config and environment info needed are passed
        public AuthorizationController(IConfiguration config, IWebHostEnvironment env)
        {
            this.config = config;
            this.env = env;
            this.db = new AppSettings(this.config, this.env).DBConn;
        }
        //These configurations and environment info are needed to create a DBConfig instance that has the right connection string depending on whether the app is running on a development or production environment
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;
        private string db;//Connection string

        [HttpPost("auth0-login")]
        public async Task<ActionResult<UserDto>> Auth0Login(UserDto user)
        {
            //Generate a token when Auth0's signing in is used
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
                            cmd.Parameters.AddWithValue("@0", user.Email);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var usersListItem = new UserDto();
                                    usersListItem.UserID = reader.GetInt64(0);//Get a long int from the first column
                                    //Use castings so that nulls get created if needed
                                    usersListItem.Email = reader[1] as string;
                                    usersListItem.Username = reader[2] as string;
                                    users.Add(user);//Add user to list
                                }
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            UserDto userForSearch;
            //If no users have that email, create that user
            //Set up variable to search for user in DB, used for token
            if (users.Count == 0)
            {
                //Create user for data passed in body, use it for token
                UsersController userControl = new UsersController(this.config, this.env);
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
                }
                catch (Exception eSql)
                {
                    Debug.WriteLine("Exception: " + eSql.Message);
                    return StatusCode(500);
                }

            }
            //Use passed UserDTO for token
            userForSearch = user;
            //Create token
            //COLLATE SQL_Latin1_General_CP1_CS_AS allows case sentitive compare, not needed for PostgreSQL
            string checkUserExists = "SELECT * FROM users WHERE email = @0 OR username = @0";
            User dbUser = new User();
            bool isAdmin = false;
            Debug.WriteLine(userForSearch.Email);
            try
            {
                bool userFound = false;
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = checkUserExists;
                            cmd.Parameters.AddWithValue("@0", userForSearch.Email);//Replace the parameteres of the string
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                userFound = reader.HasRows;//To know if the user exists there must be rows in the reader if something was found)
                                while (reader.Read())
                                {
                                    //Use castings so that nulls get created if needed
                                    dbUser.UserID = reader.GetInt64(0);
                                    dbUser.Email = reader[1] as string;
                                    dbUser.Username = reader[2] as string;
                                    dbUser.PasswordHash = reader[3] as byte[];
                                    dbUser.PasswordSalt = reader[4] as byte[];
                                }
                            }
                        }
                    }

                    if (userFound)//If the user was found, now check if they are an administrator
                    {

                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            string getProfile = "SELECT isadmin FROM profiles WHERE userid = @0";
                            cmd.CommandText = getProfile;
                            cmd.Parameters.AddWithValue("@0", user.UserID);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    isAdmin = reader.GetBoolean(0);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                if (!userFound)//If the user was not found (no rows in the reader) or is a user logged in by auth 0, return bad request
                {
                    return BadRequest("The user does not exist");
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            string token = CreateToken(dbUser, isAdmin);
            return Ok(token);

        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(Credentials request)
        {
            //COLLATE SQL_Latin1_General_CP1_CS_AS allows case sentitive compare, not needed for PostgreSQL
            string checkUserExists = "SELECT * FROM users WHERE email = @0 OR username = @0";
            User user = new User();
            bool isAdmin = false;
            bool localAccount = false;
            try
            {
                bool userFound = false;
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = checkUserExists;
                            cmd.Parameters.AddWithValue("@0", request.UserEmail);//Replace the parameteres of the string
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                userFound = reader.HasRows;//To know if the user exists there must be rows in the reader if something was found)
                                while (reader.Read())
                                {
                                    //Use castings so that nulls get created if needed
                                    user.UserID = reader.GetInt64(0);
                                    user.Email = reader[1] as string;
                                    user.Username = reader[2] as string;
                                    user.PasswordHash = reader[3] as byte[];
                                    user.PasswordSalt = reader[4] as byte[];
                                }
                            }
                        }
                    }

                    if (userFound)//If the user was found, now check if they are an administrator
                    {

                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            string getProfile = "SELECT isadmin, localaccount FROM profiles WHERE userid = @0";
                            cmd.CommandText = getProfile;
                            cmd.Parameters.AddWithValue("@0", user.UserID);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    isAdmin = reader.GetBoolean(0);
                                    localAccount = reader.GetBoolean(1);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                if (!userFound || !localAccount)//If the user was not found (no rows in the reader) or is a user logged in by auth 0, return bad request
                {
                    return BadRequest("The user does not exist");
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            if (VerifyPaswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                string token = CreateToken(user, isAdmin);
                return Ok(token);
            }
            else
            {
                return BadRequest("Wrong password");
            }
        }

        [HttpPost("check"), Authorize(Roles = "admin")]
        public async Task<IActionResult> CheckAdminRole()//If the function returns anythng but ok, client will know token is not valid, user hasnt logged in, or is forbidden
        {
            return Ok("Is logged in, token valid, role valid");
        }

        private string CreateToken(User user, bool isAdmin)
        {
            List<Claim> claims;
            if (isAdmin)//If admin logs in, give them the role
            {
                claims = new List<Claim>//Claims describe the user that is authenticated, the store infor from the user
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.UserData, user.UserID.ToString()),
                    new Claim(ClaimTypes.Role, "admin")//Add the "Admin" role to the token                
                };
                Debug.WriteLine("Admin token creation");
            }
            else
            {
                claims = new List<Claim>//Claims describe the user that is authenticated, the store infor from the user
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.UserID.ToString()),
                    new Claim(ClaimTypes.Role, "regular")//Add the "Admin" role to the token
                };
                Debug.WriteLine("Regular user token creation");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue<string>("JWTKey")));//New key using the key from settings

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),//Token expires after 2 hours
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);//Create the token

            return jwt;
        }

        private bool VerifyPaswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))//Gives the key needed for cryptography in this case PasswordSalt, SHA512 hash is 64 bytes and secret key 128, database must store the same sizes or the trailing zeros change the value
            {
                var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);//Checks if password the user inputs is the same as the one stored
            }
        }
    }
}
