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

                int ROW = 7;
                int COL = 1;
                int endCol = 0;
                int StartRow = 0;
                int EndRow = 0;
                int colDate = 0;
                string month = null;
                int monthEndCol =0;
                int monthStartCol = 2;
                COL++;
                Dictionary<DateTime, int> DateColumns = new Dictionary<DateTime, int>();
                sheet[7, 1].Text = "Date:";
                sheet[7, 1].CellStyle.Font.Bold = true;

                while (fromDate <= toDate)
                {
                    if (fromDate.ToString("MMMM")!= month)
                    {

                        sheet[ROW-1, monthStartCol].Text = fromDate.ToString("MMMM/yyyy");
                        if (monthEndCol != 0)
                        {
                            sheet.Range[ROW - 1, monthStartCol, ROW - 1, monthEndCol].Merge();
                            monthStartCol = COL;
                        }                            

                    }
                    sheet[ROW, COL].Text = fromDate.ToString("dd");
                    sheet[ROW, COL].ColumnWidth = 8;
                    
                    colDate = COL;
                    month = fromDate.ToString("MMMM");


                    DateColumns.Add(fromDate, COL);
                    fromDate = fromDate.AddDays(1);
                    monthEndCol = COL;

                    COL++;
                }
                sheet[ROW - 1, monthStartCol].Text = fromDate.ToString("MMMM/yyyy");
                sheet.Range[ROW - 1, monthStartCol, ROW - 1, monthEndCol].Merge();
                


                string PreviousLine = "";
                int BuyerStartCol = 2;
                int BuyerEndCol = 0;
                string PreviousBuyerItem = null;

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

                    sheet.Range[RowLineNo, 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                    sheet.Range[7, 1,7, colDate].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    //sheet.Range[RowLineNo, colDate].CellStyle.Interior.ColorIndex = ExcelKnownColors.Orange;


                    sheet[RowWorkingHour, 1].Text = "Working Hour";
                    sheet[RowManPower, 1].Text = "Manpower";
                    sheet[RowPlannedTarget, 1].Text = "Planned Target";
                    sheet[RowSPT, 1].Text = "SPT";
                    sheet[RowAchievement, 1].Text = "Achivement";
                    sheet[RowBalance, 1].Text = "Balance";
                    sheet[RowEfficiency, 1].Text = "Efficiency(%)";



                    if (PreviousBuyerItem != dtProductionPlanningReport.Rows[i]["BuyerItemNo"].ToString())
                    {

                        sheet[RowLineNo, CCOL].Text = dtProductionPlanningReport.Rows[i]["BuyerItemNo"].ToString();
                        sheet.Range[RowLineNo, CCOL].CellStyle.Font.Bold = true;
                        sheet.Range[RowLineNo, CCOL].CellStyle.Font.Size = 8;

                        if (BuyerEndCol != 0)
                        {
                            sheet.Range[RowLineNo, BuyerStartCol, RowLineNo, BuyerEndCol].Merge();
                            BuyerStartCol = CCOL;

                        }
                    }
                    sheet[RowWorkingHour, CCOL].Number =clsStaticInfo.dbl( dtProductionPlanningReport.Rows[i]["WorkingHour"].ToString());
                    sheet[RowWorkingHour, CCOL].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[RowManPower, CCOL].Number =clsStaticInfo.dbl( dtProductionPlanningReport.Rows[i]["Manpower"].ToString());
                    sheet[RowManPower, CCOL].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[RowPlannedTarget, CCOL].Number = clsStaticInfo.dbl(dtProductionPlanningReport.Rows[i]["TargetQty"].ToString());
                    sheet[RowPlannedTarget, CCOL].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[RowSPT, CCOL].Number = clsStaticInfo.dbl(dtProductionPlanningReport.Rows[i]["SPT"].ToString());
                    sheet[RowSPT, CCOL].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[RowAchievement, CCOL].Number = clsStaticInfo.dbl(dtProductionPlanningReport.Rows[i]["ProducedQty"].ToString());
                    sheet[RowAchievement, CCOL].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[RowBalance, CCOL].Formula = clsStaticInfo.GetxlsCol(CCOL) + RowPlannedTarget.ToString() + "-" + clsStaticInfo.GetxlsCol(CCOL) + RowAchievement.ToString();
                    sheet[RowBalance, CCOL].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[RowEfficiency, CCOL].Formula ="("+ clsStaticInfo.GetxlsCol(CCOL) +RowPlannedTarget.ToString()+ "*" + clsStaticInfo.GetxlsCol(CCOL) + RowSPT.ToString()+")" + "/" + "("+clsStaticInfo.GetxlsCol(CCOL) + RowManPower.ToString()+"*" + clsStaticInfo.GetxlsCol(CCOL) + RowWorkingHour.ToString() +"*60"+")" + "*100";
                    sheet[RowEfficiency, CCOL].NumberFormat = "#,##0.00;(#,##0.00)";
                    BuyerEndCol = CCOL;
                    endCol = colDate;
                    StartRow = RowLineNo;
                    EndRow = RowEfficiency;
                    //sheet.Range[StartRow, endCol+1].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(2) + RowWorkingHour.ToString() + ":" + clsStaticInfo.GetxlsCol(endCol) + RowWorkingHour.ToString() + ")";
                    if (PreviousLine != dtProductionPlanningReport.Rows[i]["Line"].ToString())
                    {                        
                        //sheet.Range[StartRow, 2, StartRow, endCol].Merge();
                        sheet.Range[StartRow, 1, EndRow, 1].CellStyle.Font.Bold = true;
                        sheet.Range[StartRow, 1, EndRow, 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[StartRow, 1, EndRow, 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                        sheet.Range[RowLineNo, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Orange;
                        sheet.Range[StartRow, 1, EndRow, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[StartRow, 1, EndRow, endCol].BorderInside(ExcelLineStyle.Hair);
                        sheet.IsGridLinesVisible = false;
                        sheet.UsedRange.WrapText = true;
                        sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                        sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    }
                    PreviousLine = dtProductionPlanningReport.Rows[i]["Line"].ToString();
                    PreviousBuyerItem= dtProductionPlanningReport.Rows[i]["BuyerItemNo"].ToString();
                    

                }
                StartRow = 8;
               int StartCol = 2;
                sheet["A" + StartRow.ToString()].FreezePanes();
               // sheet["A" + StartCol.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Production PLanning Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
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
                return @"SELECT w.UserName AS Line,TG.*,prd.ProducedQty
  FROM (
  
  select T.WorkCenterMasterID,BuyerItemNo,SUM(T.SalesOrderQty) AS SalesOrderQty, t.TargetDate,SUM(t.TargetQty) TargetQty,avg(T.Manpower) Manpower,avg(t.SPT) AS SPT,SUM(T.WorkingHour) WorkingHour from
  (SELECT T.WorkCenterMasterID,
  
  			SalesOrderQty=(select SUM(SOX.Qty)  from trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T.ProductionOrderId), 
  
													BuyerItemNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo--+'--'+ProductionOrderId 
																				from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id 
																join trn.DailyProductionTarget AS TX ON TX.ProductionOrderId=podx.ProductionOrderId AND t.TargetDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
							                                where TX.WorkCenterMasterID=T.WorkCenterMasterID AND TX.TargetDate=T.TargetDate	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
  
  
  t.TargetDate,t.Quantity TargetQty,T.Manpower,t.SMV AS SPT,T.TotalHour WorkingHour
  FROM trn.DailyProductionTarget AS T
WHERE t.TargetDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"') AS T
GROUP BY BuyerItemNo,t.TargetDate,T.WorkCenterMasterID
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