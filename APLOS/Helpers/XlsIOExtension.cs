using HtmlAgilityPack;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System;
using Syncfusion.XlsIO.Implementation;
using System.IO;

namespace Aplos.Helpers
{
    public static class XlsIOExtension
    {
        public static ExcelResult SaveAsActionResult(this ExcelEngine _engine, IWorkbook _workbook, string filename, HttpResponse response)
        {
            ExcelHttpContentType contentType = ExcelHttpContentType.Excel2007;
            if (_workbook.Version == ExcelVersion.Excel2007)
                contentType = ExcelHttpContentType.Excel2007;
            else if (_workbook.Version == ExcelVersion.Excel97to2003)
                contentType = ExcelHttpContentType.Excel2000;
            return new ExcelResult(_engine, _workbook, filename, response, ExcelDownloadType.PromptDialog, contentType);
        }

        public static ExcelResult SaveAsActionResult(this ExcelEngine _engine, IWorkbook _workbook, string filename, HttpResponse response, ExcelDownloadType DownloadType)
        {
            ExcelHttpContentType contentType = ExcelHttpContentType.Excel2007;
            if (_workbook.Version == ExcelVersion.Excel2007)
                contentType = ExcelHttpContentType.Excel2007;
            else if (_workbook.Version == ExcelVersion.Excel97to2003)
                contentType = ExcelHttpContentType.Excel2000;
            return new ExcelResult(_engine, _workbook, filename, response, DownloadType, contentType);
        }

        public static ExcelResult SaveAsActionResult(this ExcelEngine _engine, IWorkbook _workbook, string filename, HttpResponse response, ExcelHttpContentType contentType)
        {
            return new ExcelResult(_engine, _workbook, filename, response, ExcelDownloadType.PromptDialog, contentType);
        }

        public static ExcelResult SaveAsActionResult(this ExcelEngine _engine, IWorkbook _workbook, string filename, HttpResponse response, ExcelDownloadType DownloadType, ExcelHttpContentType contentType)
        {
            return new ExcelResult(_engine, _workbook, filename, response, DownloadType, contentType);
        }

        public static ExcelResult SaveAsActionResult(this ExcelEngine _engine, IWorkbook _workbook, string filename, ExcelSaveType saveType, HttpResponse response, ExcelDownloadType DownloadType, ExcelHttpContentType contentType)
        {
            return new ExcelResult(_engine, _workbook, filename, response, DownloadType, contentType);
        }

        public static ExcelResult SaveAsActionResult(this ExcelEngine _engine, IWorkbook _workbook, string filename, string separator, HttpResponse response, ExcelDownloadType DownloadType, ExcelHttpContentType contentType)
        {
            return new ExcelResult(_engine, _workbook, filename, separator, response, DownloadType, contentType);
        }


        public static string ExcelHtmlTable(IWorksheet sheet, string Directory)
        {
            try
            {
                string filePath = Path.Combine(Directory, System.DateTime.Now.Ticks.ToString());

                sheet.SaveAsHtml(filePath, HtmlSaveOptions.Default);

                string _html = System.IO.File.ReadAllText(filePath, System.Text.Encoding.Unicode);
                System.IO.File.Delete(filePath);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(_html);

                var styles = doc.DocumentNode.SelectNodes("//style").ToList();
                var tds = doc.DocumentNode.SelectNodes("//td").ToList();




                string[] stringSeparators = new string[] { "\r\n" };
                string[] lines = styles[0].InnerText.Split(stringSeparators, StringSplitOptions.None);
                Dictionary<string, string> dicstyles = new Dictionary<string, string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("{") == false)
                        continue;

                    for (int L = 0; L < lines[i].Length; L++)
                    {
                        if (lines[i][L] == '{')
                        {
                            string k = lines[i].Substring(0, L).Replace(".", "");
                            string v = lines[i].Substring(L + 1, lines[i].Length - 1 - L - 1);

                            dicstyles.Add(k, v);
                        }
                    }
                }

                for (int i = 0; i < tds.Count; i++)
                {

                    string styleValue = "";
                    string attrclassname = "";



                    for (int j = 0; j < tds[i].Attributes.Count; j++)
                    {
                        if (tds[i].Attributes[j].Name == "class")
                            attrclassname = tds[i].Attributes[j].Value;

                        if (tds[i].Attributes[j].Name == "class")
                            tds[i].Attributes[j].Remove();
                    }
                    for (int j = 0; j < tds[i].Attributes.Count; j++)
                    {
                        if (tds[i].Attributes[j].Name == "style")
                            styleValue = tds[i].Attributes[j].Value;

                        if (tds[i].Attributes[j].Name == "style")
                            tds[i].Attributes[j].Remove();
                    }


                    if (attrclassname != "")
                    {
                        if (dicstyles.ContainsKey(attrclassname))
                        {
                            tds[i].Attributes.Add("style", dicstyles[attrclassname] + styleValue);

                        }

                    }
                }

                var body = doc.DocumentNode.SelectNodes("//body").ToList();


                return body[0].InnerHtml;
            }
            catch (Exception)
            {

                return "";
            }

        }
    }
}