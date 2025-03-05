using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyApi.Models
{
    public class AuthModel
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

        }
    }
}