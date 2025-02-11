using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using static MyApi.Models.EmployeeModel;

namespace MyApi.Controllers
{

    [RoutePrefix("api/grid")]
    public class GridviewController : ApiController
    {
        private readonly string connectionString = "Data Source=DESKTOP-G3T7J0S\\ACSYSERP;Initial Catalog=EmployeeManagment;User ID=sa;Password=secure@123;Connect Timeout=30";


        [HttpGet]
        [Route("EmpDetails")]
        public IHttpActionResult GetEmp()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Execute the stored procedure
                    using (SqlCommand cmd = new SqlCommand("LoadEmployees", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            adapter.Fill(ds);

                            // Check if RegEmp data exists
                            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                            {
                                return Ok(new
                                {
                                    RegEmp = ds.Tables[1] 
                                });
                            }
                            else
                            {
                                return NotFound(); 
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(new Exception("Database error during retrieval: " + ex.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("An unexpected error occurred during retrieval.", ex));
            }
        }

        [HttpDelete]
        [Route("delete/{id}")]
        public IHttpActionResult DeleteEmployee(string id)
        {
           

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DeleteEmployee", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id", id);

                        SqlParameter isDeletedParam = new SqlParameter("@IsDeleted", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(isDeletedParam);

                        cmd.ExecuteNonQuery();

                        bool isDeleted = (bool)isDeletedParam.Value;
                        if (isDeleted)
                        {
                            return Ok(new { Message = "Employee deleted successfully" });
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("An error occurred during deletion.", ex));
            }
        }

        [HttpPut]
        [Route("update")]
        public IHttpActionResult UpdateEmployee(UpdateEmployee employee)
        {

            if (employee == null)
            {
                return BadRequest("Employee data is null.");
            }

            if (string.IsNullOrEmpty(employee.Id))
            {
                return BadRequest("Employee ID is required.");
            }


            if (string.IsNullOrWhiteSpace(employee.MobileNo) || employee.MobileNo.Length != 10 || !employee.MobileNo.All(char.IsDigit))
            {
                return BadRequest("The MobileNo must be exactly 10 digits and contain only numeric characters.");
            }

            if (string.IsNullOrWhiteSpace(employee.Email) || !Regex.IsMatch(employee.Email, @"^[^@\s]+@gmail\.com$"))
            {
                return BadRequest("The Email must be a valid Gmail address.");
            }

            try
            {

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UpdateEmployee", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id", employee.Id);
                        cmd.Parameters.AddWithValue("@FirstName", employee.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", employee.LastName);
                        cmd.Parameters.AddWithValue("@Email", employee.Email);
                        cmd.Parameters.AddWithValue("@MobileNo", employee.MobileNo);
                        cmd.Parameters.AddWithValue("@DOB", employee.DOB);
                        cmd.Parameters.AddWithValue("@Gender", employee.Gender);

                        SqlParameter affectedRowsParam = new SqlParameter("@AffectedRows", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(affectedRowsParam);

                        SqlParameter isDuplicateEmailParam = new SqlParameter("@IsDuplicateEmail", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(isDuplicateEmailParam);

                        cmd.ExecuteNonQuery();

                        int affectedRows = (int)affectedRowsParam.Value;
                        bool isDuplicateEmail = (bool)isDuplicateEmailParam.Value;

                        if (isDuplicateEmail)
                        {
                            return BadRequest("Duplicate email found. Update failed.");
                        }

                        if (affectedRows > 0)
                        {
                            return Ok(new { Message = "Employee updated successfully" });
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("An error occurred during update.", ex));
            }
        }

        [HttpPost]
        [Route("register")]
        public IHttpActionResult RegisterEmployee(RegEmpModel employee)
        {

            if (employee == null)
            {
                return BadRequest("Employee data is null.");
            }

            if (string.IsNullOrEmpty(employee.Id))
            {
                return BadRequest("Employee ID is required.");
            }

            if (string.IsNullOrWhiteSpace(employee.MobileNo) || employee.MobileNo.Length != 10 || !employee.MobileNo.All(char.IsDigit))
            {
                return BadRequest("The MobileNo must be exactly 10 digits and contain only numeric characters.");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("RegisterEmployee", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Id", employee.Id);
                        command.Parameters.AddWithValue("@Firstname", employee.FirstName);
                        command.Parameters.AddWithValue("@Lastname", employee.LastName);
                        command.Parameters.AddWithValue("@Gender", employee.Gender);
                        command.Parameters.AddWithValue("@DOB", employee.DOB);
                        command.Parameters.AddWithValue("@Email", employee.Email);
                        command.Parameters.AddWithValue("@Mobileno", employee.MobileNo);

                        var isDuplicateIdParam = new SqlParameter("@IsDuplicateId", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var isDuplicateEmailParam = new SqlParameter("@IsDuplicateEmail", SqlDbType.Bit) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(isDuplicateIdParam);
                        command.Parameters.Add(isDuplicateEmailParam);

                        command.ExecuteNonQuery();

                        bool isDuplicateId = (bool)isDuplicateIdParam.Value;
                        bool isDuplicateEmail = (bool)isDuplicateEmailParam.Value;

                        if (isDuplicateId)
                        {
                            return Content(HttpStatusCode.Conflict, "The ID already exists.");
                        }

                        if (isDuplicateEmail)
                        {
                            return Content(HttpStatusCode.Conflict, "The Email already exists.");
                        }

                        return Ok("Registration successful.");
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            } 

        }

        [HttpGet]
        [Route("generateNextId")]
        public IHttpActionResult GenerateNextId()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("GenerateNextId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        var nextId = command.ExecuteScalar()?.ToString();

                        if (string.IsNullOrEmpty(nextId))
                        {
                            return BadRequest("Unable to generate the next ID.");
                        }

                        return Ok(new { nextId });
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



    }


}
