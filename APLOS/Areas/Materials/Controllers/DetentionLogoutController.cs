using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.MaterialManagement.Material;
using Aplos.Properties;
using Library.Security.Core;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using System.IO;
using System.Data;

namespace Aplos.Areas.Materials.Controllers
{
    public class DetentionLogoutController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        DetentionLogoutService dl = new DetentionLogoutService();
        DetentionLogService dls = new DetentionLogService();

        public DetentionLogoutController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        [Authorize, AllowAnonymous]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult GetDetentionResponsible(string detentionTypeId)
        {

            return Json(dl.GetDetentionResponsible(detentionTypeId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, AllowAnonymous]
        public JsonResult getDetentionLogGrid()
        {
            return Json(dl.getDetentionLogGrid(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, AllowAnonymous]
        public JsonResult getByWhom()
        {
            return Json(dl.getByWhom(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, AllowAnonymous]
        public JsonResult getDetentionLogResponsiblePerson(string detentionLogId)
        {
            return Json(dl.getDetentionLogResponsiblePerson(detentionLogId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DetentionLogRespPerDelete(string Id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("update TRN.DetentionLogResponsiblePerson set  isActive = 0  where Id ='" + Id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult Save(Dictionary<string, object> data, string detentionLogId)
        {

            try
            {
                return Json(new { Error = false, Data = dl.Update(data, detentionLogId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public JsonResult saveDtentionLogResPerson(List<Dictionary<string, object>> data, string detentionLogId)
        {

            try
            {
                return Json(new { Error = false, Data = dls.saveDtentionLogResPerson(data, detentionLogId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public JsonResult saveDtentionLogout(Dictionary<string, object> data, string detentionLogId, string logouttime)
        {

            try
            {
                return Json(new { Error = "No", Data = dl.saveDtentionLogout(data, detentionLogId, logouttime), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetClosedDetentionGridReport(string from, string to, string departmentId, string detentionTypeId)
        {

            try
            {
                
                return Json(dl.GetClosedDetentionGridReport(from, to, departmentId, detentionTypeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetPendingDetentionGridView(string from, string to, string departmentId, string detentionTypeId)
        {

            try
            {

                return Json(dl.GetPendingDetentionGridView(from, to, departmentId, detentionTypeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult XlsGetClosedDetentionReport(string from, string to, string departmentId, string detentionTypeId)
        {
            try
            {
                
                string fileName = "";
                fileName = ClosedDetentionExcelView(from, to, departmentId, detentionTypeId, "ClosedDetentionReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        public string ClosedDetentionExcelView(string from, string to, string departmentId, string detentionTypeId, string SheetName)
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
                workbook.Worksheets[0].Name = "Closed Machines";
                sheet = workbook.Worksheets[0];
                DataTable data;
               dl.GetClosedDetentionExcelReport(from, to, departmentId, detentionTypeId, out data);

                int ROW = 6; int COL = 1;

                #region Columns
                sheet[ROW, COL].Text = "DetentenLog Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDLId = COL;
                COL++;

                sheet[ROW, COL].Text = "WorkCenter";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColWorkCenter = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDepartment = COL;
                COL++;

                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColProcess = COL;
                COL++;

                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRP = COL;
                COL++;

                sheet[ROW, COL].Text = "Responsible Person No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRPNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Detention Type";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDetentionType = COL;
                COL++;

                sheet[ROW, COL].Text = "Issue By No.";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColIssueByNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Login Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColLoginDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Login Time";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColLoginTime = COL;
                COL++;

                sheet[ROW, COL].Text = "Logout Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColLogoutDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Logout Time";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColLogoutTime = COL;
                COL++;

                sheet[ROW, COL].Text = "Duration";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDuration = COL;
                COL++;

                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRemarks = COL;
                COL++;
                #endregion Columns

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
                    sheet[ROW, ColDLId].Text = data.Rows[i]["Id"].ToString();
                    sheet[ROW, ColWorkCenter].Text = data.Rows[i]["WorkCenter"].ToString();
                    sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                    sheet[ROW, ColRP].Text = data.Rows[i]["ResponsiblePersonName"].ToString();
                    sheet[ROW, ColRPNo].Text = data.Rows[i]["ContactNo"].ToString();
                    sheet[ROW, ColDetentionType].Text = data.Rows[i]["DetentionType"].ToString();
                    sheet[ROW, ColIssueByNo].Text = data.Rows[i]["IssueByNo"].ToString();
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColLoginDate].Text = data.Rows[i]["AddedDate"].ToString();
                    sheet[ROW, ColLoginTime].Text = data.Rows[i]["AddedTime"].ToString();
                    sheet[ROW, ColLogoutDate].Text = data.Rows[i]["LogoutDate"].ToString();
                    sheet[ROW, ColLogoutTime].Text = data.Rows[i]["LogoutTime"].ToString();
                    sheet[ROW, ColDuration].Number = clsStaticInfo.dbl(data.Rows[i]["Duration"].ToString());

                    ROW++;
                }

                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    sheet["A" + startRow.ToString()].FreezePanes();

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    ReportUtility reportUtility = new ReportUtility();
                    reportUtility.PlantHeader(ref sheet, endCol, "Closed Machines", identity.PlantId);
                    reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                    sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.IsGridLinesVisible = false;
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

        [Authorize, HttpGet]
        public ActionResult XlsGetPendingDetentionView(string from, string to, string departmentId, string detentionTypeId)
        {
            try
            {
                string fileName = "";
                fileName = PendingDetentionExcelView(from, to, departmentId, detentionTypeId, "Pending Machine");

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        
        private string PendingDetentionExcelView(string from, string to, string departmentId, string detentionTypeId, string SheetName)
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
                workbook.Worksheets[0].Name = "Closed Machines";
                sheet = workbook.Worksheets[0];
                DataTable data;
                dl.GetPendingDetentionExcelView(from, to, departmentId, detentionTypeId, out data);

                int ROW = 6; int COL = 1;

                #region Columns
                sheet[ROW, COL].Text = "DetentenLog Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDLId = COL;
                COL++;

                sheet[ROW, COL].Text = "WorkCenter";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColWorkCenter = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDepartment = COL;
                COL++;

                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColProcess = COL;
                COL++;

                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRP = COL;
                COL++;

                sheet[ROW, COL].Text = "Responsible Person No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRPNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Detention Type";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDetentionType = COL;
                COL++;

                sheet[ROW, COL].Text = "Issue By No.";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColIssueByNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Login Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColLoginDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Login Time";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColLoginTime = COL;
                COL++;

                
                sheet[ROW, COL].Text = "Duration";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDuration = COL;
                COL++;

                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRemarks = COL;
                COL++;
                #endregion Columns

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
                    sheet[ROW, ColDLId].Text = data.Rows[i]["Id"].ToString();
                    sheet[ROW, ColWorkCenter].Text = data.Rows[i]["WorkCenter"].ToString();
                    sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                    sheet[ROW, ColRP].Text = data.Rows[i]["ResponsiblePersonName"].ToString();
                    sheet[ROW, ColRPNo].Text = data.Rows[i]["ContactNo"].ToString();
                    sheet[ROW, ColDetentionType].Text = data.Rows[i]["DetentionType"].ToString();
                    sheet[ROW, ColIssueByNo].Text = data.Rows[i]["IssueByNo"].ToString();
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColLoginDate].Text = data.Rows[i]["AddedDate"].ToString();
                    sheet[ROW, ColLoginTime].Text = data.Rows[i]["AddedTime"].ToString();
                    
                    //sheet[ROW, ColDuration].Number = clsStaticInfo.dbl(data.Rows[i]["Duration"].ToString());

                    ROW++;
                }

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Closed Machines", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
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


    }
}