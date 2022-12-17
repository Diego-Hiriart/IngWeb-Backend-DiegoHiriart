namespace WebAPI_DiegoHiriart.Settings
{
    public class AppSettings
    {
        public AppSettings(IConfiguration Configuration, IWebHostEnvironment Environment)
        {
            this.Configuration = Configuration;
            this.Environment = Environment;
            this.DBConn = Configuration.GetConnectionString("MainDB");
        }

        public IConfiguration Configuration { set; get; }

        public IWebHostEnvironment Environment { set; get; }

        public string DBConn { set; get; }
    }
}
