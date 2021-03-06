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

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(Credentials request)
        {
            //COLLATE SQL_Latin1_General_CP1_CS_AS allows case sentitive compare, not needed for PostgreSQL
            string checkUserExists = "SELECT * FROM users WHERE email = @0 OR username = @0";
            User user = new User();
            bool isAdmin = false;
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
                if (!userFound)//If the user was not foun (no rows in the reader), return bad request
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
                    new Claim(ClaimTypes.UserData, user.UserID.ToString()),
                    new Claim(ClaimTypes.Role, "regular")//Add the "Admin" role to the token
                };
                Debug.WriteLine("Regular user token creation");
            }   

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue<string>("Token")));//New key using the key from settings

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),//Token expires after 2 hours
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);//Create the otken

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
