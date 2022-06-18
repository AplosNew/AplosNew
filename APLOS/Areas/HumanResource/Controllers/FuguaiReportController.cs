#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.HumanResource.Employee;
using Syncfusion.XlsIO;
using System.Drawing;
using System.IO;
using System.Linq;
using Library.Service.Helpers;
#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class FuguaiReportController : BaseController
    {
        FuguaiReportService fr = new FuguaiReportService();
        private readonly ISqlRepository _sqlRepository;
        public FuguaiReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #region Page
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page
        #region All Get
        [Authorize, HttpPost]
        public ActionResult getByWhom()
        {
            try
            {
                return Json(fr.getByWhom(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getResponsiblePerson()
        {
            try
            {
                return Json(fr.getResponsiblePerson(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getCategory()
        {
            try
            {
                return Json(fr.getCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getFuguai(string categoryText)
        {
            try
            {
                return Json(fr.getFuguai(categoryText), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getFinalStatus(string categoryText)
        {
            try
            {
                return Json(fr.getFinalStatus(categoryText), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getFuguaiTransaction(string SystemId, string ObservedById)
        {
            try
            {
                return Json(fr.getFuguaiTransaction(SystemId, ObservedById), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult viewByDate(string FromDate, string ToDate, string FinalStatus)
        {
            try
            {
                return Json(fr.viewByDate(FromDate, ToDate, FinalStatus), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion All Get

        [Authorize, HttpPost]
        public ActionResult getReport(string FromDate, string ToDate, string FinalStatus)
        {

            try
            {
                var workbook = getReportForm(FromDate, ToDate, FinalStatus);

                var strFileName = /*DateTime.Now.ToString("yy-MM-dd") + " " + */"FuguaiReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //[Authorize, HttpGet]
        private IWorkbook getReportForm(string FromDate, string ToDate, string FinalStatus)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = fr.GetReport(FromDate, ToDate, FinalStatus);

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Fuguai Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;



            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 12, ExcelHAlign.HAlignLeft);
            int ColDate = COL;
            COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Time", 25, ExcelHAlign.HAlignLeft);
            //int ColTime = COL;
            //COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 15, ExcelHAlign.HAlignLeft);
            int ColEmpEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Observed By", 15, ExcelHAlign.HAlignLeft);
            int ColObservedBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Category", 15, ExcelHAlign.HAlignLeft);
            int ColCategory = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Tag", 15, ExcelHAlign.HAlignLeft);
            int ColTag = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Detail", 15, ExcelHAlign.HAlignLeft);
            int ColDetail = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Priority Level", 15, ExcelHAlign.HAlignLeft);
            int ColPriorityLevel = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Department", 15, ExcelHAlign.HAlignRight);
            int ColResponsibleDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 15, ExcelHAlign.HAlignRight);
            int ColResponsiblePerson = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Target Date", 15, ExcelHAlign.HAlignRight);
            int ColTargetDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 15, ExcelHAlign.HAlignLeft);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Current Status", 15, ExcelHAlign.HAlignRight);
            int ColCurrentStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process", 15, ExcelHAlign.HAlignRight);
            int ColProcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Machine", 15, ExcelHAlign.HAlignRight);
            int ColMachine = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Machine Ref No", 15, ExcelHAlign.HAlignLeft);
            int ColMachineNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Final Status", 15, ExcelHAlign.HAlignLeft);
            int ColFinalStatus = COL;
            COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "CloseDate", 15, ExcelHAlign.HAlignLeft);
            //int ColCloseDate = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Tag Color", 15, ExcelHAlign.HAlignLeft);
            int ColTagColor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Story Point", 15, ExcelHAlign.HAlignLeft);
            int ColStoryPoint = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Added By", 80, ExcelHAlign.HAlignLeft);
            int ColAddedBy = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;


            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColDate].Text = data.Rows[i]["Date"].ToString();
                //sheet[ROW, ColTime].Text = data.Rows[i]["Time"].ToString();
                sheet[ROW, ColEmpEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColObservedBy].Text = data.Rows[i]["ObservedBy"].ToString();
                sheet[ROW, ColCategory].Text = data.Rows[i]["Category"].ToString();
                sheet[ROW, ColTag].Text = data.Rows[i]["Tag"].ToString();
                sheet[ROW, ColDetail].Text = data.Rows[i]["Detail"].ToString();
                sheet[ROW, ColPriorityLevel].Text = data.Rows[i]["PriorityLevel"].ToString();
                sheet[ROW, ColResponsibleDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColResponsiblePerson].Text = data.Rows[i]["ResponsiblePerson"].ToString();
                sheet[ROW, ColTargetDate].Text =data.Rows[i]["TargetDate"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColCurrentStatus].Text = data.Rows[i]["CurrentStatus"].ToString();
                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                sheet[ROW, ColMachine].Text = data.Rows[i]["Machine"].ToString(); 
                sheet[ROW, ColMachineNo].Text = data.Rows[i]["MachineReference"].ToString();
                sheet[ROW, ColFinalStatus].Text = data.Rows[i]["FinalStatus"].ToString();
                sheet[ROW, ColAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
                //sheet[ROW, ColCloseDate].Text = data.Rows[i]["CloseDate"].ToString();
                sheet[ROW, ColTagColor].Text = data.Rows[i]["TagColor"].ToString();
                sheet[ROW, ColStoryPoint].Number = Convert.ToDouble(data.Rows[i]["StoryPoint"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ROW++;

            }

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;


            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Fuguai Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        
    }
}