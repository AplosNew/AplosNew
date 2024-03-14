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
            JsonResult json = Json(_productionSummaryData.Productionfilters(productionStatusId, poId), JsonRequestBehavior.AllowGet);
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
                sheet[ROW, COL].ColumnWidth = 41;
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
                sheet[ROW, COL].ColumnWidth = 14;
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

                COL++;
                sheet[ROW, COL].Text = "POFirstBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPOFirstProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "POLastBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPOLastProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "FirstShipmentDate";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colFirstShipmentDate = COL;

                COL++;
                sheet[ROW, COL].Text = "LastShipmentDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLastShipmentDate = COL;


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
                    sheet[ROW, colPOFirstProBookDate].Text = data.Rows[i]["POFirstBookDate"].ToString();
                    sheet[ROW, colPOLastProBookDate].Text = data.Rows[i]["POLastBookDate"].ToString();
                    sheet[ROW, colFirstShipmentDate].Text = data.Rows[i]["FirstShipmentDate"].ToString();
                    sheet[ROW, colLastShipmentDate].Text = data.Rows[i]["LastShipmentDate"].ToString();

                    // sheet[ROW, colOwnStyleNo].Text = data.Rows[i]["OwnStyleNo"].ToString();


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
                    //if (i == colProcess - 1 || i == colEntity - 1 || i == colWorkCenter - 1)
                    //    continue;
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
                _productionSummaryData.ReportSQL(parameters, out dtdata);

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
                _productionSummaryData.GetSummaryReportSQL(parameters, out dtdata);

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
                _productionSummaryData.GetWCReportSQL(parameters, out dtdata);

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
        public ActionResult ProductionDataWCXls(List<Dictionary<string, object>> data, string reportFileName)
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
                fileName = ProductionDataWCReport(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string ProductionDataWCReport(DataTable data, string ReportHeader, string reportFileName)
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
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "POData";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;


                COL++;
                int colstart = COL;
                sheet[ROW, COL].Text = "PONo";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionOrderID = COL;

                COL++;
                sheet[ROW, COL].Text = "ProcessIndex";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProcessIndex = COL;

                COL++;
                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcess = COL;

                COL++;
                sheet[ROW, COL].Text = "POProcessSequence";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPOProcessSeq = COL;

                COL++;
                sheet[ROW, COL].Text = "StandardProcessSequence";
                sheet[ROW, COL].ColumnWidth = 10;
                int colStandardProcessSeq = COL;

                COL++;
                sheet[ROW, COL].Text = "BaseProcessApplicable";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBaseProcessApplicable = COL;

                COL++;
                sheet[ROW, COL].Text = "POProcessStatus";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPOProcessStatus = COL;


                COL++;
                sheet[ROW, COL].Text = "POStatus";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPOStatus = COL;

                COL++;
                sheet[ROW, COL].Text = "WorkCenter";
                sheet[ROW, COL].ColumnWidth = 16;
                int colWorkCenter = COL;

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
                sheet[ROW, COL].Text = "SONos";
                sheet[ROW, COL].ColumnWidth = 41;
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
                sheet[ROW, COL].Text = "NoOfWorkStation";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWorkStation = COL;

                COL++;
                sheet[ROW, COL].Text = "ProductionHours";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActualWorkHours = COL;

                COL++;
                sheet[ROW, COL].Text = "PlannedQty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlannedQty = COL;


                COL++;
                sheet[ROW, COL].Text = "ActualQty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualQty = COL;


                COL++;
                sheet[ROW, COL].Text = "UpToDateProduction";
                sheet[ROW, COL].ColumnWidth = 14;
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
                sheet[ROW, COL].Text = "FirstBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colFirstProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "LastBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLastProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "POFirstBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPOFirstProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "POLastBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPOLastProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "FirstShipmentDate";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colFirstShipmentDate = COL;

                COL++;
                sheet[ROW, COL].Text = "LastShipmentDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLastShipmentDate = COL;

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
                    sheet[ROW, colActualQty].Number = clsStaticInfo.dbl(data.Rows[i]["ActualQty"].ToString());
                    sheet.Range[ROW, colUpToDate].Number = clsStaticInfo.dbl(data.Rows[i]["UpToDateProduction"].ToString());
                    sheet.Range[ROW, colPreProUDProd].Number = clsStaticInfo.dbl(data.Rows[i]["PreProUDProd"].ToString());
                    sheet.Range[ROW, colWIP].Number = clsStaticInfo.dbl(data.Rows[i]["WIP"].ToString());
                    sheet[ROW, colFirstProBookDate].Text = data.Rows[i]["FirstBookDate"].ToString();
                    sheet[ROW, colLastProBookDate].Text = data.Rows[i]["LastBookDate"].ToString();
                    sheet[ROW, colPOFirstProBookDate].Text = data.Rows[i]["POFirstBookDate"].ToString();
                    sheet[ROW, colPOLastProBookDate].Text = data.Rows[i]["POLastBookDate"].ToString();
                    sheet[ROW, colFirstShipmentDate].Text = data.Rows[i]["FirstShipmentDate"].ToString();
                    sheet[ROW, colLastShipmentDate].Text = data.Rows[i]["LastShipmentDate"].ToString();

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
                reportUtility.PlantHeader(ref sheet, endCol, "PO Wise Production Status With WC Report", identity.PlantId);
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
                sheet[ROW, COL].ColumnWidth = 41;
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
                sheet[ROW, COL].ColumnWidth = 14;
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

                COL++;
                sheet[ROW, COL].Text = "POFirstBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPOFirstProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "POLastBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPOLastProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "FirstShipmentDate";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colFirstShipmentDate = COL;

                COL++;
                sheet[ROW, COL].Text = "LastShipmentDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLastShipmentDate = COL;

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
                    sheet[ROW, colPOFirstProBookDate].Text = data.Rows[i]["POFirstBookDate"].ToString();
                    sheet[ROW, colPOLastProBookDate].Text = data.Rows[i]["POLastBookDate"].ToString();
                    sheet[ROW, colFirstShipmentDate].Text = data.Rows[i]["FirstShipmentDate"].ToString();
                    sheet[ROW, colLastShipmentDate].Text = data.Rows[i]["LastShipmentDate"].ToString();

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
                IPivotField fielda = pivotTable.Fields[colActualQty - 1];
                IPivotField fieldw = pivotTable.Fields[colWIP - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                fielda.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                fieldw.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "PreProUDProd", PivotSubtotalTypes.Sum);
                pivotTable.DataFields.Add(fielda, "ActualQty", PivotSubtotalTypes.Sum);
                pivotTable.DataFields.Add(fieldw, "WIP", PivotSubtotalTypes.Sum);

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    //if (i == colProcess - 1 || i == colCustomer - 1 || i == colProductionOrderID - 1)
                    //    continue;
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

        [HttpPost, Authorize]
        public ActionResult GetAllSummaryViewData(Dictionary<string, string> parameters)
        {
            try
            {
                DataTable dtdata;
                _productionSummaryData.GetAllSummaryReportSQL(parameters, out dtdata);

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
        public ActionResult GetProductionAllSummaryDataXls(List<Dictionary<string, object>> data, string reportFileName)
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
                fileName = GetProductionDataAllSummaryReport(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetProductionDataAllSummaryReport(DataTable data, string ReportHeader, string reportFileName)
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
                sheet[ROW, COL].ColumnWidth = 41;
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
                sheet[ROW, COL].ColumnWidth = 14;
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
                COL++;
                sheet[ROW, COL].Text = "POFirstBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPOFirstProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "POLastBookDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPOLastProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "FirstShipmentDate";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colFirstShipmentDate = COL;

                COL++;
                sheet[ROW, COL].Text = "LastShipmentDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLastShipmentDate = COL;

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
                    sheet[ROW, colPOFirstProBookDate].Text = data.Rows[i]["POFirstBookDate"].ToString();
                    sheet[ROW, colPOLastProBookDate].Text = data.Rows[i]["POLastBookDate"].ToString();
                    sheet[ROW, colFirstShipmentDate].Text = data.Rows[i]["FirstShipmentDate"].ToString();
                    sheet[ROW, colLastShipmentDate].Text = data.Rows[i]["LastShipmentDate"].ToString();
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

                string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "PO Wise Production Status All Summary Report" + identity.UserId + ".xlsx";

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
                IPivotField fielda = pivotTable.Fields[colActualQty - 1];
                IPivotField fieldw = pivotTable.Fields[colWIP - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                fielda.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                fieldw.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "PreProUDProd", PivotSubtotalTypes.Sum);
                pivotTable.DataFields.Add(fielda, "ActualQty", PivotSubtotalTypes.Sum);
                pivotTable.DataFields.Add(fieldw, "WIP", PivotSubtotalTypes.Sum);

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    //if (i == colProcess - 1 || i == colCustomer - 1 || i == colProductionOrderID - 1)
                    //    continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }

                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "PO Wise Production Status All Summary Report", identity.CompanyId, identity.CompanyName, "");

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

        //New PO Wise

        [HttpGet, Authorize]
        public ActionResult getPOWiseFilters()
        {
            JsonResult json = Json(_productionSummaryData.POWisefiltersData(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpPost, Authorize]
        public ActionResult POWiseData(Dictionary<string, string> parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(_productionSummaryData.GetPOWiseSql(parameters));
                var jsondata = Json(new { NewData, Message = AplosMessage.Success });
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetPOWiseReportDataXls(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                if (data == null)
                {
                    throw new Exception("No Data found.");
                }
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
                fileName = GetPOWiseReport(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetPOWiseReport(DataTable data, string ReportHeader, string reportFileName)
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
                workbook.Worksheets[1].Name = "PO Wise Data";
                sheet = workbook.Worksheets[1];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Entity"; sheet[ROW, COL].ColumnWidth = 16; int colEntity = COL; COL++;
                sheet[ROW, COL].Text = "PO No"; sheet[ROW, COL].ColumnWidth = 16; int colPONo = COL; COL++;
                sheet[ROW, COL].Text = "PO Status"; sheet[ROW, COL].ColumnWidth = 16; int colPOStatus = COL; COL++;
                sheet[ROW, COL].Text = "Customer"; sheet[ROW, COL].ColumnWidth = 16; int colCustomer = COL; COL++;
                sheet[ROW, COL].Text = "Article"; sheet[ROW, COL].ColumnWidth = 16; int colArticle = COL; COL++;
                sheet[ROW, COL].Text = "MO No"; sheet[ROW, COL].ColumnWidth = 16; int colMONO = COL; COL++;
                sheet[ROW, COL].Text = "SO No"; sheet[ROW, COL].ColumnWidth = 16; int colSONO = COL; COL++;
                sheet[ROW, COL].Text = "Responsible Person"; sheet[ROW, COL].ColumnWidth = 16; int colRP = COL; COL++;
                sheet[ROW, COL].Text = "Added By"; sheet[ROW, COL].ColumnWidth = 16; int colAddedBy = COL; COL++;
                sheet[ROW, COL].Text = "Added Date"; sheet[ROW, COL].ColumnWidth = 16; int colAddedDate = COL; COL++;
                sheet[ROW, COL].Text = "Updated By"; sheet[ROW, COL].ColumnWidth = 16; int colUpdatedBy = COL; COL++;
                sheet[ROW, COL].Text = "Updated Date"; sheet[ROW, COL].ColumnWidth = 16; int colUpdatedDate = COL; COL++;
                sheet[ROW, COL].Text = "SO Qty"; sheet[ROW, COL].ColumnWidth = 16; int colSOQty = COL; COL++;
                sheet[ROW, COL].Text = "Base Proc Plan Percentage"; sheet[ROW, COL].ColumnWidth = 16; int colBaseProcPlanPercentage = COL; COL++;
                sheet[ROW, COL].Text = "Actual Plan Schedule Qty"; sheet[ROW, COL].ColumnWidth = 16; int colActualPlanScheduleQty = COL; COL++;
                sheet[ROW, COL].Text = "Requested Qty"; sheet[ROW, COL].ColumnWidth = 16; int colRequestedQty = COL; COL++;
                sheet[ROW, COL].Text = "Issue Qty"; sheet[ROW, COL].ColumnWidth = 16; int colIssueQty = COL; COL++;
                sheet[ROW, COL].Text = "Total Qty"; sheet[ROW, COL].ColumnWidth = 16; int colTotalQty = COL; COL++;
                sheet[ROW, COL].Text = "First Process Book Qty"; sheet[ROW, COL].ColumnWidth = 16; int colFirstProcessProQty = COL; COL++;
                sheet[ROW, COL].Text = "Should Be Base Process Planned Qty"; sheet[ROW, COL].ColumnWidth = 16; int colShouldBeBaseProcessPlannedQty = COL; COL++;
                sheet[ROW, COL].Text = "Base Process Produce Qty"; sheet[ROW, COL].ColumnWidth = 16; int colBaseProcessProduceQty = COL; COL++;
                sheet[ROW, COL].Text = "Base Process Remaining Qty"; sheet[ROW, COL].ColumnWidth = 16; int colBaseProcessRemainingQty = COL; COL++;
                sheet[ROW, COL].Text = "Sequence"; sheet[ROW, COL].ColumnWidth = 41; int colSequence = COL; COL++;
                sheet[ROW, COL].Text = "Process"; sheet[ROW, COL].ColumnWidth = 16; int colProcess = COL; COL++;
                sheet[ROW, COL].Text = "Percent Qty"; sheet[ROW, COL].ColumnWidth = 28; int colPercentQty = COL; COL++;
                sheet[ROW, COL].Text = "Process Planned Qty"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcessPlannedQty = COL; COL++;
                sheet[ROW, COL].Text = "Proc Prod Qty"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcProdQty = COL; COL++;
                sheet[ROW, COL].Text = "Pre Proc Prod Qty"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colPreProcProdQty = COL; COL++;
                sheet[ROW, COL].Text = "WIP"; sheet[ROW, COL].ColumnWidth = 14; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colWIP = COL; COL++;
                sheet[ROW, COL].Text = "Proc Balance To Produce"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcBalanceToProduce = COL; COL++;
                sheet[ROW, COL].Text = "Relay Process"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colRelayProcess = COL; COL++;
                sheet[ROW, COL].Text = "Base Process"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colIsBaseProcess = COL; COL++;
                sheet[ROW, COL].Text = "Process Leg Days"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcessLegDays = COL; COL++;
                sheet[ROW, COL].Text = "PO First Delivery"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colPOFirstDelivery = COL; COL++;
                sheet[ROW, COL].Text = "PO Last Delivery"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colPOLastDelivery = COL; COL++;
                sheet[ROW, COL].Text = "Base Proc Prod Start Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colBaseProcProdStartDate = COL; COL++;
                sheet[ROW, COL].Text = "Base Proc Latest Prod Date"; sheet[ROW, COL].ColumnWidth = 13; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colBaseProcLatestProdDate = COL; COL++;
                sheet[ROW, COL].Text = "Base Proc Plan Start Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colBaseProcPlanStartDate = COL; COL++;
                sheet[ROW, COL].Text = "Base Proc Plan Completion Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colBaseProcPlanCompletionDate = COL; COL++;
                sheet[ROW, COL].Text = "PO Start Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colPOStartDate = COL; COL++;
                sheet[ROW, COL].Text = "PO Completion Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colPOCompletionDate = COL; COL++;
                sheet[ROW, COL].Text = "First Process Actual Book Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colFirstProcessActualBookDate = COL; COL++;
                sheet[ROW, COL].Text = "PO First Prod Book Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colPOFirstProdBookDate = COL; COL++;
                sheet[ROW, COL].Text = "PO Latest Prod Book Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colPOLatestProdBookDate = COL; COL++;
                sheet[ROW, COL].Text = "Should Be Process Start Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colShouldBeProcessStartDate = COL; COL++;
                sheet[ROW, COL].Text = "Should Be Process End Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colShouldBeProcessEndDate = COL; COL++;
                sheet[ROW, COL].Text = "Process First Book Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcessFirstBookDate = COL; COL++;
                sheet[ROW, COL].Text = "Process Latest Book Date"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcessLatestBookDate = COL; COL++;
                sheet[ROW, COL].Text = "Process Start Days"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcessStartDays = COL; COL++;
                sheet[ROW, COL].Text = "Process End Days"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcessEndDays = COL; COL++;
                sheet[ROW, COL].Text = "Process Plan Percent"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcessPlanPercent = COL; COL++;
                sheet[ROW, COL].Text = "Process Status"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcessStatus = COL; COL++;
                sheet[ROW, COL].Text = "First Process WC"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colFirstProcessWC = COL; COL++;
                sheet[ROW, COL].Text = "Proc Loss Percent"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcLossPercent = COL; COL++;
                sheet[ROW, COL].Text = "Base Proc Prod Perenct"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colBaseProcProdPerenct = COL; COL++;
                sheet[ROW, COL].Text = "Proc Prod Percent"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcProdPercent = COL; COL++;
                sheet[ROW, COL].Text = "Entry Check"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colEntryCheck = COL; COL++;
                sheet[ROW, COL].Text = "Proceess Prod Qty Vs SOQty"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProceessProdQtyVsSOQty = COL;COL++;
                sheet[ROW, COL].Text = "Process Balance Production"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colProcessBalanceProd = COL;COL++;
                sheet[ROW, COL].Text = "Process Status Remark"; sheet[ROW, COL].ColumnWidth = 16; int colProcessStatusRemark = COL; COL++;
                sheet[ROW, COL].Text = "PO Review Status"; sheet[ROW, COL].ColumnWidth = 16; int colPOReviewStatus = COL; COL++;
                sheet[ROW, COL].Text = "Input Recovery Percentage"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colInputRecoveryPercentage = COL; COL++;
                sheet[ROW, COL].Text = "Actual Input Plan Percentage"; sheet[ROW, COL].ColumnWidth = 12; sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight; int colActualInputPlanPercentage = COL; COL++;
                sheet[ROW, COL].Text = "Latest Process Prod Book Days"; sheet[ROW, COL].ColumnWidth = 20; int colLatestProcessProdBookDays = COL; COL++;
                sheet[ROW, COL].Text = "Process Review Status"; sheet[ROW, COL].ColumnWidth = 20; int colProcessReviewStatus = COL; COL++;
                sheet[ROW, COL].Text = "LotNo-Qty"; sheet[ROW, COL].ColumnWidth = 20; int colLotNoQty = COL; 

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
                    sheet[ROW, colPONo].Text = data.Rows[i]["PONo"].ToString();
                    sheet[ROW, colCustomer].Text = data.Rows[i]["Customer"].ToString();
                    sheet[ROW, colArticle].Text = data.Rows[i]["Article"].ToString();
                    sheet[ROW, colSONO].Text = data.Rows[i]["SONo"].ToString();
                    sheet[ROW, colMONO].Text = data.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colRP].Text = data.Rows[i]["ResponsiblePerson"].ToString();
                    sheet[ROW, colPOStatus].Text = data.Rows[i]["POStatus"].ToString();
                    sheet[ROW, colAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
                    sheet[ROW, colAddedDate].Text = data.Rows[i]["AddedDate"].ToString();
                    sheet[ROW, colUpdatedBy].Text = data.Rows[i]["UpdatedBy"].ToString();
                    sheet[ROW, colUpdatedDate].Text = data.Rows[i]["UpdatedDate"].ToString();
                    sheet[ROW, colSOQty].Number = clsStaticInfo.dbl(data.Rows[i]["SOQty"].ToString());
                    sheet[ROW, colBaseProcPlanPercentage].Number = clsStaticInfo.dbl(data.Rows[i]["BaseProcPlanPercentage"].ToString());
                    sheet[ROW, colActualPlanScheduleQty].Number = clsStaticInfo.dbl(data.Rows[i]["ActualPlanScheduleQty"].ToString());
                    sheet[ROW, colRequestedQty].Number = clsStaticInfo.dbl(data.Rows[i]["RequestedQty"].ToString());
                    sheet[ROW, colIssueQty].Number = clsStaticInfo.dbl(data.Rows[i]["IssueQty"].ToString());
                    sheet[ROW, colTotalQty].Number = clsStaticInfo.dbl(data.Rows[i]["TotalQty"].ToString());
                    sheet[ROW, colFirstProcessProQty].Number = clsStaticInfo.dbl(data.Rows[i]["FirstProcessProQty"].ToString());
                    sheet[ROW, colShouldBeBaseProcessPlannedQty].Number = clsStaticInfo.dbl(data.Rows[i]["ShouldBeBaseProcessPlannedQty"].ToString());
                    sheet[ROW, colBaseProcessProduceQty].Number = clsStaticInfo.dbl(data.Rows[i]["BaseProcessProduceQty"].ToString());
                    sheet[ROW, colBaseProcessRemainingQty].Number = clsStaticInfo.dbl(data.Rows[i]["BaseProcessRemainingQty"].ToString());
                    sheet[ROW, colSequence].Text = data.Rows[i]["Sequence"].ToString();
                    sheet[ROW, colProcess].Text = data.Rows[i]["Process"].ToString();
                    sheet[ROW, colPercentQty].Number = clsStaticInfo.dbl(data.Rows[i]["PercentQty"].ToString());
                    sheet[ROW, colProcessPlannedQty].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessPlannedQty"].ToString());
                    sheet[ROW, colProcProdQty].Number = clsStaticInfo.dbl(data.Rows[i]["ProcProdQty"].ToString());
                    sheet[ROW, colPreProcProdQty].Number = clsStaticInfo.dbl(data.Rows[i]["PreProcProdQty"].ToString());
                    sheet[ROW, colWIP].Number = clsStaticInfo.dbl(data.Rows[i]["WIP"].ToString());
                    sheet[ROW, colProcBalanceToProduce].Number = clsStaticInfo.dbl(data.Rows[i]["ProcBalanceToProduce"].ToString());
                    sheet[ROW, colProcessPlannedQty].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessPlannedQty"].ToString());
                    sheet[ROW, colRelayProcess].Text = data.Rows[i]["RelayProcess"].ToString();
                    sheet[ROW, colIsBaseProcess].Text = data.Rows[i]["IsBaseProcess"].ToString();
                    sheet[ROW, colProcessLegDays].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessLegDays"].ToString());
                    sheet[ROW, colPOFirstDelivery].Text = data.Rows[i]["POFirstDelivery"].ToString();
                    sheet[ROW, colPOLastDelivery].Text = data.Rows[i]["POLastDelivery"].ToString();
                    sheet[ROW, colBaseProcProdStartDate].Text = data.Rows[i]["BaseProcProdStartDate"].ToString();
                    sheet[ROW, colBaseProcLatestProdDate].Text = data.Rows[i]["BaseProcLatestProdDate"].ToString();
                    sheet[ROW, colBaseProcPlanStartDate].Text = data.Rows[i]["BaseProcPlanStartDate"].ToString();
                    sheet[ROW, colBaseProcPlanCompletionDate].Text = data.Rows[i]["BaseProcPlanCompletionDate"].ToString();
                    sheet[ROW, colPOStartDate].Text = data.Rows[i]["POStartDate"].ToString();
                    sheet[ROW, colPOCompletionDate].Text = data.Rows[i]["POCompletionDate"].ToString();
                    sheet[ROW, colPOFirstProdBookDate].Text = data.Rows[i]["POFirstProdBookDate"].ToString();
                    sheet[ROW, colPOLatestProdBookDate].Text = data.Rows[i]["POLatestProdBookDate"].ToString();
                    sheet[ROW, colShouldBeProcessStartDate].Text = data.Rows[i]["ShouldBeProcessStartDate"].ToString();
                    sheet[ROW, colShouldBeProcessEndDate].Text = data.Rows[i]["ShouldBeProcessEndDate"].ToString();
                    sheet[ROW, colProcessFirstBookDate].Text = data.Rows[i]["ProcessFirstBookDate"].ToString();
                    sheet[ROW, colProcessLatestBookDate].Text = data.Rows[i]["ProcessLatestBookDate"].ToString();
                    sheet[ROW, colProcessStartDays].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessStartDays"].ToString());
                    sheet[ROW, colProcessEndDays].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessEndDays"].ToString());
                    sheet[ROW, colProcessPlanPercent].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessPlanPercent"].ToString());
                    sheet[ROW, colProcessStatus].Text = data.Rows[i]["ProcessStatus"].ToString();
                    sheet[ROW, colFirstProcessWC].Text = data.Rows[i]["FirstProcessWC"].ToString();
                    sheet[ROW, colProcessPlanPercent].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessPlanPercent"].ToString());
                    sheet[ROW, colBaseProcProdPerenct].Number = clsStaticInfo.dbl(data.Rows[i]["BaseProcProdPerenct"].ToString());
                    sheet[ROW, colProcProdPercent].Number = clsStaticInfo.dbl(data.Rows[i]["ProcProdPercent"].ToString());
                    sheet[ROW, colEntryCheck].Text = data.Rows[i]["EntryCheck"].ToString();
                    sheet[ROW, colProceessProdQtyVsSOQty].Number = clsStaticInfo.dbl(data.Rows[i]["ProceessProdQtyVsSOQty"].ToString());
                    sheet[ROW, colProcessBalanceProd].Number = clsStaticInfo.dbl(data.Rows[i]["ProcessBalanceProd"].ToString());
                    sheet[ROW, colProcessStatusRemark].Text = data.Rows[i]["ProcessStatusRemark"].ToString();
                    sheet[ROW, colPOReviewStatus].Text = data.Rows[i]["POReviewStatus"].ToString();
                    sheet[ROW, colLotNoQty].Text = data.Rows[i]["LotNoQty"].ToString();
                    sheet[ROW, colInputRecoveryPercentage].Number = clsStaticInfo.dbl(data.Rows[i]["InputRecoveryPercentage"].ToString());
                    sheet[ROW, colActualInputPlanPercentage].Number = clsStaticInfo.dbl(data.Rows[i]["ActualInputPlanPercentage"].ToString());
                    sheet[ROW, colLatestProcessProdBookDays].Text = data.Rows[i]["LatestProcessProdBookDays"].ToString();
                    sheet[ROW, colProcessReviewStatus].Text = data.Rows[i]["ProcessReviewStatus"].ToString();


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "PO Wise Report", identity.PlantId);
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

                string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "POWiseReport" + identity.UserId + ".xlsx";

                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }

                workbook.Worksheets[0].Name = "PO Wise Report";

                IWorksheet pivotSheet = workbook.Worksheets[0];
                IPivotCache cache = workbook.PivotCaches.Add(workbook.Worksheets[1][startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);
                
                pivotTable.Fields[colPOStatus - 1].Axis = PivotAxisTypes.Row;//1
                
                //
                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;//2
                pivotTable.Fields[colArticle - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colPONo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPOStartDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPOCompletionDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPOLatestProdBookDate - 1].Axis = PivotAxisTypes.Row;
               
                pivotTable.Fields[colSequence - 1].Axis = PivotAxisTypes.Row;//7
                pivotTable.Fields[colProcess - 1].Axis = PivotAxisTypes.Row;//8
                pivotTable.Fields[colMONO - 1].Axis = PivotAxisTypes.Row;//8
                pivotTable.Fields[colRP - 1].Axis = PivotAxisTypes.Row;//8
                pivotTable.Fields[colSONO - 1].Axis = PivotAxisTypes.Row;//9
                pivotTable.Fields[colSOQty - 1].Axis = PivotAxisTypes.Row;//9
                pivotTable.Fields[colProcessPlanPercent - 1].Axis = PivotAxisTypes.Row;//10
                pivotTable.Fields[colProcessPlannedQty - 1].Axis = PivotAxisTypes.Row;//11
                pivotTable.Fields[colProcProdQty - 1].Axis = PivotAxisTypes.Row;//12
                pivotTable.Fields[colPreProcProdQty - 1].Axis = PivotAxisTypes.Row;//13
                pivotTable.Fields[colWIP - 1].Axis = PivotAxisTypes.Row;//13

                pivotTable.Fields[colProcProdPercent - 1].Axis = PivotAxisTypes.Row;//14
                pivotTable.Fields[colProceessProdQtyVsSOQty - 1].Axis = PivotAxisTypes.Row;//15
                pivotTable.Fields[colProcessBalanceProd - 1].Axis = PivotAxisTypes.Row;//15

                pivotTable.Fields[colIsBaseProcess - 1].Axis = PivotAxisTypes.Row;//16
                pivotTable.Fields[colProcessLegDays - 1].Axis = PivotAxisTypes.Row;//17
                pivotTable.Fields[colRelayProcess - 1].Axis = PivotAxisTypes.Row;//19
                pivotTable.Fields[colProcessStatusRemark - 1].Axis = PivotAxisTypes.Row;//20
                pivotTable.Fields[colProcessStatus - 1].Axis = PivotAxisTypes.Row;//21
                pivotTable.Fields[colProcessLatestBookDate - 1].Axis = PivotAxisTypes.Row;//22
                pivotTable.Fields[colPOReviewStatus - 1].Axis = PivotAxisTypes.Row;//23
                pivotTable.Fields[colInputRecoveryPercentage - 1].Axis = PivotAxisTypes.Row;//25
                pivotTable.Fields[colActualInputPlanPercentage - 1].Axis = PivotAxisTypes.Row;//26
                pivotTable.Fields[colLatestProcessProdBookDays - 1].Axis = PivotAxisTypes.Row;//26
                pivotTable.Fields[colProcessReviewStatus - 1].Axis = PivotAxisTypes.Row;//26
                pivotTable.Fields[colLotNoQty - 1].Axis = PivotAxisTypes.Row;//24

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colPOStatus || i == colPOLatestProdBookDate || i == colPONo || i == colPOStartDate || i == colPOCompletionDate || i == colSequence || i == colProcess || i == colIsBaseProcess || i == colProcessLegDays || i == colRelayProcess || i == colProcessStatus || i == colProcessPlanPercent || i == colSOQty || i == colProcessPlannedQty || i == colProcProdQty || i == colPreProcProdQty || i == colProcProdPercent || i == colProceessProdQtyVsSOQty)
                    {
                        pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                    }
                    else
                    {
                        pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                    }
                }
                
                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "PO Wise Report", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                


                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
           
                pivotSheet.Range["A6"].RowHeight = 50;
                pivotSheet.Range["A6"].WrapText = true;
                pivotSheet.Range["A6"].VerticalAlignment = ExcelVAlign.VAlignTop;
                #endregion PO Wise

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
        //End PO Wise
    }
}