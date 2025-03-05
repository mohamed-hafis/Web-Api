using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyApi.Models
{
    public class MenuGrpModel
    {
        public class Menumgt
        {
            public int CID { get; set; }
            public string MenuGroupID { get; set; }
            public string SortID { get; set; }
            public string MenuID { get; set; }
            public string Description { get; set; }
            public bool Reserved { get; set; }
            public int ApplicationType { get; set; }
            public string WebIcon { get; set; }
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
}