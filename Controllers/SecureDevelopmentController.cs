using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using System.Diagnostics;
using WebAPI_DiegoHiriart.Settings;
using WebAPI_DiegoHiriart.Models;
using Amazon.KeyManagementService;
using Amazon.Runtime;
using AWS.EncryptionSDK;
using AWS.EncryptionSDK.Core;
using System.Text.Json;
using System.Text;

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

        [HttpGet]
        public async Task<ActionResult<List<object>>> KMSEncrypt()
        {
            List<object> encryptionResults = new List<object>();
            //Get all models from database
            List<Model> models = new List<Model>();
            string readModels = "SELECT * FROM models";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readModels;
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var model = new Model();
                                    model.ModelId = reader.GetInt64(0);
                                    //Use castings so that nulls get created if needed
                                    model.BrandId = reader.GetInt32(1);
                                    model.ModelNumber = reader[2] as string;
                                    model.Name = reader[3] as string;
                                    model.Launch = reader.GetDateTime(4);
                                    model.Discontinued = reader.GetBoolean(5);
                                    models.Add(model);//Add model to list
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
                //Encrypt models
                IAwsEncryptionSdk encryptionSdk = AwsEncryptionSdkFactory.CreateDefaultAwsEncryptionSdk();
                IAwsCryptographicMaterialProviders materialProviders = AwsCryptographicMaterialProvidersFactory.CreateDefaultAwsCryptographicMaterialProviders();
                string keyArn = this.config.GetValue<string>("AWS:KeyARN");
                //Instantiate keyring input object
                //Create AWS credentials and region endpoint
                var accessKey = this.config.GetValue<string>("AWS:AccessKeyId");
                var secretKey = this.config.GetValue<string>("AWS:SecretAccessKey");
                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                Amazon.RegionEndpoint regionEnpoint =  Amazon.RegionEndpoint.USEast1;
                var kmsKeyringInput = new CreateAwsKmsKeyringInput
                {
                    KmsClient = new AmazonKeyManagementServiceClient(credentials, regionEnpoint),
                    KmsKeyId = keyArn
                };
                //Create keyring
                IKeyring keyring = materialProviders.CreateAwsKmsKeyring(kmsKeyringInput);
                // Define the encryption context, necessary for cryptography
                var encryptionContext = new Dictionary<string, string>(){
                    {"purpose", "test"}
                };
                //Transform models list to JSON and then to memory stream
                string modelsJSONString = JsonSerializer.Serialize(models);
                MemoryStream modelsJSONStream = new MemoryStream(Encoding.ASCII.GetBytes(modelsJSONString));
                //Create an EncrypInput class
                EncryptInput encryptInput = new EncryptInput
                {
                    Plaintext = modelsJSONStream,
                    Keyring = keyring,
                    EncryptionContext = encryptionContext
                };
                //Encrypt
                EncryptOutput encryptionOutput = encryptionSdk.Encrypt(encryptInput);
                MemoryStream encryptedText = encryptionOutput.Ciphertext;
                string encryptedString = Encoding.UTF8.GetString(encryptedText.ToArray());
                encryptionResults.Add(encryptedString);
                //Decrypt
                var decryptInput = new DecryptInput
                {
                    Ciphertext = encryptedText,
                    Keyring = keyring
                };
                DecryptOutput decryptOutput = encryptionSdk.Decrypt(decryptInput);
                MemoryStream decryptedText = decryptOutput.Plaintext;
                string decryptedJsonString = Encoding.ASCII.GetString(decryptedText.ToArray());
                List<Model> decryptedModels = JsonSerializer.Deserialize<List<Model>>(decryptedJsonString);
                encryptionResults.Add(decryptedModels);
            }catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return StatusCode(500);
            }
            //Return results of encryption and decryption
            return Ok(encryptionResults);
        }
    }
}
