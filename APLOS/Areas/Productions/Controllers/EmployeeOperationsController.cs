#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using System.Drawing;
using Syncfusion.XlsIO;
using System.IO;
using Library.Service.Helpers;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class EmployeeOperationsController : BaseController
    {

        EmployeeOperationsService eo = new EmployeeOperationsService();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public EmployeeOperationsController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetEntity()
        {
            return Json(eo.GetEntity(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetWorkCenter(string PId)
        {
            return Json(eo.GetWorkCenter(PId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetProcess(string EId)
        {
            return Json(eo.GetProcess(EId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPeriod()
        {
            var dS = eo.GetPeriod(out string CurrPer);
            return Json(new { Data = dS, Current = CurrPer }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetResp(string WKId)
        {
            return Json(eo.GetResp(WKId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmps()
        {
            return Json(eo.GetEmps(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetShift()
        {
            return Json(eo.GetShift(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetPOs(string entityId)
        {
            return Json(eo.GetPOs(entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getPODetails(string POId)
        {
            return Json(eo.getPODetails(POId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetOperationsData(string PId, string Period, string ProcessId)
        {
            return Json(eo.GetOperationsData(PId, Period, ProcessId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getReportView(string Date , string Wkc)
        {
            return Json(new { Data = eo.getReportView(out List<string> Cols , Date , Wkc), Cols = Cols }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult saveData(List<Dictionary<string, object>> data, string WorkCenter, string ProcessId, string ShiftId, string POId, string Date, string PeriodId, string ResponsiblePersonId)
        {
            try
            {
                eo.saveData(data, WorkCenter, ProcessId, ShiftId, POId, Date, PeriodId, ResponsiblePersonId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult processAll(string Date)
        {
            try
            {
                eo.processAll(Date);
                return Json(new { Error = false, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        // Report Download
        #region Report Operations

        [HttpPost, Authorize]
        public ActionResult getReportDownload(string Date , string Wkc)
        {

            try
            {
                var workbook = generateReportForm(Date,  Wkc);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "EOWiseReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        private IWorkbook generateReportForm(string Date, string Wkc)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = eo.getReportDownload(out List<string> DynCols , Date ,  Wkc);

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "EO  Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Operation Code", 15, ExcelHAlign.HAlignCenter);
            int ColOC = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Operation Name", 26, ExcelHAlign.HAlignCenter);
            int ColON = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process", 12, ExcelHAlign.HAlignCenter);
            int ColProcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Work Center", 10, ExcelHAlign.HAlignCenter);
            int ColWC = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PO", 10, ExcelHAlign.HAlignCenter);
            int ColPOId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 30, ExcelHAlign.HAlignCenter);
            int ColEN = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 10, ExcelHAlign.HAlignCenter);
            int ColEmpCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Dates", 12, ExcelHAlign.HAlignCenter);
            int ColDate = COL;
            COL++;

            int ColSt = COL;

            for (int i = 0; i < DynCols.Count; i++)
            {
                report.SetHeaderText(ref sheet, ROW, COL, DynCols[i], 10, ExcelHAlign.HAlignCenter);

                COL++;
            }




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

                sheet[ROW, ColOC].Text = data.Rows[i]["OperationCode"].ToString();
                sheet[ROW, ColON].Text = data.Rows[i]["OperationName"].ToString();
                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                sheet[ROW, ColWC].Text = data.Rows[i]["WorkCenter"].ToString();
                sheet[ROW, ColPOId].Text = data.Rows[i]["ProductionOrderId"].ToString();
                sheet[ROW, ColEN].Text = data.Rows[i]["EmployeeName"].ToString();

                sheet[ROW, ColEmpCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColDate].Text = data.Rows[i]["Dates"].ToString();


                int k = ColSt;
                for (int j = 0; j < DynCols.Count; j++)
                {
                    sheet[ROW, k].Number = clsStaticInfo.dbl(data.Rows[i][DynCols[j]].ToString());
                    k++;
                }




                ROW++;

            }

            ROW++;



            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1




            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;


            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Employee Operation Wise Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }


        #endregion Report Operations

        #region Process Tab
        #region Process Download


        [HttpPost, Authorize]
        public ActionResult getProcessDownload(string FromDate , string ToDate)
        {

            try
            {
                var workbook = GenerateProcessForm(FromDate , ToDate);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "EfficiencyReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        private IWorkbook GenerateProcessForm(string FromDate , string ToDate)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = eo.getProcessDownload(FromDate , ToDate);

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Efficiency Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 12, ExcelHAlign.HAlignCenter);
            int ColDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 10, ExcelHAlign.HAlignCenter);
            int ColEC = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 20, ExcelHAlign.HAlignCenter);
            int ColEN = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Available Min", 20, ExcelHAlign.HAlignCenter);
            int ColWorkDur = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Time Out", 20, ExcelHAlign.HAlignCenter);
            int ColTO = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Net Available Min", 20, ExcelHAlign.HAlignCenter);
            int ColNetMin = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Produced Min", 20, ExcelHAlign.HAlignCenter);
            int ColProdMin = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Gross Produced Min", 20, ExcelHAlign.HAlignCenter);
            int ColGProdMin = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Net Efficiency", 20, ExcelHAlign.HAlignCenter);
            int ColNetEff = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Gross Efficiency", 20, ExcelHAlign.HAlignCenter);
            int ColGEff = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate", 20, ExcelHAlign.HAlignCenter);
            int ColRate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Net Amount", 20, ExcelHAlign.HAlignCenter);
            int ColNtAmt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Final Amount", 20, ExcelHAlign.HAlignCenter);
            int ColFiAmt = COL;
            COL++;

            int ColSt = COL;

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
                sheet[ROW, ColDate].Text = data.Rows[i]["WorksDate"].ToString();
                sheet[ROW, ColEC].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEN].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColWorkDur].Text = data.Rows[i]["Duration"].ToString();
                sheet[ROW, ColTO].Text = data.Rows[i]["EmployeesTimeOutDuration"].ToString();
                sheet[ROW, ColNetMin].Text = data.Rows[i]["NetDuration"].ToString();
                sheet[ROW, ColProdMin].Text = data.Rows[i]["ProducedMin"].ToString();
                sheet[ROW, ColGProdMin].Text = data.Rows[i]["GrossProducedMin"].ToString();
                sheet[ROW, ColNetEff].Text = data.Rows[i]["NetEfficiency"].ToString();
                sheet[ROW, ColGEff].Text = data.Rows[i]["GrossEfficiency"].ToString();
                sheet[ROW, ColRate].Text = data.Rows[i]["Rate"].ToString();
                sheet[ROW, ColNtAmt].Text = data.Rows[i]["Amount"].ToString();
                sheet[ROW, ColFiAmt].Text = data.Rows[i]["FinalAmount"].ToString();


                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Efficiency Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion Process Download

        //----------------------------------------------------------------------------

        #region Employee Work Duration Report
        [HttpPost, Authorize]
        public ActionResult getEmployeeWorkDurationReport(string FromDate , string ToDate)
        {

            try
            {
                var workbook = GenerateEmployeeWorkDuration( FromDate,  ToDate);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "EmployeeWorkDurationReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        private IWorkbook GenerateEmployeeWorkDuration(string FromDate, string ToDate)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = eo.getEmployeeWorkDurationReport( FromDate,  ToDate);

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Employee Work Duration Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 12, ExcelHAlign.HAlignCenter);
            int ColDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignCenter);
            int ColEmpCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 12, ExcelHAlign.HAlignCenter);
            int ColEmpName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shift", 12, ExcelHAlign.HAlignCenter);
            int ColShift = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Work Center", 12, ExcelHAlign.HAlignCenter);
            int Colwk = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PO", 12, ExcelHAlign.HAlignCenter);
            int ColPo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Buyer Reference", 12, ExcelHAlign.HAlignCenter);
            int ColBuy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Own Reference", 12, ExcelHAlign.HAlignCenter);
            int ColORef = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article Code", 12, ExcelHAlign.HAlignCenter);
            int ColACode = COL;
            COL++;
            

            report.SetHeaderText(ref sheet, ROW, COL, "Operation Code", 12, ExcelHAlign.HAlignCenter);
            int ColOpCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Operation", 12, ExcelHAlign.HAlignCenter);
            int ColOpName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Skill Category", 12, ExcelHAlign.HAlignCenter);
            int ColSkCat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Qty", 12, ExcelHAlign.HAlignCenter);
            int ColQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SPT", 12, ExcelHAlign.HAlignCenter);
            int ColSpt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Produced Min", 12, ExcelHAlign.HAlignCenter);
            int ColProdMin = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Skill Allowance", 12, ExcelHAlign.HAlignCenter);
            int ColSkAll = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Additional Operation Allowance", 12, ExcelHAlign.HAlignCenter);
            int ColAdOpAll = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Special Operation Allowance", 12, ExcelHAlign.HAlignCenter);
            int ColSpAll = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Gross Produced Min", 12, ExcelHAlign.HAlignCenter);
            int ColGProd = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 12, ExcelHAlign.HAlignCenter);
            int ColRem = COL;
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
                sheet[ROW, ColDate].Text = data.Rows[i]["WorksDate"].ToString();
                sheet[ROW, ColPo].Number = clsStaticInfo.dbl(data.Rows[i]["ProductionOrderId"].ToString());
                sheet[ROW, ColShift].Text = data.Rows[i]["ShiftName"].ToString();
                sheet[ROW, ColSpt].Number = clsStaticInfo.dbl(data.Rows[i]["StandardProcessTime"].ToString());
                sheet[ROW, ColOpName].Text = data.Rows[i]["OperationName"].ToString();
                sheet[ROW, Colwk].Text = data.Rows[i]["WorkCenter"].ToString();
                sheet[ROW, ColBuy].Text = data.Rows[i]["BuyerRef"].ToString();
                sheet[ROW, ColORef].Text = data.Rows[i]["OwnRef"].ToString();
                sheet[ROW, ColACode].Text = data.Rows[i]["ArticleCode"].ToString();
                sheet[ROW, ColEmpCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEmpName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColOpCode].Text = data.Rows[i]["OperationCode"].ToString();
                sheet[ROW, ColQty].Number = clsStaticInfo.dbl(data.Rows[i]["Qty"].ToString());
                sheet[ROW, ColProdMin].Number = clsStaticInfo.dbl(data.Rows[i]["TotalSPT"].ToString());
                sheet[ROW, ColSkCat].Text = data.Rows[i]["SkillCategory"].ToString();
                sheet[ROW, ColSkAll].Number = clsStaticInfo.dbl(data.Rows[i]["SkillAllowance"].ToString());
                sheet[ROW, ColAdOpAll].Number = clsStaticInfo.dbl(data.Rows[i]["AdditionalOperationAllowance"].ToString());
                sheet[ROW, ColSpAll].Number = clsStaticInfo.dbl(data.Rows[i]["SpecialOperationAllowance"].ToString());
                sheet[ROW, ColGProd].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedProducedMin"].ToString());
                sheet[ROW, ColRem].Text = data.Rows[i]["Remarks"].ToString();

                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Employee Work Duration Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion Employee Work Duration Report
        #endregion Process Tab


    }
}