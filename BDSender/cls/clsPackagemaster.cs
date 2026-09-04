using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace PrescriptionManagement.cls
{
    class clsPackagemaster
    {
        public static DataTable db_packagemaster { get; set; } = new DataTable();
        public static DataTable db_drug { get; set; } = new DataTable();
        public static DataTable db_data { get; set; } = new DataTable();
        public static DataTable db_device { get; set; } = new DataTable();
        public static DataTable db_seqzone { get; set; } = new DataTable();
        public static DataTable dtprint { get; set; } = new DataTable();
        public static DataTable dtbasket { get; set; } = new DataTable();
        public static DataTable db_drugallergies { get; set; } = new DataTable();
        public static DataTable db_labs { get; set; } = new DataTable();
        public static List<object> obprescription { get; set; } = new List<object>();
        public static List<object> obpackagemaster { get; set; } = new List<object>();
        public static List<object> obuser { get; set; } = new List<object>();
        public static List<Dictionary<string, object>> objresponseBody;
    }
}
