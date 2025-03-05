using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static MyApi.Models.MenuGrpModel;

namespace MyApi.Controllers 
{

    [RoutePrefix("api/grid")]
    public class MenuGroupController : ApiController
    {

        private readonly string connectionString = "Data Source=DESKTOP-G3T7J0S\\ACSYSERP;Initial Catalog=EmployeeManagment;User ID=sa;Password=secure@123;Connect Timeout=30";

        [HttpGet]
        [Route("GetDdata")]
        public IHttpActionResult GetAllData()
        {
            try
            {
                List<object> companies = new List<object>();
                List<object> menus = new List<object>();
                List<object> webicon = new List<object>();
                List<object> menuGrouping = new List<object>();
                List<object> menumgt = new List<object>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("mg_GetDropdownData", conn)
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
                                    ID = reader["ID"],
                                    Description = reader["Description"]
                                });
                            }
                        }

                        // Move to the next result set: Data
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                webicon.Add(new
                                {
                                  WebIcon = reader["WebIcon"]
                                });
                            }
                        }

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                menuGrouping.Add(new
                                {
                                    CID = reader["CID"],
                                    ID = reader["ID"],
                                    Description = reader["Description"],
                                    ParentID = reader["ParentID"],
                                    SortID = reader["SortID"],
                                    Reserved = reader["Reserved"],
                                    ApplicationType = reader["ApplicationType"],
                                    WebIcon = reader["WebIcon"],
                                });
                            }
                        }
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                menumgt.Add(new
                                {
                                    CID = reader["CID"],
                                    MenuGroupID = reader["MenuGroupID"],
                                    SortID = reader["SortID"],
                                    MenuID = reader["MenuID"],
                                    Description = reader["Description"],
                                    Reserved = reader["Reserved"],
                                    ApplicationType = reader["ApplicationType"],
                                    WebIcon = reader["WebIcon"],

                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    Companies = companies,
                    Menus = menus,
                    Webicon = webicon,
                    MenuGrouping = menuGrouping,
                    Menumgt = menumgt
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
        [Route("SaveMenuData")]
        public IHttpActionResult SaveData(Menudata formData)
        {

            if (formData == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("mg_ManageData", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@Action", "INSERT");
                    cmd.Parameters.AddWithValue("@CID", formData.CID);
                    cmd.Parameters.AddWithValue("@ID", formData.ID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", formData.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ParentID", formData.ParentID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SortID", formData.SortID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Reserved", formData.Reserved);
                    cmd.Parameters.AddWithValue("@ApplicationType", formData.ApplicationType);
                    cmd.Parameters.AddWithValue("@WebIcon", formData.WebIcon ?? (object)DBNull.Value);

                    // Output parameter
                    SqlParameter outputParam = new SqlParameter("@Result", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    // Execute
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    // Retrieve output parameter value
                    string resultMessage = outputParam.Value?.ToString() ?? "Unknown error occurred";

                    if (resultMessage.Contains("Duplicate record exists"))
                    {
                        return Conflict(); // HTTP 409 Conflict for duplicate records
                    }

                    return Ok(new { message = resultMessage, data = formData });
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
        [Route("Update/{id}")]
        public IHttpActionResult EditMenuItem(string id, [FromBody] Menudata menuItem)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("mg_ManageData", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Action", "EDIT");
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.Parameters.AddWithValue("@CID", menuItem.CID);
                cmd.Parameters.AddWithValue("@Description", menuItem.Description);
                cmd.Parameters.AddWithValue("@ParentID", menuItem.ParentID);
                cmd.Parameters.AddWithValue("@SortID", menuItem.SortID);
                cmd.Parameters.AddWithValue("@Reserved", menuItem.Reserved);
                cmd.Parameters.AddWithValue("@ApplicationType", menuItem.ApplicationType);
                cmd.Parameters.AddWithValue("@WebIcon", menuItem.WebIcon);

                SqlParameter resultParam = new SqlParameter("@Result", SqlDbType.NVarChar, 100)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(resultParam);

                conn.Open();
                cmd.ExecuteNonQuery();
                string resultMessage = resultParam.Value.ToString();

                if (resultMessage.Contains("Update failed: A record with the same values already exists"))
                {
                    return Content(HttpStatusCode.Conflict, new { message = resultMessage }); // 409 Conflict
                }
                else if (resultMessage.Contains("Update failed, record not found"))
                {
                    return Content(HttpStatusCode.NotFound, new { message = resultMessage }); // 404 Not Found
                }
                else if (resultMessage.Contains("Record updated successfully"))
                {
                    return Ok(new { message = resultMessage }); // 200 OK
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, new { message = "Unexpected error occurred" }); // 400 Bad Request
                }
            }
        }


        [HttpDelete]
        [Route("Delete")]
        public IHttpActionResult DeleteData([FromBody] Menudata deleteItem)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("mg_ManageData", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Input Parameters
                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@CID", deleteItem.CID);
                    cmd.Parameters.AddWithValue("@ID", deleteItem.ID);
                    cmd.Parameters.AddWithValue("@Description", deleteItem.Description);
                    cmd.Parameters.AddWithValue("@ParentID", deleteItem.ParentID);
                    cmd.Parameters.AddWithValue("@SortID", deleteItem.SortID);
                    cmd.Parameters.AddWithValue("@Reserved", deleteItem.Reserved);
                    cmd.Parameters.AddWithValue("@ApplicationType", deleteItem.ApplicationType);
                    cmd.Parameters.AddWithValue("@WebIcon", deleteItem.WebIcon);

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


        [HttpGet]
        [Route("getmenumgt")]
        public IHttpActionResult GetData()
        {
            try
            {
                List<object> menumgt = new List<object>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("mg_GetMenuMgtPopup", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // First result set: Companies
                        while (reader.Read())
                        {
                            menumgt.Add(new
                            {
                                CID = reader["CID"],
                                CompanyName = reader["CompanyName"]
                            });
                        }

                    }
                }
                return Ok(new
                {
                    Menumgt = menumgt,
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
        [Route("Submit")]
        public IHttpActionResult Submit(Menumgt formData)
        {

            if (formData == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("mp_Submit", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@Action", "SUBMIT");
                    cmd.Parameters.AddWithValue("@CID", formData.CID);
                    cmd.Parameters.AddWithValue("@MenuGroupID", formData.MenuGroupID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SortID", formData.SortID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MenuID", formData.MenuID);
                    cmd.Parameters.AddWithValue("@Description", formData.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Reserved", formData.Reserved);
                    cmd.Parameters.AddWithValue("@ApplicationType", formData.ApplicationType);
                    cmd.Parameters.AddWithValue("@WebIcon", formData.WebIcon ?? (object)DBNull.Value);

                    // Output parameter
                    SqlParameter outputParam = new SqlParameter("@Result", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    // Execute
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    // Retrieve output parameter value
                    string resultMessage = outputParam.Value?.ToString() ?? "Unknown error occurred";

                    return Ok(new { message = resultMessage, data = formData });
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

    }

   
}
