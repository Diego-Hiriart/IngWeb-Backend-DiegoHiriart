using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Diagnostics;
using WebAPI_DiegoHiriart.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI_DiegoHiriart.Controllers
{
    [ApiController]
    [Route("api/models")]
    public class ModelsController : ControllerBase
    {
        [HttpPost, Authorize(Roles = "admin")]
        public async Task<ActionResult<List<Model>>> CreateModel(Model model)
        {
            string db = APIConfig.ConnectionString;
            string createModel = "INSERT INTO models(brandid, modelnumber, name, launch, discontinued) VALUES(@0, @1, @2, @3, @4)";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = createModel;
                            cmd.Parameters.AddWithValue("@0", model.BrandId);//Replace the parameters
                            cmd.Parameters.AddWithValue("@1", model.ModelNumber);
                            cmd.Parameters.AddWithValue("@2", model.Name);
                            cmd.Parameters.AddWithValue("@3", model.Launch);
                            cmd.Parameters.AddWithValue("@4", model.Discontinued);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
                return Ok(model);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500, model);
            }
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<List<Model>>> GetAllModels()
        {
            List<Model> models = new List<Model>();
            string db = APIConfig.ConnectionString;
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
                return Ok(models);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpGet("search/{id}")]
        public async Task<ActionResult<List<Model>>> Search(Int64 id)
        {
            List<Model> models = new List<Model>();
            string db = APIConfig.ConnectionString;
            string readModels = "SELECT * FROM models WHERE modelid = @0";
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
                            cmd.Parameters.AddWithValue("@0", id);
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
                return Ok(models);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpGet("by-brand/{brandid}")]
        public async Task<ActionResult<List<Model>>> GetModelsByBrand(int brandid)
        {
            List<Model> models = new List<Model>();
            string db = APIConfig.ConnectionString;
            string readModels = "SELECT * FROM models WHERE brandid = @0";
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
                            cmd.Parameters.AddWithValue("@0", brandid);
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
                return Ok(models);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpPut, Authorize(Roles = "admin")]
        public async Task<ActionResult<List<Model>>> UpdateModel(Model model)
        {
            string db = APIConfig.ConnectionString;
            string updateModel = "UPDATE models SET brandid=@0, modelnumber=@1, name=@2, launch=@3, discontinued=@4 WHERE modelid = @5";
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
                            cmd.CommandText = updateModel;
                            cmd.Parameters.AddWithValue("@0", model.BrandId);
                            cmd.Parameters.AddWithValue("@1", model.ModelNumber);
                            cmd.Parameters.AddWithValue("@2", model.Name);
                            cmd.Parameters.AddWithValue("@3", model.Launch);
                            cmd.Parameters.AddWithValue("@4", model.Discontinued);
                            cmd.Parameters.AddWithValue("@5", model.ModelId);
                            affectedRows = cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
                if (affectedRows > 0)
                {
                    return Ok(model);
                }

            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            return BadRequest("Model not found");
        }

        [HttpDelete("{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteModel(Int64 id)
        {
            string db = APIConfig.ConnectionString;
            string deleteModel = "DELETE FROM models WHERE modelid = @0";
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
                            cmd.CommandText = deleteModel;
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
            return BadRequest("Model not found");
        }
    }
}