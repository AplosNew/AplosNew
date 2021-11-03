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
using Library.Service.Productions;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class MovementMaterialMasterController : BaseController
    {
        MovementMasterData det = new MovementMasterData();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public MovementMaterialMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
            det = new MovementMasterData();
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult GetItem()
        {
            try
            {
                return Json(det.GetItem(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult GetStorageLoc(string PlantId, string CompId)
        {
            try
            {
                return Json(det.GetStorageLoc(PlantId, CompId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpGet]
        public ActionResult getPurposeCategory()
        {
            try
            {
                return Json(det.getPurposeCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }





        [Authorize, HttpPost]
        public ActionResult LoadAll(string Id)
        {
            try
            {
                return Json(new { master = det.LoadAll(Id) /*employee = det.LoadAllEmp(Id)*/ }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult Save(MovementMasterList masterdata) //List<MovementEmpList> employeedata)
        {
            try
            {
                det.saveData(masterdata);//, employeedata);
                return Json(new { Message = "Data Saved Successfully !!", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public JsonResult Delete(string id)
        {
            try
            {
                det.delete(id);
                return Json(new { Message = "Data deleted successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            try
            {
                return Json(det.GetList(column, value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPrintReport(string column, string value)
        {
            try
            {
                var workbook = GetData(column, value);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "MovementMaterialMasterReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private IWorkbook GetData(string column, string value)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            sheet.Name = "Movement Material Master Report";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            DataTable data = det.GetListRpt(column, value);
            //DataTable data = new DataTable();
            #region Headers


            report.SetHeaderText(ref sheet, ROW, COL, "Company", 15, ExcelHAlign.HAlignCenter);
            int ColCompany = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 13, ExcelHAlign.HAlignCenter);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "From Location", 13, ExcelHAlign.HAlignCenter);
            int ColFromLocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "To Location", 13, ExcelHAlign.HAlignCenter);
            int ColToLocation = COL;
            COL++;



            report.SetHeaderText(ref sheet, ROW, COL, "Movement Category", 15, ExcelHAlign.HAlignCenter);
            int ColMovementCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 13, ExcelHAlign.HAlignCenter);
            int ColEntity = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Item", 13, ExcelHAlign.HAlignCenter);
            int ColItem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "From Storage Location", 16, ExcelHAlign.HAlignCenter);
            int ColFromStorageLoc = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "To Storage Location", 15, ExcelHAlign.HAlignCenter);
            int ColToStorageLoc = COL;

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
                sheet[ROW, ColFromLocation].Text = data.Rows[i]["FromLocation"].ToString();
                sheet[ROW, ColToLocation].Text = data.Rows[i]["ToLocation"].ToString();

                sheet[ROW, ColMovementCategory].Text = data.Rows[i]["MovementCategory"].ToString();
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColItem].Text = data.Rows[i]["Item"].ToString();
                sheet[ROW, ColFromStorageLoc].Text = data.Rows[i]["FromStorageLoc"].ToString();
                sheet[ROW, ColToStorageLoc].Text = data.Rows[i]["ToStorageLoc"].ToString();
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyGroupHeader(ref sheet, endCol, "Movement Material Master Report", identity.CompanyGroupId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);

            return workbook;
        }

    }
}