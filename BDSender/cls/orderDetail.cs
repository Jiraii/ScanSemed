using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDSender.cls
{
    public class orderDetail
    {
        public string no { get; set; }
        public string qn { get; set; }
        public string name { get; set; }
        public string hn { get; set; }
        public string gender { get; set; }
        public string age { get; set; }
        public string drugcode { get; set; }
        public string drugname { get; set; }
        public string qty { get; set; }
        public string unit { get; set; }
        public static DataTable db_packagemaster { get; set; } = new DataTable();
        public static DataTable db_drug { get; set; } = new DataTable();
        public static DataTable db_data { get; set; } = new DataTable();
        public static DataTable db_device { get; set; } = new DataTable();
        public static DataTable db_labs { get; set; } = new DataTable();
        public static DataTable db_drugallergies { get; set; } = new DataTable();
        public static List<object> obuser { get; set; } = new List<object>();
        
    }

}
