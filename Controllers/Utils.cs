using System.Security.Cryptography;
using System.Text;

namespace WebAPI_DiegoHiriart.Controllers
{
   public static class Utils
    {
        public static List<byte[]> CreatePasswordHash(string password)
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