using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAPI_DiegoHiriart.Models;

namespace WebAPI_DiegoHiriart.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        public AuthorizationController() { }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(Credentials request)
        {
            string db = APIConfig.ConnectionString;
            //COLLATE SQL_Latin1_General_CP1_CS_AS allows case sentitive compare
            string checkUserExists = "SELECT * FROM Users WHERE Email = @0 OR Username = @0 COLLATE SQL_Latin1_General_CP1_CS_AS";
            User user = new User();
            try
            {
                bool userFound = false;
                using (SqlConnection conn = new SqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = checkUserExists;
                            cmd.Parameters.AddWithValue("@0", request.UserEmail);//Replace the parameteres of the string
                            using (SqlDataReader reader = cmd.ExecuteReader())
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
                string token = CreateToken(user);
                return Ok(token);
            }
            else
            {
                return BadRequest("Wrong password");
            }     
        }

        [HttpGet("checklogin"), Authorize]
        public async Task<ActionResult<string>> CheckLogin()//If the function return anythng but ok, client will know token is not valid or user hasnt logged in
        {
            return Ok("Is logged in, token valid");
        }

        [HttpGet("checkadmin"), Authorize(Roles="admin")]
        public async Task<ActionResult<string>> CheckAdminRole()//If the function return anythng but ok, client will know token is not valid, user hasnt logged in, or is forbidden
        {
            return Ok("Is logged in, token valid, role valid");
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>();
            if (user.UserID == APIConfig.Admin.UserID && user.Email.Equals(APIConfig.Admin.Email) 
                && user.Username.Equals(APIConfig.Admin.Username))//If admin logs in, give them the role
            {
                claims = new List<Claim>//Claims describe the user that is authenticated, the store infor from the user
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, "admin")//Add the "Admin" role to the token                
                };
                Debug.WriteLine("Admin token creation");
            }
            else
            {
                claims = new List<Claim>//Claims describe the user that is authenticated, the store infor from the user
                {
                    new Claim(ClaimTypes.Name, user.Username),
                };
                Debug.WriteLine("Regular user token creation");
            }   

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(APIConfig.Token));//New key using the key from APIConfig

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
