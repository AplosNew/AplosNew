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
      
        [HttpGet, Authorize]
        public ActionResult GetEntity()
        {
            return Json(eo.GetEntity() , JsonRequestBehavior.AllowGet);
        }

        [HttpPost , Authorize]
        public ActionResult GetWorkCenter(string EId)
        {
            return Json(eo.GetWorkCenter(EId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProcess()
        {
            return Json(eo.GetProcess(), JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet, Authorize]
        public ActionResult GetPeriod()
        {
            var dS = eo.GetPeriod(out string CurrPer);
            return Json(new { Data = dS , Current = CurrPer }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetShift()
        {
            return Json(eo.GetShift(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetPOs(string wk)
        {
            return Json(eo.GetPOs(wk), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetOperationsData(string PId , string Period , string ProcessId)
        {
            return Json(eo.GetOperationsData(PId , Period , ProcessId) , JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getReportView()
        {
            return Json( new {Data = eo.getReportView(out List<string> Cols) , Cols = Cols } , JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult saveData(List<Dictionary<string, object>> data , string WorkCenter , string ProcessId ,  string ShiftId , string POId , string Date , string PeriodId)
        {
            try
            {
                eo.saveData( data,  WorkCenter,  ProcessId,  ShiftId,  POId,  Date , PeriodId);
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
                eo.processAll( Date);
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
        public ActionResult getReportDownload(string PO)
        {

            try
            {
                var workbook = generateReportForm(PO);

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
        private IWorkbook generateReportForm(string PO)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = eo.getReportDownload(out List<string> DynCols);

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
                public ActionResult getProcessDownload(string PO)
                {

                    try
                    {
                        var workbook = GenerateProcessForm(PO);

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
                private IWorkbook GenerateProcessForm(string PO)
                {
                    var excelEngine = new ExcelEngine();
                    var report = new ReportUtility();
                    var workbook = report.GetWorkbook(ref excelEngine, 3);
                    workbook.Version = ExcelVersion.Excel2016;

                    var data = eo.getProcessDownload(out List<string> DynCols);

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

                    report.SetHeaderText(ref sheet, ROW, COL, "Operation Variation Code", 10, ExcelHAlign.HAlignCenter);
                    int ColOVC = COL;
                    COL++;

                    report.SetHeaderText(ref sheet, ROW, COL, "Operation Variation", 10, ExcelHAlign.HAlignCenter);
                    int ColOV = COL;
                    COL++;

                    report.SetHeaderText(ref sheet, ROW, COL, "Production Order Id", 15, ExcelHAlign.HAlignCenter);
                    int ColPOI = COL;
                    COL++;

                    report.SetHeaderText(ref sheet, ROW, COL, "Qty", 10, ExcelHAlign.HAlignCenter);
                    int ColQty = COL;
                    COL++;

                    report.SetHeaderText(ref sheet, ROW, COL, "Standard Process Time", 12, ExcelHAlign.HAlignCenter);
                    int ColSPT = COL;
                    COL++;

                    //report.SetHeaderText(ref sheet, ROW, COL, "Sequence", 10, ExcelHAlign.HAlignCenter);
                    //int ColSeq = COL;
                    //COL++;

                    report.SetHeaderText(ref sheet, ROW, COL, "Total SPT", 10, ExcelHAlign.HAlignCenter);
                    int ColTotalSPT = COL;
                    COL++;

                    report.SetHeaderText(ref sheet, ROW, COL, "Skill Allowance", 10, ExcelHAlign.HAlignCenter);
                    int ColSA = COL;
                    COL++;

                    report.SetHeaderText(ref sheet, ROW, COL, "Additional Operation Allowance", 10, ExcelHAlign.HAlignCenter);
                    int ColAOA = COL;
                    COL++;
                    
                    report.SetHeaderText(ref sheet, ROW, COL, "Alloted Produced Min", 10, ExcelHAlign.HAlignCenter);
                    int ColAPM = COL;
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
                        sheet[ROW, ColDate].Text = data.Rows[i]["Date"].ToString();
                        sheet[ROW, ColEC].Text = data.Rows[i]["EmployeeCode"].ToString();
                        sheet[ROW, ColEN].Text = data.Rows[i]["EmployeeName"].ToString();
                        sheet[ROW, ColOVC].Text = data.Rows[i]["Code"].ToString();        
                        sheet[ROW, ColOV].Text = data.Rows[i]["UserName"].ToString();        
                        sheet[ROW, ColPOI].Text = data.Rows[i]["poId"].ToString();
                        sheet[ROW, ColQty].Text = data.Rows[i]["Qty"].ToString();              
                        sheet[ROW, ColSPT].Text = data.Rows[i]["StandardProcessTime"].ToString();
                       // sheet[ROW, ColSeq].Text = data.Rows[i]["Sequence"].ToString();
                        sheet[ROW, ColTotalSPT].Text = data.Rows[i]["TotalSPT"].ToString();
                        sheet[ROW, ColSA].Text = data.Rows[i]["SkillAllowance"].ToString();
                        sheet[ROW, ColAOA].Text = data.Rows[i]["AdditionalOperationAllowance"].ToString();
                        sheet[ROW, ColAPM].Text = data.Rows[i]["AllotedProducedMin"].ToString();

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
         public ActionResult getEmployeeWorkDurationReport(string PO)
         {

             try
             {
                 var workbook = GenerateEmployeeWorkDuration(PO);

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
         private IWorkbook GenerateEmployeeWorkDuration(string PO)
         {
             var excelEngine = new ExcelEngine();
             var report = new ReportUtility();
             var workbook = report.GetWorkbook(ref excelEngine, 3);
             workbook.Version = ExcelVersion.Excel2016;

             var data = eo.getEmployeeWorkDurationReport(out List<string> DynCols);

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
             int ColEC = COL;
             COL++;

             report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 12, ExcelHAlign.HAlignCenter);
             int ColEN = COL;
             COL++;

             report.SetHeaderText(ref sheet, ROW, COL, "Total SPT", 10, ExcelHAlign.HAlignCenter);
             int ColTotalSPT = COL;
             COL++;

             report.SetHeaderText(ref sheet, ROW, COL, "Alloted Produced Min", 10, ExcelHAlign.HAlignCenter);
             int ColAPM = COL;
             COL++;

             report.SetHeaderText(ref sheet, ROW, COL, "Work Duration", 10, ExcelHAlign.HAlignCenter);
             int ColWorkDur = COL;
             COL++;

             report.SetHeaderText(ref sheet, ROW, COL, "Net Efficiency", 15, ExcelHAlign.HAlignCenter);
             int ColNE = COL;
             COL++;

             report.SetHeaderText(ref sheet, ROW, COL, "Gross Efficiency", 10, ExcelHAlign.HAlignCenter);
             int ColGE = COL;
             COL++;

             report.SetHeaderText(ref sheet, ROW, COL, "Rate", 12, ExcelHAlign.HAlignCenter);
             int ColRate = COL;
             COL++;

             report.SetHeaderText(ref sheet, ROW, COL, "Amount", 10, ExcelHAlign.HAlignCenter);
             int ColAmount = COL;
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
                 sheet[ROW, ColDate].Text = data.Rows[i]["Date"].ToString();
                 sheet[ROW, ColEC].Text = data.Rows[i]["EmployeeCode"].ToString();
                 sheet[ROW, ColEN].Text = data.Rows[i]["EmployeeName"].ToString();          
                 sheet[ROW, ColTotalSPT].Text = data.Rows[i]["TotalSPT"].ToString();
                 sheet[ROW, ColAPM].Text = data.Rows[i]["AllotedProducedMin"].ToString();
                 sheet[ROW, ColWorkDur].Text = data.Rows[i]["WorkDuration"].ToString();
                 sheet[ROW, ColNE].Text = data.Rows[i]["NetEfficiency"].ToString();
                 sheet[ROW, ColGE].Text = data.Rows[i]["GrossEfficiency"].ToString();
                 sheet[ROW, ColRate].Text = data.Rows[i]["Rate"].ToString();
                 sheet[ROW, ColAmount].Text = data.Rows[i]["Amount"].ToString();

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