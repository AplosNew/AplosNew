#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Library.Crosscutting.Security;
using System.Data;
using Library.Security.Core;
using System.Threading;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using System.Drawing;
using OTSBD;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public ProductionReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Report()
        {
            return View();
        }

        #region --- Daily Day Status Report---
        [HttpPost, Authorize]
        public JsonResult ProReport(string Date, string Entity, string ProcessId)
        {
            try
            {
                string fileName = "";
                fileName = ProductionReport("Production Report", Date, Entity, ProcessId);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string ProductionReport(string SheetName, string Date, string Entity, string ProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];
                DataTable dtOrder;
                GetReport(Date, Entity, ProcessId, out dtOrder);
                if (dtOrder.Rows.Count == 0)
                {
                    throw new Exception("No Data Foound.");
                }
                int ROW = 6; int COL = 1;
                int endGenericColumn = 0;

                #region columns

                sheet[ROW, COL].Text = "Work Centre"; sheet[ROW, COL].ColumnWidth = 16; int colWorkCentre = COL;
                COL++;
                sheet[ROW, COL].Text = "PONo"; sheet[ROW, COL].ColumnWidth = 16; int colPONumber = COL;
                COL++;
                sheet[ROW, COL].Text = "Lot No."; sheet[ROW, COL].ColumnWidth = 16; int colLotNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Code"; sheet[ROW, COL].ColumnWidth = 16; int colProductCode = COL;
                COL++;
                sheet[ROW, COL].Text = "Product"; sheet[ROW, COL].ColumnWidth = 22; int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Article"; sheet[ROW, COL].ColumnWidth = 12; int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "SONo"; sheet[ROW, COL].ColumnWidth = 12; int colSOS = COL;
                COL++;
                sheet[ROW, COL].Text = "Yesterday Production"; sheet[ROW, COL].ColumnWidth = 16; int colYesterdayProduction = COL;
                COL++;
                sheet[ROW, COL].Text = "WIP"; sheet[ROW, COL].ColumnWidth = 16; int colWIP = COL;
                COL++;
                endGenericColumn = COL;
                //sheet[ROW, COL].Text = "Parameter"; sheet[ROW, COL].ColumnWidth = 25; int colParameter = COL;
                //COL++;
                //sheet[ROW, COL].Text = "Parameter Value"; sheet[ROW, COL].ColumnWidth = 25; int colParameterValue = COL;
                CreateDynamicSHead(dtOrder,  ref sheet, ref ROW, ref COL);
                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {
                    sheet[ROW, colWorkCentre].Text = dtOrder.Rows[i]["WorkCenter"].ToString();
                    sheet[ROW, colPONumber].Text = dtOrder.Rows[i]["PONo"].ToString();
                    sheet[ROW, colLotNo].Text = dtOrder.Rows[i]["LotNumber"].ToString();
                    sheet[ROW, colProductCode].Text = dtOrder.Rows[i]["ProductCode"].ToString();
                    sheet[ROW, colProduct].Text = dtOrder.Rows[i]["Product"].ToString();
                    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();
                    sheet[ROW, colSOS].Text = dtOrder.Rows[i]["SONo"].ToString();
                    sheet[ROW, colYesterdayProduction].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["YesterdayProduction"].ToString());
                    sheet[ROW, colWIP].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["WIP"].ToString());
                    //sheet[ROW, colParameter].Text = dtOrder.Rows[i]["Parameter"].ToString();
                    //sheet[ROW, colParameterValue].Text = dtOrder.Rows[i]["ParameterValue"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Production Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;




                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreateDynamicSHead(DataTable dtSalaryHead,  ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol)
        {
            try
            {
              
                int countGrossPostion = 0;
               

                xlsCol += 1;
                countGrossPostion++;

                int countCTCPosition = countGrossPostion;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["Parameter"].ToString().Trim().Length > 0)
                    {
                            
                            sheet1.Range[xlsRow + 1, xlsCol + countCTCPosition].Text = dtSalaryHead.Rows[ci]["Parameter"].ToString();
                            sheet1.Range[xlsRow + 1, xlsCol + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
                            sheet1.Range[xlsRow + 1, xlsCol + countCTCPosition].CellStyle.Font.Size = 10;
                            sheet1.Range[xlsRow + 1, xlsCol + countCTCPosition].CellStyle.ShrinkToFit = true;

                            countCTCPosition++;
                       


                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetReport(string Date, string Entity, string ProcessId, out DataTable dtOrder)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string yd = Convert.ToDateTime(Date).AddDays(-1).ToString("dd-MMM-yyyy");

                strSql = @"select PS.Id,WCM.UserName WorkCenter,PS.ProductionOrderId PONo,PS.LotNumber,PL.Code ProductCode,PM.UserName Product
,MMA.StandardName Article,0 WIP,PSPV.UserName Parameter,PSPV.Value ParameterValue
,YesterdayProduction=(select sum(Quantity) from TRN.ProductionSummary where ProductionDate between '" + yd + @"' and '" + yd + @"' and EntityId = '" + Entity + @"' and ProcessId = '" + ProcessId + @"')
,SONo =STUFF((select distinct ','+XSO.Id from 
	trn.SalesOrder XSO 
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
    WHERE PS.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
from TRN.ProductionSummary PS
left join SCS.WorkCenterMaster WCM on WCM.Id=PS.WorkCenterMasterId AND WCM.Active=1
left join MST.MaterialMaster MM on MM.Id=PS.MaterialMasterId
left join TRN.ProductDefinition AS PD ON PD.MaterialMasterId=MM.Id
left join [MST].[ProductMaster] PM on PM.Id=PD.ProductMasterId
left join dbo.ProductLibrary PL on PL.Id=PS.ProductLibraryId
left join MST.MaterialMasterArticle MMA on MMA.Id=PS.ArticleId							  
left join (select PV.* from [dbo].[ProductionSummaryParameterValue] PV
LEFT JOIN [dbo].[ProductionBookingParameter] PB ON PB.Id=PV.ProductionBookingParameterId
Where PB.EntryState='Entry') PSPV on PSPV.ProductionSummaryId=PS.Id
WHERE PS.ProductionDate between '" + Date + @"' and '" + Date + @"' and PS.EntityId = '" + Entity + @"' and PS.ProcessId = '" + ProcessId + @"'";

                dtOrder = _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void GetReportX(string Date, string Entity, string ProcessId, out DataTable dtOrder)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string yd = Convert.ToDateTime(Date).AddDays(-1).ToString("dd-MMM-yyyy");

                strSql = @"select  B.WorkCenter,B.PONo,B.SONo,B.LotNumber,B.ProductCode,B.Product,B.Article,B.YesterdayProduction,B.WIP,B.Parameter, B.ParameterValue 
into #tempOT from
(select A.WorkCenter,A.PONo,A.SONo,A.LotNumber,A.ProductCode,A.Product,A.Article,A.YesterdayProduction,A.WIP, A.Parameter, A.ParameterValue from
(
select WCM.UserName WorkCenter,PS.ProductionOrderId PONo,PS.LotNumber,PL.Code ProductCode,PM.UserName Product
,MMA.StandardName Article,0 WIP,PSPV.UserName Parameter,PSPV.Value ParameterValue
,YesterdayProduction=(select sum(Quantity) from TRN.ProductionSummary where ProductionDate between '" + yd + @"' and '" + yd + @"' and EntityId = '" + Entity + @"' and ProcessId = '" + ProcessId + @"')
,SONo =STUFF((select distinct ','+XSO.Id from 
	trn.SalesOrder XSO 
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
    WHERE PS.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
from TRN.ProductionSummary PS
left join SCS.WorkCenterMaster WCM on WCM.Id=PS.WorkCenterMasterId AND WCM.Active=1
left join MST.MaterialMaster MM on MM.Id=PS.MaterialMasterId
left join TRN.ProductDefinition AS PD ON PD.MaterialMasterId=MM.Id
left join [MST].[ProductMaster] PM on PM.Id=PD.ProductMasterId
left join dbo.ProductLibrary PL on PL.Id=PS.ProductLibraryId
left join MST.MaterialMasterArticle MMA on MMA.Id=PS.ArticleId							  
left join (select PV.* from [dbo].[ProductionSummaryParameterValue] PV
LEFT JOIN [dbo].[ProductionBookingParameter] PB ON PB.Id=PV.ProductionBookingParameterId
Where PB.EntryState='Entry') PSPV on PSPV.ProductionSummaryId=PS.Id
WHERE PS.ProductionDate between '" + Date + @"' and '" + Date + @"' and PS.EntityId = '" + Entity + @"' and PS.ProcessId = '" + ProcessId + @"')A
)B
DECLARE @sql nvarchar(max), @col nvarchar(max)
                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(REPLACE(CONVERT(VARCHAR(40), Parameter, 113), ' ', '-'))    
                                FROM #tempOT 
                                FOR XML PATH ('')
                            )                             SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([ParameterValue]) FOR [Parameter] IN ('+STUFF(@col,1,1,'')+')
                            ) as pvt)' 
							EXEC sp_executesql @sql
                            drop table #tempOT";

                dtOrder = _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        #endregion

    }
}