//Diego Hiriart Leon

using WebAPI_DiegoHiriart.Models;

namespace WebAPI_DiegoHiriart
{
    public static class APIConfig
    {
        //Here a user with high privileges is used becasue on the free plan for a PostgreSQL from Heroku the user you get ahs no role creation permission
        private static string connectionString =
            "Host=ec2-3-218-171-44.compute-1.amazonaws.com;Port=5432;Username=nfdictzoxksvta;Password=4cd7c7a631740d749cbae126b813d6430c73f5aa2cb1a24bbc85a861c3a75126;" +
            "Database=d2r7gqmss2jjk2";//Heorku changes this periodically
            //@"Host=localhost;Username=postgres;Password=admin01;Database=IngWeb";
        //@"Data Source=DIEGOHL\SQLDEV;Database=IngWebDev;Integrated Security=SSPI";
        //@"Data Source=DIEGOHL\SQLEXPR;Database=IngWebPrd;Integrated Security=SSPI;"

        private static string token = "A_super secret key-for the T0kenS";//A key to create the login tokens

        private static UserDto admin = new UserDto(1, "diego.hiriart@udla.edu.ec", "SysAdmin", "");

        public static string ConnectionString { get => connectionString; set => connectionString = value; }

        public static string Token { get => token; }

        public static UserDto Admin { get => admin; }
    }
}
