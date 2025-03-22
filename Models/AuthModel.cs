using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyApi.Models
{
    public class Auth
    {
        public class LoginRequest
        {
            public string UserName { get; set; }
            public string Password { get; set; }

        }

        public class SignupRequest
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
            public string Role { get; set; }


        }
    }
}
