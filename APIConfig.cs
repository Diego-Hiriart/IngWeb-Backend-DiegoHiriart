//Diego Hiriart Leon

using WebAPI_DiegoHiriart.Models;

namespace WebAPI_DiegoHiriart
{
    public static class APIConfig
    {
        private static string connectionString =
        @"Data Source=DIEGOHL\SQLDEV;Database=IngWebDev;Integrated Security=SSPI";
        //@"Data Source=DIEGOHL\SQLEXPR;Database=IngWebPrd;Integrated Security=SSPI"

        private static string token = "A_super secret key-for the T0kenS";//A key to create the login tokens

        private static UserDto admin = new UserDto(1, "diego.hiriart@udla.edu.ec", "SysAdmin", "");

        public static string ConnectionString { get => connectionString; set => connectionString = value; }

        public static string Token { get => token; }

        public static UserDto Admin { get => admin; }
    }
}
