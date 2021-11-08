#region Using

using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.MaterialManagement.JobWork;
using Syncfusion.XlsIO;
using System.IO;
using Library.Service.Helpers;


#endregion Using

namespace Aplos.Areas.Outsourcing.Controllers
{
    public class MaterialReconcilationReportController : BaseController
    {
        MaterialReconcilationReportService MRR = new MaterialReconcilationReportService();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public MaterialReconcilationReportController(ISqlRepository R)
        {
            _sqlRepository = R;
            MRR = new MaterialReconcilationReportService();
    
        }


        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult LoadAllTransConForSelection(string Type)
        {
            try
            {

                return Json(MRR.LoadAllTransConForSelection(Type), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        // MATERIAL RECONCILATION VALUE ADDED REPORT
        [HttpPost]
        public ActionResult GetMatReconcilationReport(string ContractId)
        {
            try
            {
                var workbook = GetData(ContractId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "MaterialReconcilation.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private IWorkbook GetData(string ContractId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
          //  var sheet1 = workbook.Worksheets[1];

            sheet.Name = "MaterialReconcilation";
          //  sheet1.Name = "Details";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = MRR.GetReportData(ContractId);
            DataTable Childdata = MRR.GetChildDataById(ContractId);

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Line Item Id", 8, ExcelHAlign.HAlignLeft);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Job Work Item", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkItem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 12, ExcelHAlign.HAlignLeft);
            int ColArticleCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Job Work Activity", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkActivity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Unit", 15, ExcelHAlign.HAlignLeft);
            int ColOutputUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Issued Quantity", 10, ExcelHAlign.HAlignLeft);
            int ColTotalIssuedQty = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Total Receipt Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColTotalReceiptQty = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Total Balance", 12, ExcelHAlign.HAlignLeft);
            int ColDiff = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate/ Unit", 15, ExcelHAlign.HAlignLeft);
            int ColRatePerUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Value", 15, ExcelHAlign.HAlignLeft);
            int ColTotalValue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate Apply", 15, ExcelHAlign.HAlignLeft);
            int ColRateApply = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 15, ExcelHAlign.HAlignLeft);
            int ColRemarks = COL;
            ROW++;


            endCol = COL;
            #endregion Headers

            string MPId = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (MPId != data.Rows[i]["Id"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        //      sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].Merge();
                        sheet.Range[RowIndex, ColId, ROW - 1, ColId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColId, ROW - 1, ColId].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    }
                    RowIndex = ROW;
                }

                sheet[ROW, ColId].Text = data.Rows[i]["Id"].ToString();
                sheet[ROW, ColJobWorkItem].Text = data.Rows[i]["JobWorkItem"].ToString();
                sheet[ROW, ColArticleCode].Text = data.Rows[i]["ArticleCode"].ToString();
                sheet[ROW, ColJobWorkActivity].Text = data.Rows[i]["JobWorkActivity"].ToString();
                sheet[ROW, ColOutputUnit].Text = data.Rows[i]["OutputUnit"].ToString();

                sheet[ROW, ColTotalIssuedQty].Number =clsStaticInfo.dbl(data.Rows[i]["TotalIssuedQty"].ToString());
                sheet[ROW, ColTotalReceiptQty].Number = clsStaticInfo.dbl(data.Rows[i]["TotalReceiptQty"].ToString());

                sheet[ROW, ColDiff].Number =clsStaticInfo.dbl(data.Rows[i]["TotalBalance"].ToString());
                
                sheet[ROW, ColRatePerUnit].Number = clsStaticInfo.dbl(data.Rows[i]["RatePerUnit"].ToString());
                sheet[ROW, ColTotalValue].Number = clsStaticInfo.dbl(data.Rows[i]["TotalValue"].ToString());
                sheet[ROW, ColRateApply].Text = data.Rows[i]["RateApply"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
 
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                MPId = data.Rows[i]["Id"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                //      sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].Merge();
                sheet.Range[RowIndex, ColId, ROW - 1, ColId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColId, ROW - 1, ColId].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColArticleCode, ROW - 1, ColArticleCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColArticleCode, ROW - 1, ColArticleCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJobWorkActivity, ROW - 1, ColJobWorkActivity].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJobWorkActivity, ROW - 1, ColJobWorkActivity].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColOutputUnit, ROW - 1, ColOutputUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColOutputUnit, ROW - 1, ColOutputUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColTotalIssuedQty, ROW - 1, ColTotalIssuedQty].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColTotalIssuedQty, ROW - 1, ColTotalIssuedQty].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColTotalReceiptQty, ROW - 1, ColTotalReceiptQty].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColTotalReceiptQty, ROW - 1, ColTotalReceiptQty].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColDiff, ROW - 1, ColDiff].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColDiff, ROW - 1, ColDiff].HorizontalAlignment = ExcelHAlign.HAlignLeft;

               

                sheet.Range[RowIndex, ColRatePerUnit, ROW - 1, ColRatePerUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColRatePerUnit, ROW - 1, ColRatePerUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColRateApply, ROW - 1, ColRateApply].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColRateApply, ROW - 1, ColRateApply].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColRemarks, ROW - 1, ColRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColRemarks, ROW - 1, ColRemarks].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            }

            // CHILD DATA

            int MIChildROW = ROW + 1;
            int MIChildendCol = 1;
            int MIChildCOL = 1;

            #region Material Input Child Headers

          //  report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Material Reconcilation Details", 12, ExcelHAlign.HAlignLeft);
          //  int MRDCol = MIChildCOL + 6;
          //  sheet.Range[MIChildROW, MIChildCOL, MIChildROW, MRDCol].Merge();
          //  sheet.Range[MIChildROW, MIChildCOL, MIChildROW, MRDCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
          //  sheet.Range[MIChildROW, MIChildCOL, MIChildROW, MRDCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
          ////  int ColMaterial = MRDCol;
          //  MIChildROW++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Line Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColMaterial = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "JW Output Item", 12, ExcelHAlign.HAlignLeft);
            int ColMatSpecification = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Issue Date", 10, ExcelHAlign.HAlignLeft);
            int ColMICUOM = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Total Issued Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColNetConsumptionOutputUnit = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Total Receipt Quantity", 10, ExcelHAlign.HAlignLeft);
            int ColMIRejection = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Balance", 10, ExcelHAlign.HAlignLeft);
            int ColMIValueLoss = MIChildCOL;
            MIChildCOL++;

            //report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Gross Consumption", 10, ExcelHAlign.HAlignLeft);
            //int ColGrossConsumption = MIChildCOL;
            //MIChildCOL++;

            //report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Employee Code", 12, ExcelHAlign.HAlignLeft);
            //int ColMIEmployeeCode = MIChildCOL;
            //MIChildCOL++;

            //report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Responsible Person", 15, ExcelHAlign.HAlignLeft);
            //int ColResponsiblePerson = MIChildCOL;
            //MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Remarks", 12, ExcelHAlign.HAlignLeft);
            int ColMIRemarks = MIChildCOL;
            MIChildROW++;

            MIChildendCol = MIChildCOL;
            #endregion Headers

            string Material = "";
            var MIStartRows = 0;
            var MIEndRows = 0;
            int MIRowIndexNo = MIChildROW;
            MIStartRows = MIChildROW;

            for (int i = 0; i < Childdata.Rows.Count; i++)
            {

                if (Material != Childdata.Rows[i]["ContractLineItemId"].ToString())
                {

                    if (MIRowIndexNo < MIChildROW)
                    {
                        //sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].Merge();
                        sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    MIRowIndexNo = MIChildROW;
                }

                sheet[MIChildROW, ColNetConsumptionOutputUnit].Number = clsStaticInfo.dbl(Childdata.Rows[i]["TotalIssuedQty"].ToString());
                sheet[MIChildROW, ColMaterial].Text = Childdata.Rows[i]["ContractLineItemId"].ToString();
                sheet[MIChildROW, ColMatSpecification].Text = Childdata.Rows[i]["JWOutputItem"].ToString();
                sheet[MIChildROW, ColMICUOM].Text = Childdata.Rows[i]["IssueDate"].ToString();
                sheet[MIChildROW, ColMIRejection].Number = clsStaticInfo.dbl(Childdata.Rows[i]["TotalReceiptQty"].ToString());
                sheet[MIChildROW, ColMIValueLoss].Number = clsStaticInfo.dbl(Childdata.Rows[i]["Diff"].ToString());
                //sheet[MIChildROW, ColGrossConsumption].Number = clsStaticInfo.dbl(MaterialInputChilddata.Rows[i]["GrossConsumption"].ToString());
                //sheet[MIChildROW, ColMIEmployeeCode].Text = MaterialInputChilddata.Rows[i]["EmployeeCode"].ToString();
                //sheet[MIChildROW, ColResponsiblePerson].Text = MaterialInputChilddata.Rows[i]["ResponsiblePerson"].ToString();
                sheet[MIChildROW, ColMIRemarks].Text = Childdata.Rows[i]["Remarks"].ToString();

                sheet.Range[MIChildROW, 1, MIChildROW, MIChildendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[MIChildROW, 1, MIChildROW, MIChildendCol].BorderAround(ExcelLineStyle.Hair);
                Material = Childdata.Rows[i]["ContractLineItemId"].ToString();

                MIChildROW++;
            }

            // SUM OF TOTAL ISSUED QUANTITY
            int ColTotalIssQty = 4;
            var p = 0;
            var q = 0;
            var r = 0;
            for (int j = 0; j < Childdata.Rows.Count; j++)
            {

                p = Convert.ToInt32(Childdata.Rows[j]["TotalIssuedQty"]);
                r = p + q;
                q = r;
                sheet[MIChildROW, ColTotalIssQty].Number = clsStaticInfo.dbl(q);
            }

            // SUM OF TOTAL RECEIPT QUANTITY
            int ColTotalRecQty = 5;
            var x = 0;
            var y = 0;
            var z = 0;
            for (int j = 0; j < Childdata.Rows.Count; j++)
            {

                x = Convert.ToInt32(Childdata.Rows[j]["TotalReceiptQty"]);
                z = x + y;
                y = z;
                sheet[MIChildROW, ColTotalRecQty].Number = clsStaticInfo.dbl(y);
            }

            // SUM OF DIFFERENCE
            int ColTotalCol = 6;
            var a = 0;
            var b = 0;
            var c = 0;
            for(int j = 0; j < Childdata.Rows.Count; j++)
            {
                
                    a= Convert.ToInt32(Childdata.Rows[j]["Diff"]);
                    c = a + b;
                    b = c;
                sheet[MIChildROW, ColTotalCol].Number = clsStaticInfo.dbl(b);
            }

            MIEndRows = MIChildROW - 1;

            if (MIRowIndexNo < MIChildROW - 1)
            {
                //sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].Merge();
                sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            // Details Sheet
            //sheet.UsedRange.NumberFormat = "#,##0.000";
            //sheet.UsedRange.WrapText = true;
            //sheet.UsedRange.CellStyle.Font.Size = 8;

            report.CompanyPlantHeader(ref sheet, endCol, "Material Reconcilation Value Added", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        // MATERIAL RECONCILATION TRANSFORMATION REPORT

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }

        [HttpPost]
        public ActionResult GetMatReconcilationTransformationReport(string ContractId)
        {
            try
            {
                var workbook = GetTRANSFORMATIONData(ContractId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "MaterialReconcilation.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private IWorkbook GetTRANSFORMATIONData(string ContractId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
        //    var sheet1 = workbook.Worksheets[1];

            sheet.Name = "MaterialReconcilation";
          //  sheet1.Name = "Details";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = MRR.GetTransReportData(ContractId);
            DataTable Childdata = MRR.GetTransChildDataById(ContractId);
            DataTable DateWiseIssuedata = MRR.GetTransDateWiseDataById(ContractId);
            DataTable DateWiseReceiveddata = MRR.GetTransReceivedDateWiseDataById(ContractId);
            DataTable ByProductReceiveddata = MRR.GetTransReceivedByProductDataById(ContractId);

            if (data.Rows.Count > 0)
            {
                int ColFarmerNameHeader = 1;
                int ColFarmerNameEnd;
                int ColFarmerFatherHusbandNameHeader;
                int ColFarmerFatherHusbandNameEnd;
                int ColFarmerFatherHusbandName;
                int ColFarmerRegistrationIDHeader;
                int ColFarmerRegistrationIDEnd;
                int ColFarmerRegistrationIDName;
                int ColFmFarmerRegistrationDateHeader;
                int ColFmFarmerRegistrationDateEnd;
                int ColFmFarmerRegistrationDateName;
                int ColAddressHeader = 1;
                int ColAddressEnd;


                SetHeaderTextTop(ref sheet, ROW, ColFarmerNameHeader, "Contract No", 12, ExcelHAlign.HAlignLeft);
                ColFarmerNameHeader++;
                ColFarmerNameEnd = ColFarmerNameHeader + 1;
                sheet.Range[ROW, ColFarmerNameHeader, ROW, ColFarmerNameEnd].Text = data.Rows[0]["Id"].ToString();
                sheet.Range[ROW, ColFarmerNameHeader, ROW, ColFarmerNameEnd].Merge();
                sheet.Range[ROW, ColFarmerNameHeader, ROW, ColFarmerNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColFarmerNameHeader, ROW, ColFarmerNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColFarmerNameEnd++;

                ColFarmerFatherHusbandNameHeader = ColFarmerNameEnd;
                SetHeaderTextTop(ref sheet, ROW, ColFarmerFatherHusbandNameHeader, "Contract Date", 15, ExcelHAlign.HAlignLeft);
                ColFarmerFatherHusbandNameHeader++;
                ColFarmerFatherHusbandNameEnd = ColFarmerFatherHusbandNameHeader + 1;
                ColFarmerFatherHusbandName = ColFarmerFatherHusbandNameHeader;
                sheet.Range[ROW, ColFarmerFatherHusbandName, ROW, ColFarmerFatherHusbandNameEnd].Text = data.Rows[0]["TCDate"].ToString();
                sheet.Range[ROW, ColFarmerFatherHusbandName, ROW, ColFarmerFatherHusbandNameEnd].Merge();
                sheet.Range[ROW, ColFarmerFatherHusbandName, ROW, ColFarmerFatherHusbandNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColFarmerFatherHusbandName, ROW, ColFarmerFatherHusbandNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //            ROW++;
                ColFarmerFatherHusbandNameEnd++;

                ColFarmerRegistrationIDHeader = ColFarmerFatherHusbandNameEnd;
                SetHeaderTextTop(ref sheet, ROW, ColFarmerRegistrationIDHeader, "Process Start Date", 15, ExcelHAlign.HAlignLeft);
                ColFarmerRegistrationIDHeader++;
                ColFarmerRegistrationIDEnd = ColFarmerRegistrationIDHeader + 1;
                ColFarmerRegistrationIDName = ColFarmerRegistrationIDHeader;
                sheet.Range[ROW, ColFarmerRegistrationIDName, ROW, ColFarmerRegistrationIDEnd].Text = data.Rows[0]["TCPStartDate"].ToString();
                sheet.Range[ROW, ColFarmerRegistrationIDName, ROW, ColFarmerRegistrationIDEnd].Merge();
                sheet.Range[ROW, ColFarmerRegistrationIDName, ROW, ColFarmerRegistrationIDEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColFarmerRegistrationIDName, ROW, ColFarmerRegistrationIDEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //           ROW++;
                ColFarmerRegistrationIDEnd++;

                ColFmFarmerRegistrationDateHeader = ColFarmerRegistrationIDEnd;
                SetHeaderTextTop(ref sheet, ROW, ColFmFarmerRegistrationDateHeader, "Process End Date", 15, ExcelHAlign.HAlignLeft);
                ColFmFarmerRegistrationDateHeader++;
                ColFmFarmerRegistrationDateEnd = ColFmFarmerRegistrationDateHeader + 1;
                ColFmFarmerRegistrationDateName = ColFmFarmerRegistrationDateHeader;
                sheet.Range[ROW, ColFmFarmerRegistrationDateName, ROW, ColFmFarmerRegistrationDateEnd].Text = data.Rows[0]["TCPEndDate"].ToString();
                sheet.Range[ROW, ColFmFarmerRegistrationDateName, ROW, ColFmFarmerRegistrationDateEnd].Merge();
                sheet.Range[ROW, ColFmFarmerRegistrationDateName, ROW, ColFmFarmerRegistrationDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColFmFarmerRegistrationDateName, ROW, ColFmFarmerRegistrationDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //          ColFmFarmerRegistrationDateEnd++;


                SetHeaderTextTop(ref sheet, ROW, ColAddressHeader, "Contract Closing Date", 15, ExcelHAlign.HAlignLeft);
                ColAddressHeader++;
                ColAddressEnd = ColAddressHeader + 1;
                int ColAddress = ColAddressHeader;
                sheet.Range[ROW, ColAddressHeader, ROW, ColAddressEnd].Text = data.Rows[0]["TCCClosingDate"].ToString();
                sheet.Range[ROW, ColAddressHeader, ROW, ColAddressEnd].Merge();
                sheet.Range[ROW, ColAddressHeader, ROW, ColAddressEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColAddressHeader, ROW, ColAddressEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColAddressEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColAddressEnd, "Entity", 20, ExcelHAlign.HAlignLeft);
                ColAddressEnd++;
                int ColGender = ColAddressEnd;
                int ColGenderEnd = ColAddressEnd + 1;
                sheet.Range[ROW, ColGender, ROW, ColGenderEnd].Text = data.Rows[0]["Entity"].ToString();
                sheet.Range[ROW, ColGender, ROW, ColGenderEnd].Merge();
                sheet.Range[ROW, ColGender, ROW, ColGenderEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColGender, ROW, ColGenderEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColGenderEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColGenderEnd, "Party", 20, ExcelHAlign.HAlignLeft);
                ColGenderEnd++;
                int ColTotalArea = ColGenderEnd;
                int ColTotalAreaEnd = ColGenderEnd + 1;
                sheet.Range[ROW, ColTotalArea, ROW, ColTotalAreaEnd].Text = data.Rows[0]["Party"].ToString();
                sheet.Range[ROW, ColTotalArea, ROW, ColTotalAreaEnd].Merge();
                sheet.Range[ROW, ColTotalArea, ROW, ColTotalAreaEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColTotalArea, ROW, ColTotalAreaEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //   ROW++;
                ColTotalAreaEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColTotalAreaEnd, "Remarks", 20, ExcelHAlign.HAlignLeft);
                ColTotalAreaEnd++;
                int ColUOM = ColTotalAreaEnd;
                int ColUOMEnd = ColTotalAreaEnd + 1;
                sheet.Range[ROW, ColUOM, ROW, ColUOMEnd].Text = data.Rows[0]["Remarks"].ToString();
                sheet.Range[ROW, ColUOM, ROW, ColUOMEnd].Merge();
                sheet.Range[ROW, ColUOM, ROW, ColUOMEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColUOM, ROW, ColUOMEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

            }

            #region Headers

            report.SetHeaderText(ref sheet, ROW, COL, "RECEIPT QUANTITY", 12, ExcelHAlign.HAlignLeft);
            ROW++;

            report.SetHeaderText(ref sheet, ROW, COL, "JW Output Id", 8, ExcelHAlign.HAlignLeft);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "JW Output Item", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkItem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 12, ExcelHAlign.HAlignLeft);
            int ColArticleCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Job Work Activity", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkActivity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Unit", 15, ExcelHAlign.HAlignLeft);
            int ColOutputUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Planned Quantity", 15, ExcelHAlign.HAlignLeft);
            int ColPlannedQuantity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Receipt Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColTotalReceiptQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate/ Unit", 15, ExcelHAlign.HAlignLeft);
            int ColRatePerUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Value", 15, ExcelHAlign.HAlignLeft);
            int ColTotalValue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate Apply", 15, ExcelHAlign.HAlignLeft);
            int ColRateApply = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 15, ExcelHAlign.HAlignLeft);
            int ColRemarks = COL;
            ROW++;


            endCol = COL;
            #endregion Headers

            string MPId = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (MPId != data.Rows[i]["MPId"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        //      sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].Merge();
                        sheet.Range[RowIndex, ColId, ROW - 1, ColId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColId, ROW - 1, ColId].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    }
                    RowIndex = ROW;
                }

                sheet[ROW, ColId].Text = data.Rows[i]["MPId"].ToString();
                sheet[ROW, ColJobWorkItem].Text = data.Rows[i]["JWOutputItem"].ToString();
                sheet[ROW, ColArticleCode].Text = data.Rows[i]["Article"].ToString();
                sheet[ROW, ColJobWorkActivity].Text = data.Rows[i]["JWActivity"].ToString();
                sheet[ROW, ColOutputUnit].Text = data.Rows[i]["OutputUnit"].ToString();

                sheet[ROW, ColPlannedQuantity].Number = clsStaticInfo.dbl(data.Rows[i]["PlannedQuantity"].ToString());
                sheet[ROW, ColTotalReceiptQty].Number = clsStaticInfo.dbl(data.Rows[i]["TotalReceivedQty"].ToString());

            //    sheet[ROW, ColDiff].Number = clsStaticInfo.dbl(data.Rows[i]["TotalBalance"].ToString());

                sheet[ROW, ColRatePerUnit].Number = clsStaticInfo.dbl(data.Rows[i]["RatePerUnit"].ToString());
                sheet[ROW, ColTotalValue].Number = clsStaticInfo.dbl(data.Rows[i]["TotalValue"].ToString());
                sheet[ROW, ColRateApply].Text = data.Rows[i]["RateApplyId"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["MPRemarks"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                MPId = data.Rows[i]["MPId"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                //      sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].Merge();
                sheet.Range[RowIndex, ColId, ROW - 1, ColId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColId, ROW - 1, ColId].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColArticleCode, ROW - 1, ColArticleCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColArticleCode, ROW - 1, ColArticleCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJobWorkActivity, ROW - 1, ColJobWorkActivity].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJobWorkActivity, ROW - 1, ColJobWorkActivity].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColOutputUnit, ROW - 1, ColOutputUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColOutputUnit, ROW - 1, ColOutputUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                //sheet.Range[RowIndex, ColTotalIssuedQty, ROW - 1, ColTotalIssuedQty].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet.Range[RowIndex, ColTotalIssuedQty, ROW - 1, ColTotalIssuedQty].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColTotalReceiptQty, ROW - 1, ColTotalReceiptQty].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColTotalReceiptQty, ROW - 1, ColTotalReceiptQty].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                //sheet.Range[RowIndex, ColDiff, ROW - 1, ColDiff].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet.Range[RowIndex, ColDiff, ROW - 1, ColDiff].HorizontalAlignment = ExcelHAlign.HAlignLeft;



                sheet.Range[RowIndex, ColRatePerUnit, ROW - 1, ColRatePerUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColRatePerUnit, ROW - 1, ColRatePerUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColRateApply, ROW - 1, ColRateApply].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColRateApply, ROW - 1, ColRateApply].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColRemarks, ROW - 1, ColRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColRemarks, ROW - 1, ColRemarks].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            }

            // CHILD DATA

            int MIChildROW = ROW + 1;
            int MIChildendCol = 1;
            int MIChildCOL = 1;

            #region Material Input Child Headers

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "ISSUE/ RETURN QUANTITY", 12, ExcelHAlign.HAlignLeft);
            MIChildROW++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "JW Output Id", 12, ExcelHAlign.HAlignLeft);
            int ColMaterial = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "JW Input Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColMatSpecification = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "JW Input Item", 12, ExcelHAlign.HAlignLeft);
            int ColMIJWInputItem = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "JW Input Material", 15, ExcelHAlign.HAlignLeft);
            int ColMICUOM = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "JW Input Article", 15, ExcelHAlign.HAlignLeft);
            int ColMIJWInputArticle = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Unit", 15, ExcelHAlign.HAlignLeft);
            int ColMIJWUnit = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Gross Quantity to Issue", 12, ExcelHAlign.HAlignLeft);
            int ColMIRejection = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Total Issued Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColNetConsumptionOutputUnit = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Balance", 12, ExcelHAlign.HAlignLeft);
            int ColMIValueLoss = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Remarks", 12, ExcelHAlign.HAlignLeft);
            int ColMIRemarks = MIChildCOL;
            MIChildROW++;

            MIChildendCol = MIChildCOL;
            #endregion Headers

            string Material = "";
            var MIStartRows = 0;
            var MIEndRows = 0;
            int MIRowIndexNo = MIChildROW;
            MIStartRows = MIChildROW;

            for (int i = 0; i < Childdata.Rows.Count; i++)
            {

                if (Material != Childdata.Rows[i]["LineItemId"].ToString())
                {

                    if (MIRowIndexNo < MIChildROW)
                    {
                        //sheet1.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].Merge();
                        sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    MIRowIndexNo = MIChildROW;
                }


                sheet[MIChildROW, ColMaterial].Text = Childdata.Rows[i]["LineItemId"].ToString();
                sheet[MIChildROW, ColMatSpecification].Text = Childdata.Rows[i]["Id"].ToString();
                sheet[MIChildROW, ColMIJWInputItem].Text = Childdata.Rows[i]["JWInputItem"].ToString();
                sheet[MIChildROW, ColMICUOM].Text = Childdata.Rows[i]["JWInputMaterial"].ToString();
                sheet[MIChildROW, ColMIJWInputArticle].Text = Childdata.Rows[i]["JWInputArticle"].ToString();
                sheet[MIChildROW, ColMIJWUnit].Text = Childdata.Rows[i]["Unit"].ToString();
                sheet[MIChildROW, ColNetConsumptionOutputUnit].Number = clsStaticInfo.dbl(Childdata.Rows[i]["TotalIssuedQuantity"].ToString());
                sheet[MIChildROW, ColMIRejection].Number = clsStaticInfo.dbl(Childdata.Rows[i]["TotalGross"].ToString());
                sheet[MIChildROW, ColMIValueLoss].Number = clsStaticInfo.dbl(Childdata.Rows[i]["Balance"].ToString());
                sheet[MIChildROW, ColMIRemarks].Text = Childdata.Rows[i]["Remarks"].ToString();

                sheet.Range[MIChildROW, 1, MIChildROW, MIChildendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[MIChildROW, 1, MIChildROW, MIChildendCol].BorderAround(ExcelLineStyle.Hair);
                Material = Childdata.Rows[i]["LineItemId"].ToString();

                MIChildROW++;
            }

            MIEndRows = MIChildROW - 1;

            if (MIRowIndexNo < MIChildROW - 1)
            {
                //sheet1.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].Merge();
                sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            // DATE WISE ISSUE/ RETURN

            int DateWiseROW = MIChildROW + 1;
            int DateWiseendCol = 1;
            int DateWiseCOL = 1;

            #region DATE WISE ISSUE/ RETURN Headers

            report.SetHeaderText(ref sheet, DateWiseROW, DateWiseCOL, "DATE WISE ISSUE/ RETURN", 12, ExcelHAlign.HAlignLeft);
            DateWiseROW++;

            report.SetHeaderText(ref sheet, DateWiseROW, DateWiseCOL, "JW Output Id", 12, ExcelHAlign.HAlignLeft);
            int ColMaterialPlanningId = DateWiseCOL;
            DateWiseCOL++;

            report.SetHeaderText(ref sheet, DateWiseROW, DateWiseCOL, "JW Input Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColMaterialInputId = DateWiseCOL;
            DateWiseCOL++;

            report.SetHeaderText(ref sheet, DateWiseROW, DateWiseCOL, "Issue Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColIssueChildId = DateWiseCOL;
            DateWiseCOL++;

            report.SetHeaderText(ref sheet, DateWiseROW, DateWiseCOL, "Issue Date", 15, ExcelHAlign.HAlignLeft);
            int ColIssueDate = DateWiseCOL;
            DateWiseCOL++;

            report.SetHeaderText(ref sheet, DateWiseROW, DateWiseCOL, "Material", 12, ExcelHAlign.HAlignLeft);
            int ColDWMaterial = DateWiseCOL;
            DateWiseCOL++;

            report.SetHeaderText(ref sheet, DateWiseROW, DateWiseCOL, "Article", 12, ExcelHAlign.HAlignLeft);
            int ColDWInputArticle = DateWiseCOL;
            DateWiseCOL++;

            report.SetHeaderText(ref sheet, DateWiseROW, DateWiseCOL, "Total Issued Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColTotalIssuedQuantity = DateWiseCOL;
            DateWiseCOL++;

            report.SetHeaderText(ref sheet, DateWiseROW, DateWiseCOL, "Remarks", 12, ExcelHAlign.HAlignLeft);
            int ColDWRemarks = DateWiseCOL;
            DateWiseROW++;

            DateWiseendCol = DateWiseCOL;
            #endregion Headers

            string DWMaterial = "";
            var DWStartRows = 0;
            var DWEndRows = 0;
            int DWRowIndexNo = DateWiseROW;
            DWStartRows = DateWiseROW;

            for (int i = 0; i < DateWiseIssuedata.Rows.Count; i++)
            {

                if (DWMaterial != DateWiseIssuedata.Rows[i]["MaterialInputId"].ToString())
                {

                    if (DWRowIndexNo < DateWiseROW)
                    {
                        //sheet1.Range[DWRowIndexNo, ColMaterial, DateWiseROW - 1, ColMaterial].Merge();
                        sheet.Range[DWRowIndexNo, ColMaterialPlanningId, DateWiseROW - 1, ColMaterialPlanningId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[DWRowIndexNo, ColMaterialPlanningId, DateWiseROW - 1, ColMaterialPlanningId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    DWRowIndexNo = DateWiseROW;
                }

                sheet[DateWiseROW, ColMaterialPlanningId].Text = DateWiseIssuedata.Rows[i]["MaterialPlanningId"].ToString();
                sheet[DateWiseROW, ColMaterialInputId].Text = DateWiseIssuedata.Rows[i]["MaterialInputId"].ToString();
                sheet[DateWiseROW, ColIssueChildId].Text = DateWiseIssuedata.Rows[i]["IssueChildId"].ToString();
                sheet[DateWiseROW, ColIssueDate].Text = DateWiseIssuedata.Rows[i]["IssueDate"].ToString();
                sheet[DateWiseROW, ColDWMaterial].Text = DateWiseIssuedata.Rows[i]["Material"].ToString();
                sheet[DateWiseROW, ColDWInputArticle].Text = DateWiseIssuedata.Rows[i]["InputArticle"].ToString();
                sheet[DateWiseROW, ColTotalIssuedQuantity].Number = clsStaticInfo.dbl(DateWiseIssuedata.Rows[i]["TotalIssuedQuantity"].ToString());
                sheet[DateWiseROW, ColDWRemarks].Text = DateWiseIssuedata.Rows[i]["Remarks"].ToString();

                sheet.Range[DateWiseROW, 1, DateWiseROW, DateWiseendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[DateWiseROW, 1, DateWiseROW, DateWiseendCol].BorderAround(ExcelLineStyle.Hair);
                DWMaterial = DateWiseIssuedata.Rows[i]["MaterialInputId"].ToString();

                DateWiseROW++;
            }

            DWEndRows = DateWiseROW - 1;

            if (DWRowIndexNo < DateWiseROW - 1)
            {
                //sheet1.Range[DWRowIndexNo, ColMaterial, DateWiseROW - 1, ColMaterial].Merge();
                sheet.Range[DWRowIndexNo, ColMaterialPlanningId, DateWiseROW - 1, ColMaterialPlanningId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[DWRowIndexNo, ColMaterialPlanningId, DateWiseROW - 1, ColMaterialPlanningId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            // DATE WISE RECEIVED DATA

            int DateWiseRECEIVEDROW = DateWiseROW + 1;
            int DateWiseRECEIVEDendCol = 1;
            int DateWiseRECEIVEDCOL = 1;

            #region DATE WISE RECEIVED Headers

            report.SetHeaderText(ref sheet, DateWiseRECEIVEDROW, DateWiseRECEIVEDCOL, "DATE WISE RECEIPT", 12, ExcelHAlign.HAlignLeft);
            DateWiseRECEIVEDROW++;

            report.SetHeaderText(ref sheet, DateWiseRECEIVEDROW, DateWiseRECEIVEDCOL, "JW Output Id", 12, ExcelHAlign.HAlignLeft);
            int ColRMaterialPlanningId = DateWiseRECEIVEDCOL;
            DateWiseRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, DateWiseRECEIVEDROW, DateWiseRECEIVEDCOL, "Receipt Id", 12, ExcelHAlign.HAlignLeft);
            int ColRId = DateWiseRECEIVEDCOL;
            DateWiseRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, DateWiseRECEIVEDROW, DateWiseRECEIVEDCOL, "Receipt Date", 15, ExcelHAlign.HAlignLeft);
            int ColReceiptDate = DateWiseRECEIVEDCOL;
            DateWiseRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, DateWiseRECEIVEDROW, DateWiseRECEIVEDCOL, "JW Output Item", 15, ExcelHAlign.HAlignLeft);
            int ColRJWOutputItem = DateWiseRECEIVEDCOL;
            DateWiseRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, DateWiseRECEIVEDROW, DateWiseRECEIVEDCOL, "Material", 15, ExcelHAlign.HAlignLeft);
            int ColRJWOutputMaterial = DateWiseRECEIVEDCOL;
            DateWiseRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, DateWiseRECEIVEDROW, DateWiseRECEIVEDCOL, "Article", 15, ExcelHAlign.HAlignLeft);
            int ColRJWOutputArticle = DateWiseRECEIVEDCOL;
            DateWiseRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, DateWiseRECEIVEDROW, DateWiseRECEIVEDCOL, "Total Receipt Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColTotalReceiptQuantity = DateWiseRECEIVEDCOL;
            DateWiseRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, DateWiseRECEIVEDROW, DateWiseRECEIVEDCOL, "Remarks", 12, ExcelHAlign.HAlignLeft);
            int ColRRemarks = DateWiseRECEIVEDCOL;
            DateWiseRECEIVEDROW++;

            DateWiseRECEIVEDendCol = DateWiseRECEIVEDCOL;
            #endregion Headers

            string DWRMaterial = "";
            var DWRStartRows = 0;
            var DWREndRows = 0;
            int DWRRowIndexNo = DateWiseRECEIVEDROW;
            DWRStartRows = DateWiseRECEIVEDROW;

            for (int i = 0; i < DateWiseReceiveddata.Rows.Count; i++)
            {

                if (DWRMaterial != DateWiseReceiveddata.Rows[i]["MaterialPlanningId"].ToString())
                {

                    if (DWRRowIndexNo < DateWiseRECEIVEDROW)
                    {
                        //sheet1.Range[DWRRowIndexNo, ColMaterial, DateWiseRECEIVEDROW - 1, ColMaterial].Merge();
                        sheet.Range[DWRRowIndexNo, ColRMaterialPlanningId, DateWiseRECEIVEDROW - 1, ColRMaterialPlanningId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[DWRRowIndexNo, ColRMaterialPlanningId, DateWiseRECEIVEDROW - 1, ColRMaterialPlanningId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    DWRRowIndexNo = DateWiseRECEIVEDROW;
                }

                sheet[DateWiseRECEIVEDROW, ColRMaterialPlanningId].Text = DateWiseReceiveddata.Rows[i]["MaterialPlanningId"].ToString();
                sheet[DateWiseRECEIVEDROW, ColRId].Text = DateWiseReceiveddata.Rows[i]["Id"].ToString();

                sheet[DateWiseRECEIVEDROW, ColReceiptDate].Text = DateWiseReceiveddata.Rows[i]["ReceiptDate"].ToString();
                sheet[DateWiseRECEIVEDROW, ColRJWOutputItem].Text = DateWiseReceiveddata.Rows[i]["JWOutputItem"].ToString();
                sheet[DateWiseRECEIVEDROW, ColRJWOutputMaterial].Text = DateWiseReceiveddata.Rows[i]["JWOutputMaterial"].ToString();
                sheet[DateWiseRECEIVEDROW, ColRJWOutputArticle].Text = DateWiseReceiveddata.Rows[i]["JWOutputArticle"].ToString();

                sheet[DateWiseRECEIVEDROW, ColTotalReceiptQuantity].Number = clsStaticInfo.dbl(DateWiseReceiveddata.Rows[i]["TotalReceiptQuantity"].ToString());
                sheet[DateWiseRECEIVEDROW, ColRRemarks].Text = DateWiseReceiveddata.Rows[i]["Remarks"].ToString();

                sheet.Range[DateWiseRECEIVEDROW, 1, DateWiseRECEIVEDROW, DateWiseRECEIVEDendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[DateWiseRECEIVEDROW, 1, DateWiseRECEIVEDROW, DateWiseRECEIVEDendCol].BorderAround(ExcelLineStyle.Hair);
                DWRMaterial = DateWiseReceiveddata.Rows[i]["MaterialPlanningId"].ToString();

                DateWiseRECEIVEDROW++;
            }

            DWREndRows = DateWiseRECEIVEDROW - 1;

            if (DWRRowIndexNo < DateWiseRECEIVEDROW - 1)
            {
                //sheet1.Range[DWRRowIndexNo, ColMaterial, DateWiseRECEIVEDROW - 1, ColMaterial].Merge();
                sheet.Range[DWRRowIndexNo, ColRMaterialPlanningId, DateWiseRECEIVEDROW - 1, ColRMaterialPlanningId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[DWRRowIndexNo, ColRMaterialPlanningId, DateWiseRECEIVEDROW - 1, ColRMaterialPlanningId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }


            // BY PRODUCT RECEIVE

            int ByProductRECEIVEDROW = DateWiseRECEIVEDROW + 1;
            int ByProductRECEIVEDendCol = 1;
            int ByProductRECEIVEDCOL = 1;

            #region BY PRODUCT RECEIVED Headers

            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "BY PRODUCT", 12, ExcelHAlign.HAlignLeft);
            ByProductRECEIVEDROW++;

            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "JW Output Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColMPLineItemId = ByProductRECEIVEDCOL;
            ByProductRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "JW Output Item", 12, ExcelHAlign.HAlignLeft);
            int ColJWOutputItem = ByProductRECEIVEDCOL;
            ByProductRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "JW Input Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColInputLineItemId = ByProductRECEIVEDCOL;
            ByProductRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "JW Input Item", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputItem = ByProductRECEIVEDCOL;
            ByProductRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "By Product Item Id", 15, ExcelHAlign.HAlignLeft);
            int ColBPId = ByProductRECEIVEDCOL;
            ByProductRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "By Product Item", 12, ExcelHAlign.HAlignLeft);
            int ColJWByProductItem = ByProductRECEIVEDCOL;
            ByProductRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "By Product Material", 15, ExcelHAlign.HAlignLeft);
            int ColBPByProductMaterial = ByProductRECEIVEDCOL;
            ByProductRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "By Product Article", 12, ExcelHAlign.HAlignLeft);
            int ColJWByProductArticle = ByProductRECEIVEDCOL;
            ByProductRECEIVEDCOL++;



            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "Total Required Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColTotalReqQty = ByProductRECEIVEDCOL;
            ByProductRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "Total Received Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColTotalReceivedQty = ByProductRECEIVEDCOL;
            ByProductRECEIVEDCOL++;

            report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "To Receive", 12, ExcelHAlign.HAlignLeft);
            int ColToReceive = ByProductRECEIVEDCOL;
            ByProductRECEIVEDROW++;

            //report.SetHeaderText(ref sheet, ByProductRECEIVEDROW, ByProductRECEIVEDCOL, "Remarks", 12, ExcelHAlign.HAlignLeft);
            //int ColRRemarks = ByProductRECEIVEDCOL;
            //ByProductRECEIVEDROW++;

            ByProductRECEIVEDendCol = ByProductRECEIVEDCOL;
            #endregion Headers

            string BYProdMPId = "";
            var BYProdStartRows = 0;
            var BYProdEndRows = 0;
            int BYProdRowIndexNo = ByProductRECEIVEDROW;
            BYProdStartRows = ByProductRECEIVEDROW;

            for (int i = 0; i < ByProductReceiveddata.Rows.Count; i++)
            {

                if (BYProdMPId != ByProductReceiveddata.Rows[i]["MPLineItemId"].ToString())
                {

                    if (BYProdRowIndexNo < ByProductRECEIVEDROW)
                    {
                        //sheet1.Range[BYProdRowIndexNo, ColMaterial, ByProductRECEIVEDROW - 1, ColMaterial].Merge();
                        sheet.Range[BYProdRowIndexNo, ColMPLineItemId, ByProductRECEIVEDROW - 1, ColMPLineItemId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[BYProdRowIndexNo, ColMPLineItemId, ByProductRECEIVEDROW - 1, ColMPLineItemId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    BYProdRowIndexNo = ByProductRECEIVEDROW;
                }

                sheet[ByProductRECEIVEDROW, ColMPLineItemId].Text = ByProductReceiveddata.Rows[i]["MPLineItemId"].ToString();
                sheet[ByProductRECEIVEDROW, ColInputLineItemId].Text = ByProductReceiveddata.Rows[i]["InputLineItemId"].ToString();
                sheet[ByProductRECEIVEDROW, ColJWOutputItem].Text = ByProductReceiveddata.Rows[i]["JWOutputItem"].ToString();
                sheet[ByProductRECEIVEDROW, ColJWInputItem].Text = ByProductReceiveddata.Rows[i]["JWInputItem"].ToString();
                sheet[ByProductRECEIVEDROW, ColBPId].Text = ByProductReceiveddata.Rows[i]["Id"].ToString();
                sheet[ByProductRECEIVEDROW, ColJWByProductItem].Text = ByProductReceiveddata.Rows[i]["ByProductItem"].ToString();
                sheet[ByProductRECEIVEDROW, ColBPByProductMaterial].Text = ByProductReceiveddata.Rows[i]["ByProductMaterial"].ToString();
                sheet[ByProductRECEIVEDROW, ColJWByProductArticle].Text = ByProductReceiveddata.Rows[i]["ByProductArticle"].ToString();
                sheet[ByProductRECEIVEDROW, ColTotalReqQty].Number = clsStaticInfo.dbl(ByProductReceiveddata.Rows[i]["TotalReqQty"].ToString());
                sheet[ByProductRECEIVEDROW, ColTotalReceivedQty].Number = clsStaticInfo.dbl(ByProductReceiveddata.Rows[i]["TotalReceivedQty"].ToString());
                sheet[ByProductRECEIVEDROW, ColToReceive].Number = clsStaticInfo.dbl(ByProductReceiveddata.Rows[i]["ToReceive"].ToString());

                sheet.Range[ByProductRECEIVEDROW, 1, ByProductRECEIVEDROW, ByProductRECEIVEDendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ByProductRECEIVEDROW, 1, ByProductRECEIVEDROW, ByProductRECEIVEDendCol].BorderAround(ExcelLineStyle.Hair);
                BYProdMPId = ByProductReceiveddata.Rows[i]["MPLineItemId"].ToString();

                ByProductRECEIVEDROW++;
            }

            BYProdEndRows = ByProductRECEIVEDROW - 1;

            if (BYProdRowIndexNo < ByProductRECEIVEDROW - 1)
            {
                //sheet1.Range[BYProdRowIndexNo, ColMaterial, ByProductRECEIVEDROW - 1, ColMaterial].Merge();
                sheet.Range[BYProdRowIndexNo, ColMPLineItemId, ByProductRECEIVEDROW - 1, ColMPLineItemId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[BYProdRowIndexNo, ColMPLineItemId, ByProductRECEIVEDROW - 1, ColMPLineItemId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            report.CompanyPlantHeader(ref sheet, endCol, "Material Reconcilation Transformation", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

    }
}