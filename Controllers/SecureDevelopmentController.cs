using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using System.Diagnostics;
using WebAPI_DiegoHiriart.Settings;
using WebAPI_DiegoHiriart.Models;
using System.Text.Json;
using System.Text;
using Google.Cloud.Kms.V1;
using Google.Protobuf;
using System.Text.Json.Serialization;
using Google.Apis.Auth.OAuth2;

namespace WebAPI_DiegoHiriart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecureDevelopmentController : ControllerBase
    {
        //A constructor for this class is needed so that when it is called the config and environment info needed are passed
        public SecureDevelopmentController(IConfiguration config, IWebHostEnvironment env)
        {
            this.config = config;
            this.env = env;
            this.db = new AppSettings(this.config, this.env).DBConn;
        }
        //These configurations and environment info are needed to create a DBConfig instance that has the right connection string depending on whether the app is running on a development or production environment
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;
        private string db;//Connection string

        [HttpGet("get-encrypted-data")]
        public async Task<ActionResult<List<object>>> KMSEncrypt()
        {
            byte[] ciphertextIssues;
            //Get all issues from database
            List<Issue> issues = new List<Issue>();
            string getIssues = "SELECT * FROM issues";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getIssues;
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var issue = new Issue();
                                    issue.IssueId = reader.GetInt64(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    issue.PostId = reader.GetInt64(1);
                                    issue.ComponentId = reader.GetInt32(2);
                                    issue.IssueDate = reader.GetDateTime(3);
                                    issue.IsFixable = reader.GetBoolean(4);
                                    issue.Description = reader[5] as string;
                                    issues.Add(issue);
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
            try
            {
                //Create encryption cient
                string projectId = this.config.GetValue<string>("Google:ProjectId");
                string locationId = this.config.GetValue<string>("Google:LocationId");
                string keyRingId = this.config.GetValue<string>("Google:KeyRingId");
                string keyId = this.config.GetValue<string>("Google:KeyId");
                KeyManagementServiceClient KMSClient = KeyManagementServiceClient.Create();
                //Create key name
                CryptoKeyName keyName = new CryptoKeyName(projectId, locationId, keyRingId, keyId);
                //Convert retrieved issues into string
                string issuesJSON = JsonSerializer.Serialize(issues);
                //Convert content string into bytes to get encrypted
                byte[] plaintextIssues = Encoding.UTF8.GetBytes(issuesJSON);
                //Encrypt issues text calling the API
                EncryptResponse encryptionResult = KMSClient.Encrypt(keyName, ByteString.CopyFrom(plaintextIssues));
                //Convert result into cyphertext
                ciphertextIssues = encryptionResult.Ciphertext.ToByteArray();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return StatusCode(500);
            }
            //Return encrypted data
            return Ok(ciphertextIssues);
        }
    }
}
