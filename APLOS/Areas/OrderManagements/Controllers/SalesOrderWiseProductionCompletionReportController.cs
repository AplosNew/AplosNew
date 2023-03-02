using Aplos.Controllers;
using Library.Service.OrderManagements;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using OTSBD;
using System.Linq;
using Library.Service.Enums;
using Library.Service.Helpers;
using bplib;



using System.Web.Hosting;
using Library.Service.Productions.ProductionBooking;
using System.Text.RegularExpressions;
using Library.OrderManagement.Production;
using System.IO;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class SalesOrderWiseProductionCompletionReportController : BaseController
    {
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        public enum PlanningStatus { TOSTART, FREEZE, RUNNING };
        private EnumPlanningTypes ScreenPlanningType = EnumPlanningTypes.PlanningType1;

        #region Constructor

        private readonly IProductionOrderService _productionOrderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ProductionOrderReports ProductionOrderReports = null;

        public SalesOrderWiseProductionCompletionReportController(IProductionOrderService productionOrderService, IUnitOfWork U, ISqlRepository R)
        {

            _unitOfWork = U;
            _sqlRepository = R;
            _productionOrderService = productionOrderService;
            ProductionOrderReports = new ProductionOrderReports(_sqlRepository);
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
        public ActionResult getFilters()
        {
            JsonResult json = Json(_productionSummaryData.GetSOCompletionReportFilter(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
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
        private DataRow GetExpectedCompletionDate(double RequiredQty, List<DataRow> Data)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if (clsStaticInfo.dbl(Data[i]["CummTotalQty"].ToString()) >= RequiredQty)
                {
                    return Data[i];
                }
            }


            return null;
        }
        private string CellAddr(int Col, int Row)
        {
            return clsStaticInfo.GetxlsCol(Col) + Row.ToString();
        }

        [HttpPost, Authorize]
        public ActionResult GetOS3xlsReport(Dictionary<string, string> parameters)
        {
            try
            {
                var workbook = GetOS3xls(parameters);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "OS3Report.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

      
        public IWorkbook GetOS3xls(Dictionary<string, string> parameters)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
              
                Dictionary<string, List<DataRow>> dicProductionQtyDistribution;
                DataTable dt, dtOrderMaster;
                _productionSummaryData.getSalesOrderDistribution(System.DateTime.Now.ToString("dd-MMM-yyyy"), parameters, out dicProductionQtyDistribution, out dt);

                _productionSummaryData.getOrderMaster(parameters, out dtOrderMaster);


                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(3);
                workbook.Worksheets[2].Name = "OS3 Data";
                sheet = workbook.Worksheets[2];


                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                int colResponsiblePerson = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Ref No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colBuyerOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colOwnOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Material Row Id";
                sheet[ROW, COL].ColumnWidth = 22;
                int colMaterialRowId = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 22;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 22;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 22;
                int colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 22;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Code";
                sheet[ROW, COL].ColumnWidth = 22;
                int colProductCode = COL;

                COL++;
                sheet[ROW, COL].Text = "Product Attribute";
                sheet[ROW, COL].ColumnWidth = 22;
                int colProductAttribute = COL;

                COL++;
                sheet[ROW, COL].Text = "Buyer Item#";
                sheet[ROW, COL].ColumnWidth = 22;
                int colBuyerItem = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Item#";
                sheet[ROW, COL].ColumnWidth = 22;
                int colOwnItem = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Id";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "PO ProduceQty ";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPOProduceQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PO Remaining Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPORemainingQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer PO No";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPONo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer PO Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPODate = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Order Category";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderCategory = COL;    //                        

                COL++;
                sheet[ROW, COL].Text = "SO Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Production Order Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colproductionStatus = COL;
                COL++;


                sheet[ROW, COL].Text = "Sales Order Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Desc";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderDesc = COL;
                COL++;
                sheet[ROW, COL].Text = "Delivery Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Commitment Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colCommitmentDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Ex-FactoryDate";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanExFactoryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "PO StartDate";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPOStartDate = COL;
                COL++;
                sheet[ROW, COL].Text = "PO Completion Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPOCompletionDate = COL;
                COL++;
                sheet[ROW, COL].Text = "PR Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPRQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PR Plan Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPRPlannedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PR Actual Plan Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPRActualPlannedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSOQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlannedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PR/SO Cumulative Plan Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colCummPlannedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Expected Start Date";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colExpectedStartDate = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Expected Completion Date";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colExpectedCompletionDate = COL;
                //COL++;
                //sheet[ROW, COL].Text = "Expected Ex-Factory Date";
                //sheet[ROW, COL].ColumnWidth = 12;
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int colExpectedExFactoryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Available Produced Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAvailableProducedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAvailablePlanQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Total Available Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalAvailableQty = COL;

                COL++;
                sheet[ROW, COL].Text = "Early By";
                sheet[ROW, COL].ColumnWidth = 8;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colEarlyBy = COL;
                COL++;
                sheet[ROW, COL].Text = "Late By";
                sheet[ROW, COL].ColumnWidth = 8;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLateBy = COL;
                COL++;
                sheet[ROW, COL].Text = "Del. Month";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colDeliveryMonth = COL;
                COL++;
                sheet[ROW, COL].Text = "Prod. Compl. Month";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionCompletionMonth = COL;
                #endregion columns

                int endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                ROW++;

                int startRow = ROW;

                string ExpectedProductionStartDate = "";
                double PRCumulativePlanQty = 0;
                string PRId = "";
                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                {
                    
                    if (PRId != dtOrderMaster.Rows[i]["ProductionOrderId"].ToString())
                    {
                        PRCumulativePlanQty = 0;
                        ExpectedProductionStartDate = "";
                    }
                    PRId = dtOrderMaster.Rows[i]["ProductionOrderId"].ToString();

                    PRCumulativePlanQty += clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PlannedQty"].ToString());
                    dtOrderMaster.Rows[i]["CummPlannedQty"] = PRCumulativePlanQty;

                    sheet[ROW, colPlant].Text = dtOrderMaster.Rows[i]["Plant"].ToString();
                    sheet[ROW, colEntity].Text = dtOrderMaster.Rows[i]["Entity"].ToString();

                    sheet[ROW, colCummPlannedQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["CummPlannedQty"].ToString());

                    sheet[ROW, colArticle].Text = dtOrderMaster.Rows[i]["Article"].ToString();
                    sheet[ROW, colProductCategory].Text = dtOrderMaster.Rows[i]["ProductCategory"].ToString();
                    sheet[ROW, colProduct].Text = dtOrderMaster.Rows[i]["Product"].ToString();
                    sheet[ROW, colProductCode].Text = dtOrderMaster.Rows[i]["ProductCode"].ToString();
                    sheet[ROW, colProductAttribute].Text = dtOrderMaster.Rows[i]["ProductAttribute"].ToString();

                    sheet[ROW, colOwnItem].Text = dtOrderMaster.Rows[i]["OwnReferenceNo"].ToString();
                    sheet[ROW, colBuyerItem].Text = dtOrderMaster.Rows[i]["BuyerReferenceNo"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = dtOrderMaster.Rows[i]["OwnOrderNo"].ToString();
                    sheet[ROW, colBuyerOrderNo].Text = dtOrderMaster.Rows[i]["BuyerOrderNo"].ToString();
                    sheet[ROW, colSalesOrderDesc].Text = dtOrderMaster.Rows[i]["SODesc"].ToString();

                    sheet[ROW, colMaterialRowId].Text = dtOrderMaster.Rows[i]["MaterialRowId"].ToString();

                    sheet[ROW, colCustomer].Text = dtOrderMaster.Rows[i]["Customer"].ToString();
                    sheet[ROW, colBuyer].Text = dtOrderMaster.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colCommitmentDate].Text = GetDate(dtOrderMaster.Rows[i]["CommitmentDate"].ToString());
                    sheet[ROW, colPlanExFactoryDate].Text = GetDate(dtOrderMaster.Rows[i]["PlanExFactoryDate"].ToString());
                    sheet[ROW, colPOStartDate].Text = GetDate(dtOrderMaster.Rows[i]["POStartDate"].ToString());
                    sheet[ROW, colPOCompletionDate].Text = GetDate(dtOrderMaster.Rows[i]["POCompletionDate"].ToString());
                    sheet[ROW, colDeliveryDate].Text = GetDate(dtOrderMaster.Rows[i]["DeliveryDate"].ToString());
                    sheet[ROW, colMasterOrderNo].Text = dtOrderMaster.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colMaterial].Text = dtOrderMaster.Rows[i]["Material"].ToString();
                    sheet[ROW, colOrderCategory].Text = dtOrderMaster.Rows[i]["OrderCategory"].ToString();
                    sheet[ROW, colOrderStatus].Text = dtOrderMaster.Rows[i]["OrderStatus"].ToString();
                    sheet[ROW, colProductionOrderId].Text = dtOrderMaster.Rows[i]["ProductionOrderId"].ToString();
                    sheet[ROW, colPOProduceQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["POProduceQty"].ToString());
                    sheet[ROW, colPORemainingQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["RemainingQty"].ToString());
                    sheet[ROW, colproductionStatus].Text = dtOrderMaster.Rows[i]["productionStatus"].ToString();
                    sheet[ROW, colResponsiblePerson].Text = dtOrderMaster.Rows[i]["ResponsiblePerson"].ToString();
                    sheet[ROW, colSOQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["SOQty"].ToString());
                    sheet[ROW, colPlannedQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PlannedQty"].ToString());

                    sheet[ROW, colPRQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PRQty"].ToString());
                    sheet[ROW, colPRPlannedQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PRPlannedQty"].ToString());
                    sheet[ROW, colPRActualPlannedQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PRActualPlannedQty"].ToString());

                    if (clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PRPlannedQty"].ToString()) != clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PRActualPlannedQty"].ToString()))
                        sheet[ROW, colPRActualPlannedQty].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet[ROW, colSalesOrderId].Text = dtOrderMaster.Rows[i]["SalesOrderId"].ToString();
                    sheet[ROW, colPONo].Text = dtOrderMaster.Rows[i]["PONumber"].ToString();
                    sheet[ROW, colPODate].Text = dtOrderMaster.Rows[i]["PODate"].ToString();
                    //sheet[ROW, colExpectedExFactoryDate].Text = dtOrderMaster.Rows[i]["ExpectedExFactoryDate"].ToString();

                    //if (dtOrderMaster.Rows[i]["ProductionOrderId"].ToString() == "20104")
                    //{

                    //ProductionStartDate
                    //}

                    if (dicProductionQtyDistribution.ContainsKey(dtOrderMaster.Rows[i]["ProductionOrderId"].ToString()))
                    {
                        DataRow dr = GetExpectedCompletionDate(PRCumulativePlanQty, dicProductionQtyDistribution[dtOrderMaster.Rows[i]["ProductionOrderId"].ToString()]);
                        if (dr != null)
                        {
                            if (ExpectedProductionStartDate == "")
                                ExpectedProductionStartDate = GetDate(dr["ProductionStartDate"].ToString());

                            sheet[ROW, colExpectedStartDate].Text = ExpectedProductionStartDate;
                            sheet[ROW, colExpectedStartDate].NumberFormat = "dd-MMM-yyyy";

                            sheet[ROW, colExpectedCompletionDate].Text = GetDate(dr["ProductionDate"].ToString());
                            sheet[ROW, colExpectedCompletionDate].NumberFormat = "dd-MMM-yyyy";
                            sheet[ROW, colAvailableProducedQty].Number = clsStaticInfo.dbl(dr["CummProductionQty"].ToString());
                            sheet[ROW, colAvailablePlanQty].Number = clsStaticInfo.dbl(dr["CummPlanQty"].ToString());
                            sheet[ROW, colTotalAvailableQty].Formula = CellAddr(colAvailableProducedQty, ROW) + "+" + CellAddr(colAvailablePlanQty, ROW);

                            sheet[ROW, colLateBy].Formula = "IF(AND(" + CellAddr(colExpectedCompletionDate, ROW) + "<>\"\",datevalue(" + CellAddr(colExpectedCompletionDate, ROW) + ")>datevalue(" + CellAddr(colDeliveryDate, ROW) + "))," + CellAddr(colExpectedCompletionDate, ROW) + "-" + CellAddr(colDeliveryDate, ROW) + ",0)";
                            sheet[ROW, colEarlyBy].Formula = "IF(AND(" + CellAddr(colExpectedCompletionDate, ROW) + "<>\"\",datevalue(" + CellAddr(colExpectedCompletionDate, ROW) + ")<=datevalue(" + CellAddr(colDeliveryDate, ROW) + "))," + CellAddr(colDeliveryDate, ROW) + "-" + CellAddr(colExpectedCompletionDate, ROW) + ",0)";


                            ExpectedProductionStartDate = GetDate(dr["ProductionDate"].ToString());
                        }

                    }

                    sheet[ROW, colDeliveryMonth].Formula = "IF(" + CellAddr(colDeliveryDate, ROW) + "<>\"\",CONCATENATE(Month(" + CellAddr(colDeliveryDate, ROW) + "),\"/\",Year(" + CellAddr(colDeliveryDate, ROW) + ")),0)";// + CellAddr(colDeliveryDate, ROW) + "," + CellAddr(colExpectedCompletionDate, ROW) + " - " + CellAddr(colDeliveryDate, ROW) + ",0)";
                    sheet[ROW, colProductionCompletionMonth].Formula = "IF(" + CellAddr(colExpectedCompletionDate, ROW) + "<>\"\",CONCATENATE(Month(" + CellAddr(colExpectedCompletionDate, ROW) + "),\"/\",Year(" + CellAddr(colExpectedCompletionDate, ROW) + ")),0)";//"IF(" + CellAddr(colExpectedCompletionDate, ROW) + "<=" + CellAddr(colDeliveryDate, ROW) + "," + CellAddr(colDeliveryDate, ROW) + " - " + CellAddr(colExpectedCompletionDate, ROW) + ",0)";


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet[startRow, colSOQty, ROW, colSOQty].NumberFormat = clsStaticInfo.NumberFormat();
                sheet[startRow, colPlannedQty, ROW, colPlannedQty].NumberFormat = clsStaticInfo.NumberFormat();
                sheet[startRow, colPRQty, ROW, colPRQty].NumberFormat = clsStaticInfo.NumberFormat();
                sheet[startRow, colPRPlannedQty, ROW, colPRPlannedQty].NumberFormat = clsStaticInfo.NumberFormat();
                sheet[startRow, colAvailableProducedQty, ROW, colAvailableProducedQty].NumberFormat = clsStaticInfo.NumberFormat();
                sheet[startRow, colAvailablePlanQty, ROW, colAvailablePlanQty].NumberFormat = clsStaticInfo.NumberFormat();
                sheet[startRow, colAvailablePlanQty, ROW, colAvailablePlanQty].NumberFormat = clsStaticInfo.NumberFormat();
                sheet[startRow, colTotalAvailableQty, ROW, colTotalAvailableQty].NumberFormat = clsStaticInfo.NumberFormat();
                sheet[startRow, colCummPlannedQty, ROW, colCummPlannedQty].NumberFormat = clsStaticInfo.NumberFormat();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IListObject table = sheet.ListObjects.Create("Table1", sheet[clsStaticInfo.GetxlsCol(1) + (6).ToString() + ":" + clsStaticInfo.GetxlsCol(endCol) + (ROW).ToString()]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;

                sheet.IsDisplayZeros = false;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange["A7"].FreezePanes();


                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "OS3", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;



                //#endregion ******************Report Header******************

                IWorksheet sheet2 = workbook.Worksheets[1];
                sheet2.Name = "OS-W";
                //sheet2.ImportDataTable(dt, true, 1, 1);
                //int lc = sheet.UsedRange.LastColumn;
                //sheet2.Range[1, 1, 1, lc].ColumnWidth = 14;
                #region columns
                int ROW2 = 1, COL2 = 1;
                int startRow2 = ROW2;
                sheet2[ROW2, COL2].Text = "ProductionOrderID"; sheet2[ROW2, COL2].ColumnWidth = 14; int colProductionOrderID = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Production Date"; sheet2[ROW2, COL2].ColumnWidth = 14; int colProductionDate = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Production Qty"; sheet2[ROW2, COL2].ColumnWidth = 14; int colProductionQty = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Plan Qty"; sheet2[ROW2, COL2].ColumnWidth = 14; int colPlanQty = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Production StartDate"; sheet2[ROW2, COL2].ColumnWidth = 14; int colProductionStartDate = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "CummProduction Qty"; sheet2[ROW2, COL2].ColumnWidth = 14; int colCummProductionQty = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "CummPlanQty"; sheet2[ROW2, COL2].ColumnWidth = 14; int colCummPlanQty = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Total Qty"; sheet2[ROW2, COL2].ColumnWidth = 14; int colTotalQty = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Cumm TotalQty"; sheet2[ROW2, COL2].ColumnWidth = 14; int colCummTotalQty = COL2;
                int endcol2 = COL2;
                #endregion columns

                sheet2.Range[ROW2, 1, ROW2, endcol2].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet2.Range[ROW2, 1, ROW2, endcol2].CellStyle.Font.Bold = true;
                sheet2.Range[ROW2, 1, ROW2, endcol2].CellStyle.Font.Size = 9f;
                sheet2.Range[ROW2, 1, ROW2, endcol2].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[ROW2, 1, ROW2, endcol2].BorderAround(ExcelLineStyle.Hair);
                sheet2.Range[ROW2, 1, ROW2, endcol2].CellStyle.Font.Color = ExcelKnownColors.White;


                ROW2++;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sheet2[ROW2, colProductionOrderID].Text = dt.Rows[i]["ProductionOrderID"].ToString();
                    sheet2[ROW2, colProductionDate].Text = dt.Rows[i]["ProductionDate"].ToString();
                    sheet2[ROW2, colProductionQty].Text = dt.Rows[i]["ProductionQty"].ToString();
                    sheet2[ROW2, colPlanQty].Text = dt.Rows[i]["PlanQty"].ToString();
                    sheet2[ROW2, colProductionStartDate].Text = dt.Rows[i]["ProductionStartDate"].ToString();
                    sheet2[ROW2, colCummProductionQty].Text = dt.Rows[i]["CummProductionQty"].ToString();
                    sheet2[ROW2, colCummPlanQty].Text = dt.Rows[i]["CummPlanQty"].ToString();
                    sheet2[ROW2, colTotalQty].Text = dt.Rows[i]["TotalQty"].ToString();
                    sheet2[ROW2, colCummTotalQty].Text = dt.Rows[i]["CummTotalQty"].ToString();
                    ROW2++;
                }
                // sheet2.AutoFilters.FilterRange = sheet2.Range[startRow2 - 1, 1, ROW2, endcol2];

                IListObject table2 = sheet2.ListObjects.Create("Table2", sheet2[clsStaticInfo.GetxlsCol(1) + (1).ToString() + ":" + clsStaticInfo.GetxlsCol(endcol2) + (ROW2).ToString()]);
                table2.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;

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
                workbook.Version = ExcelVersion.Excel2016;


                string fPath = fPath = HostingEnvironment.MapPath("~/") + "TempReport" + identity.UserId + ".xlsx";


                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }



                #region OS3- R1
                workbook.Worksheets[0].Name = "OS3- R1";

                IWorksheet pivotSheet = workbook.Worksheets[0];

                IPivotCache cache = workbook.PivotCaches.Add(sheet[startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);
                //pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductCode - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductAttribute - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colMasterOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colproductionStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPRQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPRQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPRPlannedQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPRPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPOProduceQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPOProduceQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPORemainingQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPORemainingQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colAvailableProducedQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colAvailableProducedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPOStartDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPOCompletionDate - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colCummPlannedQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colCummPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                //pivotTable.Fields[colPONo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSOQty - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOrderCategory - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOrderStatus - 1].Axis = PivotAxisTypes.Row;

                //IPivotField field = pivotTable.Fields[colSOQty - 1];
                //field.NumberFormat = clsStaticInfo.NumberFormat();
                //pivotTable.DataFields.Add(field, "SO Qty", PivotSubtotalTypes.Sum);

                pivotTable.Fields[colArticle - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCommitmentDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPlanExFactoryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colExpectedCompletionDate - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colExpectedExFactoryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEarlyBy - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colEarlyBy - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colLateBy - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colLateBy - 1].NumberFormat = clsStaticInfo.NumberFormat();
                //pivotTable.Fields[colDeliveryMonth - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colDeliveryMonth - 1].NumberFormat = clsStaticInfo.NumberFormat();

                //pivotTable.Fields[colProductionCompletionMonth - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colCummPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colEntity - 1|| i == colCustomer - 1 || i == colProductCode - 1 || i == colProductAttribute - 1 || i == colBuyerOrderNo - 1
                        || i == colProductionOrderId - 1 || i == colproductionStatus - 1 || i == colPRQty - 1 || i == colPRPlannedQty - 1 || i == colPOProduceQty - 1 || i == colPORemainingQty - 1 || i == colPOStartDate - 1 || i == colPOCompletionDate - 1|| i==colSOQty-1
                        || i == colDeliveryDate - 1 || i == colSalesOrderId - 1 || i == colOrderCategory - 1 || i == colOrderStatus - 1 
                        || i == colArticle - 1 || i == colCommitmentDate - 1 || i == colPlanExFactoryDate - 1 || i == colExpectedCompletionDate - 1  || i == colEarlyBy - 1 || i == colLateBy - 1||i== colAvailableProducedQty-1
                      )
                    {
                        pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                    }
                    else
                    {
                       
                    }
                }


                

                //field = pivotTable.Fields[colPlannedQty - 1];
                //field.NumberFormat = clsStaticInfo.NumberFormat();
                //pivotTable.DataFields.Add(field, "Plan Qty", PivotSubtotalTypes.Sum);

                //field = pivotTable.Fields[colAvailableProducedQty - 1];
                //field.NumberFormat = clsStaticInfo.NumberFormat();
                //pivotTable.DataFields.Add(field, "Available Produced Qty", PivotSubtotalTypes.Sum);




                //int totalColumns = pivotTable2_1.RowFields.Count + pivotTable2_1.ColumnFields.Count;
                sheet = workbook.Worksheets[0];
                //int StartFormattingColumn = pivotTable.RowFields.Count + 1;
                //int endFormaatingColumn = StartFormattingColumn + pivotTable.ColumnFields[0].Items.Count + pivotTable.ColumnFields[1].Items.Count;


                pivotTable.ShowDrillIndicators = false;
                //pivotTable.ShowDataFieldInRow = true;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Sales Order Wise Production Completion Date Report", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion Buyer Summary

               
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }



        #endregion


    }

}