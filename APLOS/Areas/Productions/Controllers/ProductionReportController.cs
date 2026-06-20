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
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Library.Model.Enums;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();

        public ProductionReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Report()
        {
            return View();
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region --- Daily Day Status Report---
        [HttpGet, Authorize]
        public ActionResult ProReport(ReportFormat reportFormat, string Date, string Entity, string ProcessId, string EntityName, string Process)
        {
            try
            {
                //string fileName = "";
                //fileName = ProductionReport("Production Report", Date, Entity, ProcessId, EntityName, Process);
                //return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

                IWorkbook workbook = ProductionReport("Production Report", Date, Entity, ProcessId, EntityName, Process);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "ProductionReport";
                // return RenderReportAsPdf(workbook, reportFileName);
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;

                    case ReportFormat.PdfView:
                        PdfDocument document1 = new PdfDocument();
                        ExcelToPdfConverterSettings settings1 = new ExcelToPdfConverterSettings();
                        settings1.TemplateDocument = document1;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document1 = converter1.Convert(settings1);
                        }
                        document1.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                        //return RenderReportAsPdf(document1, reportFileName);
                        return RenderReportAsPdf(workbook, reportFileName);
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public ActionResult RenderReportAsPdf(IWorkbook workbook, string fileName, bool isOpen = true)
        {
            try
            {
                using (var converter = new ExcelToPdfConverter(workbook))
                {
                    var pdfDocument = new PdfDocument();
                    ExcelToPdfConverterSettings _settings = new ExcelToPdfConverterSettings();
                    _settings.AutoDetectComplexScript = true;
                    _settings.EmbedFonts = true;
                    _settings.LayoutOptions = LayoutOptions.FitAllColumnsOnOnePage;

                    pdfDocument = converter.Convert(_settings);

                    if (isOpen == true)
                        pdfDocument.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                    else
                        pdfDocument.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);

                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public IWorkbook ProductionReport(string SheetName, string Date, string Entity, string ProcessId, string EntityName, string Process)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
          //  var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];
                DataTable dtOrder, dtParameter;
                _productionSummaryData.GetProductionSummaryData(Date, Entity, ProcessId, out dtOrder);
                Dictionary<string, ProductionParameter> shtListNew = null;
                Dictionary<string, List<DataRow>> dicParameter = _productionSummaryData.GetProductionParameterData(Date, Entity, ProcessId, out dtParameter);
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
                //sheet[ROW, COL].Text = "Product"; sheet[ROW, COL].ColumnWidth = 22; int colProduct = COL;
                //COL++;
                sheet[ROW, COL].Text = "Article"; sheet[ROW, COL].ColumnWidth = 35; int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "SONo"; sheet[ROW, COL].ColumnWidth = 12; int colSOS = COL;
                COL++;
                sheet[ROW, COL].Text = "Prod. As On Date"; sheet[ROW, COL].ColumnWidth = 12; int colYesterdayProduction = COL;
                COL++;
                sheet[ROW, COL].Text = "WIP"; sheet[ROW, COL].ColumnWidth = 8; int colWIP = COL;
                
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
                    //sheet[ROW, colProduct].Text = dtOrder.Rows[i]["Product"].ToString();
                    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();
                    sheet[ROW, colSOS].Text = dtOrder.Rows[i]["SONo"].ToString();
                    sheet[ROW, colYesterdayProduction].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ProductionAsOnDate"].ToString());
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

                //filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                //workbook.SaveAs(filePath);
                //workbook.Close();
                //excelEngine.Dispose();
                //return filePath;
                return workbook;
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

        #region --Employee Wise Production Report
        [HttpGet, Authorize]
        public ActionResult GetEmployeeWiseProductionReport(ReportFormat reportFormat, DateTime fromDate, DateTime toDate, string entityId, string incentiveType, string shiftId, string workCenterId, string dayStatus)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var workbook = EmployeeWiseProductionReport(out string reportFileName, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, entityId, incentiveType, shiftId, workCenterId, dayStatus);
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return View();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IWorkbook EmployeeWiseProductionReport(out string reportFileName, string companyId, string plantId, string plantName, DateTime fromDate, DateTime toDate, string entityId, string incentiveType, string shiftId, string workCenterId, string dayStatus)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";


            reportFileName = "EmployeeWiseProduction" + toDate.ToString("dd-MMM-yyyy");

            var dsLocal = GetEmployeeWiseProductionSQL(fromDate, toDate, entityId, incentiveType, shiftId, workCenterId, dayStatus);


            var row = 3;

            var colLast = 1;

            int xlsCol = 1;
            int colEntity = 0;
            int colDate = 0;
            int colDayStatus = 0;
            int colEmployeeCode = 0;
            int colEmployeeName = 0;
            int colSkillCategory = 0;
            int colSkill = 0;
            int colProductionOrderId = 0;
            int colPOQty = 0;
            int colOPeration = 0;
            int colSAM = 0;
            int colQty = 0;
            int colBasicProd = 0;
            int colSequence = 0;
            int colSkillAllowance = 0;
            int colAdditionalAllowance = 0;
            int colOrderAllowance = 0;
            int colProduceMinute = 0;
            int colProductionSummary = 0;
            int colWorkCenter = 0;
            int colShiftName = 0;
            int colRemark = 0;



            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "EmployeeCode", 8); colEmployeeCode = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "EmployeeName", 10); colEmployeeName = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Entity"); colEntity = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Date", 8); colDate = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "DayStatus", 8); colDayStatus = xlsCol; xlsCol++;
            //SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Oper. Skill", 8); colSkillCategory = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Skill", 8); colSkill = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "PO", 8); colProductionOrderId = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "POQty", 8); colPOQty = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "OP", 8); colOPeration = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "SAM", 8); colSAM = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Qty", 8); colQty = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "BasicProduceMin", 8); colBasicProd = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Seq", 8); colSequence = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "SkillAllowance", 8); colSkillAllowance = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Addi. Allowance", 12); colAdditionalAllowance = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "OrderSize Allow.", 8); colOrderAllowance = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "ProduceMinute", 8); colProduceMinute = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Sq-PO-OPId-OP-Qty", 15); colProductionSummary = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "WorkCenter", 8); colWorkCenter = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "ShiftName", 8); colShiftName = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Remark", 8); colRemark = xlsCol; xlsCol++;
            colLast = xlsCol;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    reportUtility.SetText(ref sheet, row, colEmployeeCode, dsLocal.Rows[i]["EmployeeCode"].ToString());
                    reportUtility.SetText(ref sheet, row, colEmployeeName, dsLocal.Rows[i]["EmployeeName"].ToString());
                    reportUtility.SetText(ref sheet, row, colEntity, dsLocal.Rows[i]["Entity"].ToString());
                    reportUtility.SetText(ref sheet, row, colDate, dsLocal.Rows[i]["Date"].ToString());
                    reportUtility.SetText(ref sheet, row, colDayStatus, dsLocal.Rows[i]["DayStatus"].ToString());
                    //sheet.Range[row, colDate].Text = dsLocal.Rows[i]["Date"].ToString();
                    reportUtility.SetText(ref sheet, row, colSkillCategory, dsLocal.Rows[i]["SkillCategory"].ToString());
                    reportUtility.SetText(ref sheet, row, colSkill, dsLocal.Rows[i]["Skill"].ToString());
                    reportUtility.SetText(ref sheet, row, colProductionOrderId, dsLocal.Rows[i]["ProductionOrderId"].ToString());
                    reportUtility.SetText(ref sheet, row, colPOQty, Convert.ToDouble(dsLocal.Rows[i]["POQty"].ToString()));
                    reportUtility.SetText(ref sheet, row, colOPeration, dsLocal.Rows[i]["OperationName"].ToString());

                    reportUtility.SetText(ref sheet, row, colSAM, Convert.ToDouble(dsLocal.Rows[i]["SAM"].ToString()));
                    reportUtility.SetText(ref sheet, row, colQty, Convert.ToDouble(dsLocal.Rows[i]["Qty"].ToString()));
                    reportUtility.SetText(ref sheet, row, colBasicProd, Convert.ToDouble(dsLocal.Rows[i]["BasicProduceMin"].ToString()));
                    reportUtility.SetText(ref sheet, row, colSequence, Convert.ToInt16(dsLocal.Rows[i]["Sequence"].ToString()));
                    reportUtility.SetText(ref sheet, row, colSkillAllowance, Convert.ToDouble(dsLocal.Rows[i]["SkillAllowance"].ToString()));//OTSBD.clsStaticInfo.dbl
                    reportUtility.SetText(ref sheet, row, colAdditionalAllowance, Convert.ToDouble(dsLocal.Rows[i]["AdditionalAllowance"].ToString()));
                    reportUtility.SetText(ref sheet, row, colOrderAllowance, Convert.ToDouble(dsLocal.Rows[i]["OrderAllowance"].ToString()));
                    reportUtility.SetText(ref sheet, row, colProduceMinute, Convert.ToDouble(dsLocal.Rows[i]["ProduceMinute"].ToString()));
                    reportUtility.SetText(ref sheet, row, colProductionSummary, dsLocal.Rows[i]["ProductionSummary"].ToString());
                    reportUtility.SetText(ref sheet, row, colWorkCenter, dsLocal.Rows[i]["WorkCenter"].ToString());
                    reportUtility.SetText(ref sheet, row, colShiftName, dsLocal.Rows[i]["ShiftName"].ToString());
                    reportUtility.SetText(ref sheet, row, colRemark, dsLocal.Rows[i]["Remark"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                }



                //sheet.UsedRange.AutofitColumns();
                // sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                sheet.Range["A1"].RowHeight = 20;
                sheet.Range["A1"].CellStyle.Font.Size = 14;
                sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].Merge();
                sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].CellStyle.Font.Bold = true;
                sheet.Range["A1"].Text = identity.CompanyName;
                sheet.Range["A2"].RowHeight = 15;
                sheet.Range["A2"].CellStyle.Font.Size = 10;
                sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].Merge();
                sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].CellStyle.Font.Bold = true;
                sheet.Range["A2"].Text = "Detail Report of " + " On " + fromDate.ToString("dd-MMM-yyyy") + " To " + toDate.ToString("dd-MMM-yyyy");
                //reportUtility.CompanyPlantHeader(ref sheet, colLast, "Transaction Report of " + " On " + toDate.ToString("dd-MMM-yyyy"), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }
        private string GetColumnNameForXls(int ColumnNo)
        {
            ColumnNo = ColumnNo - 1;
            if (ColumnNo < 0)
            {
                return "";
            }

            var CharVelue1 = 0;
            var CharVelue2 = 0;
            char ch1, ch2;
            string ColumnName;
            int reminder, div;

            reminder = ColumnNo % 26;
            div = ColumnNo / 26;

            if (div == 0)
            {
                CharVelue1 = 65;
                CharVelue1 = CharVelue1 + reminder;
            }
            if (div > 0)
            {
                CharVelue1 = 65;
                CharVelue2 = 65;
                CharVelue1 = CharVelue1 + div;
                CharVelue2 = CharVelue2 + reminder;
            }

            if (CharVelue2 == 0)
            {
                ch1 = (char)CharVelue1;
                ColumnName = "" + ch1;
            }
            else
            {
                CharVelue1 = CharVelue1 - 1;
                ch1 = (char)CharVelue1;
                ch2 = (char)CharVelue2;
                ColumnName = "" + ch1 + ch2;
            }

            return ColumnName;
        }


        private DataTable GetEmployeeWiseProductionSQL(DateTime fromDate, DateTime toDate, string entityId, string incentiveType, string shiftId, string workCenterId, string dayStatus)
        {
            string tempincentiveType = "";
            if (string.IsNullOrEmpty(incentiveType))
            {
                tempincentiveType = @" 'Production Incentive Scheme (Individual)',  'Production Incentive Scheme (Group)' ";
            }
            else
            {
                tempincentiveType = @" '" + incentiveType + @"' ";
            }

            string tempdaystatus = "";
            if (string.IsNullOrEmpty(dayStatus))
            { tempdaystatus = @" "; }
            else { tempdaystatus = @" AND APD.DayStatus= '" + dayStatus + @"' "; }

            string tempShiftId = "";
            if (string.IsNullOrEmpty(shiftId))
            { tempShiftId = @" "; }
            else { tempShiftId = @" AND owe.ShiftId= '" + shiftId + @"' "; }

            string tempwcId = "";
            if (string.IsNullOrEmpty(workCenterId))
            { tempwcId = @" "; }
            else { tempwcId = @" AND owe.workCenterId= '" + workCenterId + @"' "; }

            var cmdText = @"declare @fromDate varchar(20)='" + fromDate + "',@toDate varchar(20)='" + toDate + @"'
declare @entityId varchar(50)='" + entityId + @"'
					select  SkillAllowance=ISNULL(Round(cast((isnull(x.Qty,0)*x.SAM)*PMC.SkillAllowance as float),1),0) 
					, AdditionalAllowance=ISNULL((isnull(x.Qty,0)*x.sam)*PMC.AdditionOperationAllowance,0)
                    , OrderAllowance=ISNULL((isnull(x.Qty,0)*x.sam)* case when x.OrderLevel='Basic' then OSA.[Basic] 
										when x.OrderLevel='SemiCritical' then OSA.SemiCritical 
										when x.OrderLevel='Critical' then OSA.[Critical] 
										when x.OrderLevel='Special' then OSA.Special end
										,0)/100
					, ProduceMinute=ISNULL((isnull(x.Qty,0)*x.SAM)+((isnull(x.Qty,0)*x.SAM)*PMC.SkillAllowance)+(isnull(x.Qty,0)*x.SAM)*PMC.AdditionOperationAllowance,0)
,productionSummary=cast(x.[Sequence] as nvarchar)+'-'+ x.ProductionOrderId+'-'+x.OperationCode+'-'+x.OperationName+'-'+cast(cast(x.Qty as decimal) as nvarchar),'' Remark
,x.* from (
					Select MB.EntityId,EN.UserName Entity, CONVERT(varchar(15),CAST(ISNULL(owe.[Date],APD.WorkDate)  AS date),100) [Date],ei.EmployeeCode , ei.EmployeeName,SC.UserName SkillCategory,SK.UserName Skill
					, (ISNULL(APD.ShiftFullDayDuration,0)+ISNULL(APD.OTHr,0)) AvailableMinute , OP.Code as OperationCode ,OP.UserName as OperationName, OP.ID as OperationId
					, OP.OperationMasterId as MasterOperationId, owe.ProductionOrderId ,ISNULL(PO.Qty,0) POQty,PO.OrderLevel 
					, ISNULL(bt.TotalSPT,0) SAM, SUM(isnull(owe.Qty,0)) as Qty , BasicProduceMin=round(cast(isnull(Sum(owe.Qty),0)*ISNULL(bt.TotalSPT,0) as float),1)
					, DENSE_RANK() OVER (PARTITION BY owe.EmployeeId,owe.[Date],wcm.UserName,OP.OperationMasterId,owe.ProductionOrderId ORDER BY round(cast(isnull(Sum(owe.Qty),0)*ISNULL(bt.TotalSPT,0) as INT),1) DESC) AS [Sequence]  , APD.EmpSystemID EmployeeId , isnull(o.WIP,0) as WIP
					,SK.SkillCategoryId,P.UserName Plant,wcm.UserName WorkCenter,SD.UserName ShiftName,APD.DayStatus
						FROM AttdnProcessData APD 
                        LEFT JOIN dbo.OperationWiseEmployees owe ON APD.EmpSystemID=owe.EmployeeId  and apd.WorkDate=owe.[Date]
						left join [SCS].[WorkCenterMaster] wcm on wcm.Id=owe.WorkcenterId
						left join hkp.ProductionBookingPeriod pr on pr.Id=owe.PeriodId
						LEFT JOIN trn.ProductionOrder PO ON PO.Id=owe.ProductionOrderId
						LEFT JOIN ORG.Plant P  ON P.Id=PO.PlantId
						left join dbo.EmployeeInformation ei on ei.SystemId = APD.EmpSystemID
                        left join dbo.EmployeeOperationWip o on o.OperationVariationId = owe.OperationVariationId and o.ProductionOrderId = owe.ProductionOrderId and o.ProcessId = owe.ProcessId
						LEFT JOIN mst.OperationVariation OP ON OP.Id=owe.OperationVariationId
						LEFT JOIN MST.OperationMaster OM ON OM.Id=OP.OperationMasterId
                        LEFT JOIN ShiftDefination SD ON SD.SystemId=OWE.ShiftId
						LEFT JOIN HKP.Skill SK on SK.Id=OM.SkillId
						LEFT JOIN HKP.SkillCategory SC on SC.Id=SK.SkillCategoryId
						left join trn.ProductionBulletinTemplate pb on owe.ProductionOrderId = pb.ProductionOrderId
                        left join trn.ProductionBulletinTemplateMaster pt on pt.ProductionBulletinTemplateId=pb.Id and pt.ProcessId=owe.ProcessId
						left join trn.ProductionBulletinTemplateDetail bt on bt.OperationVariationId=OP.Id AND pt.Id=bt.ProductionBulletinTemplateMasterId
						LEFT JOIN MST.ManpowerBudget MB ON MB.Id=ei.BudgetCode
						LEFT JOIN HKP.IncentiveType IT ON IT.Id=MB.IncentiveTypeId
						LEFT JOIN ORG.Entity EN  ON EN.Id=MB.EntityId
						
						where APD.WorkDate > Convert(date, DateAdd(DAY, -365, GetDate()))
						AND APD.[WorkDate] between DATEADD(dd, DATEDIFF(dd, 0, '" + fromDate + "'), 0) and DATEADD(dd, DATEDIFF(dd, 0, '" + toDate + @"'), 0)
                        AND IT.UserName IN (" + tempincentiveType + @") AND MB.EntityId='" + entityId + @"'
                        AND ei.EmployeeStatus='Active' " + tempdaystatus + " " + tempShiftId + " " + tempwcId + @"
                        group by OP.Id , op.Code ,APD.EmpSystemID,APD.WorkDate, op.UserName ,owe.ProductionOrderId  , owe.EmployeeId ,MB.EntityId, ei.EmployeeCode , op.OperationMasterId , o.WIP
						,SK.SkillCategoryId,APD.ShiftFullDayDuration,APD.OTHr,EN.UserName, ei.EmployeeName,SC.UserName,SK.UserName,bt.TotalSPT,bt.ProductionBulletinTemplateMasterId  
						,owe.[Date],P.UserName,wcm.UserName ,PO.Qty,PO.OrderLevel,SD.UserName,APD.DayStatus
                       ) x
					    left join dbo.ProducedMinAllowanceChild PMC ON PMC.SkillCategoryId=x.SkillCategoryId AND PMC.OperationSequence=x.[Sequence]
					    left join dbo.OrderSizeAllowance OSA ON OSA.Days=DAY(x.[Date])
					   order by x.[Date], x.EmployeeCode   ";
            return _sqlRepository.GetDataTable(cmdText);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeWiseProductionSummaryReport(ReportFormat reportFormat, DateTime fromDate, DateTime toDate, string entityId, string incentiveType, string shiftId, string workCenterId, string dayStatus)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var workbook = EmployeeWiseProductionSummaryReport(out string reportFileName, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, entityId, incentiveType, shiftId, workCenterId, dayStatus);
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return View();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IWorkbook EmployeeWiseProductionSummaryReport(out string reportFileName, string companyId, string plantId, string plantName, DateTime fromDate, DateTime toDate, string entityId, string incentiveType, string shiftId, string workCenterId, string dayStatus)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";


            reportFileName = "EmployeeWiseProduction" + toDate.ToString("dd-MMM-yyyy");

            var dsLocal = GetEmployeeWiseProductionSummarySQL(fromDate, toDate, entityId, incentiveType, shiftId, workCenterId, dayStatus);


            var row = 3;

            var colLast = 1;

            int xlsCol = 1;
            int colEntity = 0;
            int colDate = 0;
            int colDayStatus = 0;
            int colEmployeeCode = 0;
            int colEmployeeName = 0;
            int colSkillCategory = 0;
            int colSkill = 0;
            int colAvailableMinute = 0;
            int colSAM = 0;
            int colQty = 0;
            int colBasicProd = 0;
            int colSequence = 0;
            int colSkillAllowance = 0;
            int colAdditionalAllowance = 0;
            int colOrderSizeAllowance = 0;
            int colProduceMinute = 0;
            int colEffiency = 0;
            int colShift = 0;
            int colWorkCenter = 0;
            int colProductionSummary = 0;
            int colRemark = 0;



            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "EmployeeCode", 12); colEmployeeCode = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "EmployeeName", 12); colEmployeeName = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Entity"); colEntity = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Date", 8); colDate = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "DayStatus", 8); colDayStatus = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Oper. Skill", 12); colSkillCategory = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Skill", 8); colSkill = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "AvailableMinute", 8); colAvailableMinute = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "BasicProduceMin", 8); colBasicProd = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "SkillAllowance", 8); colSkillAllowance = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Addi. Allowance", 8); colAdditionalAllowance = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "OrderSizeAllowance", 8); colOrderSizeAllowance = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "ProduceMinute", 8); colProduceMinute = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Efficency", 8); colEffiency = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "WorkCenter", 8); colWorkCenter = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "ShiftName", 8); colShift = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "[PO-OPId-OP-Qty]", 12); colProductionSummary = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Remark", 12); colRemark = xlsCol; xlsCol++;
            colLast = xlsCol;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    reportUtility.SetText(ref sheet, row, colEmployeeCode, dsLocal.Rows[i]["EmployeeCode"].ToString());
                    reportUtility.SetText(ref sheet, row, colEmployeeName, dsLocal.Rows[i]["EmployeeName"].ToString());
                    reportUtility.SetText(ref sheet, row, colEntity, dsLocal.Rows[i]["Entity"].ToString());
                    reportUtility.SetText(ref sheet, row, colDate, dsLocal.Rows[i]["Date"].ToString());
                    reportUtility.SetText(ref sheet, row, colDayStatus, dsLocal.Rows[i]["DayStatus"].ToString());
                    reportUtility.SetText(ref sheet, row, colSkillCategory, dsLocal.Rows[i]["SkillCategory"].ToString());
                    reportUtility.SetText(ref sheet, row, colSkill, dsLocal.Rows[i]["Skill"].ToString());
                    reportUtility.SetText(ref sheet, row, colAvailableMinute, Convert.ToDouble(dsLocal.Rows[i]["AvailableMinute"].ToString()));

                    reportUtility.SetText(ref sheet, row, colBasicProd, Convert.ToDouble(dsLocal.Rows[i]["BasicProduceMin"].ToString()));
                    //reportUtility.SetText(ref sheet, row, colSequence, Convert.ToDouble(dsLocal.Rows[i]["Sequence"].ToString()));
                    reportUtility.SetText(ref sheet, row, colSkillAllowance, Convert.ToDouble(dsLocal.Rows[i]["SkillAllowance"].ToString()));//OTSBD.clsStaticInfo.dbl
                    reportUtility.SetText(ref sheet, row, colAdditionalAllowance, Convert.ToDouble(dsLocal.Rows[i]["AdditionalAllowance"].ToString()));
                    reportUtility.SetText(ref sheet, row, colOrderSizeAllowance, Convert.ToDouble(dsLocal.Rows[i]["OrderSizeAllowance"].ToString()));
                    reportUtility.SetText(ref sheet, row, colProduceMinute, Convert.ToDouble(dsLocal.Rows[i]["ProduceMinute"].ToString()));
                    reportUtility.SetText(ref sheet, row, colEffiency, Convert.ToDouble(dsLocal.Rows[i]["Efficency"].ToString()));
                    reportUtility.SetText(ref sheet, row, colWorkCenter, dsLocal.Rows[i]["WorkCenter"].ToString());
                    reportUtility.SetText(ref sheet, row, colShift, dsLocal.Rows[i]["ShiftName"].ToString());
                    reportUtility.SetText(ref sheet, row, colProductionSummary, dsLocal.Rows[i]["ProductionSummary"].ToString());
                    reportUtility.SetText(ref sheet, row, colRemark, dsLocal.Rows[i]["Remark"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                }


                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                sheet.Range["A1"].RowHeight = 20;
                sheet.Range["A1"].CellStyle.Font.Size = 14;
                sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].Merge();
                sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].CellStyle.Font.Bold = true;
                sheet.Range["A1"].Text = identity.CompanyName;
                sheet.Range["A2"].RowHeight = 15;
                sheet.Range["A2"].CellStyle.Font.Size = 10;
                sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].Merge();
                sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].CellStyle.Font.Bold = true;
                sheet.Range["A2"].Text = "Summary Report of " + " On " + fromDate.ToString("dd-MMM-yyyy") + " To " + toDate.ToString("dd-MMM-yyyy");

                // reportUtility.CompanyPlantHeader(ref sheet, colLast, "Summary Report of " + " On " + toDate.ToString("dd-MMM-yyyy"), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }
        private DataTable GetEmployeeWiseProductionSummarySQL(DateTime fromDate, DateTime toDate, string entityId, string incentiveType, string shiftId, string workCenterId, string dayStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string tempincentiveType = "";
            if (string.IsNullOrEmpty(incentiveType))
            {
                tempincentiveType = @" 'Production Incentive Scheme (Individual)',  'Production Incentive Scheme (Group)' ";
            }
            else
            {
                tempincentiveType = @" '" + incentiveType + @"' ";
            }
            string tempdaystatus = "";
            if (string.IsNullOrEmpty(dayStatus))
            { tempdaystatus = @" "; }
            else { tempdaystatus = @" AND APD.DayStatus= '" + dayStatus + @"' "; }

            string tempShiftId = "";
            if (string.IsNullOrEmpty(shiftId))
            { tempShiftId = @" "; }
            else { tempShiftId = @" AND owe.ShiftId= '" + shiftId + @"' "; }

            string tempwcId = "";
            if (string.IsNullOrEmpty(workCenterId))
            { tempwcId = @" "; }
            else { tempwcId = @" AND owe.workCenterId= '" + workCenterId + @"' "; }
            var cmdText = @"DECLARE @fromDate DATE = '" + fromDate + "', @toDate   DATE = '" + toDate + @"';

-- =========================
-- PRODUCTION PIPELINE
-- =========================
WITH base_prod AS (
    SELECT 
		ei.EmployeeCode,
        ei.EmployeeName,
        MB.EntityId,
        EN.UserName AS Entity,
        CAST(ISNULL(owe.[Date],APD.WorkDate) AS DATE) AS [Date],
       
        SK.SkillCategoryId,
        OP.Id AS OperationId,
        OP.OperationMasterId,
        owe.ProductionOrderId,
        APD.EmpSystemID EmployeeId,PO.OrderLevel,
        (ISNULL(APD.ShiftFullDayDuration,0)+ISNULL(APD.OTHr,0)) AS AvailableMinute,
        BasicProduceMin = ROUND(SUM(ISNULL(owe.Qty,0)) * ISNULL(bt.TotalSPT,0),1),

        DENSE_RANK() OVER (
            PARTITION BY owe.EmployeeId, owe.[Date],wcm.UserName,OP.OperationMasterId,owe.ProductionOrderId
            ORDER BY ROUND(SUM(ISNULL(owe.Qty,0)) * ISNULL(bt.TotalSPT,0),1) DESC
        ) AS Sequence,wcm.UserName WorkCenter,APD.DayStatus

    FROM  AttdnProcessData APD 
	LEFT JOIN dbo.OperationWiseEmployees owe ON APD.EmpSystemID = owe.EmployeeId  AND APD.WorkDate = owe.[Date]
	left join [SCS].[WorkCenterMaster] wcm on wcm.Id=owe.WorkcenterId
    LEFT JOIN trn.ProductionOrder PO ON PO.Id=owe.ProductionOrderId
    LEFT JOIN dbo.EmployeeInformation ei ON ei.SystemId =  APD.EmpSystemID
    LEFT JOIN mst.OperationVariation OP ON OP.Id=owe.OperationVariationId
    LEFT JOIN MST.OperationMaster OM ON OM.Id=OP.OperationMasterId
    LEFT JOIN HKP.Skill SK ON SK.Id=OM.SkillId
    LEFT JOIN trn.ProductionBulletinTemplate pb ON owe.ProductionOrderId = pb.ProductionOrderId
    LEFT JOIN trn.ProductionBulletinTemplateMaster pt 
        ON pt.ProductionBulletinTemplateId=pb.Id AND pt.ProcessId=owe.ProcessId
    LEFT JOIN trn.ProductionBulletinTemplateDetail bt 
        ON bt.OperationVariationId=OP.Id AND pt.Id=bt.ProductionBulletinTemplateMasterId
    LEFT JOIN MST.ManpowerBudget MB ON MB.Id=ei.BudgetCode
    LEFT JOIN ORG.Entity EN ON EN.Id=MB.EntityId
	LEFT JOIN HKP.IncentiveType IT ON IT.Id=MB.IncentiveTypeId
    WHERE APD.[WorkDate] BETWEEN  DATEADD(dd, DATEDIFF(dd, 0, '" + fromDate + "'), 0) and DATEADD(dd, DATEDIFF(dd, 0, '" + toDate + @"'), 0)
    AND IT.UserName IN (" + tempincentiveType + ")  AND MB.EntityId='" + entityId + @"'
    AND ei.EmployeeStatus='Active' " + tempdaystatus + " " + tempShiftId + " " + tempwcId + @"
    GROUP BY 
        MB.EntityId, EN.UserName,
        ei.EmployeeCode, ei.EmployeeName,
        SK.SkillCategoryId,
        OP.Id, OP.OperationMasterId,
        owe.ProductionOrderId,
        owe.EmployeeId,APD.EmpSystemID,APD.WorkDate,
        owe.[Date],PO.OrderLevel,
        bt.TotalSPT,wcm.UserName,APD.ShiftFullDayDuration,APD.OTHr,APD.DayStatus
),

prod_calc AS (
    SELECT 
        x.*,
        SkillAllowance = ISNULL(ROUND(x.BasicProduceMin * PMC.SkillAllowance,1),0),
        AdditionalAllowance = ISNULL(x.BasicProduceMin * PMC.AdditionOperationAllowance,0)
        , OrderSizeAllowance=ISNULL((x.BasicProduceMin)* case when x.OrderLevel='Basic' then OSA.[Basic] 
										when x.OrderLevel='SemiCritical' then OSA.SemiCritical 
										when x.OrderLevel='Critical' then OSA.[Critical] 
										when x.OrderLevel='Special' then OSA.Special end
										,0)/100,

        ProduceMinute =
            x.BasicProduceMin
            + x.BasicProduceMin * ISNULL(PMC.SkillAllowance,0)
            + x.BasicProduceMin * ISNULL(PMC.AdditionOperationAllowance,0)

    FROM base_prod x
    LEFT JOIN dbo.ProducedMinAllowanceChild PMC 
        ON PMC.SkillCategoryId=x.SkillCategoryId 
       AND PMC.OperationSequence=x.Sequence

    LEFT JOIN dbo.OrderSizeAllowance OSA 
        ON OSA.Days = DAY(x.[Date])
),

prod_group AS (
    SELECT 
        Entity,
        [Date],
        EmployeeCode,
        EmployeeName,

        SUM(BasicProduceMin) AS BasicProduceMin,
        SUM(SkillAllowance) AS SkillAllowance,
        SUM(AdditionalAllowance) AS AdditionalAllowance,
        SUM(OrderSizeAllowance) AS OrderSizeAllowance,
        SUM(ProduceMinute) AS ProduceMinute,
        MAX(AvailableMinute) AS AvailableMinute
        ,DayStatus
    FROM prod_calc
    GROUP BY Entity,[Date],EmployeeCode,EmployeeName,DayStatus
),

-- =========================
-- DISTINCT TEXT PIPELINE (YOUR VERSION)
-- =========================

base AS (
    SELECT 
        EI.EmployeeCode,
        CAST(OWE.[Date] AS DATE) AS WorkDate,
        SK.UserName AS Skill,
        SC.UserName AS SkillCategory,
        CAST(PO.Id AS VARCHAR(20)) AS PO,
        OP.UserName AS Operation,
        OP.Id AS OperationId,
        OWE.Qty,wcm.UserName WorkCenter,SD.UserName ShiftName
    FROM dbo.OperationWiseEmployees OWE
    LEFT JOIN EmployeeInformation EI ON EI.SystemId = OWE.EmployeeId
    LEFT JOIN trn.ProductionOrder PO ON PO.Id = OWE.ProductionOrderId
    LEFT JOIN mst.OperationVariation OP ON OP.Id = OWE.OperationVariationId
    LEFT JOIN MST.OperationMaster OM ON OM.Id = OP.OperationMasterId
    LEFT JOIN HKP.Skill SK ON SK.Id = OM.SkillId
    LEFT JOIN HKP.SkillCategory SC ON SC.Id = SK.SkillCategoryId
    LEFT JOIN MST.ManpowerBudget MB ON MB.Id=ei.BudgetCode
	LEFT JOIN HKP.IncentiveType IT ON IT.Id=MB.IncentiveTypeId
    LEFT JOIN [SCS].[WorkCenterMaster] wcm on wcm.Id=owe.WorkcenterId
	LEFT JOIN ShiftDefination SD ON SD.SystemId=OWE.ShiftId
    WHERE OWE.[Date] BETWEEN  DATEADD(dd, DATEDIFF(dd, 0, '" + fromDate + "'), 0) and DATEADD(dd, DATEDIFF(dd, 0, '" + toDate + @"'), 0)
AND IT.UserName IN (" + tempincentiveType + ")  AND MB.EntityId='" + entityId + @"' AND ei.EmployeeStatus='Active' " + tempShiftId + " " + tempwcId + @"
),

skill_agg AS (
    SELECT EmployeeCode, WorkDate,
           STRING_AGG(Skill, ', ') AS Skill
    FROM (SELECT DISTINCT EmployeeCode, WorkDate, Skill FROM base WHERE Skill IS NOT NULL) x
    GROUP BY EmployeeCode, WorkDate
),

skillcat_agg AS (
    SELECT EmployeeCode, WorkDate,
           STRING_AGG(SkillCategory, ', ') AS SkillCategory
    FROM (SELECT DISTINCT EmployeeCode, WorkDate, SkillCategory FROM base WHERE SkillCategory IS NOT NULL) x
    GROUP BY EmployeeCode, WorkDate
),

po_agg AS (
    SELECT EmployeeCode, WorkDate,
           STRING_AGG(PO, ', ') AS PO
    FROM (SELECT DISTINCT EmployeeCode, WorkDate, PO FROM base WHERE PO IS NOT NULL) x
    GROUP BY EmployeeCode, WorkDate
),

op_agg AS (
    SELECT EmployeeCode, WorkDate,
           STRING_AGG(Operation, ', ') AS Operation
    FROM (SELECT DISTINCT EmployeeCode, WorkDate, Operation FROM base WHERE Operation IS NOT NULL) x
    GROUP BY EmployeeCode, WorkDate
),

opid_agg AS (
    SELECT EmployeeCode, WorkDate,
           STRING_AGG(CAST(OperationId AS VARCHAR(20)), ', ') AS OperationId
    FROM (SELECT DISTINCT EmployeeCode, WorkDate, OperationId FROM base WHERE OperationId IS NOT NULL) x
    GROUP BY EmployeeCode, WorkDate
),

qty_agg AS (
    SELECT EmployeeCode, WorkDate, SUM(Qty) AS Qty
    FROM base
    GROUP BY EmployeeCode, WorkDate
),
workcenter_agg AS (
    SELECT EmployeeCode, WorkDate,
           STRING_AGG(WorkCenter, ', ') AS WorkCenter
    FROM (SELECT DISTINCT EmployeeCode, WorkDate, WorkCenter FROM base WHERE WorkCenter IS NOT NULL) x
    GROUP BY EmployeeCode, WorkDate
),
shiftName_agg AS (
    SELECT EmployeeCode, WorkDate,
           STRING_AGG(ShiftName, ', ') AS ShiftName
    FROM (SELECT DISTINCT EmployeeCode, WorkDate, ShiftName FROM base WHERE ShiftName IS NOT NULL) x
    GROUP BY EmployeeCode, WorkDate
)

-- =========================
-- FINAL OUTPUT
-- =========================

SELECT 
    p.Entity,
    CONVERT(VARCHAR(15), p.[Date], 100) AS [Date],
    p.EmployeeCode,
    p.EmployeeName,
    p.DayStatus,
    s.Skill,
    sc.SkillCategory,
    p2.PO,
    o.Operation,
    oid.OperationId,

    ProductionSummary = 
        p2.PO + '-' + oid.OperationId + '-' + o.Operation + '-' + 
        CAST(CAST(q.Qty AS DECIMAL(18,2)) AS NVARCHAR),

    p.BasicProduceMin,
    p.AvailableMinute,
    p.SkillAllowance,
    p.AdditionalAllowance,
    p.OrderSizeAllowance,
    p.ProduceMinute,wc.WorkCenter,sn.ShiftName,

    Efficency =
        CASE 
            WHEN (p.BasicProduceMin + p.SkillAllowance + p.AdditionalAllowance + p.OrderSizeAllowance) > 0
             AND p.AvailableMinute > 0
            THEN ROUND(
                (p.BasicProduceMin + p.SkillAllowance + p.AdditionalAllowance + p.OrderSizeAllowance)
                / p.AvailableMinute * 100, 2)
            ELSE 0
        END,'' Remark

FROM prod_group p
LEFT JOIN skill_agg s ON s.EmployeeCode=p.EmployeeCode AND s.WorkDate=p.[Date]
LEFT JOIN skillcat_agg sc ON sc.EmployeeCode=p.EmployeeCode AND sc.WorkDate=p.[Date]
LEFT JOIN po_agg p2 ON p2.EmployeeCode=p.EmployeeCode AND p2.WorkDate=p.[Date]
LEFT JOIN op_agg o ON o.EmployeeCode=p.EmployeeCode AND o.WorkDate=p.[Date]
LEFT JOIN opid_agg oid ON oid.EmployeeCode=p.EmployeeCode AND oid.WorkDate=p.[Date]
LEFT JOIN qty_agg q ON q.EmployeeCode=p.EmployeeCode AND q.WorkDate=p.[Date]
LEFT JOIN shiftName_agg sn ON sn.EmployeeCode=p.EmployeeCode AND sn.WorkDate=p.[Date]
LEFT JOIN workcenter_agg wc ON wc.EmployeeCode=p.EmployeeCode AND wc.WorkDate=p.[Date]

ORDER BY  p.EmployeeCode,p.[Date]   ";
            return _sqlRepository.GetDataTable(cmdText);
        }
        [HttpGet, Authorize]
        public ActionResult GetEfficencyIncentiveReport(ReportFormat reportFormat, DateTime fromDate, DateTime toDate, string entityId, string incentiveType, string shiftId, string workCenterId, string dayStatus)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var workbook = ProductionEfficencyIncentiveReport(out string reportFileName, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, entityId, incentiveType, shiftId, workCenterId, dayStatus);
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return View();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IWorkbook ProductionEfficencyIncentiveReport(out string reportFileName, string companyId, string plantId, string plantName, DateTime fromDate, DateTime toDate, string entityId, string incentiveType, string shiftId, string workCenterId, string dayStatus)
        {
            string tempEmpId = null;
            try
            {
                var reportUtility = new ReportUtility();
                var excelEngine = new ExcelEngine();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Voucher";


                reportFileName = "EfficencyIncentive" + toDate.ToString("dd-MMM-yyyy");

                var dsLocal = GetEfficencyIncentiveSQL(fromDate, toDate, entityId, incentiveType, shiftId, workCenterId, dayStatus);

                var row = 5;
                var colLast = 1;

                int xlsCol = 1;
                int colEntity = 0;
                int colEmployeeCode = 0;
                int colEmployeeName = 0;
                int colAvailableMinute = 0;
                int colDay1 = 0;
                int colDay2 = 0;
                int colDay3 = 0;
                int colDay4 = 0;
                int colDay5 = 0;
                int colDay6 = 0;
                int colDay7 = 0;
                int colWorkingDays = 0;
                int colTotalWorkingDay = 0;
                int colPresentDays = 0;
                int colTotalProduceMinute = 0;
                int colPeriodEffiency = 0;
                int colIncentiveEfficency = 0;
                int colAmount = 0;
                int colGivenDesignation = 0;
                int colNumberOfMonth = 0;
                int colSkillLevel = 0;
                int colRemark = 0;

                row++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "EmployeeCode", 12); colEmployeeCode = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "EmployeeName", 12); colEmployeeName = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Entity"); colEntity = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "ProduceMinute", 8); colTotalProduceMinute = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "AvailableMinute", 8); colAvailableMinute = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Day1", 8); colDay1 = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Day2", 12); colDay2 = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Day3", 8); colDay3 = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Day4", 8); colDay4 = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Day5", 8); colDay5 = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Day6", 8); colDay6 = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Day7", 8); colDay7 = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "IncentiveDays", 8); colWorkingDays = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "TotalWorkingDay", 8); colTotalWorkingDay = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "EfficiencyDays(Including Absent)", 8); colPresentDays = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "PeriodEfficiency", 8); colPeriodEffiency = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "IncentiveEfficiency", 8); colIncentiveEfficency = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Amount", 8); colAmount = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GivenDesignation", 12); colGivenDesignation = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "NumberOfMonth", 12); colNumberOfMonth = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "SkillLevel", 12); colSkillLevel = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Remark", 12); colRemark = xlsCol; xlsCol++;
                colLast = xlsCol;

                if (dsLocal.Rows.Count > 0)
                {
                    var xRow = row;
                    row++;
                    for (int i = 0; i < dsLocal.Rows.Count; i++)
                    {

                        reportUtility.SetText(ref sheet, row, colEmployeeCode, dsLocal.Rows[i]["EmployeeCode"].ToString());
                        reportUtility.SetText(ref sheet, row, colEmployeeName, dsLocal.Rows[i]["EmployeeName"].ToString());
                        reportUtility.SetText(ref sheet, row, colEntity, dsLocal.Rows[i]["Entity"].ToString());
                        reportUtility.SetText(ref sheet, row, colTotalProduceMinute, Convert.ToDouble(dsLocal.Rows[i]["TotalProduceMinute"].ToString()));
                        reportUtility.SetText(ref sheet, row, colAvailableMinute, Convert.ToDouble(dsLocal.Rows[i]["AvailableMinute"].ToString()));
                        reportUtility.SetText(ref sheet, row, colDay1, dsLocal.Rows[i]["Day1"].ToString());
                        reportUtility.SetText(ref sheet, row, colDay2, dsLocal.Rows[i]["Day2"].ToString());
                        reportUtility.SetText(ref sheet, row, colDay3, dsLocal.Rows[i]["Day3"].ToString());
                        reportUtility.SetText(ref sheet, row, colDay4, dsLocal.Rows[i]["Day4"].ToString());
                        reportUtility.SetText(ref sheet, row, colDay5, dsLocal.Rows[i]["Day5"].ToString());
                        reportUtility.SetText(ref sheet, row, colDay6, dsLocal.Rows[i]["Day6"].ToString());
                        reportUtility.SetText(ref sheet, row, colDay7, dsLocal.Rows[i]["Day7"].ToString());
                        reportUtility.SetText(ref sheet, row, colWorkingDays, Convert.ToDouble(dsLocal.Rows[i]["IncentiveDays"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTotalWorkingDay, Convert.ToDouble(dsLocal.Rows[i]["TotalWorkingDay"].ToString()));
                        reportUtility.SetText(ref sheet, row, colPresentDays, Convert.ToDouble(dsLocal.Rows[i]["EifficiencyDays"].ToString()));
                        reportUtility.SetText(ref sheet, row, colPeriodEffiency, Convert.ToDouble(dsLocal.Rows[i]["PeriodEffeciency"].ToString()));
                        reportUtility.SetText(ref sheet, row, colIncentiveEfficency, Convert.ToDouble(dsLocal.Rows[i]["IncentiveEffenciency"].ToString()));
                        reportUtility.SetText(ref sheet, row, colAmount, Convert.ToDouble(dsLocal.Rows[i]["Amount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colGivenDesignation, dsLocal.Rows[i]["GivenDesignation"].ToString());
                        reportUtility.SetText(ref sheet, row, colNumberOfMonth, dsLocal.Rows[i]["NumberOfMonth"].ToString());
                        reportUtility.SetText(ref sheet, row, colSkillLevel, dsLocal.Rows[i]["SkillLevel"].ToString());
                        reportUtility.SetText(ref sheet, row, colRemark, dsLocal.Rows[i]["Remark"].ToString());

                        sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                        sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                        row++;
                        tempEmpId = dsLocal.Rows[i]["EmployeeCode"].ToString();
                    }


                    //sheet.UsedRange.AutofitColumns();
                    //sheet[1, 2].ColumnWidth = 40;
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                    row += 4;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    sheet.Range["A1"].RowHeight = 20;
                    sheet.Range["A1"].CellStyle.Font.Size = 14;
                    sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].Merge();
                    sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range["A1" + ":" + GetColumnNameForXls(colLast) + "1"].CellStyle.Font.Bold = true;
                    sheet.Range["A1"].Text = identity.CompanyName;
                    sheet.Range["A2"].RowHeight = 15;
                    sheet.Range["A2"].CellStyle.Font.Size = 10;
                    sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].Merge();
                    sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range["A2" + ":" + GetColumnNameForXls(colLast) + "2"].CellStyle.Font.Bold = true;
                    sheet.Range["A2"].Text = "Incentive Report of " + " On " + fromDate.ToString("dd-MMM-yyyy") + " To " + toDate.ToString("dd-MMM-yyyy");
                    // reportUtility.CompanyPlantHeader(ref sheet, colLast, "Incentive Report of " + " On " + fromDate.ToString("dd-MMM-yyyy")+" to " + toDate.ToString("dd-MMM-yyyy"), companyId, plantId, plantName, null);
                    reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
                }

                return workbook;
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(tempEmpId))
                {

                }
                throw ex;
            }
        }
        private DataTable GetEfficencyIncentiveSQL(DateTime fromDate, DateTime toDate, string entityId, string incentiveType, string shiftId, string workCenterId, string dayStatus)
        {
            string tempincentiveType = "";
            if (string.IsNullOrEmpty(incentiveType))
            {
                tempincentiveType = @" 'Production Incentive Scheme (Individual)',  'Production Incentive Scheme (Group)' ";
            }
            else
            {
                tempincentiveType = @" '" + incentiveType + @"' ";
            }
            string tempdaystatus = "";
            if (string.IsNullOrEmpty(dayStatus))
            { tempdaystatus = @" "; }
            else { tempdaystatus = @" AND APD.DayStatus= '" + dayStatus + @"' "; }

            string tempShiftId = "";
            if (string.IsNullOrEmpty(shiftId))
            { tempShiftId = @" "; }
            else { tempShiftId = @" AND owe.ShiftId= '" + shiftId + @"' "; }

            string tempwcId = "";
            if (string.IsNullOrEmpty(workCenterId))
            { tempwcId = @" "; }
            else { tempwcId = @" AND owe.workCenterId= '" + workCenterId + @"' "; }
            var cmdText = @"DECLARE @fromDate DATE = '" + fromDate + @"';
WITH base_prod AS (
    SELECT 
		ei.EmployeeCode,
        ei.EmployeeName,
        MB.EntityId,
        EN.UserName AS Entity,
        CAST(ISNULL(owe.[Date],APD.WorkDate) AS DATE) AS [Date],
		CAST(APD.WorkDate AS DATE) AS WorkDate,
        SK.SkillCategoryId,
        OP.Id AS OperationId,
        OP.OperationMasterId,
        owe.ProductionOrderId,
        APD.EmpSystemID EmployeeId,APD.DayStatus,
        BasicProduceMin = ROUND(SUM(ISNULL(owe.Qty,0)) * ISNULL(bt.TotalSPT,0),1),
		(ISNULL(APD.ShiftFullDayDuration,0)+ISNULL(APD.OTHr,0)) AS AvailableMinute,
        DENSE_RANK() OVER (
            PARTITION BY owe.EmployeeId, owe.[Date],wcm.UserName,OP.OperationMasterId,owe.ProductionOrderId
            ORDER BY ROUND(SUM(ISNULL(owe.Qty,0)) * ISNULL(bt.TotalSPT,0),1) DESC
        ) AS Sequence,wcm.UserName WorkCenter,GD.UserName GivenDesignation
		,NumberOfMonth=DATEDIFF(MONTH,ei.DOJ,GETDATE()),PO.OrderLevel

    FROM AttdnProcessData APD
	left join dbo.OperationWiseEmployees owe ON APD.EmpSystemID=owe.EmployeeId  and apd.WorkDate=owe.[Date]
	left join [SCS].[WorkCenterMaster] wcm on wcm.Id=owe.WorkcenterId
    LEFT JOIN trn.ProductionOrder PO ON PO.Id=owe.ProductionOrderId
    LEFT JOIN dbo.EmployeeInformation ei ON ei.SystemId =  APD.EmpSystemID
	LEFT JOIN HKP.Designation GD ON GD.Id=EI.GivenDesignationId
    LEFT JOIN mst.OperationVariation OP ON OP.Id=owe.OperationVariationId
    LEFT JOIN MST.OperationMaster OM ON OM.Id=OP.OperationMasterId
    LEFT JOIN HKP.Skill SK ON SK.Id=OM.SkillId
    LEFT JOIN trn.ProductionBulletinTemplate pb ON owe.ProductionOrderId = pb.ProductionOrderId
    LEFT JOIN trn.ProductionBulletinTemplateMaster pt 
        ON pt.ProductionBulletinTemplateId=pb.Id AND pt.ProcessId=owe.ProcessId
    LEFT JOIN trn.ProductionBulletinTemplateDetail bt 
        ON bt.OperationVariationId=OP.Id AND pt.Id=bt.ProductionBulletinTemplateMasterId
    LEFT JOIN MST.ManpowerBudget MB ON MB.Id=ei.BudgetCode
	LEFT JOIN HKP.IncentiveType IT ON IT.Id=MB.IncentiveTypeId
    LEFT JOIN ORG.Entity EN ON EN.Id=MB.EntityId
    WHERE APD.[WorkDate] BETWEEN  '" + fromDate + "' AND '" + toDate + @"'
    AND IT.UserName IN (" + tempincentiveType + ")  AND MB.EntityId='" + entityId + @"'
	 AND ei.EmployeeStatus='Active'  " + tempdaystatus + " " + tempShiftId + " " + tempwcId + @"--and ei.employeecode in ('10000351')
    GROUP BY 
        MB.EntityId, EN.UserName,
        ei.EmployeeCode, ei.EmployeeName,
        SK.SkillCategoryId,
        OP.Id, OP.OperationMasterId,
        owe.ProductionOrderId,
        owe.EmployeeId,APD.EmpSystemID,APD.DayStatus,
        owe.[Date],APD.WorkDate,APD.ShiftFullDayDuration,APD.OTHr,
        bt.TotalSPT,wcm.UserName,GD.UserName,ei.DOJ,PO.OrderLevel
),

prod_calc AS (
    SELECT 
        x.*,
        SkillAllowance = ISNULL(ROUND(x.BasicProduceMin * PMC.SkillAllowance,1),0),
        AdditionalAllowance = ISNULL(x.BasicProduceMin * PMC.AdditionOperationAllowance,0),
        OrderSizeAllowance = ISNULL((x.BasicProduceMin)* case when x.OrderLevel='Basic' then OSA.[Basic] 
										when x.OrderLevel='SemiCritical' then OSA.SemiCritical 
										when x.OrderLevel='Critical' then OSA.[Critical] 
										when x.OrderLevel='Special' then OSA.Special end
										,0)/100,
        ProduceMinute =
            x.BasicProduceMin
            + x.BasicProduceMin * ISNULL(PMC.SkillAllowance,0)
            + x.BasicProduceMin * ISNULL(PMC.AdditionOperationAllowance,0)
			,Efficency=case when ISNULL(x.BasicProduceMin + x.BasicProduceMin * ISNULL(PMC.SkillAllowance,0)
            + x.BasicProduceMin * ISNULL(PMC.AdditionOperationAllowance,0),0)>0 
			then ISNULL(x.BasicProduceMin + x.BasicProduceMin * ISNULL(PMC.SkillAllowance,0)
            + x.BasicProduceMin * ISNULL(PMC.AdditionOperationAllowance,0),0)/(ISNULL(x.AvailableMinute,0)) end
			,NAPD.DayStatus NewDayStatus,NAPD2.DayStatus NewDayStatus2,NAPD3.DayStatus NewDayStatus3,NAPD4.DayStatus NewDayStatus4
			,NAPD5.DayStatus NewDayStatus5,NAPD6.DayStatus NewDayStatus6,NAPD7.DayStatus NewDayStatus7
			,'' SkillLevel--,x.NumberOfMonth
	FROM base_prod x
    LEFT JOIN dbo.ProducedMinAllowanceChild PMC 
        ON PMC.SkillCategoryId=x.SkillCategoryId 
       AND PMC.OperationSequence=x.[Sequence]
    LEFT JOIN dbo.OrderSizeAllowance OSA 
        ON OSA.Days = DAY(x.[Date])
		left JOIN AttdnProcessData NAPD 
            ON NAPD.EmpSystemID=x.EmployeeId AND NAPD.WorkDate= @fromDate
	left JOIN AttdnProcessData NAPD2 
            ON NAPD2.EmpSystemID=x.EmployeeId AND NAPD2.WorkDate= DATEADD(DAY,1,@fromDate) and DATEADD(DAY,1,@fromDate)<=@toDate
	left JOIN AttdnProcessData NAPD3 
            ON NAPD3.EmpSystemID=x.EmployeeId AND NAPD3.WorkDate= DATEADD(DAY,2,@fromDate) and DATEADD(DAY,2,@fromDate)<=@toDate
	left JOIN AttdnProcessData NAPD4 
            ON NAPD4.EmpSystemID=x.EmployeeId AND NAPD4.WorkDate= DATEADD(DAY,3,@fromDate) and DATEADD(DAY,3,@fromDate)<=@toDate
	left JOIN AttdnProcessData NAPD5 
            ON NAPD5.EmpSystemID=x.EmployeeId AND NAPD5.WorkDate= DATEADD(DAY,4,@fromDate) and DATEADD(DAY,4,@fromDate)<=@toDate
	left JOIN AttdnProcessData NAPD6 
            ON NAPD6.EmpSystemID=x.EmployeeId AND NAPD6.WorkDate= DATEADD(DAY,5,@fromDate) and DATEADD(DAY,5,@fromDate)<=@toDate
	left JOIN AttdnProcessData NAPD7 
            ON NAPD7.EmpSystemID=x.EmployeeId AND NAPD7.WorkDate= DATEADD(DAY,6,@fromDate) and DATEADD(DAY,6,@fromDate)<=@toDate
)

select z.* from (
select y.Entity,y.EmployeeCode,y.EmployeeName,ISNULL(y.AvailableMinute,0) AvailableMinute,y.GivenDesignation,y.NumberOfMonth,y.SkillLevel

,case when y.Day1>0 then cast(ROUND(y.Day1,2) as nvarchar) ELSE y.NewDayStatus END  Day1
,case when y.Day2>0 then cast(ROUND(y.Day2,2) as nvarchar) ELSE y.NewDayStatus2 END Day2
,case when y.Day3>0 then cast(ROUND(y.Day3,2) as nvarchar) ELSE y.NewDayStatus3 END Day3
,case when y.Day4>0 then cast(ROUND(y.Day4,2) as nvarchar) ELSE y.NewDayStatus4 END Day4
,case when y.Day5>0 then cast(ROUND(y.Day5,2) as nvarchar) ELSE y.NewDayStatus5 END Day5
,case when y.Day6>0 then cast(ROUND(y.Day6,2) as nvarchar) ELSE y.NewDayStatus6 END Day6
,case when y.Day7>0 then cast(ROUND(y.Day7,2) as nvarchar) ELSE y.NewDayStatus7 END Day7

,y.WorkingDays IncentiveDays,y.TotalProduceMinute--,y.produceMinuteTotal--,y.PresentDays EifficiencyDays
 ,
 (
     ISNULL(CASE WHEN NewDayStatus<>'W' THEN 1 END,0)  +
     ISNULL(CASE WHEN NewDayStatus2<>'W'  THEN  1 END,0) +
     ISNULL(CASE WHEN NewDayStatus3<>'W'  THEN  1 END,0) +
     ISNULL(CASE WHEN NewDayStatus4<>'W'  THEN  1 END,0) +
     ISNULL(CASE WHEN NewDayStatus5<>'W'  THEN  1 END,0) +
     ISNULL(CASE WHEN NewDayStatus6<>'W'  THEN  1 END,0) +
     ISNULL(CASE WHEN NewDayStatus7<>'W'  THEN  1 END,0) 
) AS TotalWorkingDay,(
     ISNULL(CASE WHEN NewDayStatus  IN ('P','HDP','WP','L','A') THEN 1 END,0)  +
     ISNULL(CASE WHEN NewDayStatus2 IN ('P','HDP','WP','L','A')  THEN  1 END,0) +
     ISNULL(CASE WHEN NewDayStatus3 IN ('P','HDP','WP','L','A')  THEN  1 END,0) +
     ISNULL(CASE WHEN NewDayStatus4 IN ('P','HDP','WP','L','A')  THEN  1 END,0) +
     ISNULL(CASE WHEN NewDayStatus5 IN ('P','HDP','WP','L','A')  THEN  1 END,0) +
     ISNULL(CASE WHEN NewDayStatus6 IN ('P','HDP','WP','L','A')  THEN  1 END,0) +
     ISNULL(CASE WHEN NewDayStatus7 IN ('P','HDP','WP','L','A')  THEN  1 END,0) 
) AS EifficiencyDays
,irc.Effeciency,irc.EffeciencyRate
,PeriodEffeciency=ISNULL(case when y.AvailableMinute>0 then (y.TotalProduceMinute/y.AvailableMinute)*100 end,0)
,IncentiveEffenciency=ROUND(ISNULL(case when y.AvailableMinute>0 and (y.TotalProduceMinute/y.AvailableMinute)*100>isnull(irc.Effeciency,0) then (y.TotalProduceMinute/y.AvailableMinute)*100 end-irc.Effeciency,0),2)
,Amount=ROUND(ISNULL((case when y.AvailableMinute>0 and (y.TotalProduceMinute/y.AvailableMinute)*100>isnull(irc.Effeciency,0) then (y.TotalProduceMinute/y.AvailableMinute)*100 end-irc.Effeciency)*irc.EffeciencyRate,0),2)*y.WorkingDays,'' Remark
from ( 
SELECT 
    Entity,
    EntityId,
    EmployeeCode,
    EmployeeName,EmployeeId,NewDayStatus,NewDayStatus2,NewDayStatus3,NewDayStatus4,NewDayStatus5,NewDayStatus6,NewDayStatus7,
    GivenDesignation,SkillLevel,NumberOfMonth,
	-- ✅ DayStatus Pivot
    MAX(CASE WHEN [WorkDate] = @fromDate  THEN DayStatus END) AS DayStatus1,
    MAX(CASE WHEN [WorkDate] = DATEADD(DAY,1,@fromDate) THEN DayStatus END) AS DayStatus2,
    MAX(CASE WHEN [WorkDate] = DATEADD(DAY,2,@fromDate) THEN DayStatus END) AS DayStatus3,
    MAX(CASE WHEN [WorkDate] = DATEADD(DAY,3,@fromDate) THEN DayStatus END) AS DayStatus4,
    MAX(CASE WHEN [WorkDate] = DATEADD(DAY,4,@fromDate) THEN DayStatus END) AS DayStatus5,
    MAX(CASE WHEN [WorkDate] = DATEADD(DAY,5,@fromDate) THEN DayStatus END) AS DayStatus6,
    MAX(CASE WHEN [WorkDate] = DATEADD(DAY,6,@fromDate) THEN DayStatus END) AS DayStatus7,

    -- ✅ Optional: Efficiency Pivot
    MAX(CASE WHEN [Date] = @fromDate THEN Efficency END) AS Day1,
    MAX(CASE WHEN [Date] = DATEADD(DAY,1,@fromDate) THEN Efficency END) AS Day2,
    MAX(CASE WHEN [Date] = DATEADD(DAY,2,@fromDate) THEN Efficency END) AS Day3,
    MAX(CASE WHEN [Date] = DATEADD(DAY,3,@fromDate) THEN Efficency END) AS Day4,
    MAX(CASE WHEN [Date] = DATEADD(DAY,4,@fromDate) THEN Efficency END) AS Day5,
    MAX(CASE WHEN [Date] = DATEADD(DAY,5,@fromDate) THEN Efficency END) AS Day6,
    MAX(CASE WHEN [Date] = DATEADD(DAY,6,@fromDate) THEN Efficency END) AS Day7,

	 (
     ISNULL(MAX(CASE WHEN [Date] = @fromDate AND NewDayStatus IN ('P','HDP','WP','L','A') THEN AvailableMinute END),0)  +
     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,1,@fromDate) AND NewDayStatus2 IN ('P','HDP','WP','L','A')  THEN  AvailableMinute END),0) +
     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,2,@fromDate) AND NewDayStatus3 IN ('P','HDP','WP','L','A')  THEN  AvailableMinute END),0) +
     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,3,@fromDate) AND NewDayStatus4 IN ('P','HDP','WP','L','A')  THEN  AvailableMinute END),0) +
     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,4,@fromDate) AND NewDayStatus5 IN ('P','HDP','WP','L','A')  THEN  AvailableMinute END),0) +
     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,5,@fromDate) AND NewDayStatus6 IN ('P','HDP','WP','L','A')  THEN  AvailableMinute END),0) +
     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,6,@fromDate) AND NewDayStatus7 IN ('P','HDP','WP','L','A')  THEN  AvailableMinute END),0) 
) AS AvailableMinute
--	,(
--     ISNULL(MAX(CASE WHEN [Date] = @fromDate THEN ProduceMinute END),0)  +
--     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,1,@fromDate) THEN  ProduceMinute END),0) +
--     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,2,@fromDate) THEN  ProduceMinute END),0) +
--     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,3,@fromDate) THEN  ProduceMinute END),0) +
--     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,4,@fromDate) THEN  ProduceMinute END),0) +
--     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,5,@fromDate) THEN  ProduceMinute END),0) +
--     ISNULL(MAX(CASE WHEN [Date] = DATEADD(DAY,6,@fromDate) THEN  ProduceMinute END),0) 
--) AS TotalProduceMinute
,sum(ProduceMinute) TotalProduceMinute
,(
    CASE WHEN ISNULL(MAX(CASE WHEN  [Date] = @fromDate THEN Efficency END),0) > 0 THEN 1 ELSE 0 END +
    CASE WHEN ISNULL(MAX(CASE WHEN  [Date] = DATEADD(DAY,1,@fromDate) THEN  Efficency END),0) > 0 THEN 1 ELSE 0 END +
    CASE WHEN ISNULL(MAX(CASE WHEN  [Date] = DATEADD(DAY,2,@fromDate) THEN  Efficency END),0) > 0 THEN 1 ELSE 0 END +
    CASE WHEN ISNULL(MAX(CASE WHEN  [Date] = DATEADD(DAY,3,@fromDate) THEN  Efficency END),0) > 0 THEN 1 ELSE 0 END +
    CASE WHEN ISNULL(MAX(CASE WHEN  [Date] = DATEADD(DAY,4,@fromDate) THEN  Efficency END),0) > 0 THEN 1 ELSE 0 END +
    CASE WHEN ISNULL(MAX(CASE WHEN  [Date] = DATEADD(DAY,5,@fromDate) THEN  Efficency END),0) > 0 THEN 1 ELSE 0 END +
    CASE WHEN ISNULL(MAX(CASE WHEN  [Date] = DATEADD(DAY,6,@fromDate) THEN  Efficency END),0) > 0 THEN 1 ELSE 0 END
) AS WorkingDays
,(
    CASE WHEN MAX(CASE WHEN  [Date]=@fromDate THEN DayStatus END) IN ('P','HDP','WP','L','A') THEN 1 ELSE 0 END +
    CASE WHEN MAX(CASE WHEN  [Date]=DATEADD(DAY,1,@fromDate) THEN  DayStatus END) IN ('P','HDP','WP','L','A') THEN 1 ELSE 0 END +
    CASE WHEN MAX(CASE WHEN  [Date]=DATEADD(DAY,2,@fromDate) THEN  DayStatus END) IN ('P','HDP','WP','L','A') THEN 1 ELSE 0 END +
    CASE WHEN MAX(CASE WHEN  [Date]=DATEADD(DAY,3,@fromDate) THEN  DayStatus END) IN ('P','HDP','WP','L','A') THEN 1 ELSE 0 END +
    CASE WHEN MAX(CASE WHEN  [Date]=DATEADD(DAY,4,@fromDate) THEN  DayStatus END) IN ('P','HDP','WP','L','A') THEN 1 ELSE 0 END +
    CASE WHEN MAX(CASE WHEN  [Date]=DATEADD(DAY,5,@fromDate) THEN  DayStatus END) IN ('P','HDP','WP','L','A') THEN 1 ELSE 0 END +
    CASE WHEN MAX(CASE WHEN  [Date]=DATEADD(DAY,6,@fromDate) THEN  DayStatus END) IN ('P','HDP','WP','L','A') THEN 1 ELSE 0 END
) AS PresentDays

FROM prod_calc

GROUP BY 
    Entity, EntityId,EmployeeId,  EmployeeCode,  EmployeeName,GivenDesignation,NumberOfMonth,SkillLevel 
	,NewDayStatus,NewDayStatus2,NewDayStatus3,NewDayStatus4,NewDayStatus5,NewDayStatus6,NewDayStatus7
	) y

	LEFT JOIN IncentiveRateSetupEntity irs ON irs.EntityId=y.EntityId
	left join dbo.IncentiveRateSetupChild irc on irc.HeaderId=irs.HeaderId
  ) z       
ORDER BY 
    z.PeriodEffeciency  desc
  ";
            return _sqlRepository.GetDataTable(cmdText);
        }

        [HttpPost, Authorize]
        public ActionResult GetDayStatusCbo()
        {
            return Json(GetDayStatusCboData(), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetDayStatusCboData()
        {
            try
            {
                var str = @"select distinct DayStatus [Value],DayStatus [Text] from AttdnProcessData where DayStatus IS Not NULL";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
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