using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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
                    }
                }

                return Ok(new
                {
                    Companies = companies,
                    Menus = menus,
                    Webicon = webicon,
                    MenuGrouping = menuGrouping
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

    public class Menudata
    {
        public int CID { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public int? ParentID { get; set; }
        public int? SortID { get; set; }
        public bool Reserved { get; set; }
        public int ApplicationType { get; set; }
        public string WebIcon { get; set; }
    }
}
