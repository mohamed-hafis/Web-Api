using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.Web.Http;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using static MyApi.Models.AuthModel;


namespace MyApi.Controllers
{

    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly string connectionString = "Data Source=DESKTOP-G3T7J0S\\ACSYSERP;Initial Catalog=EmployeeManagment;User ID=sa;Password=secure@123;Connect Timeout=30";

        private const string SecretKey = "your_secret_key_herefahfiugyr*&T*IiugiugtyhugtiuggyF%T*VJhvyGYG&T8G";

        [HttpPost]
        [Route("Login")]
        public IHttpActionResult Login(LoginRequest request)
        {
            // Validate the input
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required");
            }

            try
            {

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("ManageUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Action", "Login");
                        cmd.Parameters.AddWithValue("@UserName", request.UserName);
                        cmd.Parameters.AddWithValue("@Password", request.Password);

                        cmd.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();

                        bool isAuthenticated = (bool)cmd.Parameters["@Status"].Value;

                        if (isAuthenticated)
                        {
                            var token = GenerateJwtToken(request.UserName);
                            Console.WriteLine("Generated Token: " + token);
                            return Ok(new { Message = "Login successful", Token = token });
                        }
                        else
                        {
                            return Unauthorized();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(new Exception("Database error during login: " + ex.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("An unexpected error occurred during login.", ex));
            }
        }

       

        private bool IsValidPassword(string password)
        {
            // Add password validation rules (length, complexity, etc.)
            if (password.Length < 8) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]")) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]")) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]")) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[\W_]")) return false;
            return true;
        }


        [HttpPost]
        [Route("Signup")]
        public IHttpActionResult Signup(SignupRequest request)
        {
            // Validate the input
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
            {
                return BadRequest("Username and password are required");
            }

            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest("Password and confirmation password do not match");
            }

            if (!IsValidPassword(request.Password))
            {
                return BadRequest("Password must be at least 8 characters long, include uppercase and lowercase letters, a digit, and a special character.");
            }

            try
            {
                // Hash password for storage 

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("ManageUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Action", "Signup");
                        cmd.Parameters.AddWithValue("@UserName", request.UserName);
                        cmd.Parameters.AddWithValue("@Password", request.Password);

                        // Output parameter for status
                        cmd.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;

                        // Execute the command
                        cmd.ExecuteNonQuery();

                        // Retrieve the status of the operation
                        bool isUserCreated = (bool)cmd.Parameters["@Status"].Value;

                        if (isUserCreated)
                        {
                            return Ok(new { Message = "User created successfully" });
                        }
                        else
                        {
                            return BadRequest("Username already exists");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(new Exception("Database error during signup: " + ex.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("An unexpected error occurred during signup.", ex));
            }
        }

      
        private string GenerateJwtToken(string userName)
        {
            // Use a secure key for signing
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Claims for the token (additional claims can be added if necessary)
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, userName),  // Subject (user name)
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  // Unique JWT ID
        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.DateTime),  // Issued At (optional)
    };

            // You can add roles or other claims if required
            // Example: new Claim(ClaimTypes.Role, "Admin");

            // Generate the token
            var token = new JwtSecurityToken(
                issuer: "your_issuer",  // e.g., "MyApi" or "YourApp"
                audience: "your_audience",  // e.g., "YourClientApp"
                claims: claims,
                expires: DateTime.Now.AddHours(1),  // Set expiration time (1 hour in this case)
                signingCredentials: credentials
            );

            // Return the generated token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }



    }
}
