#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using System;
using OTSBD;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;
using Syncfusion.XlsIO;
using Library.Service.Helpers;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionPlanningReportController : BaseController
    {
        #region Constructor
        /// <summary>   The CostingTypesService service. </summary>
        private readonly ISqlRepository _sqlRepository;
        public ProductionPlanningReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetProductionPlanningReport(string fromDate, string toDate)
        {
            try
            {   
                ProductionPlanningReport(fromDate, toDate);
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void ProductionPlanningReport(string fromDate, string toDate)
        {
            try
            {
                string sql = GetProductionPlanningReportSQL(fromDate, toDate);
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Production PLanning Report";

                DataTable dtProductionPlanningReport = _sqlRepository.GetDataTable(sql);

                int ROW = 6;
                int COL = 1;


                sheet[ROW, COL].Text = "Sl No.";
                sheet[ROW, COL].ColumnWidth = 5;
                int colSlNo = COL;
                COL++;



                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtProductionPlanningReport.Rows.Count; i++)
                {
                    sheet[ROW, colSlNo].Number = (i + 1);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                //sheet.Range[StartRow, colOrderQty, ROW, colOrderQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colPlanOrderQty, ROW, colPlanOrderQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colActualQty, ROW, colActualQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colProducedQty, ROW, colProducedQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colVariance, ROW, colVariance].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colQty, ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Production PLanning Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "Production PLanning Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private string GetProductionPlanningReportSQL(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return @"SELECT w.UserName AS Line, TG.*,prd.ProducedQty
  FROM (SELECT T.WorkCenterMasterID,'BuyerItemNo' AS BuyerItemNo,t.TargetDate,SUM(t.Quantity) TargetQty,SUM(T.Manpower) Manpower,SUM(t.SMV) AS SPT 
  FROM trn.DailyProductionTarget AS T
WHERE t.TargetDate BETWEEN '"+fromDate+@"' AND '"+toDate+ @"'
GROUP BY t.TargetDate,T.WorkCenterMasterID
) TG
LEFT JOIN (

SELECT ps.WorkCenterMasterId,ps.ProductionDate,SUM(ps.Quantity) AS ProducedQty
  FROM trn.ProductionSummary AS ps 
WHERE ps.ProductionDate BETWEENs '" + fromDate + @"' AND '" + toDate + @"'
GROUP BY  ps.WorkCenterMasterId,ps.ProductionDate) PRD ON prd.WorkCenterMasterId=tg.WorkCenterMasterID AND tg.TargetDate=prd.ProductionDate

JOIN scs.WorkCenterMaster AS w ON w.Id=tg.WorkCenterMasterID

ORDER BY w.Sequence,tg.TargetDate";

        }
        #endregion
    }


}