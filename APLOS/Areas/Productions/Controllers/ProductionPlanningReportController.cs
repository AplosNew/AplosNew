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
        public ActionResult GetProductionPlanningReport(DateTime fromDate, DateTime toDate)
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

        public void ProductionPlanningReport(DateTime fromDate, DateTime toDate)
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
                int endCol = 0;
                int StartRow = 0;
                int EndRow = 0;
                int colDate = 0;
                COL++;
                Dictionary<DateTime, int> DateColumns = new Dictionary<DateTime, int>();
                sheet[6, 1].Text = "Date:";
                sheet[6, 1].CellStyle.Font.Bold = true;

                while (fromDate <= toDate)
                {
                    sheet[ROW, COL].Text = fromDate.ToString("dd");
                    sheet[ROW, COL].ColumnWidth = 8;
                     colDate = COL;

                    DateColumns.Add(fromDate, COL);
                    fromDate = fromDate.AddDays(1);
                    COL++;
                }

               
                string PreviousLine = "";
                ROW = 0;

                COL = 1;
                for (int i = 0; i < dtProductionPlanningReport.Rows.Count; i++)
                {

                    if (PreviousLine != dtProductionPlanningReport.Rows[i]["Line"].ToString())
                    {
                        ROW += 8;
                        
                    }

                    DateTime dtCurrentDate = Convert.ToDateTime(dtProductionPlanningReport.Rows[i]["TargetDate"].ToString());

                    int CCOL = DateColumns[dtCurrentDate];
                    if (DateColumns.ContainsKey(dtCurrentDate) == false)
                        continue;


                    int RowLineNo = ROW;
                    int RowWorkingHour = ROW + 1;
                    int RowManPower = ROW + 2;
                    int RowPlannedTarget = ROW + 3;
                    int RowSPT = ROW + 4;
                    int RowAchievement = ROW + 5;
                    int RowBalance = ROW + 6;
                    int RowEfficiency = ROW + 7;

                    sheet[RowLineNo,1].Text = dtProductionPlanningReport.Rows[i]["Line"].ToString();
                    sheet[RowLineNo, 1].ColumnWidth = 15;

                    sheet.Range[RowLineNo, 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.LightGreen;
                    sheet.Range[6, 1,6, colDate].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;

                    sheet[RowWorkingHour, 1].Text = "Working Hour";
                    sheet[RowManPower, 1].Text = "Manpower";
                    sheet[RowPlannedTarget, 1].Text = "Planned Target";
                    sheet[RowSPT, 1].Text = "SPT";
                    sheet[RowAchievement, 1].Text = "Achivement";
                    sheet[RowBalance, 1].Text = "Balance";
                    sheet[RowEfficiency, 1].Text = "Efficiency";


                    sheet[RowLineNo, CCOL+1].Text = dtProductionPlanningReport.Rows[i]["BuyerItemNo"].ToString();
                    sheet.Range[RowLineNo, CCOL + 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Sea_green;

                    sheet[RowWorkingHour, CCOL].Text = dtProductionPlanningReport.Rows[i]["WorkingHour"].ToString();
                    sheet[RowManPower, CCOL].Number =clsStaticInfo.dbl( dtProductionPlanningReport.Rows[i]["Manpower"].ToString());
                    sheet[RowPlannedTarget, CCOL].Number = clsStaticInfo.dbl(dtProductionPlanningReport.Rows[i]["TargetQty"].ToString());
                    sheet[RowSPT, CCOL].Number = clsStaticInfo.dbl(dtProductionPlanningReport.Rows[i]["SPT"].ToString());
                    sheet[RowAchievement, CCOL].Number = clsStaticInfo.dbl(dtProductionPlanningReport.Rows[i]["ProducedQty"].ToString());
                    sheet[RowBalance, CCOL].Formula = clsStaticInfo.GetxlsCol(CCOL) + RowPlannedTarget.ToString() + "-" + clsStaticInfo.GetxlsCol(CCOL) + RowAchievement.ToString();

                    sheet[RowEfficiency, CCOL].Formula = clsStaticInfo.GetxlsCol(CCOL) +RowPlannedTarget.ToString()+ "*" + clsStaticInfo.GetxlsCol(CCOL) + RowSPT.ToString() + "/" + clsStaticInfo.GetxlsCol(CCOL) + RowManPower.ToString()+"*" + clsStaticInfo.GetxlsCol(CCOL) + RowWorkingHour.ToString() +"*60";

                    endCol = colDate;
                    StartRow = RowLineNo;
                    EndRow = RowEfficiency;
                    if (PreviousLine != dtProductionPlanningReport.Rows[i]["Line"].ToString())
                    {                        
                        sheet.Range[StartRow, 2, StartRow, endCol].Merge();
                        sheet.Range[StartRow, 1, EndRow, 1].CellStyle.Font.Bold = true;
                        sheet.Range[StartRow, 1, EndRow, 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                        sheet.Range[StartRow, 1, EndRow, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[StartRow, 1, EndRow, endCol].BorderInside(ExcelLineStyle.Hair);
                        sheet.IsGridLinesVisible = false;
                        sheet.UsedRange.WrapText = true;
                        sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                        sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    }
                    PreviousLine = dtProductionPlanningReport.Rows[i]["Line"].ToString();


                }
                StartRow = 7;
                sheet["A" + StartRow.ToString()].FreezePanes();

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

        private string GetProductionPlanningReportSQL(DateTime fromDate, DateTime toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return @"SELECT w.UserName AS Line, TG.*,prd.ProducedQty
  FROM (SELECT T.WorkCenterMasterID,'BuyerItemNo' AS BuyerItemNo,t.TargetDate,SUM(t.Quantity) TargetQty,SUM(T.Manpower) Manpower,SUM(t.SMV) AS SPT ,T.TotalHour WorkingHour
  FROM trn.DailyProductionTarget AS T
WHERE t.TargetDate BETWEEN '" + fromDate+@"' AND '"+toDate+ @"'
GROUP BY t.TargetDate,T.WorkCenterMasterID,T.TotalHour
) TG
LEFT JOIN (

SELECT ps.WorkCenterMasterId,ps.ProductionDate,SUM(ps.Quantity) AS ProducedQty
  FROM trn.ProductionSummary AS ps 
WHERE ps.ProductionDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
GROUP BY  ps.WorkCenterMasterId,ps.ProductionDate) PRD ON prd.WorkCenterMasterId=tg.WorkCenterMasterID AND tg.TargetDate=prd.ProductionDate

JOIN scs.WorkCenterMaster AS w ON w.Id=tg.WorkCenterMasterID

ORDER BY w.Sequence,tg.TargetDate";

        }
        #endregion
    }


}