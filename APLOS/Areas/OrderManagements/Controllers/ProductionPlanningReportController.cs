using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using Library.Planning.PlanningType1;
using Library.Service.Helpers;
using Library.Model.Enums;
using Library.Security.Core;
using System.IO;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ProductionPlanningReportController : BaseController
    {
        ProductionPlanningReportService rep = new ProductionPlanningReportService();

        public ProductionPlanningReportController()
        {
            rep = new ProductionPlanningReportService();
        }


        public ActionResult Aplos()
        {
            return View();
        }
             

        [HttpGet, Authorize]
        public JsonResult GetSnapShotData(string From,string To,string SnapShotType,string SnapId)
        {
            try
            {
                var jsondata = Json(new { Error = false, DATA = rep.GetSnapShotData(From, To, SnapShotType, SnapId) }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch(Exception ex)
            {
                return Json(new { Error =true,Message= ex.Message },JsonRequestBehavior.AllowGet);
               
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetSnapShotNames(string SnapShotType)
        {
            try {
                return Json(new { Error = false, DATA = rep.GetSnapShotNames(SnapShotType) }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost, Authorize]
        public ActionResult GetPrintReport(string From, string To, string SnapShotType, 
            string CompanyId,string SnapDate,string PlantId,string EntityId,string ProcessId,string SnapName,string WkCenterId,
            string CustomerId,string POId)
        {

            try
            {
                var workbook = GetSnapShotFilterData(From, To, SnapShotType,
                 CompanyId, SnapDate, PlantId, EntityId, ProcessId, SnapName, WkCenterId,
                 CustomerId, POId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "Production-Planning.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        } 

      
        private IWorkbook GetSnapShotFilterData(string From, string To, string SnapShotType,
            string CompanyId, string SnapDate, string PlantId, string EntityId, string ProcessId, string SnapName, string WkCenterId,
            string CustomerId, string POId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];           
            sheet.Name = "ProductionPlanning";

            
            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            DataTable data = rep.GetReportData(From, To, SnapShotType,
             CompanyId,  SnapDate,  PlantId,  EntityId,  ProcessId,  SnapName,  WkCenterId,
             CustomerId,  POId);

                #region Headers
                report.SetHeaderText(ref sheet, ROW, COL, "Company", 12, ExcelHAlign.HAlignCenter);
                int ColCompany = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Plant", 12, ExcelHAlign.HAlignCenter);
                int ColPlant = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Entity", 12, ExcelHAlign.HAlignCenter);
                int ColEntity = COL;
                COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "WorkCenter", 12, ExcelHAlign.HAlignCenter);
                int ColWorkCenter = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Process", 12, ExcelHAlign.HAlignCenter);
                int ColProcess = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Snapshot Date", 13, ExcelHAlign.HAlignCenter);
                int ColSnapshotDate = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Snapshot Name", 12, ExcelHAlign.HAlignCenter);
                int ColSnapshotName = COL;
                COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "ProductionOrder Id", 13, ExcelHAlign.HAlignCenter);
                int ColPOId = COL;
                COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "Customer", 12, ExcelHAlign.HAlignCenter);
                int ColCustomer = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Production Date", 13, ExcelHAlign.HAlignCenter);
                int ColProducnDate = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Quantity", 12, ExcelHAlign.HAlignCenter);
                int ColQuantity = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Production Hours", 12, ExcelHAlign.HAlignCenter);
                int ColProducnHours = COL;
                COL++;

              

                report.SetHeaderText(ref sheet, ROW, COL, "IsBuildUp", 12, ExcelHAlign.HAlignCenter);
                int ColIsBuildUp = COL;
                ROW++;
                endCol = COL;
                #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColCompany].Text = data.Rows[i]["Company"].ToString();
                sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                sheet[ROW, ColWorkCenter].Text = data.Rows[i]["WorkCenter"].ToString();
                sheet[ROW, ColSnapshotDate].Text = data.Rows[i]["SnapshotDate"].ToString();
                sheet[ROW, ColSnapshotName].Text = data.Rows[i]["SnapshotName"].ToString();
                sheet[ROW, ColPOId].Text = data.Rows[i]["ProductionOrderID"].ToString();
                sheet[ROW, ColCustomer].Text = data.Rows[i]["Customer"].ToString();
                sheet[ROW, ColProducnDate].Text = data.Rows[i]["ProductionDate"].ToString();
                sheet[ROW, ColIsBuildUp].Text = data.Rows[i]["isBuildUp"].ToString();
                sheet[ROW, ColProducnHours].Number = clsStaticInfo.dbl(data.Rows[i]["ProductionHours"].ToString());
                sheet[ROW, ColQuantity].Number = clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
               
                ROW++;

            }
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Production Planning", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);

            return workbook;
        }          
            
    }
}
 