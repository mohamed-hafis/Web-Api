using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyApi.Models
{
    public class EmployeeModel
    {

        public class RegEmpModel
        {
            public string Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string MobileNo { get; set; }
            public DateTime DOB { get; set; }
            public string Gender { get; set; }
        }

        public class UpdateEmployee
        {
            public string Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string MobileNo { get; set; }
            public DateTime DOB { get; set; }
            public string Gender { get; set; }

        }
    }
}