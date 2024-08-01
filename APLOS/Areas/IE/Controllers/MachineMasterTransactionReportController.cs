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

namespace Aplos.Areas.IE.Controllers
{
    public class MachineMasterTransactionReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public MachineMasterTransactionReportController(ISqlRepository R)
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
        public ActionResult getFilters(string fromDate, string toDate)
        {
            try
            {
                var sql = @"select format(MMT.FromTime,'dd-MMM-yyyy') [From],format(MMT.ToTime,'dd-MMM-yyyy')[To],P.Id ProcessId,P.UserName Process
                                            ,E.Id EntityId,E.UserName Entity,D.Id DepartmentId,D.UserName Department,DM.Id DetentionId
											,DT.UserName DetentionType,SD.SystemID ShiftId,SD.UserName Shift,EI.SystemId ResponsiblePersonId,EI.EmployeeName ResponsiblePerson
											,DMM.DetentionCategory,DMM.DetentionSubCategory,0 as Avoidable,0 as Criticality
											from MachineMasterTransaction MMT
											left join ORG.Entity E on E.Id=MMT.EntityId
											left join HKP.Process P on P.Id=MMT.ProcessId
											left join ORG.Department D on D.Id=MMT.DepartmentId
											left join DetentionMaster DM on DM.Id=MMT.DetentionId
											left join ShiftDefination SD on SD.SystemID=MMT.ShiftId
											left join EmployeeInformation EI on EI.SystemId=MMT.ResponsiblePersonId
											left join DetentionMaster DMM on DMM.Id=MMT.DetentionId
                                            left join [HKP].[DetentionType] DT on DT.Id = DM.DetentionTypeId
                                            Where MMT.FromTime between '"+fromDate+@"' and '"+toDate+ @"' and MMT.ToTime between'" + fromDate + @"' and '" + toDate + @"'";
                
                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata; 
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetMachineMasterTransactionReport(Dictionary<string, string> parameters)
        {
            try
            {
                string fileName = "";
                fileName = MachineMasterTransactionReport(parameters, "MachineMasterTransactionReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string MachineMasterTransactionReport(Dictionary<string, string> parameters, string SheetName)
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
                workbook.Worksheets[0].Name = "MachineMasterTransactionReports";
                sheet = workbook.Worksheets[0];
                DataTable data;
                MachineMasterTransactionReportSQL(parameters, out data);

                int ROW = 6; int COL = 1;
                #region columns
                sheet[ROW, COL].Text = "From";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColFrom = COL;
                COL++;

                sheet[ROW, COL].Text = "To";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColTo = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColProcess = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDepartment = COL;
                COL++;

                sheet[ROW, COL].Text = "Detention Type";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDetentionType = COL;
                COL++;

                sheet[ROW, COL].Text = "Shift";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColShift = COL;
                COL++;

                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 22;
                int ColResponsiblePerson = COL;
                COL++;

                sheet[ROW, COL].Text = "Detention Category";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColDetentionCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Detention Sub Category";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColDetentionSubCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Avoidable";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColAvoidable = COL;
                COL++;
                
                sheet[ROW, COL].Text = "Critically";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCritically = COL;

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
                    sheet[ROW, ColFrom].Text = clsStaticInfo.GetDate(data.Rows[i]["From"].ToString());
                    sheet[ROW, ColTo].Text = clsStaticInfo.GetDate(data.Rows[i]["To"].ToString());
                    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColDetentionType].Text = data.Rows[i]["DetentionType"].ToString();
                    sheet[ROW, ColShift].Text = data.Rows[i]["Shift"].ToString();
                    sheet[ROW, ColResponsiblePerson].Text = data.Rows[i]["ResponsiblePerson"].ToString();
                    sheet[ROW, ColDetentionCategory].Text = data.Rows[i]["DetentionCategory"].ToString();
                    sheet[ROW, ColDetentionSubCategory].Text = data.Rows[i]["DetentionSubCategory"].ToString();
                    sheet[ROW, ColAvoidable].Text = data.Rows[i]["Avoidable"].ToString();
                    sheet[ROW, ColCritically].Text = data.Rows[i]["Criticality"].ToString();

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
                reportUtility.PlantHeader(ref sheet, endCol, "Machine Master Transaction Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

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

        public void MachineMasterTransactionReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            { 
                string strSQL = @"select format(MMT.FromTime,'dd-MMM-yyyy') [From],format(MMT.ToTime,'dd-MMM-yyyy')[To],P.UserName Process,E.UserName Entity
											,D.UserName Department ,SD.UserName Shift,EI.EmployeeName ResponsiblePerson
											,DMM.DetentionCategory,DMM.DetentionSubCategory,0 as Avoidable,0 as Criticality,DT.UserName DetentionType
											from MachineMasterTransaction MMT
											left join ORG.Entity E on E.Id=MMT.EntityId
											left join HKP.Process P on P.Id=MMT.ProcessId
											left join ORG.Department D on D.Id=MMT.DepartmentId
											left join DetentionMaster DM on DM.Id=MMT.DetentionId
											left join ShiftDefination SD on SD.SystemID=MMT.ShiftId
											left join EmployeeInformation EI on EI.SystemId=MMT.ResponsiblePersonId
											left join DetentionMaster DMM on DMM.Id=MMT.DetentionId
										    left join [HKP].[DetentionType] DT on DT.Id = DM.DetentionTypeId

                            where MMT.ProcessId in(" + parameters["ProcessId"] + @")
                            AND MMT.DepartmentId in(" + parameters["DepartmentId"] + @")
                            AND MMT.DetentionId in(" + parameters["DetentionId"] + @")
                            AND MMT.ShiftId in(" + parameters["ShiftId"] + @")
                            AND MMT.ResponsiblePersonId in(" + parameters["ResponsiblePersonId"] + @")";
                 
                var jsondata = Json(data = _sqlRepository.GetDataTable(strSQL), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue; 
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