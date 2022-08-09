#region lib
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos;
using Aplos.Properties;
using Library.HumanResource.NewOTProcess;
using Library.Data.Sql;
using Library.Security.Core;
using Aplos.Controllers;
using OTSBD;
using Syncfusion.XlsIO;
using System.IO;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using Library.HumanResource.Employee;
#endregion lib

namespace Aplos.Areas.HumanResource.Controllers
{
    public class FurniturePolicyReportController : Aplos.Controllers.BaseController
    {
        FurniturePolicyReportService fpr = new FurniturePolicyReportService();
        private readonly ISqlRepository _sqlRepository;

        public FurniturePolicyReportController(ISqlRepository R )
        { _sqlRepository = R; }

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getDesignation(string employeeCategoryId)
        {
            try
            {
                return Json(fpr.getDesignation(employeeCategoryId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getEmployeeCategory()
        {
            try
            {
                return Json(fpr.getEmployeeCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize,HttpPost]
        public ActionResult getPolicyGrid(string designationId)
        {
            try
            {
                return Json(fpr.getPolicyGrid(designationId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region -- Furniture Wise Report



        [Authorize,HttpPost]
        public ActionResult XlsFurnitureWiseReport(string designationId)
        {
            try
            {
                var workbook = FurnitureReport(designationId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "FurniturePolicyReport.xlsx";
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
        private IWorkbook FurnitureReport(string designationId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = fpr.furnitureWiseReport(designationId);

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Furniture Policy";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Policy Head", 12, ExcelHAlign.HAlignCenter);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Srl No.", 12, ExcelHAlign.HAlignCenter);
            int ColSrlNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Policy Name", 12, ExcelHAlign.HAlignCenter);
            int ColPolicyName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 12, ExcelHAlign.HAlignCenter);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Category", 20, ExcelHAlign.HAlignCenter);
            int ColCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Category", 20, ExcelHAlign.HAlignCenter);
            int ColSubCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Furniture", 12, ExcelHAlign.HAlignCenter);
            int ColFurniture = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Grade", 12, ExcelHAlign.HAlignCenter);
            int ColType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Quantity", 12, ExcelHAlign.HAlignCenter);
            int ColQuantity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Budget", 20, ExcelHAlign.HAlignCenter);
            int ColBudget = COL;
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
                    sheet[ROW, ColSrlNo].Text = data.Rows[i]["sequence"].ToString();
                    sheet[ROW, ColPolicyName].Text = data.Rows[i]["PolicyName"].ToString();

                    sheet[ROW, ColCategory].Text = data.Rows[i]["Category"].ToString();
                    sheet[ROW, ColSubCategory].Text = data.Rows[i]["SubCategory"].ToString();
                    sheet[ROW, ColFurniture].Text = data.Rows[i]["Furniture"].ToString();
                    sheet[ROW, ColType].Text = data.Rows[i]["Type"].ToString();
                    sheet[ROW, ColBudget].Number = Library.Security.Core.clsStaticInfo.dbl(data.Rows[i]["Budget"].ToString());
                    sheet[ROW, ColQuantity].Number = Library.Security.Core.clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString());
                    sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();

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
            reportUtility.CompanyHeader(ref sheet, endCol, "Furniture Policy Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion -- Furniture Wise Report  

        #region -- Designation Wise Report
        /*
        [HttpPost, Authorize]
        public ActionResult XlsDesignationWiseReport()
        {
            try
            {
                var workbook = FurnitureReport();

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "FurnitureWiseReport.xlsx";
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
        private IWorkbook FurniturePolicyDesignationReport()
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = fpr.designationWiseReport();

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Furniture Master";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Id", 12, ExcelHAlign.HAlignCenter);
            int ColId = COL;
            COL++;



            report.SetHeaderText(ref sheet, ROW, COL, "Furniture", 12, ExcelHAlign.HAlignCenter);
            int ColStandardName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "User Name", 12, ExcelHAlign.HAlignCenter);
            int ColUserName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Budget", 20, ExcelHAlign.HAlignCenter);
            int ColBudget = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Category", 20, ExcelHAlign.HAlignCenter);
            int ColCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Category", 12, ExcelHAlign.HAlignCenter);
            int ColSubCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Description", 12, ExcelHAlign.HAlignCenter);
            int ColDescription = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 20, ExcelHAlign.HAlignCenter);
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

                sheet[ROW, ColStandardName].Text = data.Rows[i]["StandardName"].ToString();
                sheet[ROW, ColUserName].Text = data.Rows[i]["UserName"].ToString();
                sheet[ROW, ColBudget].Number = Library.Security.Core.clsStaticInfo.dbl(data.Rows[i]["Budget"].ToString());
                sheet[ROW, ColCategory].Text = data.Rows[i]["Category"].ToString();
                sheet[ROW, ColSubCategory].Text = data.Rows[i]["SubCategory"].ToString();
                sheet[ROW, ColDescription].Text = data.Rows[i]["Description"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();



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
            reportUtility.CompanyHeader(ref sheet, endCol, "Furniture Wise Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }*/
        #endregion -- Designation Wise Report  
    }
}