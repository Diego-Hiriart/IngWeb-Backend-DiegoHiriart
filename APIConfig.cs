﻿//Diego Hiriart Leon

using WebAPI_DiegoHiriart.Models;

namespace WebAPI_DiegoHiriart
{
    public static class APIConfig
    {
        private static string connectionString =
            @"Host=localhost;Username=app_user;Password=userpass;Database=IngWeb";
        //@"Data Source=DIEGOHL\SQLDEV;Database=IngWebDev;Integrated Security=SSPI";
        //@"Data Source=DIEGOHL\SQLEXPR;Database=IngWebPrd;Integrated Security=SSPI;"

        private static string token = "A_super secret key-for the T0kenS";//A key to create the login tokens

        public static string ConnectionString { get => connectionString; set => connectionString = value; }

        public static string Token { get => token; }
    }
}
