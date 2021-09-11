using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Attendances;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.HumanResource.NewAttendanceProcess;
//using TBS;

namespace Aplos.Areas.HumanResource.Controllers
{

    public class TablesUploadController : BaseController
    {
        // add a header verification - 1. Basic Authentication .... 2. Payload

        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;

        TablesUploadService rs = new TablesUploadService();
        public TablesUploadController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion


        #region Aplos       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        //#region -- Operations

        

        [HttpGet, Authorize]
        public ActionResult getCurrentList()
        {
            return Json(rs.getCurrentList(), JsonRequestBehavior.AllowGet);
        }


        //The Second Page 
        /// 
        //The Getting of Sample Report

        [HttpPost, Authorize]
        public ActionResult SaveFileList(List<Dictionary<string,object>>data , string tab )
        {
            try
            {
                 rs.SaveFileList(data ,tab);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetSampleReport(ReportFormat reportFormat , string tab)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string date = DateTime.Now.Date.ToString("dd-MMM");//.Substring(0, DateTime.Now.Date.ToString().Length - 12);
            var reportFileName = tab+"-"+date;
            var workbook = GetCurrentReportWorkSheet(tab);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        private IWorkbook GetCurrentReportWorkSheet(string tab)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            //var sheet2 = workbook.Worksheets[1];

            /// Sheet 1 
            DataTable data = rs.getCurrentTableFile(tab);

            sheet.Name = tab;



            int ROW = 1;
            int endCol = 1;
            int COL = 1;
            var startRow = 0;
            var endRow = 0;

            if (tab == "SALGL")
            {
                #region Headers
                //report.SetHeaderText(ref sheet, ROW, COL, "Employee Id ", 12, ExcelHAlign.HAlignLeft);
                //int ColEmpSystemId = COL;
                //COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "AccountsGroupId", 8, ExcelHAlign.HAlignLeft);
                int ColAccGropId = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "AccountsGroup", 8, ExcelHAlign.HAlignLeft);
                int ColAccGrop = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "SalaryHeadId", 8, ExcelHAlign.HAlignLeft);
                int ColSalHeadId = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "SalaryHead", 8, ExcelHAlign.HAlignLeft);
                int ColSalHead = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "SalaryHeadCategory", 8, ExcelHAlign.HAlignLeft);
                int ColSalHeadCat = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "AplosGLDR", 8, ExcelHAlign.HAlignLeft);
                int ColAGldr = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "AplosGLCR", 8, ExcelHAlign.HAlignLeft);
                int ColAGlcr = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "AplosGLDRName", 8, ExcelHAlign.HAlignLeft);
                int ColAGldrN = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "AplosGLCRName", 8, ExcelHAlign.HAlignLeft);
                int ColAGlcrN = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "ClientGLDR", 8, ExcelHAlign.HAlignLeft);
                int ColCGldr = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "ClientGLCR", 8, ExcelHAlign.HAlignLeft);
                int ColCGlcr = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "ClientGLDRName", 8, ExcelHAlign.HAlignLeft);
                int ColCGldrN = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "ClientGLCRName", 8, ExcelHAlign.HAlignLeft);
                int ColCGlcrN = COL;
                COL++;

                endCol = COL;
                #endregion Headers
                ROW++;
                 startRow = 0;
                 endRow = 0;
                int RowIndex = ROW;
                startRow = ROW;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    //sheet[ROW, ColEmpSystemId].Text = data.Rows[i]["EmpSystemId"].ToString();
                    sheet[ROW, ColAccGropId].Text = data.Rows[i]["AccountGroupId"].ToString();
                    sheet[ROW, ColAccGrop].Text = data.Rows[i]["AccountGroup"].ToString();
                    sheet[ROW, ColSalHeadId].Text = data.Rows[i]["SalaryHeadId"].ToString();
                    sheet[ROW, ColSalHead].Text = data.Rows[i]["SalaryHead"].ToString();
                    sheet[ROW, ColSalHeadCat].Text = data.Rows[i]["SalaryHeadCategory"].ToString();
                    sheet[ROW, ColAGldr].Text = data.Rows[i]["AplosGLDR"].ToString();
                    sheet[ROW, ColAGlcr].Text = data.Rows[i]["AplosGLCR"].ToString();
                    sheet[ROW, ColAGldrN].Text = data.Rows[i]["AplosGLDRName"].ToString();
                    sheet[ROW, ColAGlcrN].Text = data.Rows[i]["AplosGLCRName"].ToString();
                    sheet[ROW, ColCGldr].Text = data.Rows[i]["ClientGLDR"].ToString();
                    sheet[ROW, ColCGlcr].Text = data.Rows[i]["ClientGLCR"].ToString();
                    sheet[ROW, ColCGldrN].Text = data.Rows[i]["ClientGLDRName"].ToString();
                    sheet[ROW, ColCGlcrN].Text = data.Rows[i]["ClientGLCRName"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                    ROW++;

                }
            }

            if(tab == "CostCenter")
            {
                #region Headers
                //report.SetHeaderText(ref sheet, ROW, COL, "Employee Id ", 12, ExcelHAlign.HAlignLeft);
                //int ColEmpSystemId = COL;
                //COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "PositionId", 8, ExcelHAlign.HAlignLeft);
                int ColPosId = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "PositionCode", 8, ExcelHAlign.HAlignLeft);
                int ColPosCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "CostCenter", 8, ExcelHAlign.HAlignLeft);
                int ColCostCenter = COL;
                COL++;


                endCol = COL;
                #endregion Headers
                ROW++;
                 startRow = 0;
                 endRow = 0;
                int RowIndex = ROW;
                startRow = ROW;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    //sheet[ROW, ColEmpSystemId].Text = data.Rows[i]["EmpSystemId"].ToString();
                    sheet[ROW, ColPosId].Text = data.Rows[i]["PositionId"].ToString();
                    sheet[ROW, ColPosCode].Text = data.Rows[i]["PositionCode"].ToString();
                    sheet[ROW, ColCostCenter].Text = data.Rows[i]["CostCenter"].ToString();
                    

                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                    ROW++;

                }
            }

            if(tab == "Entity")
            {
                #region Headers
                //report.SetHeaderText(ref sheet, ROW, COL, "Employee Id ", 12, ExcelHAlign.HAlignLeft);
                //int ColEmpSystemId = COL;
                //COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "EntityId", 8, ExcelHAlign.HAlignLeft);
                int ColEntityId = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "EntityCode", 8, ExcelHAlign.HAlignLeft);
                int ColEntityCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "EntityName", 8, ExcelHAlign.HAlignLeft);
                int ColEntityName = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "BusinessArea", 8, ExcelHAlign.HAlignLeft);
                int ColBizArea = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "ProfitCenter", 8, ExcelHAlign.HAlignLeft);
                int ColPrftCent = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "CompanyCode", 8, ExcelHAlign.HAlignLeft);
                int ColCCode = COL;
                COL++;

                endCol = COL;
                #endregion Headers
                ROW++;
                 startRow = 0;
                 endRow = 0;
                int RowIndex = ROW;
                startRow = ROW;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    //sheet[ROW, ColEmpSystemId].Text = data.Rows[i]["EmpSystemId"].ToString();
                    sheet[ROW, ColEntityId].Text = data.Rows[i]["EntityId"].ToString();
                    sheet[ROW, ColEntityCode].Text = data.Rows[i]["EntityCode"].ToString();
                    sheet[ROW, ColEntityName].Text = data.Rows[i]["EntityName"].ToString();
                    sheet[ROW, ColBizArea].Text = data.Rows[i]["BusinessArea"].ToString();
                    sheet[ROW, ColPrftCent].Text = data.Rows[i]["ProfitCenter"].ToString();
                    sheet[ROW, ColCCode].Text = data.Rows[i]["CompanyCode"].ToString();
                    

                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                    ROW++;

                }
            }
            
            endRow = ROW - 1;

            #region Sheet2

            //Sheet 2

            //DataTable data2 = rs.getRostersFile();

            //sheet2.Name = "WeekOffTable";



            //int ROW2 = 1;
            //int endCol2 = 1;
            //int COL2 = 1;

            //#region Headers
            //report.SetHeaderText(ref sheet2, ROW2, COL2, "Week Off Id ", 12, ExcelHAlign.HAlignLeft);
            //int ColRostersId = COL2;
            //COL2++;

            //report.SetHeaderText(ref sheet2, ROW2, COL2, "Week Off Standard Name", 8, ExcelHAlign.HAlignLeft);
            //int ColStdName = COL2;
            //COL2++;

            //report.SetHeaderText(ref sheet2, ROW2, COL2, "Week Off User Name", 8, ExcelHAlign.HAlignLeft);
            //int ColUsrName = COL2;
            //COL2++;

            //report.SetHeaderText(ref sheet2, ROW2, COL2, "Week Off Description", 8, ExcelHAlign.HAlignLeft);
            //int ColDes = COL2;
            //COL2++;

            //report.SetHeaderText(ref sheet2, ROW2, COL2, "Week Off Remarks", 8, ExcelHAlign.HAlignLeft);
            //int ColRems = COL2;
            //COL2++;
            //endCol2 = COL2;
            //#endregion Headers
            //ROW2++;
            //var startRow2 = 0;
            //var endRow2 = 0;
            //int RowIndex2 = ROW2;
            //startRow2 = ROW2;
            //for (int i = 0; i < data2.Rows.Count; i++)
            //{
            //    sheet2[ROW2, ColRostersId].Text = data2.Rows[i]["Id"].ToString();
            //    sheet2[ROW2, ColStdName].Text = data2.Rows[i]["StandardName"].ToString();
            //    sheet2[ROW2, ColUsrName].Text = data2.Rows[i]["UserName"].ToString();
            //    sheet2[ROW2, ColDes].Text = data2.Rows[i]["Description"].ToString();
            //    sheet2[ROW2, ColRems].Text = data2.Rows[i]["Remarks"].ToString();

            //    sheet2.Range[ROW2, 1, ROW2, endCol2].BorderInside(ExcelLineStyle.Hair);
            //    sheet2.Range[ROW2, 1, ROW2, endCol2].BorderAround(ExcelLineStyle.Hair);

            //    ROW2++;

            //}
            //endRow2 = ROW2 - 1;

            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //report.PageSetup(ref sheet2, 5, ExcelPageOrientation.Landscape);
            #endregion Sheet2

            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            
            return workbook;
        }


        [HttpPost, Authorize]
        public ActionResult ImportData( )
        {
            string path;

            try
            {
                var file = Request.Files["file"];
                string tab = Request.Params["Imp"].ToString();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = ReadData(path , tab);

                var json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public List<object> ReadData( string path , string tab)
        {

            DataSet dsExcel = null;
            try
            {
                
                List<object> ret = new List<object>();
                ReadFile(path, out dsExcel);

                if(tab == "SALGL")
                {
                    List<SalGlData> data = new List<SalGlData>();
                    data = dsExcel.Tables[0].ToList<SalGlData>();
                    if (data.Count > 0)
                    {
                        for (int i = 0; i < data.Count; i++)
                        {
                            ret.Add(data[i]);
                        }
                    }
                }
                if (tab == "CostCenter")
                {
                    List<CostCenterData> data = new List<CostCenterData>();
                    data = dsExcel.Tables[0].ToList<CostCenterData>();
                    if (data.Count > 0)
                    {
                        for (int i = 0; i < data.Count; i++)
                        {
                            ret.Add(data[i]);
                        }
                    }
                }
                if (tab == "Entity")
                {
                    List<EntityData> data = new List<EntityData>();
                    data = dsExcel.Tables[0].ToList<EntityData>();
                    if (data.Count > 0)
                    {
                        for (int i = 0; i < data.Count; i++)
                        {
                            ret.Add(data[i]);
                        }
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ReadFile(string path, out DataSet dsExcel)
        {
            FileInfo docFile;
            dsExcel = null;
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = excelEngine.Excel.Workbooks.Open(path);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                dsExcel = new DataSet();
                dsExcel.Tables.Add(dt);
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    //exception += "\r\nTrying to delete";
                    docFile.Delete();
                }
            }
            catch (Exception ex)
            {
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
                throw (ex);
            }
        }


        public void SaveFile(out string path)
        {
            path = "";
            try
            {
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetOTManualFile(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public class SalGlData
        {
            
            public string AccountsGroupId { get; set; }
            public string AccountsGroup { get; set; }
            public string SalaryHeadId { get; set; }
            public string SalaryHead { get; set; }
            public string SalaryHeadCategory { get; set; }
            public string AplosGLDR { get; set; }
            public string AplosGLCR { get; set; }
            public string AplosGLDRName { get; set; }
            public string AplosGLCRName { get; set; }
            public string ClientGLDR { get; set; }
            public string ClientGLCR { get; set; }
            public string ClientGLDRName { get; set; }
            public string ClientGLCRName { get; set; }
            
        }

        public class CostCenterData
        {

            public string PositionId { get; set; }
            public string PositionCode { get; set; }
            public string CostCenter { get; set; }

        }

        public class EntityData
        {

            public string EntityId { get; set; }
            public string EntityCode { get; set; }
            public string EntityName { get; set; }
            public string BusinessArea { get; set; }
            public string ProfitCenter { get; set; }
            public string CompanyCode { get; set; }

        }

    }
}