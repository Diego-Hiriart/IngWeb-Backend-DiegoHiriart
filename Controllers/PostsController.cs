using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Diagnostics;
using WebAPI_DiegoHiriart.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebAPI_DiegoHiriart.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private string userDataClaim = ClaimTypes.UserData;

        [HttpPost, Authorize]
        public async Task<IActionResult> CreatePost(Post Post)
        {
            //This next block of code is need to check that the user wanting to edit or delete the post is the post's owner, this is done by checking the token they have
            string plainToken = Request.Headers.Authorization.ToString();
            plainToken = plainToken.Replace("bearer ", "");
            JwtSecurityTokenHandler validator = new JwtSecurityTokenHandler();
            JwtSecurityToken token = validator.ReadJwtToken(plainToken);//Reads the string (token variable above) and turns it into an instance of a token that can be read
            string userData = token.Claims.First(claim => claim.Type == userDataClaim).Value;//Read the token's claims, then get the first one which type matches the type we are looking for and get the value, itll be the UserID
            Debug.WriteLine($"The claim's UserId is: {userData}");
            return Ok(userData);
        }
    }
}
