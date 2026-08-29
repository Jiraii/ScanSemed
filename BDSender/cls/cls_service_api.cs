using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BDSender.cls;
using System.Windows;
using Newtonsoft.Json;

namespace BDSender.cls
{
    public class cls_service_api
    {
        orderDetail cls_orderDetail = new orderDetail();
        public static async Task RequestPackagemasterSEmed(string keyword)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    cls.orderDetail.db_data = new DataTable();
                    cls.orderDetail.db_drugallergies = new DataTable();
                    cls.orderDetail.db_labs = new DataTable();
                    cls.orderDetail.db_packagemaster = new DataTable();
                    cls.orderDetail.db_drug = new DataTable();

                    //string apiUrl = "http://office.gd4.co.th:6426/packagemaster/order/semed";
                    string apiUrl = BDSender.Properties.Settings.Default.apiUrl + "/packagemaster/order/semed";
                    //apiUrl = string.Format(apiUrl);
                    // สร้างข้อมูล JSON ที่ต้องการส่ง
                    string jsonContent = $@"{{
                                                ""basketid"": ""{keyword.ToLower()}""
                                            }}";

                    // สร้าง HttpContent (StringContent)
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        //responseBody = responseBody.Substring(1, responseBody.Length - 2);
                        
                        Console.WriteLine(responseBody);
                        JObject obj = JObject.Parse(responseBody);
                        if (obj["status"].ToString() == "200")
                        {
                            Console.WriteLine($"Find Success");

                            // ใช้ JArray เพื่อเข้าถึง "data"
                            JArray dataArray = (JArray)obj["data"];
                            DataTable dtobjdata = new DataTable();

                            // แปลง JArray เป็น List ของ Dictionary
                            List<Dictionary<string, object>> objresponseBody = new List<Dictionary<string, object>>();
                            objresponseBody = dataArray.ToObject<List<Dictionary<string, object>>>();

                            JArray dataArraydata = new JArray();
                            List<Dictionary<string, object>> objdata = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            foreach (var dataObject in objresponseBody)
                            {
                                foreach (var key in dataObject.Keys)
                                {
                                    if (key != "drugallergies" && key != "packagemaster" && key != "drugs" && key != "labs")
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        dtobjdata.Columns.Add($"{key}", typeof(object));
                                    }

                                }
                                //dataArraydata = (JArray)dataObject["data"];
                                //objdata = dataArraydata.ToObject<List<Dictionary<string, object>>>();
                                Console.WriteLine("Data Details:");
                                DataRow row = dtobjdata.NewRow();
                                foreach (var key in dataObject.Keys)
                                {
                                    if (key != "drugallergies" && key != "packagemaster" && key != "drugs" && key != "labs")
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        row[key] = $"{dataObject[key]}"; // ถ้า null ให้ใส่ DBNull.Value                                       

                                    }

                                }
                                dtobjdata.Rows.Add(row);
                                Console.WriteLine();
                            }
                            if (dtobjdata.Rows.Count > 0)
                            {
                                cls.orderDetail.db_data.Merge(dtobjdata);
                            }




                            JArray dataArraypackagemaster = new JArray();
                            List<Dictionary<string, object>> objpackage = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            foreach (var dataObject in objresponseBody)
                            {
                                dataArraypackagemaster = (JArray)dataObject["packagemaster"];
                                objpackage = dataArraypackagemaster.ToObject<List<Dictionary<string, object>>>();
                                Console.WriteLine("Packagemaster Details:");
                                foreach (var key in dataObject.Keys)
                                {
                                    Console.WriteLine($"{key}: {dataObject[key]}");
                                }
                                Console.WriteLine();
                            }
                            DataTable dtobjpackage = new DataTable();
                            if (objpackage.Count > 0)
                            {
                                Console.WriteLine($"Can't Find Success");
                                // สร้างคอลัมน์จาก key ใน Dictionary ตัวแรก
                                foreach (var key in objpackage.First().Keys)
                                {
                                    dtobjpackage.Columns.Add(key, typeof(object)); // ใช้ object รองรับทุกประเภทข้อมูล
                                }
                                // เพิ่มข้อมูลแต่ละ Dictionary เป็นแถวของ DataTable
                                foreach (var dict in objpackage)
                                {
                                    DataRow row = dtobjpackage.NewRow();
                                    foreach (var key in dict.Keys)
                                    {
                                        string propertyName = "Name";  // เปลี่ยนเป็น Property ที่ต้องการเช็ค
                                        bool hasProperty = obj.GetType().GetProperty(propertyName) != null;
                                        row[key] = dict[key] ?? DBNull.Value; // ถ้า null ให้ใส่ DBNull.Value
                                    }
                                    dtobjpackage.Rows.Add(row);

                                }
                                if (dtobjpackage.Rows.Count > 0)
                                {

                                    cls.orderDetail.db_packagemaster.Merge(dtobjpackage);
                                }
                            }


                            // drugs
                            JArray dataArraydrugs = new JArray();
                            List<Dictionary<string, object>> objdrugs = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            foreach (var dataObject in objresponseBody)
                            {
                                dataArraydrugs = (JArray)dataObject["drugs"];
                                objdrugs = dataArraydrugs.ToObject<List<Dictionary<string, object>>>();
                                Console.WriteLine("drug Details:");
                                foreach (var key in dataObject.Keys)
                                {
                                    Console.WriteLine($"{key}: {dataObject[key]}");
                                }
                                Console.WriteLine();
                            }
                            DataTable dtobjdrugse = new DataTable();

                            if (objdrugs.Count > 0)
                            {
                                // สร้างคอลัมน์จาก key ใน Dictionary ตัวแรก
                                foreach (var key in objdrugs.First().Keys)
                                {
                                    dtobjdrugse.Columns.Add(key, typeof(object)); // ใช้ object รองรับทุกประเภทข้อมูล
                                }
                                // เพิ่มข้อมูลแต่ละ Dictionary เป็นแถวของ DataTable
                                foreach (var dict in objdrugs)
                                {
                                    DataRow row = dtobjdrugse.NewRow();
                                    foreach (var key in dict.Keys)
                                    {
                                        row[key] = dict[key] ?? DBNull.Value; // ถ้า null ให้ใส่ DBNull.Value
                                    }
                                    dtobjdrugse.Rows.Add(row);

                                }
                                if (dtobjdrugse.Rows.Count > 0)
                                {

                                    cls.orderDetail.db_drug.Merge(dtobjdrugse);
                                }

                            }

                            // Labs
                            JArray dataArraylabs = new JArray();
                            List<Dictionary<string, object>> objlabs = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            foreach (var dataObjectlabs in objresponseBody)
                            {
                                dataArraylabs = (JArray)dataObjectlabs["labs"];
                                objlabs = dataArraylabs.ToObject<List<Dictionary<string, object>>>();
                                Console.WriteLine("labs Details:");
                                foreach (var key in dataObjectlabs.Keys)
                                {
                                    Console.WriteLine($"{key}: {dataObjectlabs[key]}");
                                }
                                Console.WriteLine();
                            }
                            DataTable dtobjlabs = new DataTable();
                            if (objlabs == null || objlabs.Count == 0)
                                Console.WriteLine($"Can't Find Success");
                            // สร้างคอลัมน์จาก key ใน Dictionary ตัวแรก
                            foreach (var key in objlabs.First().Keys)
                            {
                                dtobjlabs.Columns.Add(key, typeof(object)); // ใช้ object รองรับทุกประเภทข้อมูล
                            }
                            // เพิ่มข้อมูลแต่ละ Dictionary เป็นแถวของ DataTable
                            foreach (var dict in objlabs)
                            {
                                DataRow row = dtobjlabs.NewRow();
                                foreach (var key in dict.Keys)
                                {
                                    row[key] = dict[key] ?? DBNull.Value; // ถ้า null ให้ใส่ DBNull.Value
                                }
                                dtobjlabs.Rows.Add(row);

                            }
                            if (dtobjlabs.Rows.Count > 0)
                            {

                                cls.orderDetail.db_labs.Merge(dtobjlabs);
                            }

                            // drugallergies
                            JArray dataArraydrugallergies = new JArray();
                            List<Dictionary<string, object>> objdrugallergies = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            foreach (var dataObjectdrugallergies in objresponseBody)
                            {
                                dataArraydrugallergies = (JArray)dataObjectdrugallergies["drugallergies"];
                                objdrugallergies = dataArraydrugallergies.ToObject<List<Dictionary<string, object>>>();
                                Console.WriteLine("labs Details:");
                                foreach (var key in dataObjectdrugallergies.Keys)
                                {
                                    Console.WriteLine($"{key}: {dataObjectdrugallergies[key]}");
                                }
                                Console.WriteLine();
                            }
                            DataTable dtobjdrugallergies = new DataTable();
                            if (objdrugallergies == null || objdrugallergies.Count == 0)
                                Console.WriteLine($"Can't Find Success");
                            // สร้างคอลัมน์จาก key ใน Dictionary ตัวแรก
                            foreach (var key in objdrugallergies.First().Keys)
                            {
                                dtobjdrugallergies.Columns.Add(key, typeof(object)); // ใช้ object รองรับทุกประเภทข้อมูล
                            }
                            // เพิ่มข้อมูลแต่ละ Dictionary เป็นแถวของ DataTable
                            foreach (var dict in objdrugallergies)
                            {
                                DataRow row = dtobjdrugallergies.NewRow();
                                foreach (var key in dict.Keys)
                                {
                                    row[key] = dict[key] ?? DBNull.Value; // ถ้า null ให้ใส่ DBNull.Value
                                }
                                dtobjdrugallergies.Rows.Add(row);

                            }
                            if (dtobjdrugallergies.Rows.Count > 0)
                            {

                                cls.orderDetail.db_drugallergies.Merge(dtobjdrugallergies);
                            }


                        }
                        else
                        {
                            Console.WriteLine($"Can't Find Success");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine($" Can't Find Success ");
                }
            }

        }
        public static async Task<bool> update_resultSemed(string Json)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    bool result = false;
                    //string apiUrl = "http://office.gd4.co.th:6426/order/update";
                    string apiUrl = BDSender.Properties.Settings.Default.apiUrl + "/dih/sendoredrdish";
                    apiUrl = string.Format(apiUrl);
                    // สร้างข้อมูล JSON ที่ต้องการส่ง
                    string jsonContent = "";
                    var jsonString = Json ;
                    Console.WriteLine(jsonString);
                    // สร้าง HttpContent (StringContent)
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    Console.WriteLine(content);
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        //responseBody = responseBody.Substring(1, responseBody.Length - 2);
                        Console.WriteLine(responseBody);
                        JObject obj = JObject.Parse(responseBody);
                        if (obj["status"].ToString() == "200")
                        {
                            Console.WriteLine($" Find Success ");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($" Can't Find Success ");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                        return false;
                    }
                }
                catch (Exception Ex)
                {
                    return false;
                    MessageBox.Show(Ex.ToString());
                }
                finally
                {

                }
            }
        }
        public static async Task<DataTable> Request_Getsemedstock(string keyword)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    DataTable dtobjdata = new DataTable();
                    //string apiUrl = "http://192.168.30.14:6426/dih/getsemedstock";
                    string apiUrl = BDSender.Properties.Settings.Default.apiUrl + "/dih/getsemedstock";
                    //apiUrl = string.Format(apiUrl);
                    // สร้างข้อมูล JSON ที่ต้องการส่ง
                    string jsonContent = $@"{{
                                                ""drugcode"": { keyword}
                                            }}";

                    // สร้าง HttpContent (StringContent)
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        
                        Console.WriteLine(responseBody);
                        JObject obj = JObject.Parse(responseBody);
                        if (obj["status"].ToString() == "200")
                        {
                            Console.WriteLine($"Find Success");                            

                            // ใช้ JArray เพื่อเข้าถึง "data"
                            JArray dataArray = (JArray)obj["data"];

                            // แปลง JArray เป็น List ของ Dictionary
                            List<Dictionary<string, object>> objresponseBody = new List<Dictionary<string, object>>();
                            objresponseBody = dataArray.ToObject<List<Dictionary<string, object>>>();
                            JArray dataArraydata = new JArray();
                            List<Dictionary<string, object>> objdata = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            foreach (var dataObject in objresponseBody)
                            {
                                foreach (var key in dataObject.Keys)
                                {
                                    Console.WriteLine($"{key}: {dataObject[key]}");

                                    if (!dtobjdata.Columns.Contains(key))
                                    {
                                        dtobjdata.Columns.Add(key, typeof(object));
                                    }
                                }

                                //dataArraydata = (JArray)dataObject["data"];
                                //objdata = dataArraydata.ToObject<List<Dictionary<string, object>>>();
                                Console.WriteLine("Data Details:");
                                DataRow row = dtobjdata.NewRow();
                                foreach (var key in dataObject.Keys)
                                {
                                    Console.WriteLine($"{key}: {dataObject[key]}");
                                    row[key] = $"{dataObject[key]}"; // ถ้า null ให้ใส่ DBNull.Value 

                                }
                                dtobjdata.Rows.Add(row);
                                Console.WriteLine();
                            }

                        }
                        else
                        {
                            Console.WriteLine($"Can't Find Success");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");

                       
                        
                    }

                    return dtobjdata;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine($" Can't Find Success ");
                    return new DataTable();
                }
            }

        }
        public static async Task<DataTable> Request_GetOutporder(string keyword)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    DataTable dtobjdata = new DataTable();
                    //string apiUrl = "http://192.168.30.14:6426/dih/getsemedstock";
                    string apiUrl = BDSender.Properties.Settings.Default.apiUrl + "/dih/getsemedstock";
                    //apiUrl = string.Format(apiUrl);
                    // สร้างข้อมูล JSON ที่ต้องการส่ง
                    string jsonContent = $@"{{
                                                ""drugcode"": { keyword}
                                            }}";

                    // สร้าง HttpContent (StringContent)
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(responseBody);
                        JObject obj = JObject.Parse(responseBody);
                        if (obj["status"].ToString() == "200")
                        {
                            Console.WriteLine($"Find Success");


                            // ใช้ JArray เพื่อเข้าถึง "data"
                            JArray dataArray = (JArray)obj["data"];

                            // แปลง JArray เป็น List ของ Dictionary
                            List<Dictionary<string, object>> objresponseBody = new List<Dictionary<string, object>>();
                            objresponseBody = dataArray.ToObject<List<Dictionary<string, object>>>();
                            JArray dataArraydata = new JArray();
                            List<Dictionary<string, object>> objdata = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            foreach (var dataObject in objresponseBody)
                            {
                                foreach (var key in dataObject.Keys)
                                {
                                    Console.WriteLine($"{key}: {dataObject[key]}");

                                    if (!dtobjdata.Columns.Contains(key))
                                    {
                                        dtobjdata.Columns.Add(key, typeof(object));
                                    }
                                }

                                //dataArraydata = (JArray)dataObject["data"];
                                //objdata = dataArraydata.ToObject<List<Dictionary<string, object>>>();
                                Console.WriteLine("Data Details:");
                                DataRow row = dtobjdata.NewRow();
                                foreach (var key in dataObject.Keys)
                                {
                                    Console.WriteLine($"{key}: {dataObject[key]}");
                                    row[key] = $"{dataObject[key]}"; // ถ้า null ให้ใส่ DBNull.Value 

                                }
                                dtobjdata.Rows.Add(row);
                                Console.WriteLine();
                            }

                        }
                        else
                        {
                            Console.WriteLine($"Can't Find Success");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");



                    }

                    return dtobjdata;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine($" Can't Find Success ");
                    return new DataTable();
                }
            }

        }

        public static async Task<DataTable> Request_Getorderitem(string keyword)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    DataTable dtobjdata = new DataTable();
                    //string apiUrl = "http://192.168.30.14:6426/dih/getorderitem";
                    string apiUrl = BDSender.Properties.Settings.Default.apiUrl + "/dih/getorderitem";
                    //apiUrl = string.Format(apiUrl);
                    // สร้างข้อมูล JSON ที่ต้องการส่ง
                    string jsonContent = $@"{{
                                                ""windowno"": ""{ keyword}""
                                            }}";

                    // สร้าง HttpContent (StringContent)
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        //responseBody = responseBody.Substring(1, responseBody.Length - 2);

                        Console.WriteLine(responseBody);
                        JObject obj = JObject.Parse(responseBody);
                        if (obj["status"].ToString() == "200")
                        {
                            Console.WriteLine($"Find Success");


                            // ใช้ JArray เพื่อเข้าถึง "data"
                            JArray dataArray = (JArray)obj["data"];

                            // แปลง JArray เป็น List ของ Dictionary
                            List<Dictionary<string, object>> objresponseBody = new List<Dictionary<string, object>>();
                            objresponseBody = dataArray.ToObject<List<Dictionary<string, object>>>();
                            JArray dataArraydata = new JArray();
                            List<Dictionary<string, object>> objdata = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            if (objresponseBody != null || objresponseBody.Count > 0)
                            {
                                foreach (var dataObject in objresponseBody)
                                {
                                    foreach (var key in dataObject.Keys)
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        dtobjdata.Columns.Add($"{key}", typeof(object));
                                    }
                                    //dataArraydata = (JArray)dataObject["data"];
                                    //objdata = dataArraydata.ToObject<List<Dictionary<string, object>>>();
                                    Console.WriteLine("Data Details:");
                                    DataRow row = dtobjdata.NewRow();
                                    foreach (var key in dataObject.Keys)
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        row[key] = $"{dataObject[key]}"; // ถ้า null ให้ใส่ DBNull.Value 

                                    }
                                    dtobjdata.Rows.Add(row);
                                    Console.WriteLine();
                                }

                                return dtobjdata;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Can't Find Success");
                            return null;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                        return null;
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine($" Can't Find Success ");
                    return new DataTable();
                }
            }
        }
        public static async Task<DataTable> Request_Getorderdetails(string keyword)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    DataTable dtobjdata = new DataTable();
                    string apiUrl = "http://192.168.30.14:6426/dih/getorderdetails";
                    //string apiUrl = BDSender.Properties.Settings.Default.apiUrl + "/dih/getsemedstock";
                    //apiUrl = string.Format(apiUrl);
                    // สร้างข้อมูล JSON ที่ต้องการส่ง
                    string jsonContent = $@"{{
                                                ""orderno"": {keyword}
                                            }}";

                    // สร้าง HttpContent (StringContent)
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        //responseBody = responseBody.Substring(1, responseBody.Length - 2);
                        
                        Console.WriteLine(responseBody);
                        JObject obj = JObject.Parse(responseBody);
                        if (obj["status"].ToString() == "200")
                        {
                            Console.WriteLine($"Find Success");


                            // ใช้ JArray เพื่อเข้าถึง "data"
                            JArray dataArray = (JArray)obj["data"];

                            // แปลง JArray เป็น List ของ Dictionary
                            List<Dictionary<string, object>> objresponseBody = new List<Dictionary<string, object>>();
                            objresponseBody = dataArray.ToObject<List<Dictionary<string, object>>>();
                            JArray dataArraydata = new JArray();
                            List<Dictionary<string, object>> objdata = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            if(objresponseBody != null || objresponseBody.Count >0)
                            {
                                foreach (var dataObject in objresponseBody)
                                {
                                    foreach (var key in dataObject.Keys)
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        dtobjdata.Columns.Add($"{key}", typeof(object));
                                    }
                                    //dataArraydata = (JArray)dataObject["data"];
                                    //objdata = dataArraydata.ToObject<List<Dictionary<string, object>>>();
                                    Console.WriteLine("Data Details:");
                                    DataRow row = dtobjdata.NewRow();
                                    foreach (var key in dataObject.Keys)
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        row[key] = $"{dataObject[key]}"; // ถ้า null ให้ใส่ DBNull.Value 

                                    }
                                    dtobjdata.Rows.Add(row);
                                    Console.WriteLine();
                                }

                                return dtobjdata;
                            }
                            else
                            {
                                return new DataTable() ;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Can't Find Success");
                            return new DataTable();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                        return new DataTable();
                    }

                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine($" Can't Find Success ");
                    return new DataTable();
                }
            }
        }
        public static async Task<DataTable> Request_Getorder(string keyword)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    DataTable dtobjdata = new DataTable();
                    //string apiUrl = "http://192.168.30.14:6426/dih/getorder";
                    string apiUrl = BDSender.Properties.Settings.Default.apiUrl + "/dih/getorder";
                    //apiUrl = string.Format(apiUrl);
                    // สร้างข้อมูล JSON ที่ต้องการส่ง
                    string jsonContent = $@"{{
                                                ""windowno"": ""{ keyword}""
                                            }}";

                    // สร้าง HttpContent (StringContent)
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        //responseBody = responseBody.Substring(1, responseBody.Length - 2);

                        Console.WriteLine(responseBody);
                        JObject obj = JObject.Parse(responseBody);
                        if (obj["status"].ToString() == "200")
                        {
                            Console.WriteLine($"Find Success");


                            // ใช้ JArray เพื่อเข้าถึง "data"
                            JArray dataArray = (JArray)obj["data"];

                            // แปลง JArray เป็น List ของ Dictionary
                            List<Dictionary<string, object>> objresponseBody = new List<Dictionary<string, object>>();
                            objresponseBody = dataArray.ToObject<List<Dictionary<string, object>>>();
                            JArray dataArraydata = new JArray();
                            List<Dictionary<string, object>> objdata = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            if (objresponseBody != null || objresponseBody.Count > 0)
                            {
                                foreach (var dataObject in objresponseBody)
                                {
                                    foreach (var key in dataObject.Keys)
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        dtobjdata.Columns.Add($"{key}", typeof(object));
                                    }
                                    //dataArraydata = (JArray)dataObject["data"];
                                    //objdata = dataArraydata.ToObject<List<Dictionary<string, object>>>();
                                    Console.WriteLine("Data Details:");
                                    DataRow row = dtobjdata.NewRow();
                                    foreach (var key in dataObject.Keys)
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        row[key] = $"{dataObject[key]}"; // ถ้า null ให้ใส่ DBNull.Value 

                                    }
                                    dtobjdata.Rows.Add(row);
                                    Console.WriteLine();
                                }

                                return dtobjdata;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Can't Find Success");
                            return null;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                        return null;
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine($" Can't Find Success ");
                    return new DataTable();
                }
            }
        }
        public static async Task<DataTable> Request_getoutput(string keyword)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    DataTable dtobjdata = new DataTable();
                    //string apiUrl = "http://192.168.30.14:6426/dih/getorder";
                    string apiUrl = BDSender.Properties.Settings.Default.apiUrl + "/dih/getoutput";
                    //apiUrl = string.Format(apiUrl);
                    // สร้างข้อมูล JSON ที่ต้องการส่ง
                    string jsonContent = $@"{{
                                                ""prescriptionno_sup"": ""{ keyword}""
                                            }}";

                    // สร้าง HttpContent (StringContent)
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        //responseBody = responseBody.Substring(1, responseBody.Length - 2);

                        Console.WriteLine(responseBody);
                        JObject obj = JObject.Parse(responseBody);
                        if (obj["status"].ToString() == "200")
                        {
                            Console.WriteLine($"Find Success");


                            // ใช้ JArray เพื่อเข้าถึง "data"
                            JArray dataArray = (JArray)obj["data"];

                            // แปลง JArray เป็น List ของ Dictionary
                            List<Dictionary<string, object>> objresponseBody = new List<Dictionary<string, object>>();
                            objresponseBody = dataArray.ToObject<List<Dictionary<string, object>>>();
                            JArray dataArraydata = new JArray();
                            List<Dictionary<string, object>> objdata = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            if (objresponseBody != null || objresponseBody.Count > 0)
                            {
                                foreach (var dataObject in objresponseBody)
                                {
                                    foreach (var key in dataObject.Keys)
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        dtobjdata.Columns.Add($"{key}", typeof(object));
                                    }
                                    //dataArraydata = (JArray)dataObject["data"];
                                    //objdata = dataArraydata.ToObject<List<Dictionary<string, object>>>();
                                    Console.WriteLine("Data Details:");
                                    DataRow row = dtobjdata.NewRow();
                                    foreach (var key in dataObject.Keys)
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        row[key] = $"{dataObject[key]}"; // ถ้า null ให้ใส่ DBNull.Value 

                                    }
                                    dtobjdata.Rows.Add(row);
                                    Console.WriteLine();
                                }

                                return dtobjdata;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Can't Find Success");
                            return null;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                        return null;
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine($" Can't Find Success ");
                    return new DataTable();
                }
            }
        }
        public static async Task<DataTable> Request_Getdrug()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    DataTable dtobjdata = new DataTable();
                    //string apiUrl = "http://192.168.30.14:6426/dih/getorder";
                    string apiUrl = BDSender.Properties.Settings.Default.apiUrl + "/setting/devicedrugmanage/drugs/all";
                    //apiUrl = string.Format(apiUrl);
                    // สร้างข้อมูล JSON ที่ต้องการส่ง
                    string jsonContent = "";

                    // สร้าง HttpContent (StringContent)
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        //responseBody = responseBody.Substring(1, responseBody.Length - 2);

                        Console.WriteLine(responseBody);
                        JObject obj = JObject.Parse(responseBody);
                        if (obj["status"].ToString() == "200")
                        {
                            Console.WriteLine($"Find Success");


                            // ใช้ JArray เพื่อเข้าถึง "data"
                            JArray dataArray = (JArray)obj["data"];

                            // แปลง JArray เป็น List ของ Dictionary
                            List<Dictionary<string, object>> objresponseBody = new List<Dictionary<string, object>>();
                            objresponseBody = dataArray.ToObject<List<Dictionary<string, object>>>();
                            JArray dataArraydata = new JArray();
                            List<Dictionary<string, object>> objdata = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            if (objresponseBody != null || objresponseBody.Count > 0)
                            {
                                foreach (var dataObject in objresponseBody)
                                {
                                    foreach (var key in dataObject.Keys)
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        dtobjdata.Columns.Add($"{key}", typeof(object));
                                    }
                                    //dataArraydata = (JArray)dataObject["data"];
                                    //objdata = dataArraydata.ToObject<List<Dictionary<string, object>>>();
                                    Console.WriteLine("Data Details:");
                                    DataRow row = dtobjdata.NewRow();
                                    foreach (var key in dataObject.Keys)
                                    {
                                        Console.WriteLine($"{key}: {dataObject[key]}");
                                        row[key] = $"{dataObject[key]}"; // ถ้า null ให้ใส่ DBNull.Value 

                                    }
                                    dtobjdata.Rows.Add(row);
                                    Console.WriteLine();
                                }

                                return dtobjdata;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Can't Find Success");
                            return null;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                        return null;
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine($" Can't Find Success ");
                    return new DataTable();
                }
            }
        }


        public static async Task<DataTable> RequestUserID(string userid)
        {           
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    DataTable dtobjdata = new DataTable();
                    //string apiUrl = "http://office.gd4.co.th:6426/users/userid";
                    string apiUrl = BDSender.Properties.Settings.Default.apiUrl + "/auth/users/userid";

                    // สร้างข้อมูล JSON ที่ต้องการส่ง
                    string jsonContent = $@"{{
                                                ""userID"": ""{userid}""
                                            }}";

                    // สร้าง HttpContent (StringContent)
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        //responseBody = responseBody.Substring(1, responseBody.Length - 2);
                        Console.WriteLine(responseBody);
                        JObject obj = JObject.Parse(responseBody);
                        if (obj["status"].ToString() == "200")
                        {
                            Console.WriteLine($" Find Success ");                           
                            // ใช้ JArray เพื่อเข้าถึง "data"
                            JArray dataArray = (JArray)obj["data"];

                            // แปลง JArray เป็น List ของ Dictionary
                            List<Dictionary<string, object>> objresponseBody = new List<Dictionary<string, object>>();
                            objresponseBody = dataArray.ToObject<List<Dictionary<string, object>>>();
                            JArray dataArraydata = new JArray();
                            List<Dictionary<string, object>> objdata = new List<Dictionary<string, object>>();
                            // วนลูปแสดงผลข้อมูลแต่ละ Object
                            foreach (var dataObject in objresponseBody)
                            {
                                foreach (var key in dataObject.Keys)
                                {
                                    Console.WriteLine($"{key}: {dataObject[key]}");
                                    dtobjdata.Columns.Add($"{key}", typeof(object));
                                }
                                //dataArraydata = (JArray)dataObject["data"];
                                //objdata = dataArraydata.ToObject<List<Dictionary<string, object>>>();
                                Console.WriteLine("Data Details:");
                                DataRow row = dtobjdata.NewRow();
                                foreach (var key in dataObject.Keys)
                                {
                                    Console.WriteLine($"{key}: {dataObject[key]}");
                                    row[key] = $"{dataObject[key]}"; // ถ้า null ให้ใส่ DBNull.Value 

                                }
                                dtobjdata.Rows.Add(row);
                                Console.WriteLine();
                            }
                        }
                        else
                        {
                            Console.WriteLine($" Can't Find Success ");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                    }

                    return dtobjdata;
                }
                catch (Exception ex)
                {
                    return new DataTable();
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine($" Can't Find Success ");
                }
            }
        }

        public static async Task<bool> update_regisbasket(List<object> ListJson)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string apiUrl = BDSender.Properties.Settings.Default.apiUrl + "/packagemaster/updatepackagemaster/update";
                    apiUrl = string.Format(apiUrl);
                    // สร้างข้อมูล JSON ที่ต้องการส่ง
                    var jsonString = "";
                    jsonString = System.Text.Json.JsonSerializer.Serialize(ListJson, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    //Console.WriteLine(jsonString);
                    if (jsonString.Length > 0)
                    {
                        // สร้าง HttpContent (StringContent)
                        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                        //Console.WriteLine(content);
                        HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            //responseBody = responseBody.Substring(1, responseBody.Length - 2);
                            //Console.WriteLine(responseBody);
                            JObject obj = JObject.Parse(responseBody);
                            if (obj["status"].ToString() == "200")
                            {
                                //clsPackagemaster.obprescription = obj["data"].ToObject<List<object>>();
                                Console.WriteLine($" Find Success ");
                            }
                            else
                            {
                                Console.WriteLine($" Can't Find Success ");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.ToString());
                    return false;
                }
                finally
                {

                }
            }
        }
    }
}
