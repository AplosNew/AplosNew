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

        private DataRow GetExpectedSOCompletionDate(double RequiredQty, string POId, DataTable Data)
        {
            for (int i = 0; i < Data.Rows.Count; i++)
            {
                if (Data.Rows[i]["POId"].ToString() == POId)
                {

                    if (clsStaticInfo.dbl(Data.Rows[i]["CumProdQty"].ToString()) >= RequiredQty)
                    {
                        return Data.Rows[i];
                    }
                }
            }


            return null;
        }

        [HttpPost, Authorize]
        public ActionResult GetOS3xlsReport(Dictionary<string, string> parameters)
        {
            try
            {
                var workbook = GetSOCompletionWorkook(parameters);

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

        public IWorkbook GetSOCompletionWorkook(Dictionary<string, string> parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            DataTable dtSOComplete, dtOrderMaster;
            string ExpectedDate = "";
            try
            {
                _productionSummaryData.GetProductionOrderMaster(parameters, out dtOrderMaster);
                _productionSummaryData.GetSOCompletionData(parameters, out dtSOComplete);

                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[1].Name = "PrO Data";
                sheet = workbook.Worksheets[1];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Sr."; sheet[ROW, COL].ColumnWidth = 8; int colSeq = COL; COL++;
                sheet[ROW, COL].Text = "POId"; sheet[ROW, COL].ColumnWidth = 10; int colPOId = COL; COL++;
                sheet[ROW, COL].Text = "ScheduleId"; sheet[ROW, COL].ColumnWidth = 12; int colScheduleId = COL; COL++;
                sheet[ROW, COL].Text = "POStatus"; sheet[ROW, COL].ColumnWidth = 8; int colPOStatus = COL; COL++;
                sheet[ROW, COL].Text = "POCreationDate"; sheet[ROW, COL].ColumnWidth = 16; int colPOCreationDate = COL; COL++;
                sheet[ROW, COL].Text = "BaseProcProdStartDate"; sheet[ROW, COL].ColumnWidth = 14; int colBaseProcProdStartDate = COL; COL++;
                sheet[ROW, COL].Text = "BaseProductionEndDate"; sheet[ROW, COL].ColumnWidth = 14; int colBaseProductionEndDate = COL; COL++;
                sheet[ROW, COL].Text = "BaseProcPlanStartDate"; sheet[ROW, COL].ColumnWidth = 14; int colBaseProcPlanStartDate = COL; COL++;
                sheet[ROW, COL].Text = "BaseProcPlanEndDate"; sheet[ROW, COL].ColumnWidth = 22; int colBaseProcPlanEndDate = COL; COL++;
                sheet[ROW, COL].Text = "POStartDate"; sheet[ROW, COL].ColumnWidth = 22; int colPOStartDate = COL; COL++;
                sheet[ROW, COL].Text = "POCompletionDate"; sheet[ROW, COL].ColumnWidth = 22; int colPOCompletionDate = COL; COL++;
                sheet[ROW, COL].Text = "NoOfSO"; sheet[ROW, COL].ColumnWidth = 8; int colNoOfSO = COL; COL++;
                sheet[ROW, COL].Text = "Date"; sheet[ROW, COL].ColumnWidth = 22; int colDate = COL; COL++;
                sheet[ROW, COL].Text = "PlanningStatus"; sheet[ROW, COL].ColumnWidth = 10; int colPlanningStatus = COL; COL++;
                sheet[ROW, COL].Text = "POCompletion"; sheet[ROW, COL].ColumnWidth = 10; int colPOCompletion = COL; COL++;
                sheet[ROW, COL].Text = "ProdQty"; sheet[ROW, COL].ColumnWidth = 10; int colProdQty = COL; COL++;
                sheet[ROW, COL].Text = "PlanQty"; sheet[ROW, COL].ColumnWidth = 10; int colPlanQty = COL; COL++;
                sheet[ROW, COL].Text = "AvailableQty"; sheet[ROW, COL].ColumnWidth = 10; int colAvailableQty = COL; COL++;
                sheet[ROW, COL].Text = "CumProdQty"; sheet[ROW, COL].ColumnWidth = 10; int colCumProdQty = COL;
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

                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                {
                    sheet[ROW, colSeq].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["Seq"].ToString());
                    sheet[ROW, colPOId].Text = dtOrderMaster.Rows[i]["POId"].ToString();
                    sheet[ROW, colScheduleId].Text = dtOrderMaster.Rows[i]["ScheduleId"].ToString();
                    sheet[ROW, colPOStatus].Text = dtOrderMaster.Rows[i]["POStatus"].ToString();
                    sheet[ROW, colPOCreationDate].Text = dtOrderMaster.Rows[i]["POCreationDate"].ToString();
                    sheet[ROW, colBaseProcProdStartDate].Text = dtOrderMaster.Rows[i]["BaseProcProdStartDate"].ToString();
                    sheet[ROW, colBaseProductionEndDate].Text = dtOrderMaster.Rows[i]["BaseProductionEndDate"].ToString();
                    sheet[ROW, colBaseProcPlanStartDate].Text = dtOrderMaster.Rows[i]["BaseProcPlanStartDate"].ToString();
                    sheet[ROW, colBaseProcPlanEndDate].Text = dtOrderMaster.Rows[i]["BaseProcPlanEndDate"].ToString();
                    sheet[ROW, colPOStartDate].Text = dtOrderMaster.Rows[i]["POStartDate"].ToString();
                    sheet[ROW, colPOCompletionDate].Text = dtOrderMaster.Rows[i]["POCompletionDate"].ToString();
                    sheet[ROW, colNoOfSO].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["NoOfSO"].ToString());
                    sheet[ROW, colDate].Text = dtOrderMaster.Rows[i]["Date"].ToString();
                    sheet[ROW, colPlanningStatus].Text = dtOrderMaster.Rows[i]["PlanningStatus"].ToString();
                    sheet[ROW, colPOCompletion].Text = dtOrderMaster.Rows[i]["POCompletion"].ToString();
                    sheet[ROW, colProdQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["ProdQty"].ToString());
                    sheet[ROW, colPlanQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PlanQty"].ToString());
                    sheet[ROW, colAvailableQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["AvailableQty"].ToString());
                    sheet[ROW, colCumProdQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["CumProdQty"].ToString());

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.IsDisplayZeros = false;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange["A7"].FreezePanes();


                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Production Data", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;


                IWorksheet sheet2 = workbook.Worksheets[0];
                sheet2.Name = "SOComData";

                #region columns
                int ROW2 = 6, COL2 = 1;
                int startRow2 = ROW2;
                sheet2[ROW2, COL2].Text = "Responsible Person"; sheet2[ROW2, COL2].ColumnWidth = 15; int colResponsiblePerson = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Customer"; sheet2[ROW2, COL2].ColumnWidth = 15; int colCustomer = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Buyer Ref"; sheet2[ROW2, COL2].ColumnWidth = 12; int colBuyerRef = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Own Ref"; sheet2[ROW2, COL2].ColumnWidth = 12; int colOwnRef = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "LineitemId"; sheet2[ROW2, COL2].ColumnWidth = 10; int colLineitemId = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Article"; sheet2[ROW2, COL2].ColumnWidth = 20; int colArticle = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Product Code"; sheet2[ROW2, COL2].ColumnWidth = 8; int colProductCode = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Product Library Detail"; sheet2[ROW2, COL2].ColumnWidth = 15; int colProductCodeDetail = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "SOId"; sheet2[ROW2, COL2].ColumnWidth = 10; int colSOId = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "SO Status"; sheet2[ROW2, COL2].ColumnWidth = 10; int colSOStatus = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "POId"; sheet2[ROW2, COL2].ColumnWidth = 8; int colProductionId = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "PO Status"; sheet2[ROW2, COL2].ColumnWidth = 10; int colPOStat = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Delivery Date"; sheet2[ROW2, COL2].ColumnWidth = 14; int colDeliveryDate = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Ex-Factory Date"; sheet2[ROW2, COL2].ColumnWidth = 14; int colExFactoryDate = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Commitment Date"; sheet2[ROW2, COL2].ColumnWidth = 14; int colCommitmentDate = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "SO Completion Date"; sheet2[ROW2, COL2].ColumnWidth = 14; int colSOComDate = COL2++;
                sheet2[ROW2, COL2].Text = "Exp Ex Factory Date"; sheet2[ROW2, COL2].ColumnWidth = 14; int colExpExFactory = COL2++;
                sheet2[ROW2, COL2].Text = "SO Qty"; sheet2[ROW2, COL2].ColumnWidth = 10; int colSOQty = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "EarlyBy"; sheet2[ROW2, COL2].ColumnWidth = 10; int colEarlyBy = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "LateBy"; sheet2[ROW2, COL2].ColumnWidth = 10; int colLateBy = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Diff. From Commitmeny/ExfactoryDate"; sheet2[ROW2, COL2].ColumnWidth = 10; int colCE = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Sequence"; sheet2[ROW2, COL2].ColumnWidth = 8; int colSequence = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "So Commqty"; sheet2[ROW2, COL2].ColumnWidth = 10; int colSoCommqty = COL2; COL2++;
                sheet2[ROW2, COL2].Text = "Leg Days"; sheet2[ROW2, COL2].ColumnWidth = 7; int colLegDays = COL2;
                int endcol2 = COL2;
                #endregion columns

                sheet2.Range[ROW2, 1, ROW2, endcol2].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet2.Range[ROW2, 1, ROW2, endcol2].CellStyle.Font.Bold = true;
                sheet2.Range[ROW2, 1, ROW2, endcol2].CellStyle.Font.Size = 9f;
                sheet2.Range[ROW2, 1, ROW2, endcol2].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[ROW2, 1, ROW2, endcol2].BorderAround(ExcelLineStyle.Hair);
                sheet2.Range[ROW2, 1, ROW2, endcol2].CellStyle.Font.Color = ExcelKnownColors.White;


                ROW2++;
                for (int i = 0; i < dtSOComplete.Rows.Count; i++)
                {
                    sheet2[ROW2, colSequence].Number = clsStaticInfo.dbl(dtSOComplete.Rows[i]["Seq"].ToString());
                    sheet2[ROW2, colProductionId].Text = dtSOComplete.Rows[i]["ProductionOrderId"].ToString();
                    sheet2[ROW2, colSOStatus].Text = dtSOComplete.Rows[i]["SOStatus"].ToString();
                    sheet2[ROW2, colDeliveryDate].Text = dtSOComplete.Rows[i]["DeliveryDate"].ToString();
                    sheet2[ROW2, colSOId].Text = dtSOComplete.Rows[i]["SOId"].ToString();
                    sheet2[ROW2, colSOQty].Number = clsStaticInfo.dbl(dtSOComplete.Rows[i]["SOQty"].ToString());
                    sheet2[ROW2, colSoCommqty].Number = clsStaticInfo.dbl(dtSOComplete.Rows[i]["SoCommqty"].ToString());
                    sheet2[ROW2, colLegDays].Number = clsStaticInfo.dbl(dtSOComplete.Rows[i]["Days"].ToString());
                    sheet2[ROW2, colResponsiblePerson].Text = dtSOComplete.Rows[i]["ResponsiblePerson"].ToString();
                    sheet2[ROW2, colCustomer].Text = dtSOComplete.Rows[i]["Customer"].ToString();
                    sheet2[ROW2, colBuyerRef].Text = dtSOComplete.Rows[i]["BuyerReferenceNo"].ToString();
                    sheet2[ROW2, colOwnRef].Text = dtSOComplete.Rows[i]["OwnReferenceNo"].ToString();
                    sheet2[ROW2, colLineitemId].Text = dtSOComplete.Rows[i]["LineitemId"].ToString();
                    sheet2[ROW2, colArticle].Text = dtSOComplete.Rows[i]["Article"].ToString();
                    sheet2[ROW2, colProductCode].Text = dtSOComplete.Rows[i]["ProductCode"].ToString();
                    sheet2[ROW2, colProductCodeDetail].Text = dtSOComplete.Rows[i]["ProductLibraryDetail"].ToString();
                    sheet2[ROW2, colPOStat].Text = dtSOComplete.Rows[i]["POStatus"].ToString();
                    sheet2[ROW2, colExFactoryDate].Text = dtSOComplete.Rows[i]["ExFactoryDate"].ToString();
                    sheet2[ROW2, colCommitmentDate].Text = dtSOComplete.Rows[i]["CommitmentDate"].ToString();
                    sheet2[ROW2, colCE].Text = dtSOComplete.Rows[i]["DiffComEx"].ToString();

                    DataRow dr = GetExpectedSOCompletionDate(clsStaticInfo.dbl(dtSOComplete.Rows[i]["SoCommqty"].ToString()), dtSOComplete.Rows[i]["ProductionOrderId"].ToString(), dtOrderMaster);

                    if (dr != null)
                    {
                        ExpectedDate = GetDate(dr["Date"].ToString());
                        dtSOComplete.Rows[i]["ExDate"] = ExpectedDate;

                        sheet2[ROW2, colSOComDate].Text = ExpectedDate;
                        sheet2[ROW2, colSOComDate].NumberFormat = "dd-MMM-yyyy";

                        sheet2[ROW2, colExpExFactory].Text = Convert.ToDateTime(ExpectedDate).AddDays(clsStaticInfo.dbl(dtSOComplete.Rows[i]["Days"].ToString())).ToString("dd-MMM-yyyy");

                        if (!string.IsNullOrEmpty(dtSOComplete.Rows[i]["ExFactoryDate"].ToString()))
                        {
                            if (Convert.ToDateTime(ExpectedDate).AddDays(clsStaticInfo.dbl(dtSOComplete.Rows[i]["Days"].ToString())) > Convert.ToDateTime(dtSOComplete.Rows[i]["ExFactoryDate"].ToString()))
                            {
                                sheet2[ROW2, colEarlyBy].Number = 0;
                            }
                            else
                            {
                                TimeSpan ts = Convert.ToDateTime(ExpectedDate).AddDays(clsStaticInfo.dbl(dtSOComplete.Rows[i]["Days"].ToString())) - Convert.ToDateTime(dtSOComplete.Rows[i]["ExFactoryDate"].ToString());

                                sheet2[ROW2, colEarlyBy].Number = ts.Days;
                            }

                            if (Convert.ToDateTime(ExpectedDate).AddDays(clsStaticInfo.dbl(dtSOComplete.Rows[i]["Days"].ToString())) < Convert.ToDateTime(dtSOComplete.Rows[i]["ExFactoryDate"].ToString()))
                            {
                                sheet2[ROW2, colLateBy].Number = 0;
                            }
                            else
                            {
                                TimeSpan ts = Convert.ToDateTime(dtSOComplete.Rows[i]["ExFactoryDate"].ToString()) - Convert.ToDateTime(ExpectedDate).AddDays(clsStaticInfo.dbl(dtSOComplete.Rows[i]["Days"].ToString()));

                                sheet2[ROW2, colLateBy].Number = ts.Days;
                            }
                        }


                    }


                    ROW2++;
                }



                sheet2.AutoFilters.FilterRange = sheet2.Range[startRow2, 1, ROW2, endcol2];

                reportUtility.CompanyPlantHeaderNew(ref sheet2, 1, "Sales Order Wise Production Completion Date Report", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet2, 6, ExcelPageOrientation.Landscape);
                sheet2[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet2.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet2.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet2.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet2.IsGridLinesVisible = false;




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