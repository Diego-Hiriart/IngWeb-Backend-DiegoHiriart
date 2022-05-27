using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Diagnostics;
using WebAPI_DiegoHiriart.Models;

namespace WebAPI_DiegoHiriart.Controllers
{
    [Route("api/components")]
    [ApiController]
    public class ComponentsController : ControllerBase
    {
        [HttpPost, Authorize(Roles = "admin")]
        public async Task<ActionResult<List<Component>>> CreateComponent(Component component)
        {
            string db = APIConfig.ConnectionString;
            string createComponent = "INSERT INTO components(name, description) VALUES(@0, @1)";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = createComponent;
                            cmd.Parameters.AddWithValue("@0", component.Name);//Replace the parameters
                            cmd.Parameters.AddWithValue("@1", component.Description);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return Ok(component);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500, component);
            }
        }

        [HttpGet("search/{id}"), Authorize]
        public async Task<ActionResult<List<Component>>> SearchComponent(int id)
        {
            List<Component> components = new List<Component>();
            string db = APIConfig.ConnectionString;
            string searchComponent = "SELECT * FROM components WHERE componentid = @0";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = searchComponent;
                            cmd.Parameters.AddWithValue("@0", id);
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var component = new Component();
                                    component.ComponentId = reader.GetInt32(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    component.Name = reader[1] as string;
                                    component.Description = reader[2] as string;
                                    components.Add(component);
                                }
                            }
                        }
                    }
                }
                return Ok(components);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpGet, Authorize]
        public async Task<ActionResult<List<Component>>> GetAll()
        {
            List<Component> components = new List<Component>();
            string db = APIConfig.ConnectionString;
            string getComponents = "SELECT * FROM components";
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(db))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getComponents;
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var component = new Component();
                                    component.ComponentId = reader.GetInt32(0);//Get int from the first column
                                    //Use castings so that nulls get created if needed
                                    component.Name = reader[1] as string;
                                    component.Description = reader[2] as string;
                                    components.Add(component);
                                }
                            }
                        }
                    }
                }
                return Ok(components);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
        }

        [HttpPut, Authorize(Roles = "admin")]
        public async Task<ActionResult<List<Component>>> UpdateComponent(Component component)
        {
            string db = APIConfig.ConnectionString;
            string updateComponent = "UPDATE components SET name=@0, description=@1 WHERE componentid = @2";
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
                            cmd.CommandText = updateComponent;
                            cmd.Parameters.AddWithValue("@0", component.Name);
                            cmd.Parameters.AddWithValue("@1", component.Description);
                            cmd.Parameters.AddWithValue("@2", component.ComponentId);
                            affectedRows = cmd.ExecuteNonQuery();
                        }
                    }
                }
                if (affectedRows > 0)
                {
                    return Ok(component);
                }

            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return StatusCode(500);
            }
            return BadRequest("Component not found");
        }

        [HttpDelete("{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteComponent(int id)
        {
            string db = APIConfig.ConnectionString;
            string deleteComponent = "DELETE FROM components WHERE componentid = @0";
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
                            cmd.CommandText = deleteComponent;
                            cmd.Parameters.AddWithValue("@0", id);
                            affectedRows = cmd.ExecuteNonQuery();
                        }
                    }
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
            return BadRequest("Component not found");
        }
    }
}
