#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Materials.Controllers
{
    public class UtilityTransactionReportController : BaseController
    {


        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public UtilityTransactionReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ReportView()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            try
            {
                var sql = @"select Id,FORMAT(Date,'dd-MMM-yyyy') [Date],Quantity,Remarks from UtilityTransaction";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetUtilityTransactionReport(Dictionary<string, string> parameters)
        {
            try
            {
                string fileName = "";
                fileName = UtilityTransactionReport(parameters, "Utility Transaction Report");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string UtilityTransactionReport(Dictionary<string, string> parameters, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "UtilityTransactionReport";
                sheet = workbook.Worksheets[0];
                DataTable data;
                UtilityTransactionReportSQL(parameters, out data);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Sub Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSubCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Quantity";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColQuantity = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRemarks = COL;
                
                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColDate].Text = data.Rows[i]["Date"].ToString();
                    sheet[ROW, ColCategory].Text = data.Rows[i]["Category"].ToString();
                    sheet[ROW, ColSubCategory].Text = data.Rows[i]["SubCategory"].ToString();
                    sheet[ROW, ColQuantity].Text = data.Rows[i]["Quantity"].ToString();
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Utility Transaction Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;




                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UtilityTransactionReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            {


                string strSQL = @"select UT.Id,FORMAT(UT.Date,'dd-MMM-yyyy') [Date],UM.UtilityCategory Category,UMA.UtilitySubCategory SubCategory 
							                ,UT.CategoryId,UT.SubCategoryId,UT.Quantity,UT.Remarks
							                from UtilityTransaction UT
							                left join UtilityMaster UM on UM.Id=UT.CategoryId
							                left join UtilityMaster UMA on UMA.Id=UT.SubCategoryId
										
                                            where UT.Date in(" + parameters["Date"] + @")
                                            AND UT.Quantity in(" + parameters["Quantity"] + @")
                                            AND UT.Remarks in(" + parameters["Remarks"] + @")";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpGet, Authorize]
        public ActionResult DownloadUsingFullPath(string FullPath, string fileName)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(FullPath);
                try
                {
                    System.IO.File.Delete(FullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }


    }
}