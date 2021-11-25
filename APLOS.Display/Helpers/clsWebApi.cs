using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WebApi
{
    public delegate void onStartTransaction();
    public delegate void onEndTransaction(string message, bool success);

    public class clsWebApi
    {

        /*
         This library has been created by 
         TAREK TALUKDER
         Mail:tarektalukder@gmail.com
        */

        private bool RunningProcess = false;

        public event onStartTransaction StartTransaction;
        public event onEndTransaction EndTransaction;

        public int ConnectionTimeout { get; set; } = int.MaxValue;//default 30000 (30 seconds)
        private static string baseurl = @"about:blank";
        public clsWebApi(string BaseURL)
        {
            baseurl = BaseURL;
        }

        public List<T> GetJson<T>(string JsonData)
        {

            System.Collections.Generic.List<T> _list = new List<T>();
            try
            {


                if (JsonData != "[]")
                {
                    if (JsonData.StartsWith("["))
                        _list = (List<T>)Newtonsoft.Json.JsonConvert.DeserializeObject(JsonData, typeof(List<T>));
                   
                   
                }



            }
            catch (Exception ex)
            {


            }
            finally
            {

            }


            return _list;
        }

        public List<T> GetMessage<T>(string ApiName, string Parameters = "")
        {

            if (RunningProcess == true)
                return null;
            RunningProcess = true;
            StartTransaction?.Invoke();


            ReturnType returnType = null;
            System.Collections.Generic.List<T> _list = new List<T>();
            try
            {
                returnType = new ReturnType();

                Parameters = Parameters != "" ? "?" + Parameters : "";
                Uri uri = new Uri(baseurl + ApiName + Parameters);
                using (WebClientForAPI wc = new WebClientForAPI())
                {

                    try
                    {
                        var json = wc.DownloadString(uri);
                        

                        if (json.ToString() != "[]")
                        {

                            _list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(json.ToString());
                            returnType.Status = true;
                            returnType.Message = "Success";
                        }
                        else
                        {
                            returnType.Status = false;
                            returnType.Message = "No data found!!!";
                        }

                    }
                    catch (Exception ex)
                    {
                        returnType.Status = false;
                        returnType.Message = ex.Message;
                    }
                }


            }
            catch (Exception ex)
            {
                returnType.Status = false;
                returnType.Message = ex.Message;

            }
            finally
            {
                RunningProcess = false;
                EndTransaction?.Invoke(returnType.Message, returnType.Status);

            }


            return _list;
        }
        public async Task<List<T>> GetMessageNested<T>(string ApiName, string Parameters = "")
        {
            if (RunningProcess == true)
                return null;
            RunningProcess = true;

            StartTransaction?.Invoke();

            ReturnType returnType = null;
            List<T> _list = new List<T>();
            try
            {
                returnType = new ReturnType();


                Parameters = Parameters != "" ? "?" + Parameters : "";
                Uri uri = new Uri(baseurl + ApiName + Parameters);

                using (WebClientForAPI wc = new WebClientForAPI())
                {
                    try
                    {
                        var json = await wc.DownloadStringTaskAsync(uri);


                        if (json != "[]")
                        {
                            _list = new List<T> { JsonConvert.DeserializeObject<T>(json) };

                            returnType.Status = true;
                            returnType.Message = "Success";


                        }
                        else
                        {
                            returnType.Status = false;
                            returnType.Message = "No data found!!!";
                        }

                    }
                    catch (Exception ex)
                    {
                        returnType.Status = false;
                        returnType.Message = ex.Message;
                    }
                }


            }
            catch (Exception ex)
            {

                returnType.Status = false;
                returnType.Message = ex.Message;
            }
            finally
            {
                RunningProcess = false;
                EndTransaction?.Invoke(returnType.Message, returnType.Status);

            }
            return _list;

        }

        public async void PostMessage<T>(string ApiName, T Data, string Parameters = "")
        {
            if (RunningProcess == true)
                return;
            RunningProcess = true;

            StartTransaction?.Invoke();

            bool Success = true;
            string resultContent = "";

            var bodyMessage = Newtonsoft.Json.JsonConvert.SerializeObject(Data);
            try
            {
                Parameters = Parameters != "" ? "?" + Parameters : "";
                ApiName = ApiName + Parameters;


                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseurl);
                    var content = new StringContent(bodyMessage, Encoding.UTF8, "application/json");
                    var result = await client.PostAsync(ApiName, content);
                    resultContent = await result.Content.ReadAsStringAsync();

                }
            }
            catch (Exception ex)
            {
                resultContent = ex.Message;
                Success = false;
            }
            finally
            {
                RunningProcess = false;
                EndTransaction?.Invoke(resultContent, Success);

            }

        }
        public  List<T> PostMessageWithResponse<T>(string ApiName, T Data, string Parameters = "")
        {
            if (RunningProcess == true)
                return null;
            RunningProcess = true;

            StartTransaction?.Invoke();

            bool Success = true;
            string resultContent = "";

            var bodyMessage = Newtonsoft.Json.JsonConvert.SerializeObject(Data);
            try
            {
                Parameters = Parameters != "" ? "?" + Parameters : "";
                ApiName = ApiName + Parameters;


                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseurl);
                    var content = new StringContent(bodyMessage, Encoding.UTF8, "application/json");
                    var result =  client.PostAsync(ApiName, content).Wait(3000);
                  
                    return GetJson<T>(resultContent.ToString());
                }
            }
            catch (Exception ex)
            {
                resultContent = ex.Message;
                Success = false;
            }
            finally
            {
                RunningProcess = false;
                EndTransaction?.Invoke(resultContent, Success);

            }

            return null;
        }
        public async Task<List<T>> PostObjectData<T>(string ApiName, object Data, string Parameters = "")
        {
            if (RunningProcess == true)
                return null;
            RunningProcess = true;

            StartTransaction?.Invoke();

            bool Success = true;
            string resultContent = "";

            var bodyMessage = Newtonsoft.Json.JsonConvert.SerializeObject(Data);
            try
            {
                Parameters = Parameters != "" ? "?" + Parameters : "";
                ApiName = ApiName + Parameters;


                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseurl);
                    var content = new StringContent(bodyMessage, Encoding.UTF8, "application/json");
                    var result = await client.PostAsync(ApiName, content);
                    resultContent = await result.Content.ReadAsStringAsync();

                    return GetJson<T>(resultContent.ToString());
                }
            }
            catch (Exception ex)
            {
                resultContent = ex.Message;
                Success = false;
            }
            finally
            {
                RunningProcess = false;
                EndTransaction?.Invoke(resultContent, Success);

            }

            return null;
        }

        public async void PutMessage<T>(string ApiName, T Data, string Parameters = "")
        {
            if (RunningProcess == true)
                return;
            RunningProcess = true;

            StartTransaction?.Invoke();

            bool Success = true;
            string resultContent = "";

            var bodyMessage = Newtonsoft.Json.JsonConvert.SerializeObject(Data);
            try
            {
                Parameters = Parameters != "" ? "?" + Parameters : "";
                ApiName = ApiName + Parameters;

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseurl);
                    var content = new StringContent(bodyMessage, Encoding.UTF8, "application/json");
                    var result = await client.PutAsync(ApiName, content);
                    resultContent = await result.Content.ReadAsStringAsync();

                }
            }
            catch (Exception ex)
            {
                resultContent = ex.Message;
                Success = false;
            }
            finally
            {
                RunningProcess = false;
                EndTransaction?.Invoke(resultContent, Success);

            }

        }

        public async void DeleteMessage<T>(string ApiName, string Parameters = "")
        {
            if (RunningProcess == true)
                return;
            RunningProcess = true;

            StartTransaction?.Invoke();

            bool Success = true;
            string resultContent = "";


            try
            {
                Parameters = Parameters != "" ? "?" + Parameters : "";
                ApiName = ApiName + Parameters;

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseurl);
                    var result = await client.DeleteAsync(ApiName);
                    resultContent = await result.Content.ReadAsStringAsync();

                }
            }
            catch (Exception ex)
            {
                resultContent = ex.Message;
                Success = false;
            }
            finally
            {
                RunningProcess = false;
                EndTransaction?.Invoke(resultContent, Success);

            }

        }

        public async void ExecuteURL(string URL = "")
        {
            if (RunningProcess == true)
                return;
            RunningProcess = true;
            StartTransaction?.Invoke();

            URL = URL.ToString() == "" ? baseurl : URL;

            bool Success = true;
            string resultContent = "";
            DataSet dsServerResponse = new DataSet();
            try
            {

                HttpWebRequest request = WebRequest.Create(URL) as HttpWebRequest;
                request.Timeout = ConnectionTimeout;
                HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;

                resultContent = response.StatusDescription;

            }
            catch (Exception ex)
            {
                resultContent = ex.Message;
                Success = false;
            }
            finally
            {
                RunningProcess = false;
                EndTransaction?.Invoke(resultContent, Success);

            }
        }
        public string TriggerAPILink(string requestUrl, out DataSet dsServerResponse)
        {
            dsServerResponse = new DataSet();
            string Response = "Unable to update data.";
            try
            {
                //coded by tarek talukder



                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                request.Timeout = ConnectionTimeout;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;


                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());

                try
                {
                    XmlReader xmlReader = new XmlNodeReader(xmlDoc);
                    dsServerResponse = new DataSet();
                    dsServerResponse.ReadXml(xmlReader);
                }
                catch { }


                return xmlDoc.InnerText;


            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }

        public DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable("TEMP");
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
    [Serializable]
    public class ReturnType
    {
        public bool Status { get; set; } = false;
        public string Message { get; set; } = string.Empty;
    }

    public class WebClientForAPI : WebClient, IDisposable
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }


        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest wr = base.GetWebRequest(address);
            wr.Timeout = int.MaxValue; // timeout in milliseconds (ms)
            return wr;
        }
    }
}
