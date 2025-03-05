using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static MyApi.Models.SampleformModel;

namespace MyApi.Controllers
{

    [RoutePrefix("api/grid")]
    public class SampleformController : ApiController
    {
        private readonly string connectionString = "Data Source=DESKTOP-G3T7J0S\\ACSYSERP;Initial Catalog=EmployeeManagment;User ID=sa;Password=secure@123;Connect Timeout=30";

        [HttpGet]
        [Route("GetDDdata")]
        public IHttpActionResult GetAllData()
        {
            try
            {
                List<object> companies = new List<object>();
                List<object> menus = new List<object>();
                List<object> data = new List<object>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sf_GetDDdata", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // First result set: Companies
                        while (reader.Read())
                        {
                            companies.Add(new
                            {
                                CID = reader["CID"],
                                CompanyName = reader["CompanyName"]
                            });
                        }

                        // Move to the next result set: Menus
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                menus.Add(new
                                {
                                    MenuID = reader["MenuID"],
                                    MenuName = reader["Description"]
                                });
                            }
                        }

                        // Move to the next result set: Data
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    CID = reader["CID"],
                                    MenuID = reader["MenuID"],
                                    MenuName = reader["MenuName"]
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    Companies = companies,
                    Menus = menus,
                    Data = data
                });
            }
            catch (SqlException ex)
            {
                return InternalServerError(new Exception("Database error: " + ex.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("An unexpected error occurred.", ex));
            }
        }


        [HttpPost]
        [Route("SaveData")]
        public IHttpActionResult SaveData(SampleRecord formData)
        {

            if (formData == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sf_ManageActions", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@Action", "INSERT"); // Specify action type
                    cmd.Parameters.AddWithValue("@CID", formData.CID);
                    cmd.Parameters.AddWithValue("@MenuID", formData.MenuID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MenuName", formData.MenuName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SqlQuery1", formData.SqlQuery1 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SqlQuery2", formData.SqlQuery2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RelationColumn1", formData.RelationColumn1 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RelationColumn2", formData.RelationColumn2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FormatColumn", formData.FormatColumn ?? (object)DBNull.Value);

                    SqlParameter outputParam = new SqlParameter("@Result", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);


                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    // Retrieve output parameter value
                    string resultMessage = outputParam.Value?.ToString() ?? "Unknown error occurred";

                    if (resultMessage.Contains("successfully"))
                    {
                        return Ok(new { message = resultMessage, data = formData });
                    }
                    else
                    {
                        return BadRequest(resultMessage);
                    }
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(new Exception("Database error: " + ex.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("An unexpected error occurred.", ex));
            }
        }

        [HttpPut]
        [Route("UpdateData")]
        public IHttpActionResult UpdateData(SampleRecord formData)
        {
            if (formData == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sf_ManageActions", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@Action", "UPDATE"); // Specify the action type
                    cmd.Parameters.AddWithValue("@CID", formData.CID);
                    cmd.Parameters.AddWithValue("@MenuID", formData.MenuID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MenuName", formData.MenuName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SqlQuery1", formData.SqlQuery1 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SqlQuery2", formData.SqlQuery2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RelationColumn1", formData.RelationColumn1 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RelationColumn2", formData.RelationColumn2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FormatColumn", formData.FormatColumn ?? (object)DBNull.Value);

                    SqlParameter outputParam = new SqlParameter("@Result", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    string resultMessage = outputParam.Value?.ToString() ?? "Unknown error occurred";

                    if (resultMessage.Contains("successfully"))
                    {
                        return Ok(new { message = resultMessage, data = formData });
                    }
                    else
                    {
                        return BadRequest(resultMessage);
                    }
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(new Exception("Database error: " + ex.Message));
            }
        }



        [HttpGet]
        [Route("GetRecordForEdit/{cid}/{menuId}")]
        public IHttpActionResult GetRecordForEdit(string cid, string menuId)
        {
            try
            {
                SampleRecord record = null;
                string resultMessage = "";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {

                    SqlCommand cmd = new SqlCommand("sf_ManageActions", conn)
                    {
                        CommandType = CommandType.StoredProcedure // Set the command type to StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@Action", "EDIT");
                    cmd.Parameters.AddWithValue("@CID", cid);
                    cmd.Parameters.AddWithValue("@MenuID", menuId);

                    SqlParameter outputParam = new SqlParameter("@Result", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            record = new SampleRecord
                            {
                                CID = reader["CID"].ToString(),
                                MenuID = reader["MenuID"].ToString(),
                                MenuName = reader["MenuName"].ToString(),
                                SqlQuery1 = reader["SqlQuery1"].ToString(),
                                SqlQuery2 = reader["SqlQuery2"].ToString(),
                                RelationColumn1 = reader["RelationColumn1"].ToString(),
                                RelationColumn2 = reader["RelationColumn2"].ToString(),
                                FormatColumn = reader["FormatColumn"].ToString(),
                                CompanyName = reader["CompanyName"].ToString() // Fetch CompanyName here
                            };
                        }
                    }
                    resultMessage = outputParam.Value.ToString();
                }

                if (record != null)
                {
                    return Ok(new { Message = resultMessage, Data = record });
                }
                else
                {
                    return NotFound();
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(new Exception("Database error: " + ex.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("An unexpected error occurred.", ex));
            }
        }

        [HttpPost]
        [Route("CheckMenuId")]
        public IHttpActionResult CheckMenuExists([FromBody] SampleRecord1 menu)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    connection.Open();

                    // Create the command and set its properties
                    using (SqlCommand command = new SqlCommand("sf_CheckMenuExist", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@MenuID", SqlDbType.VarChar, 50).Value = menu.MenuID;
                        command.Parameters.Add("@MenuName", SqlDbType.VarChar, 100).Value = menu.MenuName;

                        // Execute the command
                        var result = command.ExecuteScalar();

                        // Return response based on the result
                        bool doesNotExist = result != null && Convert.ToInt32(result) == 0; // 0 means does not exist
                        return Ok(new { MenuDoesNotExist = doesNotExist });
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
        }


        [HttpDelete]
        [Route("DeleteData/{cid}/{menuId}")]
        public IHttpActionResult DeleteData(string cid, string menuId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sf_ManageActions", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Input Parameters
                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@CID", cid);
                    cmd.Parameters.AddWithValue("@MenuID", menuId);

                    // Output Parameter
                    SqlParameter outputParam = new SqlParameter("@Result", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    string resultMessage = outputParam.Value?.ToString() ?? "Unknown error occurred";

                    if (resultMessage.Contains("successfully"))
                    {
                        return Ok(new { message = resultMessage });
                    }
                    else
                    {
                        return BadRequest(resultMessage);
                    }
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(new Exception("Database error during delete: " + ex.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("An unexpected error occurred during delete.", ex));
            }
        }
    }
}
