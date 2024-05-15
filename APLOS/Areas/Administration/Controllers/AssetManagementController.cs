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

namespace Aplos.Areas.Administration.Controllers
{
    public class AssetManagementController : BaseController
    {
        AssetManagementService fm = new AssetManagementService();

        private readonly ISqlRepository _sqlRepository;

        public AssetManagementController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        string TableName = "[HKP].[EmpDocAssetMaster]";
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = fm.Get(Id);


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            return Json(fm.GetList(column, value), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            try
            {
                return Json(fm.GetSequence(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(Dictionary<string, object> data)
        {
            try
            {
                string ret = fm.Save(data);
                if (ret == "Success")
                {
                    return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });
                }
                else
                {
                    return Json(new { Error = true, Message = ret });
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            try
            {

                string ret = fm.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return Library.Security.Core.clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        #region -- Operations

        [HttpPost, Authorize]
        public ActionResult XlsFurnitureMasterReport()
        {
            try
            {
                var workbook = FurnitureReport();

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "FurnitureMasterReport.xlsx";
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
        private IWorkbook FurnitureReport()
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = fm.furnituremasterReport();

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

           

            report.SetHeaderText(ref sheet, ROW, COL, "Category", 20, ExcelHAlign.HAlignCenter);
            int ColCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Category", 12, ExcelHAlign.HAlignCenter);
            int ColSubCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Budget", 12, ExcelHAlign.HAlignCenter);
            int ColBudget = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Type", 8, ExcelHAlign.HAlignCenter);
            int ColType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Description", 12, ExcelHAlign.HAlignCenter);
            int ColDescription = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 20, ExcelHAlign.HAlignCenter);
            int ColRemarks = COL;
            COL++;

          /* report.SetHeaderText(ref sheet, ROW, COL, "Added By", 20, ExcelHAlign.HAlignCenter);
            int ColAddedBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Added Date", 20, ExcelHAlign.HAlignCenter);
            int ColAddedDate = COL;
            COL++;*/

           
           

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
                //sheet[ROW, ColSequence].Text = data.Rows[i]["Sequence"].ToString();
                //sheet[ROW, ColCode].Number = Library.Security.Core.clsStaticInfo.dbl(data.Rows[i]["Code"].ToString());
                //sheet[ROW, ColShortName].Text = data.Rows[i]["ShortName"].ToString();
                sheet[ROW, ColStandardName].Text = data.Rows[i]["StandardName"].ToString();
                sheet[ROW, ColUserName].Text = data.Rows[i]["UserName"].ToString();
                sheet[ROW, ColBudget].Number = Library.Security.Core.clsStaticInfo.dbl(data.Rows[i]["Budget"].ToString());
                sheet[ROW, ColCategory].Text = data.Rows[i]["Category"].ToString();
                sheet[ROW, ColSubCategory].Text = data.Rows[i]["SubCategory"].ToString();
                sheet[ROW, ColType].Text = data.Rows[i]["Type"].ToString();
                sheet[ROW, ColDescription].Text = data.Rows[i]["Description"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                //sheet[ROW, ColAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
                //sheet[ROW, ColAddedDate].DateTime = Convert.ToDateTime(data.Rows[i]["AddedDate"].ToString());
                

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
            reportUtility.CompanyHeader(ref sheet, endCol, "Furniture Master", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion -- Operations  
    }
}