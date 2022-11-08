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
        public ActionResult XlsGetMedinceStockReport(string from, string to, string departmentId, string detentionTypeId)
        {
            try
            {
                var workbook = ClosedDetentionExcelView(from, to, departmentId, detentionTypeId);

                var strFileName = DateTime.Now.ToString("yy-MMM-dd") + " " + "MedicalLogReport.xlsx";
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
        private IWorkbook ClosedDetentionExcelView(string from, string to, string departmentId, string detentionTypeId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = dl.GetClosedDetentionExcelReport(from, to, departmentId, detentionTypeId);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Medical Log Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers


            report.SetHeaderText(ref sheet, ROW, COL, "Id", 6, ExcelHAlign.HAlignCenter);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 12, ExcelHAlign.HAlignCenter);
            int ColDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sickness Name", 50, ExcelHAlign.HAlignCenter);
            int ColSicknessName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Medicines", 50, ExcelHAlign.HAlignCenter);
            int ColMedicines = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Days", 5, ExcelHAlign.HAlignCenter);
            int ColSDays = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 30, ExcelHAlign.HAlignCenter);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 20, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 20, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 20, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 20, ExcelHAlign.HAlignCenter);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Given Designation", 20, ExcelHAlign.HAlignCenter);
            int ColGivenDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Skill", 20, ExcelHAlign.HAlignCenter);
            int ColSkill = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 20, ExcelHAlign.HAlignCenter);
            int ColEntity = COL;
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
                sheet[ROW, ColDate].Text = data.Rows[i]["Date"].ToString();
                sheet[ROW, ColEmployeeCode].Number = clsStaticInfo.dbl(data.Rows[i]["EmployeeCode"].ToString());
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColSicknessName].Text = data.Rows[i]["Sickness"].ToString();
                sheet[ROW, ColMedicines].Text = data.Rows[i]["Medicines"].ToString();
                sheet[ROW, ColSDays].Number = clsStaticInfo.dbl(data.Rows[i]["Days"].ToString());
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColGivenDesignation].Text = data.Rows[i]["GivenDesignation"].ToString();
                //sheet[ROW, ColSkill].Text = data.Rows[i]["Skill"].ToString();
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();

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