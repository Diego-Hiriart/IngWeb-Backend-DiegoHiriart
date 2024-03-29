﻿using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Diagnostics;
using WebAPI_DiegoHiriart.Models;
using Microsoft.AspNetCore.Authorization;
using WebAPI_DiegoHiriart.Settings;

namespace WebAPI_DiegoHiriart.Controllers
{
    [ApiController]
    [Route("api/brands")]
    public class BrandsController : ControllerBase
    {
        //A constructor for this class is needed so that when it is called the config and environment info needed are passed
        public BrandsController(IConfiguration config, IWebHostEnvironment env)
        {
            this.config = config;
            this.env = env;
            this.db = new AppSettings(this.config, this.env).DBConn;
        }
        //These configurations and environment info are needed to create a DBConfig instance that has the right connection string depending on whether the app is running on a development or production environment
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;
        private string db;//Connection string

        [HttpPost, Authorize(Roles = "admin")]
        public async Task<ActionResult<List<Brand>>> CreateBrand(Brand brand)
        {
            string createBrand = "INSERT INTO brands(name, isdefunct) VALUES(@0, @1)";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = createBrand;
                            cmd.Parameters.AddWithValue("@0", brand.Name);//Replace the parameters
                            cmd.Parameters.AddWithValue("@1", brand.IsDefunct);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
                return Ok(brand);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500, brand);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Brand>>> GetBrands()
        {
            List<Brand> brands = new List<Brand>();
            string readBrands = "SELECT * FROM brands";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readBrands;
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var brand = new Brand();
                                    brand.BrandId = reader.GetInt32(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    brand.Name = reader[1] as string;
                                    brand.IsDefunct = reader.GetBoolean(2);
                                    brands.Add(brand);//Add brand to list
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return Ok(brands);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpGet("search/{id}"), Authorize]
        public async Task<ActionResult<List<Brand>>> SeachBrand(int id)
        {
            List<Brand> brands = new List<Brand>();
            string readBrands = "SELECT * FROM brands WHERE brandid = @0";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = readBrands;
                            cmd.Parameters.AddWithValue("@0", id);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var brand = new Brand();
                                    brand.BrandId = reader.GetInt32(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    brand.Name = reader[1] as string;
                                    brand.IsDefunct = reader.GetBoolean(2);
                                    brands.Add(brand);//Add brand to list
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return Ok(brands);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpPut, Authorize(Roles = "admin")]
        public async Task<ActionResult<List<Brand>>> UpdateBrand(Brand brand)
        {
            string updateBrand = "UPDATE brands SET name=@0, isdefunct=@1 WHERE brandid = @2";
            try
            {
                int affectedRows = 0;
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = updateBrand;
                            cmd.Parameters.AddWithValue("@0", brand.Name);
                            cmd.Parameters.AddWithValue("@1", brand.IsDefunct);
                            cmd.Parameters.AddWithValue("@2", brand.BrandId);
                            affectedRows = cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
                if (affectedRows > 0)
                {
                    return Ok(brand);
                }

            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            return BadRequest("Brand not found");
        }

        [HttpDelete("{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            string deleteBrand = "DELETE FROM brands WHERE brandid = @0";
            try
            {
                int affectedRows = 0;
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = deleteBrand;
                            cmd.Parameters.AddWithValue("@0", id);
                            affectedRows = cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
                if (affectedRows > 0)
                {
                    return Ok();
                }

            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            return BadRequest("Brand not found");
        }
    }
}