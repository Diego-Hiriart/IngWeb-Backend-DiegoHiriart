namespace WebAPI_DiegoHiriart.Settings
{
    public class AppSettings
    {
        public AppSettings(IConfiguration Configuration, IWebHostEnvironment Environment)
        {
            this.Configuration = Configuration;
            this.Environment = Environment;
            if (Environment.IsProduction())
            {
                this.DBConn = Configuration.GetConnectionString("Prod");
            }
            else if (Environment.IsDevelopment())
            {
                this.DBConn = Configuration.GetConnectionString("Dev");
            }
        }

        public IConfiguration Configuration { set; get; }

        public IWebHostEnvironment Environment { set; get; }

        public string DBConn { set; get; }
    }
}
