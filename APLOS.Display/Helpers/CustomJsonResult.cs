using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Web.Mvc;
using Library.Core;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Aplos.Helpers
{
    public class CustomJsonResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context.IsNull())
                throw new ArgumentNullException("context");
            var response = context.HttpContext.Response;
            response.ContentType = string.IsNullOrEmpty(ContentType) ? "application/json" : ContentType;
            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (Data.IsNull()) return;
            // Using Json.NET serializer
            var isoConvert = new IsoDateTimeConverter { DateTimeFormat = Util.ConvertedDateFormat };
            response.Write(JsonConvert.SerializeObject(Data, isoConvert));
        }

        public static List<Dictionary<string, object>> DataTableToJson(DataTable dt)
        {
            List<Dictionary<string, object>> dictionaries = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> dictionary = Enumerable.Range(0, dt.Columns.Count)
                    .ToDictionary(i => dt.Columns[i].ColumnName, i => row.ItemArray[i]);



                dictionaries.Add(dictionary);
            }
            return dictionaries;
        }
        public static DataTable ToDataTable(string jsonString)
        {
            DataTable dt = new DataTable();
            string[] jsonStringArray = Regex.Split(jsonString.Replace("[", "").Replace("]", ""), "},{");

            List<string> ColumnsName = new List<string>();

            string ColumnsNameString = "";

            foreach (string jSA in jsonStringArray)
            {
                string[] jsonStringData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                foreach (string ColumnsNameData in jsonStringData)
                {
                    try
                    {

                        string[] pair = ColumnsNameData.Split(new[] { "\":" }, StringSplitOptions.None);

                        if (pair != null)
                        {

                            if (pair.Length > 0)
                            {
                                ColumnsNameString = pair[0].Replace("\"", "");
                                if (!ColumnsName.Contains(ColumnsNameString))
                                {
                                    ColumnsName.Add(ColumnsNameString);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error Parsing Column Name : {0}", ColumnsNameData));
                    }
                }
                break;
            }
            foreach (string AddColumnName in ColumnsName)
            {
                dt.Columns.Add(AddColumnName);
            }



            foreach (string jSA in jsonStringArray)
            {

                string[] RowData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                DataRow nr = dt.NewRow();
                foreach (string rowData in RowData)
                {
                    try
                    {
                        string[] pair = rowData.Split(new[] { "\":" }, StringSplitOptions.None);

                        if (pair != null)
                        {

                            if (pair.Length == 2)
                            {
                                ColumnsNameString = pair[0].Replace("\"", "");
                                nr[ColumnsNameString] = pair[1].Replace("\"", ""); ;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                dt.Rows.Add(nr);
            }
            return dt;
        }
    }
}