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
using ConnectionManager;

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
        public JsonResult ProReport(string Date, string Entity, string ProcessId, string EntityName, string Process)
        {
            try
            {
                string fileName = "";
                fileName = ProductionReport("Production Report", Date, Entity, ProcessId, EntityName, Process);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string ProductionReport(string SheetName, string Date, string Entity, string ProcessId, string EntityName, string Process)
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
                DataTable dtOrder, dtParameter;
                GetProductionSummaryData(Date, Entity, ProcessId, out dtOrder);
                Dictionary<string, ProductionParameter> shtListNew = null;
                Dictionary<string, List<DataRow>> dicParameter = GetProductionParameterData(Date, Entity, ProcessId, out dtParameter);
                if (dtOrder.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }
                int ROW = 4; int COL = 1;
                sheet.Range[ROW, COL].Text = "Entity";
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[ROW, COL + 1].Text = ": " + EntityName;

                sheet.Range[ROW, COL+2].Text = "Process";
                sheet.Range[ROW, COL+2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[ROW, COL + 3].Text = ": " + Process;


                sheet.Range[ROW, COL+4].Text = "Production Date";
                sheet.Range[ROW, COL+4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[ROW, COL + 5].Text = ": " + Date;
                sheet.Range[ROW, COL, ROW, COL + 5].CellStyle.Font.Bold = true;

                ROW = 6;  COL = 1;
                int endGenericColumn = 0;

                #region ColumnsHeader

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
                
                endGenericColumn = COL;

                CreateDynamicSHead(dtParameter, ref sheet, ref ROW, ref COL, ref colWIP, out shtListNew);

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                #endregion columns

                ROW++;
                int startRow = ROW;

                #region DataPlot
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

                    if (dicParameter.ContainsKey(dtOrder.Rows[i]["ProductionSummaryId"].ToString()))
                    {
                        List<DataRow> drSalaryHeadCollection = dicParameter[dtOrder.Rows[i]["ProductionSummaryId"].ToString()];
                        for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                        {
                            try
                            {
                                ProductionParameter xx = shtListNew[drSalaryHeadCollection[CI]["ProductionBookingParameterId"].ToString()];
                                if (xx != null)
                                {
                                    sheet.Range[ROW, xx.XLColIndex].Number = Library.Security.Core.clsStaticInfo.dbl(drSalaryHeadCollection[CI]["Value"].ToString());
                                    sheet.Range[ROW, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                    sheet.Range[ROW, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }

                        }
                    }

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                #endregion

                #region ReportHeader
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


                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true; 
                #endregion

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

        private void CreateDynamicSHead(DataTable dtSalaryHead, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out Dictionary<string, ProductionParameter> list)
        {
            try
            {
                list = new Dictionary<string, ProductionParameter>();
                int countGrossPostion = 0;


                xlsCol += 0;
                countGrossPostion++;

                int countCTCPosition = countGrossPostion;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    xlsCol ++;
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["UserName"].ToString().Trim().Length > 0)
                    {

                        sheet1.Range[xlsRow, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["UserName"].ToString();
                        sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
                        sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.Font.Size = 10;
                        sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;

                        ProductionParameter salaryHeadSequence = new ProductionParameter();

                        salaryHeadSequence.ProductionBookingParameterId = dtSalaryHead.Rows[ci]["ProductionBookingParameterId"].ToString();
                        salaryHeadSequence.UserName = dtSalaryHead.Rows[ci]["UserName"].ToString();
                        
                        salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

                        list.Add(dtSalaryHead.Rows[ci]["ProductionBookingParameterId"].ToString(), salaryHeadSequence);
                        countCTCPosition++;



                    }//Parameter 
                    #endregion
              
                }//for

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetProductionSummaryData(string Date, string Entity, string ProcessId, out DataTable dtOrder)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string yd = Convert.ToDateTime(Date).AddDays(-1).ToString("dd-MMM-yyyy");

                strSql = @"select PS.Id ProductionSummaryId,WCM.UserName WorkCenter,PS.ProductionOrderId PONo,PS.LotNumber,PL.Code ProductCode,PM.UserName Product
,MMA.StandardName Article,0 WIP
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

        public Dictionary<string, List<DataRow>> GetProductionParameterData(string Date, string Entity, string ProcessId, out DataTable dtParameter)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicParameter = new Dictionary<string, List<DataRow>>();
            dtParameter = new DataTable("Tmp");
            try
            {
                strSql = @"SELECT PV.* FROM [dbo].[ProductionSummaryParameterValue] PV
LEFT JOIN [dbo].[ProductionBookingParameter] PB ON PB.Id=PV.ProductionBookingParameterId
Where PB.EntryState='Entry' AND ProductionSummaryId IN(select Id from TRN.ProductionSummary where ProductionDate between '" + Date + @"' and '" + Date + @"' and EntityId = '" + Entity + @"' and ProcessId = '" + ProcessId + "') Order by ProductionSummaryId";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(strSql, out dsRef);

                dtParameter = dsRef.Tables[0].DefaultView.ToTable(true, "ProductionBookingParameterId", "UserName");
                dtParameter = dtParameter.DefaultView.ToTable();

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["ProductionSummaryId"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicParameter.Add(dt.Rows[i]["ProductionSummaryId"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["ProductionSummaryId"].ToString();
                }

                return dicParameter;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
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

    public class ProductionParameter
    {
        public string ProductionBookingParameterId { get; set; }
        public string ProductionSummaryId { get; set; }
        public string UserName { get; set; }
        public string Value { get; set; }
        public int XLColIndex { get; set; }
    }
}