using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyApi.Models
{
    public class SampleformModel
    {
        public class SampleRecord
        {
            public string CID { get; set; }
            public string MenuID { get; set; }
            public string MenuName { get; set; }
            public string SqlQuery1 { get; set; }
            public string SqlQuery2 { get; set; }
            public string RelationColumn1 { get; set; }
            public string RelationColumn2 { get; set; }
            public string FormatColumn { get; set; }
            public string CompanyName { get; set; }
        }


        public class SampleRecord1
        {
            public string MenuID { get; set; }
            public string MenuName { get; set; }
        }

    }
}