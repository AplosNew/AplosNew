#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.General.TaskScheduler;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.OrderManagement.Production;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class POWiseProductionStatusReportController : BaseController
    {


        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        TasksService tasksService = new TasksService();
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();

        public POWiseProductionStatusReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionOrderDataList(string productionStatusId)
        {
            return Json(_productionSummaryData.GetProductionOrderDataList(productionStatusId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult getFilters(string productionStatusId, string poId)
        {
            JsonResult json = Json(_productionSummaryData.Productionfilters(productionStatusId,poId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

       
        [HttpPost, Authorize]
        public ActionResult GetPOWiseProductionStatusData()
        {
            var jsondata = Json(_productionSummaryData.GetPOWiseProductionStatusData(), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        private string GetDate(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            try
            {
                return Convert.ToDateTime(s).ToString("dd-MMM-yyyy");
            }
            catch (Exception)
            {
                return "";
            }
        }
        private void SetDate(IRange Cell, string s)
        {
            if (string.IsNullOrEmpty(s))
                return;

            try
            {
                Cell.DateTime = Convert.ToDateTime(s);
            }
            catch (Exception)
            {
                return;
            }
        }
        private string CellAddr(int Col, int Row)
        {
            return clsStaticInfo.GetxlsCol(Col) + Row.ToString();
        }

        [HttpPost, Authorize]
        public ActionResult ProductionDataXls(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("ID") || item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;

                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("ID") || item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;

                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }
                //string filename = GridToExcelReportUpd(dt, "", reportFileName);

                string fileName = "";
                fileName = ProductionDataReport(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string ProductionDataReport(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[1].Name = "POData";
                sheet = workbook.Worksheets[1];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;


                COL++;
                int colstart = COL;
                sheet[ROW, COL].Text = "PONo";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrderID = COL;

                COL++;
                sheet[ROW, COL].Text = "ProcessIndex";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcessIndex = COL;

                COL++;
                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcess = COL;

                COL++;
                sheet[ROW, COL].Text = "POProcessSequence";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOProcessSeq = COL;

                COL++;
                sheet[ROW, COL].Text = "StandardProcessSequence";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStandardProcessSeq = COL;

                COL++;
                sheet[ROW, COL].Text = "BaseProcessApplicable";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBaseProcessApplicable = COL;

                COL++;
                sheet[ROW, COL].Text = "POProcessStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOProcessStatus = COL;


                COL++;
                sheet[ROW, COL].Text = "POStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOStatus = COL;

                COL++;
                sheet[ROW, COL].Text = "WorkCenter";
                sheet[ROW, COL].ColumnWidth = 16;
                int colWorkCenter = COL;

                COL++;
                sheet[ROW, COL].Text = "Shift";
                sheet[ROW, COL].ColumnWidth = 16;
                int colShift = COL;

                COL++;
                sheet[ROW, COL].Text = "Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlanDate = COL;

                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colbuyer = COL;

                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomer = COL;

                COL++;
                sheet[ROW, COL].Text = "LotNumber";
                sheet[ROW, COL].ColumnWidth = 16;
                int colLotNumber = COL;

                COL++;
                sheet[ROW, COL].Text = "OwnOrderNo";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOwnOrderNo = COL;

                //COL++;
                //sheet[ROW, COL].Text = "OwnItemNo";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int colOwnStyleNo = COL;

                COL++;
                sheet[ROW, COL].Text = "SalesOrderIds";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderIds = COL;

                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProduct = COL;

                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 28;
                int colArticle = COL;

                COL++;
                sheet[ROW, COL].Text = "WorkStation";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWorkStation = COL;

                COL++;
                sheet[ROW, COL].Text = "WorkingHours";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActualWorkHours = COL;

                COL++;
                sheet[ROW, COL].Text = "PlannedQty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlannedQty = COL;

                COL++;
                sheet[ROW, COL].Text = "ProcessWisePlanQty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProcessWisePlanQty = COL;

                COL++;
                sheet[ROW, COL].Text = "ProductionQty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualQty = COL;


                COL++;
                sheet[ROW, COL].Text = "UpToDateProduction";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colUpToDate = COL;

                COL++;
                sheet[ROW, COL].Text = "PreProUDProd";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPreProUDProd = COL;

                COL++;
                sheet[ROW, COL].Text = "WIP";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWIP = COL;

                COL++;
                sheet[ROW, COL].Text = "UptoDateProPercentage";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colUptoDateProduction = COL;

                COL++;
                sheet[ROW, COL].Text = "FirstBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colFirstProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "LastBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLastProBookDate = COL;

                //COL++;
                //sheet[ROW, COL].Text = "FirstShipmentDate";
                //sheet[ROW, COL].ColumnWidth = 12;
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int colFirstshipmentDate = COL;

                //COL++;
                //sheet[ROW, COL].Text = "LastShipmentDate";
                //sheet[ROW, COL].ColumnWidth = 12;
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int colLastshipmentDate = COL;

               

                //COL++;
                //sheet[ROW, COL].Text = "RelayProcess";
                //sheet[ROW, COL].ColumnWidth = 12;
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int colRelayProcess = COL;

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
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, colEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, colProcess].Text = data.Rows[i]["Process"].ToString();
                    sheet[ROW, colPOProcessSeq].Number = clsStaticInfo.dbl(data.Rows[i]["POProcessSequence"].ToString());
                    sheet[ROW, colStandardProcessSeq].Number = clsStaticInfo.dbl(data.Rows[i]["StandardProcessSequence"].ToString());
                    sheet[ROW, colProcessIndex].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessIndex"].ToString());
                    sheet[ROW, colBaseProcessApplicable].Text = data.Rows[i]["BaseProcess"].ToString();
                    sheet[ROW, colPOProcessStatus].Text = data.Rows[i]["POProcessStatus"].ToString();
                    sheet[ROW, colProductionOrderID].Text = data.Rows[i]["PONo"].ToString();
                    sheet[ROW, colPOStatus].Text = data.Rows[i]["POStatus"].ToString();
                    sheet[ROW, colWorkCenter].Text = data.Rows[i]["WorkCenter"].ToString();
                    sheet[ROW, colShift].Text = data.Rows[i]["ProductionShift"].ToString();
                    sheet[ROW, colPlanDate].Text = GetDate(data.Rows[i]["ActualDate"].ToString());
                    sheet[ROW, colbuyer].Text = data.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colCustomer].Text = data.Rows[i]["Customer"].ToString();
                    sheet[ROW, colLotNumber].Text = data.Rows[i]["LotNumber"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = data.Rows[i]["OwnOrderNo"].ToString();
                    sheet[ROW, colSalesOrderIds].Text = data.Rows[i]["SONos"].ToString();
                    sheet[ROW, colProduct].Text = data.Rows[i]["Product"].ToString();
                    sheet[ROW, colArticle].Text = data.Rows[i]["Article"].ToString();
                    sheet[ROW, colWorkStation].Number = clsStaticInfo.dbl(data.Rows[i]["NoOfWorkStation"].ToString());
                    sheet[ROW, colActualWorkHours].Number = clsStaticInfo.dbl(data.Rows[i]["ProductionHours"].ToString());
                    sheet[ROW, colPlannedQty].Number = clsStaticInfo.dbl(data.Rows[i]["PlannedQty"].ToString());
                    sheet[ROW, colProcessWisePlanQty].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessWisePlanQty"].ToString());
                    sheet[ROW, colActualQty].Number = clsStaticInfo.dbl(data.Rows[i]["ActualQty"].ToString());
                    sheet.Range[ROW, colUpToDate].Number = clsStaticInfo.dbl(data.Rows[i]["UpToDateProduction"].ToString());
                    sheet.Range[ROW, colPreProUDProd].Number = clsStaticInfo.dbl(data.Rows[i]["PreProUDProd"].ToString());
                    sheet.Range[ROW, colWIP].Number = clsStaticInfo.dbl(data.Rows[i]["WIP"].ToString());
                    sheet[ROW, colUptoDateProduction].Number = clsStaticInfo.dbl(data.Rows[i]["UptoDateProPercentage"].ToString());
                    sheet[ROW, colFirstProBookDate].Text = data.Rows[i]["FirstBookDate"].ToString();
                    sheet[ROW, colLastProBookDate].Text = data.Rows[i]["LastBookDate"].ToString();

                   // sheet[ROW, colOwnStyleNo].Text = data.Rows[i]["OwnStyleNo"].ToString();

                    //sheet[ROW, colFirstshipmentDate].Text = data.Rows[i]["FirstShipmentDate"].ToString();
                    //sheet[ROW, colLastshipmentDate].Text = data.Rows[i]["LastShipmentDate"].ToString();

                    //sheet.Range[ROW, colWIP].Formula = "IF(MAX($" + clsStaticInfo.GetxlsCol(colPlanDate) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPlanDate) + "$" + LastRow.ToString() + "<>" + clsStaticInfo.GetxlsCol(colPlanDate) + ROW.ToString() + "),0,IF(" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + ROW.ToString() + "=1,0,SUMIFS($" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + LastRow.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colPOProcessSeq) + ROW.ToString() + "-1,$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colProductionOrderID) + startRow.ToString() + ")-SUMIFS($" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + LastRow.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colPOProcessSeq) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colProductionOrderID) + startRow.ToString() + ")))";

                    //sheet.Range[ROW, colWIP].Formula =  "SUMIFS($" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + LastRow.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colPOProcessSeq) + ROW.ToString() + "-1,$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colProductionOrderID) + startRow.ToString() + "))";

                    //sheet[ROW, colCurrent].Formula = "SUMIFS($" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + LastRow.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colPOProcessSeq) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colProductionOrderID) + startRow.ToString() + ")";

                    //var formuolac = "SUMIFS($" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + LastRow.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colPOProcessSeq) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colProductionOrderID) + startRow.ToString() + ")";


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "PO Wise Production Status Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


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

                #region Pivot

                string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "PO Wise Production Status Report" + identity.UserId + ".xlsx";

                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }

                workbook.Worksheets[0].Name = "POReport";

                IWorksheet pivotSheet = workbook.Worksheets[0];
                IPivotCache cache = workbook.PivotCaches.Add(workbook.Worksheets[1][startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);

                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProcessIndex - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProcess - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colArticle - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderID - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPOProcessSeq - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProcess - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colUpToDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPreProUDProd - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colWIP - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOwnOrderNo - 1].Axis = PivotAxisTypes.Row;


                pivotTable.Fields[colPlanDate - 1].Axis = PivotAxisTypes.Column;
                pivotTable.Fields[colActualQty - 1].Axis = PivotAxisTypes.Data;



                IPivotField field = pivotTable.Fields[colActualQty - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "ActualQty", PivotSubtotalTypes.None);

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colProcess - 1 || i == colEntity - 1 || i == colWorkCenter - 1)
                        continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }

                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "PO Wise Production Status Report", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                workbook.Worksheets[0].UsedRange["A7"].FreezePanes();


                #endregion Buyer Summary

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
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

        public List<Dictionary<string, object>> ReadData(string path)
        {
            List<Dictionary<string, object>> data = null;
            //string path = "";
            DataSet dsExcel = null;
            try
            {
                data = new List<Dictionary<string, object>>();
                //SaveFile(out path);
                ReadFile(path, out dsExcel);
                data = dsExcel.Tables[0].ToList<Dictionary<string, object>>();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetViewData(Dictionary<string, string> parameters)
        {
            try
            {
                //DataSet dsExcel = null;
                //string fileName = "";
                //fileName = ProductionDataReport(parameters, "Production Report");

                //ReadFile(fileName, out dsExcel);
                //for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                //{
                //    dsExcel.Tables[0].Columns.Add("Id", typeof(string));
                //    break;
                //}
                DataTable dtdata;
                ReportSQL(parameters, out dtdata);

                List<Dictionary<string, object>> data = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtdata);

                //DeleteData();
                //SaveReportData(data);

                //var sql = @"SELECT * FROM [dbo].[POWiseProductionStatusReport]";
                //JsonResult json = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void DeleteData()
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[POWiseProductionStatusReport]";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        public void ReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            {
                string partyId = "AND 1=1";
                if (!string.IsNullOrEmpty(parameters["CustomerId"].ToString()))
                {
                    partyId = "AND XMO.PartyId in(" + parameters["CustomerId"] + @")";
                }

                string sql = @"Select A.* from (SELECT DISTINCT PP.Id PSId,trke.UserName AS Entity,PP.ProductionOrderID PONo,PSEQ.ProcessIndex,isnull(p.UserName, '') AS Process,p.Sequence StandardProcessSequence,POPS.[Sequence] POProcessSequence
		,BaseProcess = CASE WHEN P.IsProductionProcess = 1 THEN 'Yes' ELSE 'No' END,FORMAT(PP.ProductionDate, 'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty,ProcessWisePlanQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)*POPS.Qty,PSEQ.Qty UpToDateProduction,ISNULL(PSEQ.PreQty, 0) PreProUDProd
		,WIP = ISNULL(PSEQ.PreQty-PSEQ.Qty, 0),UptoDateProPercentage = (pp.Quantity / (select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)) / 100,wcm.UserName AS WorkCenter,CPL.UserName AS ProductionShift,ISNULL(pp.StandardName, ord.Article) Article
		,ord.Product,PS.UserName POStatus,FLB.FirstBookDate,FLB.LastBookDate
		,PP.LotNumber,POProcessStatus=CASE WHEN POPS.IsCompleted=1 THEN 'Completed' ELSE 'Not Completed' END
--additional info
		,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pp.ProductionOrderId=Xpod.ProductionOrderId " + partyId + @" for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')	
,Buyer=STUFF((select distinct ','+XB.UserName from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
SONos=STUFF((select distinct ','+XSO.Id from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
PlannedQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId),
BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
wcm.NoOfWorkStation,ProductionHours=(select top(1) Hour from scs.WorkCenterMasterEffectiveDate Where WorkCenterMasterId=wcm.Id Order BY StartDate Desc)
                            
FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,COUNT(*) AS ProductionHours,SUM(ps.Quantity) AS Quantity,PS.ResponsiblePersonId,PS.LotNumber

FROM trn.ProductionSummary AS ps 
left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,PS.ResponsiblePersonId,PS.LotNumber
) AS pp
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS FirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS LastBookDate,ProcessId,ProductionOrderId from TRN.ProductionSummary GROUP BY ProcessId,ProductionOrderId) FLB ON FLB.ProcessId=PP.ProcessId AND FLB.ProductionOrderId=PP.ProductionOrderId
LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId=pp.ProductionShiftId
LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
LEFT JOIN trn.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id AND POPS.ProcessId=pp.ProcessId
left outer join (
select POD.ProductionOrderId,MA.StandardName AS Article,PM.UserName AS Product
from trn.ProductionOrderDetail POD 
left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
group by MA.StandardName,PM.UserName,POD.ProductionOrderId
) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN(
SELECT T1.*,T2.Qty PreQty FROM
(Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence) ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T1
LEFT JOIN (Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence)+1 ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T2 ON T1.ProcessIndex=T2.ProcessIndex AND  T1.ProductionOrderId=T2.ProductionOrderId
) PSEQ ON PSEQ.ProductionOrderId=PP.ProductionOrderID AND POPS.[Sequence]=PSEQ.Sequence
Where TRKE.Id in(" + parameters["EntityId"] + @")
AND ISNULL(PP.ResponsiblePersonId,'') in(" + parameters["ResponsiblePersonId"] + @")
AND ps.Id in(" + parameters["ProductionStatusId"] + @"))A Order BY A.PONo,A.ProcessIndex ";

                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        private string GetGeneralPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "POWiseProductionStatusReport", out idFromDB);
            systemID = idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        private void SaveReportData(List<Dictionary<string, object>> dataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
               string systemid = GetGeneralPK();
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.POWiseProductionStatusReport WHERE Id =''", out dsDetail, false, "1");
                int count = 0;
                foreach (var item in dataList)
                {
                    count++;
                    DataView dv = new DataView(dsDetail.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = systemid + "-" + count;
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsDetail);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetReportData()
        {
            var sql = @"SELECT *FROM [dbo].[POWiseProductionStatusReport]";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
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
                //DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(6, 1, 50000, 27, ExcelExportDataTableOptions.ColumnNames);
                dt.DefaultView.RowFilter = "isnull(Entity,'')<>''";
                dt = dt.DefaultView.ToTable();
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

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

           
            dr.EndEdit();
        }

        [HttpPost, Authorize]
        public ActionResult GetSummaryViewData(Dictionary<string, string> parameters)
        {
            try
            {
                DataTable dtdata;
                GetSummaryReportSQL(parameters, out dtdata);

                List<Dictionary<string, object>> data = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtdata);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [HttpPost, Authorize]
        public ActionResult GetWCViewData(Dictionary<string, string> parameters)
        {
            try
            {
                DataTable dtdata;
                GetWCReportSQL(parameters, out dtdata);

                List<Dictionary<string, object>> data = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtdata);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void GetWCReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            {
                string partyId = "AND 1=1";
                if (!string.IsNullOrEmpty(parameters["CustomerId"].ToString()))
                {
                    partyId = "AND XMO.PartyId in(" + parameters["CustomerId"] + @")";
                }
                string sql = @"Select A.Entity,A.Process,A.POProcessSequence,A.StandardProcessSequence,A.PONo,A.ProcessIndex,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.WorkCenter,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article,A.NoOfWorkStation,A.ProductionHours
,A.PlannedQty,SUM(A.ActualQty) ActualQty,A.UpToDateProduction,A.PreProUDProd,A.FirstBookDate,A.LastBookDate 
from (SELECT DISTINCT PP.Id PSId,trke.UserName AS Entity,PP.ProductionOrderID PONo,PSEQ.ProcessIndex,isnull(p.UserName, FSFG.UserName) AS Process,p.Sequence StandardProcessSequence,POPS.[Sequence] POProcessSequence
		,BaseProcess = CASE WHEN P.IsProductionProcess = 1 THEN 'Yes' ELSE 'No' END,pp.Quantity AS ActualQty,ProcessWisePlanQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)*POPS.Qty,PSEQ.Qty UpToDateProduction,ISNULL(PSEQ.PreQty, 0) PreProUDProd
		,WIP = ISNULL(PSEQ.PreQty-PSEQ.Qty, 0),UptoDateProPercentage = (pp.Quantity / (select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)) / 100,wcm.UserName AS WorkCenter,ISNULL(pp.StandardName, ord.Article) Article
		,ord.Product,PS.UserName POStatus,FLB.FirstBookDate,FLB.LastBookDate --,ORD.FirstShipmentDate,ORD.LastShipmentDate,
		,PP.LotNumber,POProcessStatus=CASE WHEN POPS.IsCompleted=1 THEN 'Completed' ELSE 'Not Completed' END
--additional info
		,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pp.ProductionOrderId=Xpod.ProductionOrderId " + partyId + @" for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')	
,Buyer=STUFF((select distinct ','+XB.UserName from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
SONos=STUFF((select distinct ','+XSO.Id from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
PlannedQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId),
BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
wcm.NoOfWorkStation,ProductionHours=(select top(1) Hour from scs.WorkCenterMasterEffectiveDate Where WorkCenterMasterId=wcm.Id Order BY StartDate Desc)
 FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId, ps.ProductionOrderId,ps.WorkCenterMasterId,SUM(ps.Quantity) AS Quantity,PS.ResponsiblePersonId,PS.LotNumber
FROM trn.ProductionSummary AS ps 
left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionOrderId,ps.WorkCenterMasterId,PS.ResponsiblePersonId,PS.LotNumber
) AS pp
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS FirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS LastBookDate,ProcessId,ProductionOrderId from TRN.ProductionSummary GROUP BY ProcessId,ProductionOrderId) FLB ON FLB.ProcessId=PP.ProcessId AND FLB.ProductionOrderId=PP.ProductionOrderId
LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
LEFT JOIN trn.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id AND POPS.ProcessId=pp.ProcessId
left outer join (
select POD.ProductionOrderId,MA.StandardName AS Article,PM.UserName AS Product
from trn.ProductionOrderDetail POD 
left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
group by MA.StandardName,PM.UserName,POD.ProductionOrderId
) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN(
SELECT T1.*,T2.Qty PreQty FROM
(Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence) ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T1
LEFT JOIN (Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence)+1 ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T2 ON T1.ProcessIndex=T2.ProcessIndex AND  T1.ProductionOrderId=T2.ProductionOrderId
) PSEQ ON PSEQ.ProductionOrderId=PP.ProductionOrderID AND POPS.[Sequence]=PSEQ.Sequence
Where TRKE.Id in(" + parameters["EntityId"] + @")
AND ISNULL(PP.ResponsiblePersonId,'') in(" + parameters["ResponsiblePersonId"] + @")
AND ps.Id in(" + parameters["ProductionStatusId"] + @"))A 
GROUP BY A.PONo,A.ProcessIndex,A.Process,A.UpToDateProduction,A.PreProUDProd,A.POProcessSequence
,A.Entity,A.Process,A.POProcessSequence,A.StandardProcessSequence,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.WorkCenter,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article,A.NoOfWorkStation,A.ProductionHours,A.PlannedQty,A.FirstBookDate,A.LastBookDate
Order BY A.PONo,A.ProcessIndex";


                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpPost, Authorize]
        public ActionResult ProductionSummaryDataXls(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("ID") || item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;

                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("ID") || item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;

                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = ProductionDataSummaryReport(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string ProductionDataSummaryReport(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[1].Name = "POData";
                sheet = workbook.Worksheets[1];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;



                COL++;
                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcess = COL;

                COL++;
                sheet[ROW, COL].Text = "POProcessSequence";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOProcessSeq = COL;

                COL++;
                sheet[ROW, COL].Text = "StandardProcessSequence";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStandardProcessSeq = COL;


                COL++;
                int colstart = COL;
                sheet[ROW, COL].Text = "PONo";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrderID = COL;

                COL++;
                sheet[ROW, COL].Text = "ProcessIndex";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcessIndex = COL;

                COL++;
                sheet[ROW, COL].Text = "BaseProcessApplicable";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBaseProcessApplicable = COL;

                COL++;
                sheet[ROW, COL].Text = "POProcessStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOProcessStatus = COL;

                COL++;
                sheet[ROW, COL].Text = "POStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOStatus = COL;

                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colbuyer = COL;

                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomer = COL;

                COL++;
                sheet[ROW, COL].Text = "LotNumber";
                sheet[ROW, COL].ColumnWidth = 16;
                int colLotNumber = COL;

                COL++;
                sheet[ROW, COL].Text = "OwnOrderNo";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOwnOrderNo = COL;

                COL++;
                sheet[ROW, COL].Text = "SONos";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderIds = COL;

                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProduct = COL;

                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 28;
                int colArticle = COL;

                COL++;
                sheet[ROW, COL].Text = "PlannedQty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlannedQty = COL;

                //COL++;
                //sheet[ROW, COL].Text = "ProcessWisePlanQty";
                //sheet[ROW, COL].ColumnWidth = 12;
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int colProcessWisePlanQty = COL;

                COL++;
                sheet[ROW, COL].Text = "ActualQty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualQty = COL;


                COL++;
                sheet[ROW, COL].Text = "UpToDateProduction";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colUpToDate = COL;

                COL++;
                sheet[ROW, COL].Text = "PreProUDProd";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPreProUDProd = COL;

                COL++;
                sheet[ROW, COL].Text = "WIP";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWIP = COL;

                //COL++;
                //sheet[ROW, COL].Text = "UptoDateProPercentage";
                //sheet[ROW, COL].ColumnWidth = 12;
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int colUptoDateProduction = COL;

                COL++;
                sheet[ROW, COL].Text = "FirstBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colFirstProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "LastBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLastProBookDate = COL;


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
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, colEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, colProcess].Text = data.Rows[i]["Process"].ToString();
                    sheet[ROW, colPOProcessSeq].Number = clsStaticInfo.dbl(data.Rows[i]["POProcessSequence"].ToString());
                    sheet[ROW, colStandardProcessSeq].Number = clsStaticInfo.dbl(data.Rows[i]["StandardProcessSequence"].ToString());
                    sheet[ROW, colProcessIndex].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessIndex"].ToString());
                    sheet[ROW, colBaseProcessApplicable].Text = data.Rows[i]["BaseProcess"].ToString();
                    sheet[ROW, colPOProcessStatus].Text = data.Rows[i]["POProcessStatus"].ToString();
                    sheet[ROW, colProductionOrderID].Text = data.Rows[i]["PONo"].ToString();
                    sheet[ROW, colPOStatus].Text = data.Rows[i]["POStatus"].ToString();
                    sheet[ROW, colbuyer].Text = data.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colCustomer].Text = data.Rows[i]["Customer"].ToString();
                    sheet[ROW, colLotNumber].Text = data.Rows[i]["LotNumber"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = data.Rows[i]["OwnOrderNo"].ToString();
                    sheet[ROW, colSalesOrderIds].Text = data.Rows[i]["SONos"].ToString();
                    sheet[ROW, colProduct].Text = data.Rows[i]["Product"].ToString();
                    sheet[ROW, colArticle].Text = data.Rows[i]["Article"].ToString();
                    sheet[ROW, colPlannedQty].Number = clsStaticInfo.dbl(data.Rows[i]["PlannedQty"].ToString());
                    //sheet[ROW, colProcessWisePlanQty].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessWisePlanQty"].ToString());
                    sheet[ROW, colActualQty].Number = clsStaticInfo.dbl(data.Rows[i]["ActualQty"].ToString());
                    sheet.Range[ROW, colUpToDate].Number = clsStaticInfo.dbl(data.Rows[i]["UpToDateProduction"].ToString());
                    sheet.Range[ROW, colPreProUDProd].Number = clsStaticInfo.dbl(data.Rows[i]["PreProUDProd"].ToString());
                    sheet.Range[ROW, colWIP].Number = clsStaticInfo.dbl(data.Rows[i]["WIP"].ToString());
                    //sheet[ROW, colUptoDateProduction].Number = clsStaticInfo.dbl(data.Rows[i]["UptoDateProPercentage"].ToString());
                    sheet[ROW, colFirstProBookDate].Text = data.Rows[i]["FirstBookDate"].ToString();
                    sheet[ROW, colLastProBookDate].Text = data.Rows[i]["LastBookDate"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "PO Wise Production Status Summary Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


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

                #region Pivot

                string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "PO Wise Production Status Summary Report" + identity.UserId + ".xlsx";

                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }

                workbook.Worksheets[0].Name = "POReport";

                IWorksheet pivotSheet = workbook.Worksheets[0];
                IPivotCache cache = workbook.PivotCaches.Add(workbook.Worksheets[1][startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);

                pivotTable.Fields[colProductionOrderID - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProcessIndex - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colStandardProcessSeq - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProcess - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colPreProUDProd - 1].Axis = PivotAxisTypes.Data;
                pivotTable.Fields[colActualQty - 1].Axis = PivotAxisTypes.Data;
                pivotTable.Fields[colWIP - 1].Axis = PivotAxisTypes.Data;


                IPivotField field = pivotTable.Fields[colPreProUDProd - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "PreProUDProd", PivotSubtotalTypes.None);

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colProcess - 1 || i == colEntity - 1 || i == colProductionOrderID - 1)
                        continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }

                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "PO Wise Production Status Summary Report", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                workbook.Worksheets[0].UsedRange["A7"].FreezePanes();


                #endregion Buyer Summary

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
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

        public void GetSummaryReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            {
                string partyId = "AND 1=1";
                if (!string.IsNullOrEmpty(parameters["CustomerId"].ToString()))
                {
                    partyId = "AND XMO.PartyId in(" + parameters["CustomerId"] + @")";
                }
                string sql = @"Select A.Entity,A.Process,A.POProcessSequence,A.StandardProcessSequence,A.PONo,A.ProcessIndex,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article
,A.PlannedQty,SUM(A.ActualQty) ActualQty,A.UpToDateProduction,A.PreProUDProd,A.WIP,A.FirstBookDate,A.LastBookDate 
from (SELECT DISTINCT PP.Id PSId,trke.UserName AS Entity,PP.ProductionOrderID PONo,PSEQ.ProcessIndex,isnull(p.UserName, '') AS Process,p.Sequence StandardProcessSequence,POPS.[Sequence] POProcessSequence
		,BaseProcess = CASE WHEN P.IsProductionProcess = 1 THEN 'Yes' ELSE 'No' END,pp.Quantity AS ActualQty,ProcessWisePlanQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)*POPS.Qty,PSEQ.Qty UpToDateProduction,ISNULL(PSEQ.PreQty, 0) PreProUDProd
		,WIP = ISNULL(PSEQ.PreQty-PSEQ.Qty, 0),UptoDateProPercentage = (pp.Quantity / (select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)) / 100,ISNULL(pp.StandardName, ord.Article) Article
		,ord.Product,PS.UserName POStatus,FLB.FirstBookDate,FLB.LastBookDate --,ORD.FirstShipmentDate,ORD.LastShipmentDate,
		,PP.LotNumber,POProcessStatus=CASE WHEN POPS.IsCompleted=1 THEN 'Completed' ELSE 'Not Completed' END
--additional info
		,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pp.ProductionOrderId=Xpod.ProductionOrderId " + partyId + @" for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')	
,Buyer=STUFF((select distinct ','+XB.UserName from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
SONos=STUFF((select distinct ','+XSO.Id from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
PlannedQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId),
BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionOrderId,SUM(ps.Quantity) AS Quantity,PS.ResponsiblePersonId,PS.LotNumber

FROM trn.ProductionSummary AS ps 
left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId, ps.ProductionOrderId,PS.ResponsiblePersonId,PS.LotNumber
) AS pp
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS FirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS LastBookDate,ProcessId,ProductionOrderId from TRN.ProductionSummary GROUP BY ProcessId,ProductionOrderId) FLB ON FLB.ProcessId=PP.ProcessId AND FLB.ProductionOrderId=PP.ProductionOrderId
left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
LEFT JOIN trn.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id AND POPS.ProcessId=pp.ProcessId
left outer join (
select POD.ProductionOrderId,MA.StandardName AS Article,PM.UserName AS Product
from trn.ProductionOrderDetail POD 
left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
group by MA.StandardName,PM.UserName,POD.ProductionOrderId
) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN(
SELECT T1.*,T2.Qty PreQty FROM
(Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence) ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T1
LEFT JOIN (Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence)+1 ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T2 ON T1.ProcessIndex=T2.ProcessIndex AND  T1.ProductionOrderId=T2.ProductionOrderId
) PSEQ ON PSEQ.ProductionOrderId=PP.ProductionOrderID AND POPS.[Sequence]=PSEQ.Sequence
Where TRKE.Id in(" + parameters["EntityId"] + @")
AND ISNULL(PP.ResponsiblePersonId,'') in(" + parameters["ResponsiblePersonId"] + @")
AND ps.Id in(" + parameters["ProductionStatusId"] + @"))A 
GROUP BY A.PONo,A.ProcessIndex,A.Process,A.UpToDateProduction,A.PreProUDProd,A.WIP,A.POProcessSequence
,A.Entity,A.Process,A.POProcessSequence,A.StandardProcessSequence,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article,A.PlannedQty,A.FirstBookDate,A.LastBookDate
Order BY A.PONo,A.ProcessIndex";


                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetAllSummaryViewData(Dictionary<string, string> parameters)
        {
            try
            {
                DataTable dtdata;
                GetAllSummaryReportSQL(parameters, out dtdata);

                List<Dictionary<string, object>> data = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtdata);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void GetAllSummaryReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            {
                string partyId = "AND 1=1";
                if (!string.IsNullOrEmpty(parameters["CustomerId"].ToString()))
                {
                    partyId = "AND XMO.PartyId in(" + parameters["CustomerId"] + @")";
                }
                string sql = @"Select A.Entity,A.Process,A.POProcessSequence,A.ProductionProcess,A.StandardProcessSequence,A.PONo,A.ProcessIndex,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article
,A.PlannedQty,SUM(A.ActualQty) ActualQty,A.UpToDateProduction,A.PreProUDProd,A.WIP,A.FirstBookDate,A.LastBookDate 
from (SELECT DISTINCT PP.Id PSId,trke.UserName AS Entity,PP.ProductionOrderID PONo,PSEQ.ProcessIndex,isnull(p.UserName, FSFG.UserName) AS Process,p.Sequence StandardProcessSequence,POPS.[Sequence] POProcessSequence,pps.UserName ProductionProcess
		,BaseProcess = CASE WHEN P.IsProductionProcess = 1 THEN 'Yes' ELSE 'No' END,pp.Quantity AS ActualQty,ProcessWisePlanQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)*POPS.Qty,PSEQ.Qty UpToDateProduction,ISNULL(PSEQ.PreQty, 0) PreProUDProd
		,WIP = ISNULL(PSEQ.PreQty-PSEQ.Qty, 0),UptoDateProPercentage = (pp.Quantity / (select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)) / 100,ISNULL(pp.StandardName, ord.Article) Article
		,ord.Product,PS.UserName POStatus,FLB.FirstBookDate,FLB.LastBookDate --,ORD.FirstShipmentDate,ORD.LastShipmentDate,
		,PP.LotNumber,POProcessStatus=CASE WHEN POPS.IsCompleted=1 THEN 'Completed' ELSE 'Not Completed' END
--additional info
		,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pp.ProductionOrderId=Xpod.ProductionOrderId " + partyId + @" for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')	
,Buyer=STUFF((select distinct ','+XB.UserName from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
SONos=STUFF((select distinct ','+XSO.Id from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
PlannedQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId),
BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId, ps.ProductionOrderId,SUM(ps.Quantity) AS Quantity,PS.ResponsiblePersonId,PS.LotNumber

FROM trn.ProductionSummary AS ps 
left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId, ps.ProductionOrderId,PS.ResponsiblePersonId,PS.LotNumber
) AS pp
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS FirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS LastBookDate,ProcessId,ProductionOrderId from TRN.ProductionSummary GROUP BY ProcessId,ProductionOrderId) FLB ON FLB.ProcessId=PP.ProcessId AND FLB.ProductionOrderId=PP.ProductionOrderId
left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
LEFT JOIN trn.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id AND POPS.ProcessId=pp.ProcessId
left join hkp.Process PPS on pps.Id=POPS.ProcessId
left outer join (
select POD.ProductionOrderId,MA.StandardName AS Article,PM.UserName AS Product
from trn.ProductionOrderDetail POD 
left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
group by MA.StandardName,PM.UserName,POD.ProductionOrderId
) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN(
SELECT T1.*,T2.Qty PreQty FROM
(Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence) ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T1
LEFT JOIN (Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence)+1 ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T2 ON T1.ProcessIndex=T2.ProcessIndex AND  T1.ProductionOrderId=T2.ProductionOrderId
) PSEQ ON PSEQ.ProductionOrderId=PP.ProductionOrderID AND POPS.[Sequence]=PSEQ.Sequence
Where TRKE.Id in(" + parameters["EntityId"] + @")
AND ISNULL(PP.ResponsiblePersonId,'') in(" + parameters["ResponsiblePersonId"] + @")
AND ps.Id in(" + parameters["ProductionStatusId"] + @"))A 
GROUP BY A.PONo,A.ProcessIndex,A.Process,A.UpToDateProduction,A.PreProUDProd,A.WIP,A.POProcessSequence,A.ProductionProcess
,A.Entity,A.Process,A.POProcessSequence,A.StandardProcessSequence,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article,A.PlannedQty,A.FirstBookDate,A.LastBookDate
Order BY A.PONo,A.ProcessIndex";


                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
    }
}