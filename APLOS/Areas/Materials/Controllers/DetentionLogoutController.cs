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
        public ActionResult GetDetentionResponsible(string detentionId)
        {

            return Json(dl.GetDetentionResponsible(detentionId), JsonRequestBehavior.AllowGet);
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
                var workbook = ClosedDetentionExcelView(from, to, departmentId, detentionTypeId);

                var strFileName = DateTime.Now.ToString("yy-MMM-dd") + " " + "ClosedDetentionReport.xlsx";
                //string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(strFileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        private IWorkbook ClosedDetentionExcelView(string from, string to, string departmentId, string detentionTypeId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = dl.GetClosedDetentionExcelReport(from, to, departmentId, detentionTypeId);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Closed Machine";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers


            report.SetHeaderText(ref sheet, ROW, COL, "Id", 6, ExcelHAlign.HAlignCenter);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Work Center", 12, ExcelHAlign.HAlignCenter);
            int ColWorkCenter = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 20, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Machine", 20, ExcelHAlign.HAlignCenter);
            int ColMachine = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 20, ExcelHAlign.HAlignCenter);
            int ColResponsiblePerson = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person No", 20, ExcelHAlign.HAlignCenter);
            int ColResPerNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Detention Type", 12, ExcelHAlign.HAlignCenter);
            int ColDetentionType = COL;
            COL++;
           
            report.SetHeaderText(ref sheet, ROW, COL, "Issue By No.", 20, ExcelHAlign.HAlignCenter);
            int ColIssueByNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Login Date", 12, ExcelHAlign.HAlignCenter);
            int ColLogDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Login Time", 12, ExcelHAlign.HAlignCenter);
            int ColLogTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Logout Date", 12, ExcelHAlign.HAlignCenter);
            int ColLogoutDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Logout Time", 12, ExcelHAlign.HAlignCenter);
            int ColLogoutTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Duration", 10, ExcelHAlign.HAlignCenter);
            int ColDuration = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 30, ExcelHAlign.HAlignCenter);
            int ColRemarks = COL;
            COL++;


            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColId].Text = data.Rows[i]["Id"].ToString();
                sheet[ROW, ColWorkCenter].Text = data.Rows[i]["WorkCenter"].ToString();
                sheet[ROW, ColMachine].Text = data.Rows[i]["MachineMaster"].ToString();
                sheet[ROW, ColResponsiblePerson].Text = data.Rows[i]["ResponsiblePersonName"].ToString();
                sheet[ROW, ColResPerNo].Text = data.Rows[i]["ContactNo"].ToString();
                sheet[ROW, ColDetentionType].Text = data.Rows[i]["DetentionType"].ToString();
                sheet[ROW, ColIssueByNo].Text = data.Rows[i]["IssueByNo"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColLogDate].Text = data.Rows[i]["AddedDate"].ToString();
                sheet[ROW, ColLogTime].Text = data.Rows[i]["AddedTime"].ToString();
                sheet[ROW, ColLogoutDate].Text = data.Rows[i]["LogoutDate"].ToString();
                sheet[ROW, ColLogoutTime].Text = data.Rows[i]["LogoutTime"].ToString();
                sheet[ROW, ColDuration].Number = clsStaticInfo.dbl(data.Rows[i]["Duration"].ToString());
                
                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, startRow, endCol];


            sheet.Range[startRow - 1, 1, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyHeader(ref sheet, endCol, "PendingForAllocation", identity.CompanyId);
            //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        [Authorize, HttpGet]
        public ActionResult XlsGetPendingDetentionView(string from, string to, string departmentId, string detentionTypeId)
        {
            try
            {
                var workbook = PendingDetentionExcelView(from, to, departmentId, detentionTypeId);

                var strFileName = DateTime.Now.ToString("yy-MMM-dd") + " " + "PendingDetentionReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        private IWorkbook PendingDetentionExcelView(string from, string to, string departmentId, string detentionTypeId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = dl.GetPendingDetentionExcelView(from, to, departmentId, detentionTypeId);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Machine Understoppage";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers


            report.SetHeaderText(ref sheet, ROW, COL, "Id", 6, ExcelHAlign.HAlignCenter);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Work Center", 12, ExcelHAlign.HAlignCenter);
            int ColWorkCenter = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 20, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Machine", 20, ExcelHAlign.HAlignCenter);
            int ColMachine = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 20, ExcelHAlign.HAlignCenter);
            int ColResponsiblePerson = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person No", 20, ExcelHAlign.HAlignCenter);
            int ColResPerNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Detention Type", 12, ExcelHAlign.HAlignCenter);
            int ColDetentionType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Issue By No.", 20, ExcelHAlign.HAlignCenter);
            int ColIssueByNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Login Date", 12, ExcelHAlign.HAlignCenter);
            int ColLogDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Login Time", 12, ExcelHAlign.HAlignCenter);
            int ColLogTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 30, ExcelHAlign.HAlignCenter);
            int ColRemarks = COL;
            COL++;


            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColId].Text = data.Rows[i]["Id"].ToString();
                sheet[ROW, ColWorkCenter].Text = data.Rows[i]["WorkCenter"].ToString();
                sheet[ROW, ColMachine].Text = data.Rows[i]["MachineMaster"].ToString();
                sheet[ROW, ColResponsiblePerson].Text = data.Rows[i]["ResponsiblePersonName"].ToString();
                sheet[ROW, ColResPerNo].Text = data.Rows[i]["ContactNo"].ToString();
                sheet[ROW, ColDetentionType].Text = data.Rows[i]["DetentionType"].ToString();
                sheet[ROW, ColIssueByNo].Text = data.Rows[i]["IssueByNo"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColLogDate].Text = data.Rows[i]["AddedDate"].ToString();
                sheet[ROW, ColLogTime].Text = data.Rows[i]["AddedTime"].ToString();
                
                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, startRow, endCol];


            sheet.Range[startRow - 1, 1, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyHeader(ref sheet, endCol, "PendingForAllocation", identity.CompanyId);
            //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }


    }
}