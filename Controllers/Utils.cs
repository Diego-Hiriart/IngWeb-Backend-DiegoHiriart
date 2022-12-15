using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Npgsql;
using WebAPI_DiegoHiriart.Models;
using WebAPI_DiegoHiriart.Settings;

namespace WebAPI_DiegoHiriart.Controllers
{
    public class Utils
    {

        //A constructor for this class is needed so that when it is called the config and environment info needed are passed
        public Utils(IConfiguration config, IWebHostEnvironment env)
        {
            this.config = config;
            this.env = env;
            this.db = new AppSettings(this.config, this.env).DBConn;
        }

        //These configurations and environment info are needed to create a DBConfig instance that has the right connection string depending on whether the app is running on a development or production environment
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;
        private string db;//Connection string

        public List<byte[]> CreatePasswordHash(string password)
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