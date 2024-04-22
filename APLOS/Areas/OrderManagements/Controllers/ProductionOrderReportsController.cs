using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using OTSBD;
using System.Linq;
using Library.Service.Enums;
using System.Text;
using System.Collections.Specialized;
using static Library.Service.Helpers.ReportUtility;
using Library.Service.Helpers;
using System.Threading.Tasks;
using Syncfusion.XlsIO.Implementation;
using Aplos.Helpers;
using Library.Crosscutting;
using bplib;
using System.Web.Hosting;
using Syncfusion.Office;
using Library.Service.Productions.ProductionBooking;
using System.Text.RegularExpressions;
using Syncfusion.Pdf.Parsing;
using Library.OrderManagement.Production;
using Library.Model.Enums;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ProductionOrderReportsController : BaseController
    {
        public enum PlanningStatus { TOSTART, FREEZE, RUNNING };
        private EnumPlanningTypes ScreenPlanningType = EnumPlanningTypes.PlanningType1;
        private IWorksheet pivotSheet;
        private IPivotTable pivotTable;
        private IPivotCache cache;
        #region Constructor

        private readonly IProductionOrderService _productionOrderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly Library.Service.OrderManagements.ProductionOrderReports ProductionOrderReports = null;

        public ProductionOrderReportsController(IProductionOrderService productionOrderService, IUnitOfWork U, ISqlRepository R)
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
        public JsonResult GetProcessForPlanning()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT p.* FROM PlanningTypes AS pt 
                                INNER JOIN hkp.Process AS p ON p.Id=pt.BaseProcessId
                                WHERE PT.PlanningType='" + ScreenPlanningType.ToString() + "' AND pt.CompanyGroupId='" + identity.CompanyGroupId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPlantAndProcess()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sqlPlants = @"SELECT Id,c.UserName AS PlantName,convert(bit,0) AS isChecked,'' AS SelectedEntities FROM org.Plant AS c
                                    WHERE c.CompanyId='" + identity.CompanyId + @"' and [Active]=1
                                    ORDER BY c.Sequence";

            string sqlEntitites = @"SELECT Id,e.PlantId,e.UserName AS Entity,convert(bit,0) AS isChecked
  FROM org.Entity AS e WHERE e.CompanyId='" + identity.CompanyId + @"' and [Active]=1
ORDER BY e.PlantId, e.UserName";

            return Json(new { Plants = _sqlRepository.GetDataCollection(sqlPlants), Entities = _sqlRepository.GetDataCollection(sqlEntitites) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPivotData()
        {

            Library.HumanResource.Dashboard.SkillMapping skill = new Library.HumanResource.Dashboard.SkillMapping();

            var data = skill.DateWiseSkillData(out List<string> ColumnList);
            return Json(new { DATA = data, Columns = ColumnList }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetEntity()
        {
            try
            {

                string sql = @"";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (identity.IsSysAdmin)
                {
                    sql = @"SELECT distinct E.* FROM [ORG].[Entity] E
                            LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E.Id
                            WHERE E.PlantId='" + identity.PlantId + @"' AND ECC.IsProductionEntity=1 AND E.[Active]=1 ORDER BY E.Code";
                    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                }

                sql = @"SELECT distinct e2.* FROM [SEC].[UserEntity] E
                        LEFT JOIN org.Entity AS e2 ON e2.Id=e.EntityId
                        LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E2.Id
                        WHERE E.UserId='" + identity.UserId + @"' AND e.PlantId='" + identity.PlantId + "' AND ECC.IsProductionEntity=1 AND E2.[Active]=1 ORDER BY E2.Code";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);


                throw new Exception("No entity configurations was found in the system for the current user");
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }


        [HttpPost, Authorize]
        public JsonResult RunProductionTargetScheduler()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ProductionServices services = new ProductionServices(_sqlRepository);
                services.UpdateDailyTarget(System.DateTime.Now.ToString("dd-MMM-yyyy"), identity.PlantId);

                return Json(new { Error = false, Message = "Data Updated Successfully" });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        #endregion


        #region OS2 based on base process
        //based on base process therefore no need the select the base process

        [HttpGet, Authorize]
        public ActionResult GetDateRange(string entityid)
        {
            string FromDate = System.DateTime.Now.ToString("dd-MMM-yyyy");
            string ToDate = System.DateTime.Now.AddMonths(3).ToString("dd-MMM-yyyy");

            DataTable dt = _sqlRepository.GetDataTable("SELECT MIN(ProductionDate) MinPlanDate,MAX(ProductionDate) AS MaxPlanDate FROM ProductionPlanningType1 WHERE EntityID='" + entityid + @"'");
            if (dt.Rows.Count > 0)
            {
                //    FromDate = Convert.ToDateTime(dt.Rows[0]["MinPlanDate"].ToString()).ToString("dd-MMM-yyyy");
                //    ToDate = Convert.ToDateTime(FromDate).AddMonths(3).ToString("dd-MMM-yyyy");

            }

            return Json(new { FromDate = FromDate, ToDate = ToDate }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public ActionResult OS2xls(string entityid)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();

                DataTable dt;
                getOS2Query(entityid, out dt);
                IWorkbook workbook = OS2xlsFile(excelEngine, entityid, dt);

                string strFileName = "OS2.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();

            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return null;
        }
        [HttpGet, Authorize]
        public ActionResult OS5xls(string entityid, string fromDate, string toDate)
        {
            try
            {
                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");

                if (string.IsNullOrEmpty(fromDate))
                    throw new Exception("Select from date");

                if (string.IsNullOrEmpty(toDate))
                    throw new Exception("Select to date");

                if (bplib.clsWebLib.IsDateOK(fromDate) == false || fromDate == "undefined")
                    throw new Exception("Select from date");

                if (bplib.clsWebLib.IsDateOK(toDate) == false || fromDate == "undefined")
                    throw new Exception("Select to date");

                if (Convert.ToDateTime(fromDate) > Convert.ToDateTime(toDate))
                    throw new Exception("To date cannot be earlier than from date");


                if (Math.Abs(clsStaticInfo.dateDiff(fromDate, toDate)) > 180)
                    throw new Exception("Cannot set date range greater than six months");

                ExcelEngine excelEngine = new ExcelEngine();

                DataTable dt;
                getOS5Query(entityid, fromDate, toDate, out dt);
                IWorkbook workbook = OS5xlsFile(excelEngine, entityid, fromDate, toDate, dt);

                string strFileName = "OS5.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();

            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        [HttpGet, Authorize]
        public ActionResult OS2Snapshotxls(string entityid, string SnapshotId)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();

                DataTable dt;
                getOS2SnapshotQuery(entityid, SnapshotId, out dt);
                IWorkbook workbook = OS2xlsFile(excelEngine, entityid, dt);

                string strFileName = "OS2 SnapshotNo " + SnapshotId + ".xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();

            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        public IWorkbook OS2xlsFile(ExcelEngine excelEngine, string entityid, DataTable dt)
        {

            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {

                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");




                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (dt.Rows.Count == 0)
                    throw new Exception("No data found");


                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(5);
                workbook.Worksheets[4].Name = "OS2 Data";
                sheet = workbook.Worksheets[4];


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
                sheet[ROW, COL].Text = "Work Center";
                sheet[ROW, COL].ColumnWidth = 12;
                int colWorkCenter = COL;
                COL++;

                sheet[ROW, COL].Text = "First Process";
                sheet[ROW, COL].ColumnWidth = 12;
                int colFirstProcess = COL;
                COL++;

                sheet[ROW, COL].Text = "First Process Work Center";
                sheet[ROW, COL].ColumnWidth = 12;
                int colFPWorkCenter = COL;
                COL++;

                sheet[ROW, COL].Text = "Work Center Group";
                sheet[ROW, COL].ColumnWidth = 12;
                int colWorkCenterGroup = COL;
                COL++;
                sheet[ROW, COL].Text = "Work Center Plan Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTargetDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Work Center Plan Month";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTargetMonth = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order ID";
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionOrderID = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrderRemarks = COL;
                //COL++;
                //sheet[ROW, COL].Text = "Work Center Plan Qty";
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet[ROW, COL].ColumnWidth = 10;
                //int colPlannedQtyForTheDay = COL;
                COL++;
                sheet[ROW, COL].Text = "SPT (Minutes)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colSPT = COL;

                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 14;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 14;
                int colbuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Style Group";
                sheet[ROW, COL].ColumnWidth = 10;
                int colStyleGroup = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Order No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBuyerOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Order No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOwnOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Line Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBuyerStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Line Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOwnStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Ids";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSalesOrderIds = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Desc";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSalesOrderDesc = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductCategory = COL;

                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 10;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "SKU1";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSKU1 = COL;
                COL++;
                sheet[ROW, COL].Text = "SKU2";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSKU2 = COL;
                COL++;
                sheet[ROW, COL].Text = "SKU3";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSKU3 = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Raw Material Inhouse Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMainRawMaterialInhouseDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Inhouse Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOtherRawMaterialInhouseDate = COL;


                COL++;
                sheet[ROW, COL].Text = "Production Order Priority";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionPriority = COL;
                COL++;

                //sheet[ROW, COL].Text = "Order Status";
                //sheet[ROW, COL].ColumnWidth = 10;
                //int colOrderStatus = COL;
                //COL++;



                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colFOB = COL;
                //COL++;
                //sheet[ROW, COL].Text = "CM";
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet[ROW, COL].ColumnWidth = 8;
                //int colCM = COL;
                COL++;
                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colCurrency = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colOrderQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan SO Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanOrderQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Qty (WC)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionQtyAtWC = COL;
                COL++;
                sheet[ROW, COL].Text = "Balance Qty (WC)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colBalanceQtyAtWC = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Qty (PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionQtyAtPR = COL;
                COL++;
                sheet[ROW, COL].Text = "Balance Production Qty (PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colBalanceProductionQtyAtPR = COL;
                COL++;
                sheet[ROW, COL].Text = "Unit";
                sheet[ROW, COL].ColumnWidth = 12;
                int colUnit = COL;

                COL++;
                sheet[ROW, COL].Text = "Account Incharge";
                sheet[ROW, COL].ColumnWidth = 14;
                int colAccountIncharge = COL;
                COL++;
                sheet[ROW, COL].Text = "Account Holder";
                sheet[ROW, COL].ColumnWidth = 14;
                int colAccountHolder = COL;
                COL++;
                sheet[ROW, COL].Text = "Commitment Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCommitmentDate = COL;
                COL++;
                sheet[ROW, COL].Text = "First Delivery Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colFirstDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Last Delivery Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLastDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Completion Date (Last Commitment Date)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionCompletionDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Base Process CompletionDate";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBaseProcessCompletionDate = COL;



                COL++;
                sheet[ROW, COL].Text = "Workcenter Start Date (Prod. For Current WC)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLineStartDateAtWC = COL;
                COL++;
                sheet[ROW, COL].Text = "Workcenter Start Date (Prod. For Current Entity)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLineStartDateAtPR = COL;
                COL++;
                sheet[ROW, COL].Text = "Planning Start Date (For Current WC)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanningStartDateAtWC = COL;
                COL++;
                sheet[ROW, COL].Text = "Planning Start Date (For Current Entity)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanningStartDateAtPR = COL;

                COL++;
                sheet[ROW, COL].Text = "Workcenter End Date (Prod. For Current WC)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLineEndDateAtWC = COL;
                COL++;
                sheet[ROW, COL].Text = "Workcenter End Date (Prod. For Current Entity)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLineEndDateAtPR = COL;
                COL++;
                sheet[ROW, COL].Text = "Planning End Date (For Current WC)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanningEndDateAtWC = COL;
                COL++;
                sheet[ROW, COL].Text = "Planning End Date (For Current Entity)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanningEndDateAtPR = COL;

                //COL++;
                //sheet[ROW, COL].Text = "Line Target Per Day";
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet[ROW, COL].ColumnWidth = 10;
                //int colLineTargetPerDay = COL;

                COL++;
                sheet[ROW, COL].Text = "Workcenter Target UOM";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLineTargetUOM = COL;

                COL++;
                sheet[ROW, COL].Text = "Production Order Status";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "No Of Work Station(PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colNoOfWorkStation = COL;
                COL++;
                sheet[ROW, COL].Text = "LSD";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLSD = COL;

                COL++;
                sheet[ROW, COL].Text = "Booked CM";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colBookedCM = COL;

                COL++;
                sheet[ROW, COL].Text = "Workcenter Plan Hour(Calendar)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colLinePlanHourCalendar = COL;


                COL++;
                sheet[ROW, COL].Text = "Workcenter Std Working hour(Workcenter)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colWorkcenterStdWorkinghour = COL;
                COL++;
                sheet[ROW, COL].Text = "No Of Work Station(Workcenter)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colNoOfWorkStationWorkcenter = COL;
                //COL++;
                //sheet[ROW, COL].Text = "WC Std Hour Cost(Product)";
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet[ROW, COL].ColumnWidth = 10;
                //int colWCStdHourCostProduct = COL;

                COL++;
                sheet[ROW, COL].Text = "WC Fixed Cost(Workcenter)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colWCStdHourCostWorkCenter = COL;
                COL++;
                sheet[ROW, COL].Text = "Variable Cost per hour(Workcenter)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colVariableCostperhour = COL;
                COL++;
                sheet[ROW, COL].Text = "Actual Workcenter Cost For The Day";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colActualLineCostForTheDay = COL;


                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colCM = COL;

                COL++;
                sheet[ROW, COL].Text = "Workcenter Target Per Day (PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colLineTargetPerDay = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Target Per Hour(PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanTargetPerHourPR = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Plan Hours(PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionPlanHoursPR = COL;

                COL++;
                sheet[ROW, COL].Text = "Work Center Plan Qty(Simulated)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlannedQtyForTheDay = COL;
                COL++;
                sheet[ROW, COL].Text = "Working Hours(Simulated)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colWorkingHours = COL;
                COL++;
                sheet[ROW, COL].Text = "Build Up";
                sheet[ROW, COL].ColumnWidth = 6;
                int colIsBuildup = COL;
                COL++;
                sheet[ROW, COL].Text = "Minimum Workcenter Target (Based on the Workcenter cost)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colBreakEvenQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Profit/Loss";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossPcs = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Loss Due To Workcenter Target";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossLineTargetPcs = COL;

                COL++;
                sheet[ROW, COL].Text = "Production Loss Due To Buildup";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossBuildUpPcs = COL;
                COL++;
                sheet[ROW, COL].Text = "P/L UOM";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProfitLossProductionUOM = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Profit/Loss Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossAmount = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Loss Due To Workcenter Target Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossLineTargetAmount = COL;

                COL++;
                sheet[ROW, COL].Text = "Production Loss Due To Buildup Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossBuildUpAmount = COL;
                COL++;
                sheet[ROW, COL].Text = "P/L Currency";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProfitLossProductionCurrency = COL;
                COL++;
                sheet[ROW, COL].Text = "Base Process Completion Before First Delivery Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBaseProcessCompletionBeforeFirstDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Efficiency %";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colEfficiencyPercentage = COL;
                COL++;
                sheet[ROW, COL].Text = "Bulletin Id";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBulletinId = COL;
                COL++;
                sheet[ROW, COL].Text = "MC SPT";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colMachineSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "NON-MC SPT";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colNonMachineSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "MC Manpower";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colMachineManpower = COL;
                COL++;
                sheet[ROW, COL].Text = "NON-MC Manpower";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colNonMachineManpower = COL;
                COL++;
                sheet[ROW, COL].Text = "Total Manpower";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colTotalManpower = COL;
                COL++;
                sheet[ROW, COL].Text = "Total SPT(Bulletin)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colTotalSPTBulletin = COL;
                COL++;
                sheet[ROW, COL].Text = "Total SPT (Hours)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colTotalSPT = COL;
                #endregion columns

                int endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;



                double StandardWS, StandardHours, StandardFixedCost, StandardAdditionalVCPerHour, PlanHour, PlanWS, WSCostPerHour, TotalPlanCost;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //text fields
                    sheet[ROW, colPlant].Text = dt.Rows[i]["Plant"].ToString();
                    sheet[ROW, colEntity].Text = dt.Rows[i]["Entity"].ToString();
                    sheet[ROW, colFirstProcess].Text = dt.Rows[i]["FirstProcess"].ToString();
                    sheet[ROW, colFPWorkCenter].Text = dt.Rows[i]["FirstProcessWC"].ToString();
                    sheet[ROW, colMaterial].Text = dt.Rows[i]["Material"].ToString();
                    sheet[ROW, colProduct].Text = dt.Rows[i]["Product"].ToString();
                    sheet[ROW, colArticle].Text = dt.Rows[i]["Article"].ToString();
                    sheet[ROW, colSKU1].Text = dt.Rows[i]["SKU1"].ToString();
                    sheet[ROW, colSKU2].Text = dt.Rows[i]["SKU2"].ToString();
                    sheet[ROW, colSKU3].Text = dt.Rows[i]["SKU3"].ToString();

                    sheet[ROW, colAccountHolder].Text = dt.Rows[i]["AccountHolder"].ToString();
                    sheet[ROW, colAccountIncharge].Text = dt.Rows[i]["AccountIncharge"].ToString();
                    sheet[ROW, colbuyer].Text = dt.Rows[i]["buyer"].ToString();
                    sheet[ROW, colCustomer].Text = dt.Rows[i]["Customer"].ToString();
                    sheet[ROW, colMasterOrderNo].Text = dt.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colProductCategory].Text = dt.Rows[i]["ProductCategory"].ToString();
                    sheet[ROW, colProductionOrderID].Text = dt.Rows[i]["ProductionOrderID"].ToString();
                    sheet[ROW, colProductionOrderRemarks].Text = dt.Rows[i]["Remarks"].ToString();

                    sheet[ROW, colProductionStatus].Text = dt.Rows[i]["ProductionStatus"].ToString();
                    sheet[ROW, colStyleGroup].Text = dt.Rows[i]["StyleGroup"].ToString();
                    sheet[ROW, colCurrency].Text = dt.Rows[i]["Currency"].ToString();
                    //sheet[ROW, colOrderStatus].Text = dt.Rows[i]["OrderStatus"].ToString();


                    sheet[ROW, colBuyerOrderNo].Text = dt.Rows[i]["BuyerRefNo"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = dt.Rows[i]["OwnRefNo"].ToString();
                    sheet[ROW, colBuyerStyleNo].Text = dt.Rows[i]["StyleNo"].ToString();
                    sheet[ROW, colOwnStyleNo].Text = dt.Rows[i]["OwnStyleNo"].ToString();

                    sheet[ROW, colSalesOrderIds].Text = dt.Rows[i]["SONo"].ToString().Replace(",", ", ");
                    sheet[ROW, colSalesOrderDesc].Text = dt.Rows[i]["SODesc"].ToString();


                    sheet[ROW, colUnit].Text = dt.Rows[i]["Unit"].ToString();
                    sheet[ROW, colWorkCenter].Text = dt.Rows[i]["WorkCenter"].ToString();
                    sheet[ROW, colWorkCenterGroup].Text = dt.Rows[i]["GroupingData"].ToString();

                    //number fields
                    sheet[ROW, colBookedCM].Number = clsStaticInfo.dbl(dt.Rows[i]["BookedCM"].ToString());
                    sheet[ROW, colCM].Number = clsStaticInfo.dbl(dt.Rows[i]["CM"].ToString());
                    sheet[ROW, colFOB].Number = clsStaticInfo.dbl(dt.Rows[i]["FOB"].ToString());
                    sheet[ROW, colLineTargetPerDay].Number = clsStaticInfo.dbl(dt.Rows[i]["LineTargetPerDay"].ToString());
                    sheet[ROW, colNoOfWorkStation].Number = clsStaticInfo.dbl(dt.Rows[i]["NoOfWorkStation"].ToString());
                    sheet[ROW, colOrderQty].Number = clsStaticInfo.dbl(dt.Rows[i]["OrderQty"].ToString());
                    sheet[ROW, colPlannedQtyForTheDay].Number = clsStaticInfo.dbl(dt.Rows[i]["PlannedQtyForTheDay"].ToString());
                    sheet[ROW, colPlanOrderQty].Number = clsStaticInfo.dbl(dt.Rows[i]["PlanOrderQty"].ToString());
                    sheet[ROW, colProductionPriority].Number = clsStaticInfo.dbl(dt.Rows[i]["ProductionPriority"].ToString());
                    sheet[ROW, colProductionQtyAtPR].Number = clsStaticInfo.dbl(dt.Rows[i]["ProductionQtyAtPR"].ToString());
                    sheet[ROW, colProductionQtyAtWC].Number = clsStaticInfo.dbl(dt.Rows[i]["ProductionQtyAtWC"].ToString());
                    sheet[ROW, colSPT].Number = clsStaticInfo.dbl(dt.Rows[i]["SPT"].ToString());
                    sheet[ROW, colTotalSPT].Formula = CellAddr(colPlannedQtyForTheDay, ROW) + "*" + CellAddr(colSPT, ROW);
                    sheet[ROW, colWorkingHours].Number = clsStaticInfo.dbl(dt.Rows[i]["WorkingHours"].ToString());
                    sheet[ROW, colBalanceQtyAtWC].Number = clsStaticInfo.dbl(dt.Rows[i]["PlanningQtyAtWC"].ToString());// - clsStaticInfo.dbl(dt.Rows[i]["ProductionQtyAtWC"].ToString());
                    sheet[ROW, colBalanceProductionQtyAtPR].Number = clsStaticInfo.dbl(dt.Rows[i]["PlanningQtyAtPR"].ToString());// - clsStaticInfo.dbl(dt.Rows[i]["ProductionQtyAtPR"].ToString());

                    sheet[ROW, colPlanTargetPerHourPR].Number = clsStaticInfo.dbl(dt.Rows[i]["PlanTargetPerHour"].ToString());
                    sheet[ROW, colProductionPlanHoursPR].Number = clsStaticInfo.dbl(dt.Rows[i]["PlanWorkingHoursPerDay"].ToString());

                    sheet[ROW, colLinePlanHourCalendar].Number = clsStaticInfo.dbl(dt.Rows[i]["CalendarWorkingHours"].ToString());

                    sheet[ROW, colWorkcenterStdWorkinghour].Number = clsStaticInfo.dbl(dt.Rows[i]["StandardWorkingHours"].ToString());
                    //sheet[ROW, colWCStdHourCostProduct].Number = clsStaticInfo.dbl(dt.Rows[i]["StandardWorkingHourCost"].ToString());
                    sheet[ROW, colWCStdHourCostWorkCenter].Number = clsStaticInfo.dbl(dt.Rows[i]["DailyFixedCost"].ToString());
                    sheet[ROW, colVariableCostperhour].Number = clsStaticInfo.dbl(dt.Rows[i]["VariableCostPerHour"].ToString());

                    //date fields
                    sheet[ROW, colBaseProcessCompletionDate].Text = GetDate(dt.Rows[i]["BaseProcessCompletionDate"].ToString());
                    sheet[ROW, colCommitmentDate].Text = GetDate(dt.Rows[i]["CommitmentDate"].ToString());
                    sheet[ROW, colFirstDeliveryDate].Text = GetDate(dt.Rows[i]["FirstDeliveryDate"].ToString());
                    sheet[ROW, colLastDeliveryDate].Text = GetDate(dt.Rows[i]["LastDeliveryDate"].ToString());
                    sheet[ROW, colLineTargetUOM].Text = GetDate(dt.Rows[i]["UOM"].ToString());


                    //if (GetDate(dt.Rows[i]["LineStartDateAtWC"].ToString()) != "")
                    //{
                    sheet[ROW, colLineStartDateAtWC].Text = GetDate(dt.Rows[i]["LineStartDateAtWC"].ToString());
                    sheet[ROW, colLineEndDateAtWC].Text = GetDate(dt.Rows[i]["LineEndDateAtWC"].ToString());
                    sheet[ROW, colLineStartDateAtPR].Text = GetDate(dt.Rows[i]["LineStartDateAtPR"].ToString());
                    sheet[ROW, colLineEndDateAtPR].Text = GetDate(dt.Rows[i]["LineEndDateAtPR"].ToString());
                    //}
                    //else
                    //{
                    sheet[ROW, colPlanningStartDateAtWC].Text = GetDate(dt.Rows[i]["PlanningStartDateAtWC"].ToString());
                    sheet[ROW, colPlanningEndDateAtWC].Text = GetDate(dt.Rows[i]["PlanningEndDateAtWC"].ToString());
                    sheet[ROW, colPlanningStartDateAtPR].Text = GetDate(dt.Rows[i]["PlanningStartDateAtPR"].ToString());
                    sheet[ROW, colPlanningEndDateAtPR].Text = GetDate(dt.Rows[i]["PlanningEndDateAtPR"].ToString());
                    //}


                    sheet[ROW, colBulletinId].Text = dt.Rows[i]["BulletinId"].ToString();
                    sheet[ROW, colMachineSPT].Number = clsStaticInfo.dbl(dt.Rows[i]["MachineSPT"].ToString());
                    sheet[ROW, colNonMachineSPT].Number = clsStaticInfo.dbl(dt.Rows[i]["NonMachineSPT"].ToString());
                    sheet[ROW, colMachineManpower].Number = clsStaticInfo.dbl(dt.Rows[i]["MachineManpower"].ToString());
                    sheet[ROW, colNonMachineManpower].Number = clsStaticInfo.dbl(dt.Rows[i]["NonMachineManpower"].ToString());
                    sheet[ROW, colTotalManpower].Number = clsStaticInfo.dbl(dt.Rows[i]["MachineManpower"].ToString()) + clsStaticInfo.dbl(dt.Rows[i]["NonMachineManpower"].ToString());
                    sheet[ROW, colTotalSPT].Number = clsStaticInfo.dbl(dt.Rows[i]["TotalSPT"].ToString());


                    sheet[ROW, colMainRawMaterialInhouseDate].Text = GetDate(dt.Rows[i]["MainRawMaterialInhouseDate"].ToString());
                    sheet[ROW, colOtherRawMaterialInhouseDate].Text = GetDate(dt.Rows[i]["OtherRawMaterialInhouseDate"].ToString());
                    sheet[ROW, colProductionCompletionDate].Text = GetDate(dt.Rows[i]["ProductionCompletionDate"].ToString());
                    sheet[ROW, colTargetDate].DateTime = Convert.ToDateTime(GetDate(dt.Rows[i]["TargetDate"].ToString()));
                    sheet[ROW, colTargetMonth].Formula = "CONCATENATE(Month(" + CellAddr(colTargetDate, ROW) + "),\"/\",Year(" + CellAddr(colTargetDate, ROW) + "))";

                    sheet[ROW, colLSD].Text = GetDate(dt.Rows[i]["LSD"].ToString());


                    #region Actual Line Cost For the day    
                    sheet[ROW, colNoOfWorkStationWorkcenter].Number = clsStaticInfo.dbl(dt.Rows[i]["StandardWorkStations"].ToString());


                    sheet[ROW, colProfitLossProductionUOM].Text = dt.Rows[i]["UOM"].ToString();


                    StandardWS = clsStaticInfo.dbl(dt.Rows[i]["StandardWorkStations"].ToString());
                    StandardHours = clsStaticInfo.dbl(dt.Rows[i]["StandardWorkingHours"].ToString());
                    StandardFixedCost = clsStaticInfo.dbl(dt.Rows[i]["DailyFixedCost"].ToString());
                    StandardAdditionalVCPerHour = clsStaticInfo.dbl(dt.Rows[i]["VariableCostPerHour"].ToString());

                    PlanHour = clsStaticInfo.dbl(dt.Rows[i]["WorkingHours"].ToString());
                    PlanWS = clsStaticInfo.dbl(dt.Rows[i]["NoOfWorkStation"].ToString());


                    WSCostPerHour = 0;
                    if (StandardWS > 0 && StandardHours > 0)
                        WSCostPerHour = StandardFixedCost / StandardWS;

                    TotalPlanCost = PlanWS * WSCostPerHour;
                    if (PlanHour > StandardHours)
                    {
                        if (StandardWS > 0 && PlanWS > 0)
                            TotalPlanCost += (StandardAdditionalVCPerHour / StandardWS * PlanWS) * (PlanHour - StandardHours);
                        //TotalPlanCost += (PlanHour - StandardHours) * StandardAdditionalVCPerHour;
                    }


                    if (bplib.clsWebLib.GetBoolData(dt.Rows[i]["isBuildUp"].ToString()) == true)
                        sheet[ROW, colIsBuildup].Text = "YES";


                    sheet[ROW, colActualLineCostForTheDay].Number = TotalPlanCost;

                    if (clsStaticInfo.dbl(dt.Rows[i]["CM"].ToString()) > 0)
                        sheet[ROW, colBreakEvenQty].Formula = string.Concat("IF(", CellAddr(colCM, ROW), ">0,", TotalPlanCost.ToString() + "/" + dt.Rows[i]["CM"].ToString(), ",0)");

                    string BU = CellAddr(colIsBuildup, ROW) + "=\"YES\"";
                    string A = CellAddr(colBreakEvenQty, ROW);
                    string B = CellAddr(colPlanTargetPerHourPR, ROW) + "*" + CellAddr(colWorkingHours, ROW);
                    string C = CellAddr(colPlannedQtyForTheDay, ROW);

                    sheet[ROW, colProductionProfitLossPcs].Formula = C + "-" + A;
                    string PL = CellAddr(colProductionProfitLossPcs, ROW);


                    sheet[ROW, colProductionProfitLossLineTargetPcs].Formula = "IF(" + PL + "<0,IF(" + B + "<" + A + ",IF(" + BU + ",(" + A + "-" + B + ")," + A + "-" + C + "),IF(" + BU + ",0," + PL + "*-1)),0)";
                    sheet[ROW, colProductionProfitLossBuildUpPcs].Formula = "IF(" + PL + "<0,IF(" + B + "<" + A + ",IF(" + BU + ",(" + B + "-" + C + "),0),IF(" + BU + "," + PL + "*-1,0)),0)";


                    sheet[ROW, colProductionProfitLossAmount].Formula = CellAddr(colProductionProfitLossPcs, ROW) + "*" + CellAddr(colCM, ROW);
                    sheet[ROW, colProductionProfitLossLineTargetAmount].Formula = CellAddr(colProductionProfitLossLineTargetPcs, ROW) + "*" + CellAddr(colCM, ROW);
                    sheet[ROW, colProductionProfitLossBuildUpAmount].Formula = CellAddr(colProductionProfitLossBuildUpPcs, ROW) + "*" + CellAddr(colCM, ROW);


                    sheet[ROW, colProfitLossProductionUOM].Text = dt.Rows[i]["UOM"].ToString();
                    sheet[ROW, colProfitLossProductionCurrency].Text = dt.Rows[i]["Currency"].ToString();


                    #endregion Actual Line Cost For the day

                    sheet[ROW, colEfficiencyPercentage].Formula = "(" + CellAddr(colSPT, ROW) + "*" + CellAddr(colPlannedQtyForTheDay, ROW) + ")/(" + CellAddr(colNoOfWorkStation, ROW) + "*" + CellAddr(colWorkingHours, ROW) + "*60)";
                    sheet[ROW, colBaseProcessCompletionBeforeFirstDeliveryDate].Formula = "(" + CellAddr(colFirstDeliveryDate, ROW) + "-" + CellAddr(colPlanningEndDateAtWC, ROW) + ")";

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                sheet.Range[startRow, colTargetDate, ROW, colTargetDate].NumberFormat = "dd-MMM-yyyy";

                sheet.Range[startRow, colPlanTargetPerHourPR, ROW, colPlanTargetPerHourPR].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[startRow, colCM, ROW, colCM].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet.Range[startRow, colActualLineCostForTheDay, ROW, colActualLineCostForTheDay].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[startRow, colBreakEvenQty, ROW, colBreakEvenQty].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[startRow, colProductionProfitLossPcs, ROW, colProductionProfitLossPcs].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[startRow, colProductionProfitLossLineTargetPcs, ROW, colProductionProfitLossLineTargetPcs].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[startRow, colProductionProfitLossBuildUpPcs, ROW, colProductionProfitLossBuildUpPcs].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[startRow, colEfficiencyPercentage, ROW, colEfficiencyPercentage].NumberFormat = "0.00%";





                sheet.Range[startRow, colProductionProfitLossAmount, ROW, colProductionProfitLossAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[startRow, colProductionProfitLossLineTargetAmount, ROW, colProductionProfitLossLineTargetAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[startRow, colProductionProfitLossBuildUpAmount, ROW, colProductionProfitLossBuildUpAmount].NumberFormat = clsStaticInfo.NumberFormat(2);


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
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "OS2", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;



                string fPath = fPath = HostingEnvironment.MapPath("~/") + "TempReport" + identity.UserId + ".xlsx";


                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }


                #region Buyer Summary
                workbook.Worksheets[0].Name = "OS2 -Workcenter wise order wise plan";

                IWorksheet pivotSheet = workbook.Worksheets[0];

                IPivotCache cache = workbook.PivotCaches.Add(sheet[startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);
                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colWorkCenterGroup - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colWorkCenter - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionPriority - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colbuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMasterOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderID - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderIds - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderDesc - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMainRawMaterialInhouseDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOtherRawMaterialInhouseDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colLSD - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colFirstDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPlanningEndDateAtWC - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOrderQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colOrderQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPlanOrderQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPlanOrderQty - 1].NumberFormat = clsStaticInfo.NumberFormat();

                pivotTable.Fields[colProductionQtyAtPR - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colProductionQtyAtPR - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colProductionQtyAtWC - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colProductionQtyAtWC - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colBalanceQtyAtWC - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colBalanceQtyAtWC - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colBalanceProductionQtyAtPR - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colBalanceProductionQtyAtPR - 1].NumberFormat = clsStaticInfo.NumberFormat();

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colWorkCenter - 1 || i == colPlant - 1 || i == colEntity - 1)
                        continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }


                pivotTable.Fields[colTargetMonth - 1].Axis = PivotAxisTypes.Column;
                pivotTable.Fields[colTargetMonth - 1].Name = "Month";
                pivotTable.Fields[colTargetMonth - 1].Subtotals = PivotSubtotalTypes.Sum;

                pivotTable.Fields[colTargetDate - 1].Axis = PivotAxisTypes.Column;
                pivotTable.Fields[colTargetDate - 1].Name = "Date";
                pivotTable.Fields[colTargetDate - 1].NumberFormat = "mm/dd";


                IPivotField field = pivotTable.Fields[colPlannedQtyForTheDay - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "", PivotSubtotalTypes.Sum);





                //int totalColumns = pivotTable2_1.RowFields.Count + pivotTable2_1.ColumnFields.Count;
                sheet = workbook.Worksheets[0];
                int StartFormattingColumn = pivotTable.RowFields.Count + 1;
                int endFormaatingColumn = StartFormattingColumn + pivotTable.ColumnFields[0].Items.Count + pivotTable.ColumnFields[1].Items.Count;


                pivotTable.ShowDrillIndicators = false;
                pivotTable.ShowDataFieldInRow = true;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Workcenter Wise Order Wise Day Wise Production Planning. Last Updated(" + Convert.ToDateTime(dt.Rows[0]["TargetDate"].ToString()).ToString("dd-MMM-yyyy") + ")", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion Buyer Summary

                #region OS2-2Order Wise planning
                sheet = workbook.Worksheets[1];
                sheet.Name = "OS2-Order Wise planning";

                pivotSheet = sheet;

                pivotTable = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A6"], cache);
                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colbuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMasterOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionPriority - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colFirstDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colLastDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderID - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderIds - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderDesc - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOrderQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colOrderQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPlanOrderQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPlanOrderQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colMainRawMaterialInhouseDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOtherRawMaterialInhouseDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colWorkCenter - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colWorkCenterGroup - 1].Axis = PivotAxisTypes.Row;

                //pivotTable.Fields[colLineStartDateAtWC - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPlanningStartDateAtWC - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPlanningEndDateAtWC - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colBaseProcessCompletionDate - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colProductionCompletionDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBaseProcessCompletionBeforeFirstDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionQtyAtWC - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colProductionQtyAtWC - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colBalanceQtyAtWC - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colBalanceQtyAtWC - 1].NumberFormat = clsStaticInfo.NumberFormat();

                //field = pivotTable.Fields[colProductionQtyAtWC - 1];
                //field.NumberFormat = clsStaticInfo.NumberFormat();
                //pivotTable.DataFields.Add(field, "Prod. Qty(WC)", PivotSubtotalTypes.None);

                //field = pivotTable.Fields[colBalanceQtyAtWC - 1];
                //field.NumberFormat = clsStaticInfo.NumberFormat();
                //pivotTable.DataFields.Add(field, "Balance Qty(WC)", PivotSubtotalTypes.None);

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colbuyer - 1 || i == colCustomer - 1 || i == colBuyerOrderNo - 1 || i == colPlant - 1 || i == colEntity - 1)
                        continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }




                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                //sheet[8, 1].RowHeight = 100;


                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Buyer / Order Wise Planning Status", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion Buyer Summary

                #region OS2-3Planning Performance
                sheet = workbook.Worksheets[2];
                sheet.Name = "OS2-3Planning Performance";

                pivotSheet = sheet;

                pivotTable = pivotSheet.PivotTables.Add("PivotTable3", pivotSheet["A6"], cache);
                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colTargetMonth - 1].Axis = PivotAxisTypes.Column;
                pivotTable.Fields[colTargetMonth - 1].Name = "Month";
                pivotTable.Fields[colTargetDate - 1].Axis = PivotAxisTypes.Column;
                pivotTable.Fields[colTargetDate - 1].Name = "Date";
                pivotTable.Fields[colTargetDate - 1].NumberFormat = "mm/dd";

                pivotTable.Fields[colWorkCenterGroup - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colWorkCenter - 1].Axis = PivotAxisTypes.Row;

                field = pivotTable.Fields[colWorkingHours - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Working Hours (Simulated)", PivotSubtotalTypes.Average);

                field = pivotTable.Fields[colNoOfWorkStation - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "No Of Workstations (PR)", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colEfficiencyPercentage - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(2, true);
                pivotTable.DataFields.Add(field, "Planned Efficency (%)", PivotSubtotalTypes.Average);

                field = pivotTable.Fields[colPlannedQtyForTheDay - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Work Center Plan Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colCM - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "CM", PivotSubtotalTypes.Average);

                field = pivotTable.Fields[colBookedCM - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Booked CM", PivotSubtotalTypes.Sum);


                field = pivotTable.Fields[colActualLineCostForTheDay - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Actual Workcenter Cost For The Day", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossAmount - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Production Profit/Loss Amount", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colBreakEvenQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Minimum Workcenter Target", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossPcs - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Production Profit/Loss Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossLineTargetPcs - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Loss Qty (Due To Workcenter Target)", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossBuildUpPcs - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Loss Qty (Due To Buildup)", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossLineTargetAmount - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Loss Amount (Due To Workcenter Target)", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossBuildUpAmount - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Loss Amount (Due To Buildup)", PivotSubtotalTypes.Sum);


                pivotTable.ShowDataFieldInRow = true;
                pivotTable.ShowDrillIndicators = false;
                //pivotTable.ShowColumnGrand = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                //sheet[8, 1].RowHeight = 100;


                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Poduction Planning Performance Report ( Efficency , CM Profit/ Loss, Production Profit / Loss)", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion Buyer Summary

                #region OS2-4 Performance Summary
                sheet = workbook.Worksheets[3];
                sheet.Name = "OS2-4 Performance Summary";

                pivotSheet = sheet;

                pivotTable = pivotSheet.PivotTables.Add("PivotTable4", pivotSheet["A6"], cache);

                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colTargetMonth - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colTargetMonth - 1].Name = "Month";

                pivotTable.Fields[colTargetDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colTargetDate - 1].Name = "Date";
                pivotTable.Fields[colTargetDate - 1].Subtotals = PivotSubtotalTypes.None;
                pivotTable.Fields[colTargetDate - 1].NumberFormat = "dd-MMM-yyyy";

                field = pivotTable.Fields[colWorkingHours - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Working Hours (Simulated)", PivotSubtotalTypes.Average);

                field = pivotTable.Fields[colNoOfWorkStation - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "No Of Workstations (PR)", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colEfficiencyPercentage - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(2, true);
                pivotTable.DataFields.Add(field, "Planned Efficency (%)", PivotSubtotalTypes.Average);


                field = pivotTable.Fields[colPlannedQtyForTheDay - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Work Center Plan Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colCM - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "CM", PivotSubtotalTypes.Average);

                field = pivotTable.Fields[colBookedCM - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Booked CM", PivotSubtotalTypes.Sum);


                field = pivotTable.Fields[colActualLineCostForTheDay - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Actual Workcenter Cost For The Day", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossAmount - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Production Profit/Loss Amount", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colBreakEvenQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Minimum Workcenter Target", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossPcs - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Production Profit/Loss Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossLineTargetPcs - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Loss Qty (Due To Workcenter Target)", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossBuildUpPcs - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Loss Qty (Due To Buildup)", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossLineTargetAmount - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Loss Amount (Due To Workcenter Target)", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionProfitLossBuildUpAmount - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Loss Amount (Due To Buildup)", PivotSubtotalTypes.Sum);


                //pivotTable.ShowDataFieldInRow = true;
                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                //sheet[8, 1].RowHeight = 100;


                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Poduction Planning Performance Report Summary", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion OS2-4 Performance Summary

                return workbook;



            }
            catch (Exception ex)
            {
                throw ex;

            }



        }
        public IWorkbook OS5xlsFile(ExcelEngine excelEngine, string entityid, string fromDate, string toDate, DataTable dt)
        {

            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {

                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");




                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (dt.Rows.Count == 0)
                    throw new Exception("No data found");


                DataTable dtDistinctProcesses = dt.DefaultView.ToTable(true, "ProcessName");

                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(dtDistinctProcesses.Rows.Count + 1);

                sheet = workbook.Worksheets[dtDistinctProcesses.Rows.Count];
                sheet.Name = "OS5 Data";

                int ROW = 6; int COL = 1;

                #region columns


                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 12;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Work Center Plan Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTargetDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Work Center Plan Month";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTargetMonth = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order ID";
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionOrderID = COL;

                COL++;
                sheet[ROW, COL].Text = "SPT (Minutes)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 14;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 14;
                int colbuyer = COL;

                COL++;
                sheet[ROW, COL].Text = "Style Group";
                sheet[ROW, COL].ColumnWidth = 10;
                int colStyleGroup = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Order No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBuyerOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Order No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOwnOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Line Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBuyerStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Line Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOwnStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Ids";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSalesOrderIds = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Desc";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSalesOrderDesc = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductCategory = COL;

                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 10;
                int colArticle = COL;


                COL++;
                sheet[ROW, COL].Text = "Production Order Priority";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionPriority = COL;

                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colOrderQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan SO Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanOrderQty = COL;

                COL++;
                sheet[ROW, COL].Text = "Production Qty (PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionQtyAtPR = COL;
                COL++;
                sheet[ROW, COL].Text = "Balance Production Qty (PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colBalanceProductionQtyAtPR = COL;
                COL++;
                sheet[ROW, COL].Text = "Unit";
                sheet[ROW, COL].ColumnWidth = 12;
                int colUnit = COL;


                COL++;
                sheet[ROW, COL].Text = "Commitment Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCommitmentDate = COL;
                COL++;
                sheet[ROW, COL].Text = "First Delivery Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colFirstDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Last Delivery Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLastDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Completion Date (Last Commitment Date)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionCompletionDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Base Process CompletionDate";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBaseProcessCompletionDate = COL;


                COL++;
                sheet[ROW, COL].Text = "Workcenter Start Date (Prod. For Current Entity)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLineStartDateAtPR = COL;

                COL++;
                sheet[ROW, COL].Text = "Planning Start Date (For Current Entity)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanningStartDateAtPR = COL;

                COL++;
                sheet[ROW, COL].Text = "Workcenter End Date (Prod. For Current Entity)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLineEndDateAtPR = COL;


                //COL++;
                //sheet[ROW, COL].Text = "Workcenter Target UOM";
                //sheet[ROW, COL].ColumnWidth = 10;
                //int colLineTargetUOM = COL;

                COL++;
                sheet[ROW, COL].Text = "Production Order Status";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionStatus = COL;

                COL++;
                sheet[ROW, COL].Text = "LSD";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLSD = COL;

                COL++;
                sheet[ROW, COL].Text = "Work Center Plan Qty(Simulated)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlannedQtyForTheDay = COL;

                COL++;
                sheet[ROW, COL].Text = "Process Sequence";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProcessSequence = COL;
                COL++;
                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProcessName = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Cycle Time";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int ColProcessCycleTime = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Total Time(In hour)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int ColTotalProcessTime = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColProcessDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Date Month";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int ColProcessDateMonth = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Date Year";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int ColProcessDateYear = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Date Week";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int ColProcessDateWeek = COL;

                #endregion columns

                int endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;




                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //text fields
                    sheet[ROW, colMaterial].Text = dt.Rows[i]["Material"].ToString();
                    sheet[ROW, colProduct].Text = dt.Rows[i]["Product"].ToString();
                    sheet[ROW, colArticle].Text = dt.Rows[i]["Article"].ToString();

                    //sheet[ROW, colAccountHolder].Text = dt.Rows[i]["AccountHolder"].ToString();
                    //sheet[ROW, colAccountIncharge].Text = dt.Rows[i]["AccountIncharge"].ToString();
                    sheet[ROW, colCustomer].Text = dt.Rows[i]["Customer"].ToString();
                    sheet[ROW, colbuyer].Text = dt.Rows[i]["buyer"].ToString();
                    sheet[ROW, colPlant].Text = dt.Rows[i]["Plant"].ToString();
                    sheet[ROW, colEntity].Text = dt.Rows[i]["Entity"].ToString();
                    sheet[ROW, colMasterOrderNo].Text = dt.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colProductCategory].Text = dt.Rows[i]["ProductCategory"].ToString();
                    sheet[ROW, colProductionOrderID].Text = dt.Rows[i]["ProductionOrderID"].ToString();
                    sheet[ROW, colProductionStatus].Text = dt.Rows[i]["ProductionStatus"].ToString();
                    sheet[ROW, colStyleGroup].Text = dt.Rows[i]["StyleGroup"].ToString();


                    sheet[ROW, colBuyerOrderNo].Text = dt.Rows[i]["BuyerRefNo"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = dt.Rows[i]["OwnRefNo"].ToString();
                    sheet[ROW, colBuyerStyleNo].Text = dt.Rows[i]["StyleNo"].ToString();
                    sheet[ROW, colOwnStyleNo].Text = dt.Rows[i]["OwnStyleNo"].ToString();

                    sheet[ROW, colSalesOrderIds].Text = dt.Rows[i]["SONo"].ToString().Replace(",", ", ");
                    sheet[ROW, colSalesOrderDesc].Text = dt.Rows[i]["SODesc"].ToString();


                    sheet[ROW, colUnit].Text = dt.Rows[i]["Unit"].ToString();

                    //number fields
                    sheet[ROW, colOrderQty].Number = clsStaticInfo.dbl(dt.Rows[i]["OrderQty"].ToString());
                    sheet[ROW, colPlannedQtyForTheDay].Number = clsStaticInfo.dbl(dt.Rows[i]["PlannedQtyForTheDay"].ToString());
                    sheet[ROW, colPlanOrderQty].Number = clsStaticInfo.dbl(dt.Rows[i]["PlanOrderQty"].ToString());
                    sheet[ROW, colProductionPriority].Number = clsStaticInfo.dbl(dt.Rows[i]["ProductionPriority"].ToString());
                    sheet[ROW, colProductionQtyAtPR].Number = clsStaticInfo.dbl(dt.Rows[i]["ProductionQtyAtPR"].ToString());
                    sheet[ROW, colSPT].Number = clsStaticInfo.dbl(dt.Rows[i]["SPT"].ToString());
                    sheet[ROW, colBalanceProductionQtyAtPR].Number = clsStaticInfo.dbl(dt.Rows[i]["PlanningQtyAtPR"].ToString());// - clsStaticInfo.dbl(dt.Rows[i]["ProductionQtyAtPR"].ToString());


                    //date fields
                    sheet[ROW, colBaseProcessCompletionDate].Text = GetDate(dt.Rows[i]["BaseProcessCompletionDate"].ToString());
                    sheet[ROW, colCommitmentDate].Text = GetDate(dt.Rows[i]["CommitmentDate"].ToString());
                    sheet[ROW, colFirstDeliveryDate].Text = GetDate(dt.Rows[i]["FirstDeliveryDate"].ToString());
                    sheet[ROW, colLastDeliveryDate].Text = GetDate(dt.Rows[i]["LastDeliveryDate"].ToString());
                    //sheet[ROW, colLineTargetUOM].Text = GetDate(dt.Rows[i]["UOM"].ToString());


                    sheet[ROW, colLineStartDateAtPR].Text = GetDate(dt.Rows[i]["LineStartDateAtPR"].ToString());
                    sheet[ROW, colLineEndDateAtPR].Text = GetDate(dt.Rows[i]["LineEndDateAtPR"].ToString());
                    sheet[ROW, colPlanningStartDateAtPR].Text = GetDate(dt.Rows[i]["PlanningStartDateAtPR"].ToString());


                    sheet[ROW, colProductionCompletionDate].Text = GetDate(dt.Rows[i]["ProductionCompletionDate"].ToString());
                    sheet[ROW, colTargetDate].DateTime = Convert.ToDateTime(GetDate(dt.Rows[i]["TargetDate"].ToString()));

                    sheet[ROW, colTargetMonth].Formula = "CONCATENATE(Month(" + CellAddr(colTargetDate, ROW) + "),\"/\",Year(" + CellAddr(colTargetDate, ROW) + "))";


                    sheet[ROW, colLSD].Text = GetDate(dt.Rows[i]["LSD"].ToString());


                    sheet[ROW, colProcessSequence].Number = clsStaticInfo.dbl(dt.Rows[i]["Sequence"].ToString());
                    sheet[ROW, colProcessName].Text = dt.Rows[i]["ProcessName"].ToString();
                    sheet[ROW, ColProcessCycleTime].Number = clsStaticInfo.dbl(dt.Rows[i]["ProductionCycleTime"].ToString());
                    sheet[ROW, ColTotalProcessTime].Formula = "(" + CellAddr(colPlannedQtyForTheDay, ROW) + "*" + CellAddr(ColProcessCycleTime, ROW) + ")/60";
                    sheet[ROW, ColProcessDate].Text = GetDate(dt.Rows[i]["ProcessDate"].ToString());


                    sheet[ROW, ColProcessDateMonth].Formula = "CONCATENATE(Month(" + CellAddr(ColProcessDate, ROW) + "),\"/\",Year(" + CellAddr(ColProcessDate, ROW) + "))";

                    sheet[ROW, ColProcessDateYear].Formula = "Year(" + CellAddr(ColProcessDate, ROW) + ")";
                    sheet[ROW, ColProcessDateWeek].Formula = "WeekNum(" + CellAddr(ColProcessDate, ROW) + ")";




                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                sheet.Range[startRow, colTargetDate, ROW, colTargetDate].NumberFormat = "dd-MMM-yyyy";
                sheet.Range[startRow, ColTotalProcessTime, ROW, ColTotalProcessTime].NumberFormat = clsStaticInfo.NumberFormat(2);



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
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "OS5 (From " + fromDate + " to " + toDate + ")", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;



                string fPath = fPath = HostingEnvironment.MapPath("~/") + "TempReport" + identity.UserId + ".xlsx";


                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }


                IPivotCache cache = workbook.PivotCaches.Add(sheet[startRow - 1, 1, ROW - 1, endCol]);

                for (int KK = 0; KK < dtDistinctProcesses.Rows.Count; KK++)
                {



                    #region Buyer Summary
                    workbook.Worksheets[KK].Name = dtDistinctProcesses.Rows[KK]["ProcessName"].ToString();

                    IWorksheet pivotSheet = workbook.Worksheets[KK];


                    IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable" + dtDistinctProcesses.Rows[KK]["ProcessName"].ToString(), pivotSheet["A6"], cache);
                    pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colbuyer - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colOwnOrderNo - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colMasterOrderNo - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colProductionOrderID - 1].Axis = PivotAxisTypes.Row;

                    pivotTable.Fields[colSalesOrderIds - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colSalesOrderDesc - 1].Axis = PivotAxisTypes.Row;

                    for (int i = 4; i < pivotTable.Fields.Count; i++)
                    {
                        if (i == colProduct - 1 || i == colbuyer - 1 || i == colCustomer - 1)
                            continue;

                        pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                    }


                    pivotTable.Fields[ColProcessDateMonth - 1].Axis = PivotAxisTypes.Column;
                    pivotTable.Fields[ColProcessDateMonth - 1].Name = "Month";
                    pivotTable.Fields[ColProcessDateMonth - 1].Subtotals = PivotSubtotalTypes.Sum;

                    pivotTable.Fields[ColProcessDateWeek - 1].Axis = PivotAxisTypes.Column;
                    pivotTable.Fields[ColProcessDateWeek - 1].Name = "Week No";


                    IPivotField field = pivotTable.Fields[colPlannedQtyForTheDay - 1];
                    field.NumberFormat = clsStaticInfo.NumberFormat();
                    pivotTable.DataFields.Add(field, "", PivotSubtotalTypes.Sum);




                    //int totalColumns = pivotTable2_1.RowFields.Count + pivotTable2_1.ColumnFields.Count;
                    IWorksheet sheetCurrent = workbook.Worksheets[KK];
                    int StartFormattingColumn = pivotTable.RowFields.Count + 1;
                    int endFormaatingColumn = StartFormattingColumn + pivotTable.ColumnFields[0].Items.Count + pivotTable.ColumnFields[1].Items.Count;


                    pivotTable.ShowDrillIndicators = false;
                    //// pivotTable.ShowDataFieldInRow = true;
                    pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable.Options.NullString = "";
                    pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                    reportUtility.CompanyPlantHeaderNew(ref sheetCurrent, 1, "OS-5 (" + dtDistinctProcesses.Rows[KK]["ProcessName"].ToString() + ")", identity.CompanyId, identity.CompanyName, "");

                    reportUtility.PageSetup(ref sheetCurrent, 6, ExcelPageOrientation.Landscape);
                    sheetCurrent[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheetCurrent.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    sheetCurrent.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    sheetCurrent.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheetCurrent.IsGridLinesVisible = false;


                    #endregion Buyer Summary
                    //Set field axis to page
                    pivotTable.Fields[colProcessName - 1].Axis = PivotAxisTypes.Page;


                    IPivotFilter CurrencyFilterValue = pivotTable.Fields[colProcessName - 1].PivotFilters.Add();
                    CurrencyFilterValue.Value1 = dtDistinctProcesses.Rows[KK]["ProcessName"].ToString();
                }

                return workbook;



            }
            catch (Exception ex)
            {
                throw ex;

            }



        }
        private string CellAddr(int Col, int Row)
        {
            return clsStaticInfo.GetxlsCol(Col) + Row.ToString();
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

        private void getOS5Query(string entityid, string fromDate, string toDate, out DataTable dtOS2)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            dtOS2 = new DataTable();
            try
            {
                string sql = @"select  trkp.UserName AS Plant,trke.UserName AS Entity,PO.EntityId,
                               BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where T1.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where T1.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                             StyleGroup=STUFF((select distinct ','+xmoi.ProductionGrouping from 
	                                                                             trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                   
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                            MasterOrderNo=STUFF((select distinct ','+XMO.MasterOrderNo from 
	                                                                                trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                               
			                               
                      FORMAT(p1.ProductionDate,'dd-MMM-yyyy') AS TargetDate,p1.ProductionDate AS ProcessDate,po.Id AS ProductionOrderID,
                            t1.SPT,t1.ProductionPriority,pm.UserName AS Product,
                             pc.UserName AS ProductCategory,mm.UserName AS Material,ord.Article,                     
                                                              
                           CEILING(t1.TargetPerDay) AS LineTargetPerDay,ord.OrderQty,ord.PlannedQty AS PlanOrderQty,prodpr.ProductionQtyAtPR,u.UserName AS Unit,
                            ord.FirstDeliveryDate,ord.LastDeliveryDate,ord.ProductionCompletionDate,
                            ISNULL(PRDTIME.LastDayOfProduction, ord.LastDeliveryDate) AS BaseProcessCompletionDate,
                            ps.UserName AS ProductionStatus,

                         t1.LSD,t1.CommitmentDate,
                            --ord.OrderStatus,

                            PRODPR.LineStartDateAtPR,PRODPR.LineEndDateAtPR, 
                            p2.PlanningStartDateAtPR,p2.PlanningEndDateAtPR, p2.PlanningQtyAtPR,
                             --targetdate,earlyStartDate,
                           t1.PlanWorkingHoursPerDay,
                            SUM(p1.Quantity) AS PlannedQtyForTheDay,
                            popc.Sequence, popc.Days, popc.ProductionCycleTime,
                            popc.IsBaseProcess, popc.Symbol,pros.UserName AS ProcessName
                            
                           
                            from trn.ProductionOrder PO
                            inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
                           INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID and ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
						   INNER JOIN trn.ProductionOrderProcessSet AS popc ON popc.ProductionOrderId=po.Id
						   INNER JOIN hkp.Process AS PROS ON PROS.Id=popc.ProcessId
							LEFT JOIN ProductionPlanningCalendar AS ppc ON ppc.EntityID=po.EntityId AND ppc.ProcessID=p1.ProcessID AND ppc.WorkingDate=p1.ProductionDate
						
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PO.EntityId
					        LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
                            
                            LEFT OUTER JOIN (SELECT MAX(pops.ProductionDate) AS LastDayOfProduction,pops.ProductionOrderId 
                              FROM ProductionPlanningType1 AS pops 
                            GROUP BY pops.ProductionOrderId) AS PRDTIME ON prdtime.ProductionOrderId=po.Id

                            left outer join mst.MaterialMaster mm on mm.id=p1.MaterialMasterId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
							

                            LEFT OUTER JOIN org.Unit AS u ON u.Id=trke.UnitId
                            left outer join org.Plant PLN on pln.Id=PO.PlantId
							LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId

                             --planning at PR Level
                            left outer join (
				                            SELECT P1.ProductionOrderID,P1.ProcessID,
				                            MIN(ProductionDate) AS PlanningStartDateAtPR,SUM(Quantity) AS PlanningQtyAtPR ,max(ProductionDate) AS PlanningEndDateAtPR 
				                            FROM ProductionPlanningType1 p1 
				                            group by  P1.ProductionOrderID,P1.ProcessID
                                            ) as p2 
				                            on p2.ProductionOrderID=t1.ProductionOrderID  
				                            and p2.ProcessID=(select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and  ProductionOrderID=p1.ProductionOrderID)
                            
                       
                          
                            --production at PR Level
                            LEFT OUTER JOIN (
				                           SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS LineStartDateAtPR,MAX(s.ProductionDate) AS LineEndDateAtPR
				                            FROM   trn.ProductionSummary S 
				                            GROUP BY  s.ProductionOrderId,s.ProcessId
                            ) AS PRODPR ON  PRODPR.ProductionOrderId=p1.ProductionOrderID AND p1.ProcessID=PRODPR.ProcessId


                            left outer join (
                            select POD.ProductionOrderId,ma.StandardName AS Article,
                            SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty,
                            min(so.DeliveryDate) AS FirstDeliveryDate,cur.Code AS Currency,uom.UserName AS UOM,
                            max(so.DeliveryDate) AS LastDeliveryDate,
                            MAX(so.CommitmentDate) AS ProductionCompletionDate,
                            SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate* so.Qty ELSE  so.Rate* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(so.Qty) AS FOB,
                            SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE  so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty) AS CM,
                            sum(SO.Qty) AS OrderQty from trn.ProductionOrderDetail POD 
                            left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                            left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                            left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
                            left JOIN org.Company AS com ON com.Id=mo.CompanyId
                            LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            left outer join [HKP].[Party] p on P.Id=MO.plantID
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            --left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
							LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId
							LEFT OUTER JOIN scs.Currency AS cur ON cur.Id=mo.CurrencyId
                            group by POD.ProductionOrderId,ma.StandardName,cur.Code,uom.UserName--,MO.MasterOrderNo,b.UserName,os.UserName
                            ) AS ORD on ord.ProductionOrderID=PO.Id

WHERE po.EntityId IN(" + entityid + @") AND convert(date, p1.ProductionDate) between convert(date,'" + fromDate + @"') AND convert(date,'" + toDate + @"')
                               
GROUP BY  trkp.UserName,trke.UserName,PO.EntityId, PRDTIME.LastDayOfProduction,T1.ProductionOrderId ,p1.ProductionDate,po.Id,t1.SPT,t1.ProductionPriority,pm.UserName,pc.UserName,mm.UserName,ord.Article,                     
                                                              
                          t1.TargetPerDay,ord.OrderQty,ord.PlannedQty,prodpr.ProductionQtyAtPR,u.UserName,
                            ord.FirstDeliveryDate,ord.LastDeliveryDate,ord.ProductionCompletionDate,ps.UserName,

                         t1.LSD,t1.CommitmentDate,
                            --ord.OrderStatus,

                            PRODPR.LineStartDateAtPR,PRODPR.LineEndDateAtPR, 
                            p2.PlanningStartDateAtPR,p2.PlanningEndDateAtPR, p2.PlanningQtyAtPR,
                             --targetdate,earlyStartDate,
                            t1.PlanWorkingHoursPerDay,
                             popc.Sequence, popc.Days, popc.ProductionCycleTime,
                            popc.IsBaseProcess, popc.Symbol,pros.UserName

ORDER BY  p1.ProductionDate,po.Id,popc.Sequence
                            
						
";





                dtOS2 = _sqlRepository.GetDataTable(sql);

                string SqlCalendar = @"SELECT PC.EntityId, format(pc.WorkingDate,'dd-MMM-yyyy') AS WorkingDate FROM ProductionPlanningCalendar AS pc WHERE pc.WorkingDate 
                                        BETWEEN '" + Convert.ToDateTime(fromDate).AddDays(-30).ToString("dd-MMM-yyyy")
                                        + @"' AND '" + Convert.ToDateTime(toDate).AddDays(30).ToString("dd-MMM-yyyy")
                                        + @"' AND ISNULL(pc.DayType,'')='' AND PC.EntityID IN(" + entityid + @") AND pc.ProcessID IN (SELECT BaseProcessId FROM PlanningTypes AS pt WHERE pt.PlanningType='" + ScreenPlanningType.ToString() + @"')
                                        ORDER BY CONVERT(DATE, pc.WorkingDate) ASC";
                DataTable dtCalendar = _sqlRepository.GetDataTable(SqlCalendar);


                Dictionary<string, List<DataRow>> dicData = new Dictionary<string, List<DataRow>>();
                string Id = "";
                List<DataRow> PrData = new List<DataRow>();

                double PositiveDayCount = 0;
                double NegativeDayCount = 0;
                System.DateTime dtPreviousProcessDate = DateTime.Now;
                System.DateTime BaseProcessDate = DateTime.Now;
                for (int i = 0; i < dtOS2.Rows.Count; i++)
                {
                    if (Id != dtOS2.Rows[i]["TargetDate"].ToString() + dtOS2.Rows[i]["ProductionOrderID"].ToString())
                    {
                        if (PrData.Count > 0)
                        {
                            for (int k = PrData.Count - 1; k >= 0; k--)
                            {
                                if (PrData[k]["Symbol"].ToString().Trim() == "-")
                                {
                                    NegativeDayCount = clsStaticInfo.dbl(PrData[k]["Days"].ToString());
                                    string dtTempDate = BaseProcessDate.AddDays(NegativeDayCount * -1).ToString("dd-MMM-yyyy");
                                    dtCalendar.DefaultView.RowFilter = "WorkingDate<=#" + dtTempDate + "# AND EntityId='" + dtOS2.Rows[i]["EntityId"].ToString() + "'";
                                    if (dtCalendar.DefaultView.Count > 0)
                                    {
                                        PrData[k]["ProcessDate"] = dtCalendar.DefaultView[dtCalendar.DefaultView.Count - 1]["WorkingDate"].ToString();
                                    }
                                    else
                                    {
                                        PrData[k]["ProcessDate"] = Convert.ToDateTime(PrData[k]["TargetDate"].ToString()).AddDays(NegativeDayCount * -1);
                                    }
                                    BaseProcessDate = Convert.ToDateTime(PrData[k]["ProcessDate"].ToString());
                                }
                            }
                        }

                        PrData = new List<DataRow>();
                        dicData.Add(dtOS2.Rows[i]["TargetDate"].ToString() + dtOS2.Rows[i]["ProductionOrderID"].ToString(), PrData);
                        PositiveDayCount = 0;
                        NegativeDayCount = 0;
                    }
                    if (clsWebLib.GetBoolData(dtOS2.Rows[i]["IsBaseProcess"]) == true)
                    {
                        dtPreviousProcessDate = Convert.ToDateTime(dtOS2.Rows[i]["TargetDate"].ToString());
                        BaseProcessDate = Convert.ToDateTime(dtOS2.Rows[i]["TargetDate"].ToString());
                    }

                    if (dtOS2.Rows[i]["Symbol"].ToString().Trim() == "+")
                    {
                        PositiveDayCount = clsStaticInfo.dbl(dtOS2.Rows[i]["Days"].ToString());

                        string dtTempDate = Convert.ToDateTime(dtPreviousProcessDate).AddDays(PositiveDayCount).ToString("dd-MMM-yyyy");
                        dtCalendar.DefaultView.RowFilter = "WorkingDate>=#" + dtTempDate + "# AND EntityId='" + dtOS2.Rows[i]["EntityId"].ToString() + "'";
                        if (dtCalendar.DefaultView.Count > 0)
                        {
                            dtOS2.Rows[i]["ProcessDate"] = dtCalendar.DefaultView[0]["WorkingDate"].ToString();
                        }
                        else
                        {
                            dtOS2.Rows[i]["ProcessDate"] = Convert.ToDateTime(dtOS2.Rows[i]["TargetDate"].ToString()).AddDays(PositiveDayCount);
                        }

                        dtPreviousProcessDate = Convert.ToDateTime(dtOS2.Rows[i]["ProcessDate"].ToString());
                    }


                    PrData.Add(dtOS2.Rows[i]);

                    Id = dtOS2.Rows[i]["TargetDate"].ToString() + dtOS2.Rows[i]["ProductionOrderID"].ToString();
                }
                if (PrData.Count > 0)
                {
                    for (int k = PrData.Count - 1; k >= 0; k--)
                    {
                        if (PrData[k]["Symbol"].ToString().Trim() == "-")
                        {
                            NegativeDayCount = clsStaticInfo.dbl(PrData[k]["Days"].ToString());
                            string dtTempDate = BaseProcessDate.AddDays(NegativeDayCount * -1).ToString("dd-MMM-yyyy");
                            dtCalendar.DefaultView.RowFilter = "WorkingDate<=#" + dtTempDate + "# AND EntityId='" + PrData[k]["EntityId"].ToString() + "'";
                            if (dtCalendar.DefaultView.Count > 0)
                            {
                                PrData[k]["ProcessDate"] = dtCalendar.DefaultView[dtCalendar.DefaultView.Count - 1]["WorkingDate"].ToString();
                            }
                            else
                            {
                                PrData[k]["ProcessDate"] = Convert.ToDateTime(PrData[k]["TargetDate"].ToString()).AddDays(NegativeDayCount * -1);
                            }

                            BaseProcessDate = Convert.ToDateTime(PrData[k]["ProcessDate"].ToString());
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw (ex);

            }

        }

        private void getOS2Query(string entityid, out DataTable dtOS2)
        {
            dtOS2 = new DataTable();
            try
            {
                string sql = @"select 
                        isnull(WC.GroupingData,'Internal') AS GroupingData,    WC.UserName as WorkCenter,p1.ProductionDate AS TargetDate,po.Id AS ProductionOrderID,
                             p1.Quantity AS PlannedQtyForTheDay,t1.SPT,t1.ProductionPriority,pm.UserName AS Product,
                             pc.UserName AS ProductCategory,mm.UserName AS Material,ord.Article,PO.Remarks,
                               BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where T1.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where T1.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                             StyleGroup=STUFF((select distinct ','+xmoi.ProductionGrouping from 
	                                                                             trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                   
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                            MasterOrderNo=STUFF((select distinct ','+XMO.MasterOrderNo from 
	                                                                                trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                                                               SKU1=STUFF((select distinct ','+cv.UserName FROM 
	                                                                                trn.FirstCharacteristics c
		                                                                                left outer join hkp.CharacteristicsValue cv on cv.Id=c.CharacteristicsValueId
		                                                                               left outer join TRN.SalesOrder AS so ON SO.Id=c.SalesOrderId
		                                                                               left outer join TRN.ProductionOrderDetail PD ON PD.SalesOrderId=SO.Id
			                                                                                where ord.ProductionOrderId=PD.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                  SKU2=STUFF((select distinct ','+cv.UserName FROM 
	                                                                                trn.SecondCharacteristics AS sc 
		                                                                                left outer join hkp.CharacteristicsValue cv on cv.Id=sc.CharacteristicsValueId
		                                                                               left outer join TRN.SalesOrder AS so ON SO.Id=sc.SalesOrderId
		                                                                               left outer join TRN.ProductionOrderDetail PD ON PD.SalesOrderId=SO.Id
			                                                                                where ord.ProductionOrderId=PD.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                                                
			                  SKU3=STUFF((select distinct ','+cv.UserName FROM 
	                                                                                trn.ThirdCharacteristics AS tc 
		                                                                                left outer join hkp.CharacteristicsValue cv on cv.Id=tc.CharacteristicsValueId
		                                                                               left outer join TRN.SalesOrder AS so ON SO.Id=tc.SalesOrderId
		                                                                               left outer join TRN.ProductionOrderDetail PD ON PD.SalesOrderId=SO.Id
			                                                                                where ord.ProductionOrderId=PD.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            ord.FOB,ord.CM,CEILING(t1.TargetPerDay) AS LineTargetPerDay,ord.OrderQty,ord.PlannedQty AS PlanOrderQty,prodWC.ProductionQtyAtWC,prodpr.ProductionQtyAtPR,u.UserName AS Unit,
                            ACCI.EmployeeName AS AccountIncharge,acch.EmployeeName AS AccountHolder,ord.FirstDeliveryDate,ord.LastDeliveryDate,ord.ProductionCompletionDate,
                            DATEADD(DD,ISNULL(PRDTIME.LastDayOfProduction,0)*-1, ord.LastDeliveryDate) AS BaseProcessCompletionDate,
                            ps.UserName AS ProductionStatus,ORD.Currency,ORD.UOM,E.UserName AS Entity,PLN.UserName AS Plant,

                            ord.CM*p1.Quantity AS BookedCM,t1.NoOfWorkStation,t1.LSD,t1.CommitmentDate,t1.MainRawMaterialInhouseDate,
                            t1.OtherRawMaterialInhouseDate,
                            --ord.OrderStatus,
                            PRODWC.LineStartDateAtWC,PRODWC.LineEndDateAtWC,
                            PRODPR.LineStartDateAtPR,PRODPR.LineEndDateAtPR, 
                            
                            p3.PlanningStartDateAtWC,p3.PlanningEndDateAtWC,p3.PlanningQtyAtWC,
                            p2.PlanningStartDateAtPR,p2.PlanningEndDateAtPR, p2.PlanningQtyAtPR,                         
                            
                            
                             
                             --targetdate,earlyStartDate,
                            p1.ProductionHours AS WorkingHours,t1.PlanWorkingHoursPerDay,
                            t1.TargetPerDay/t1.PlanWorkingHoursPerDay AS PlanTargetPerHour,
                            ppc.WorkingHours AS CalendarWorkingHours,p1.isBuildUp,
                            WC.StandardTimePerDay AS StandardWorkingHours, eff.StandardWorkingHourCost,eff.AdditionalWorkingHourCostPerHour,
                            wc.NoOfWorkStation AS StandardWorkStations,wc.DailyFixedCost,wc.VariableCost AS VariableCostPerHour
                            ,bul.Id BulletinId, bul.MachineSPT, bul.NonMachineSPT,
                            bul.MachineManpower, bul.NonMachineManpower,bul.TotalSPT,wcm.UserName FirstProcessWC,P.UserName FirstProcess
                            from trn.ProductionOrder PO
                            inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
                            INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID and ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)

							LEFT JOIN ProductionPlanningCalendar AS ppc ON ppc.EntityID=po.EntityId AND ppc.ProcessID=p1.ProcessID AND ppc.WorkingDate=p1.ProductionDate
						
                             left outer join scs.WorkCenterMaster WC on wc.id=p1.WorkCenterMasterId
                            LEFT OUTER JOIN EmployeeInformation AS ACCI ON ACCI.SystemId=wc.AccountInCharge
                            LEFT OUTER JOIN EmployeeInformation AS ACCH ON ACCH.SystemId=wc.AccountHolder

                            LEFT OUTER JOIN (SELECT MAX(days) AS LastDayOfProduction,pops.ProductionOrderId 
                              FROM trn.ProductionOrderProcessSet AS pops WHERE pops.Symbol='+'
                            GROUP BY pops.ProductionOrderId) AS PRDTIME ON prdtime.ProductionOrderId=po.Id

                            left outer join mst.MaterialMaster mm on mm.id=p1.MaterialMasterId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
							LEFT JOIN [TRN].[ProductMasterEfficency] EFF ON eff.ProductMasterId=PM.Id AND EFF.EfficencyName='Costing'  
							
							
                            left outer join org.Entity E  on e.Id=p1.EntityID
                            LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
                            left outer join org.Plant PLN on pln.Id=E.PlantId
							LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId

                        --Bulletin Data
						LEFT JOIN (	SELECT T.Id, T.ProductionOrderId,TM.ProcessId,SUM(D.TotalSPT) AS TotalSPT,
							SUM(CASE WHEN ISNULL(D.MachineVarientId,'')<>'' THEN D.TotalSPT ELSE 0 END) AS MachineSPT,
							SUM(CASE WHEN ISNULL(D.MachineVarientId,'')='' THEN D.TotalSPT ELSE 0 END) AS NonMachineSPT,
							SUM(CASE WHEN ISNULL(D.MachineVarientId,'')<>'' THEN D.AllotedManpower ELSE 0 END) AS MachineManpower,
							SUM(CASE WHEN ISNULL(D.MachineVarientId,'')='' THEN D.AllotedManpower ELSE 0 END) AS NonMachineManpower
							FROM trn.ProductionBulletinTemplate T
							JOIN trn.ProductionBulletinTemplateMaster AS  TM ON tm.ProductionBulletinTemplateId=t.Id
							JOIN trn.ProductionBulletinTemplateDetail AS D ON d.ProductionBulletinTemplateMasterId=TM.Id
						           	group by T.Id, T.ProductionOrderId,TM.ProcessId
						) BUL ON bul.ProductionOrderId=po.Id AND p1.ProcessID=BUL.ProcessId

                             --planning at PR Level
                            left outer join (
				                            SELECT P1.ProductionOrderID,P1.ProcessID,
				                            MIN(ProductionDate) AS PlanningStartDateAtPR,SUM(Quantity) AS PlanningQtyAtPR ,max(ProductionDate) AS PlanningEndDateAtPR 
				                            FROM ProductionPlanningType1 p1 
				                            group by  P1.ProductionOrderID,P1.ProcessID
                                            ) as p2 
				                            on p2.ProductionOrderID=t1.ProductionOrderID  
				                            and p2.ProcessID=(select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and  ProductionOrderID=p1.ProductionOrderID)
                            
                            --planning at WC Level
                            left outer join (
				                            SELECT P1.ProductionOrderID,P1.ProcessID,p1.WorkCenterMasterId,
				                             MIN(ProductionDate) AS PlanningStartDateAtWC,SUM(Quantity)AS PlanningQtyAtWC ,max(ProductionDate) AS PlanningEndDateAtWC FROM ProductionPlanningType1 p1
				                             group by P1.ProductionOrderID,P1.ProcessID,p1.WorkCenterMasterId
                                            ) as p3 
				                            on p1.WorkCenterMasterId=p3.WorkCenterMasterId
				                            AND p3.ProductionOrderID=t1.ProductionOrderID  
				                            and p3.ProcessID=(select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and  ProductionOrderID=p1.ProductionOrderID)

                             

                            --production at WC Level
                            LEFT OUTER JOIN (
				                                  SELECT s.EntityId,s.WorkCenterMasterId,s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtWC,MIN(s.ProductionDate) AS LineStartDateAtWC,MAX(s.ProductionDate) AS LineENDDateAtWC
				                            FROM   trn.ProductionSummary S
				                            GROUP BY  s.ProductionOrderId,s.WorkCenterMasterId,s.EntityId,s.ProcessId
                            ) AS PRODWC ON  p1.EntityID=PRODWC.EntityId AND PRODWC.WorkCenterMasterId=p1.WorkCenterMasterId AND p1.ProcessID=PRODWC.ProcessId AND PRODWC.ProductionOrderId=p1.ProductionOrderID

                            --production at PR Level
                            LEFT OUTER JOIN (
				                           SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS LineStartDateAtPR,MAX(s.ProductionDate) AS LineEndDateAtPR
				                            FROM   trn.ProductionSummary S 
				                            GROUP BY  s.ProductionOrderId,s.ProcessId
                            ) AS PRODPR ON  PRODPR.ProductionOrderId=p1.ProductionOrderID AND p1.ProcessID=PRODPR.ProcessId
                            LEFT JOIN [dbo].[ProductionOrderFirstProcessWorkCenter] FP ON FP.ProductionOrderId=p1.ProductionOrderID
							LEFT JOIN SCS.WorkCenterMaster wcm ON wcm.Id=FP.WorkCenterMasterId
							LEFT JOIN HKP.Process P ON P.Id=FP.ProcessId

                            left outer join (
                            select POD.ProductionOrderId,ma.StandardName AS Article,
                            SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty,
                            --SUM((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100))) AS PlannedQty,
                            min(so.DeliveryDate) AS FirstDeliveryDate,cur.Code AS Currency,uom.UserName AS UOM,
                            max(so.DeliveryDate) AS LastDeliveryDate,
                            MAX(so.CommitmentDate) AS ProductionCompletionDate,
                           -- SUM(so.Rate * isnull(RT.ExchangeRate,1) * so.Qty)/SUM(so.Qty) AS FOB,
                           -- SUM(so.CM * isnull(RT.ExchangeRate,1) * so.Qty)/SUM(so.Qty) AS CM,

SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate* so.Qty ELSE  so.Rate* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(so.Qty) AS FOB,
SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE  so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty) AS CM,
                            sum(SO.Qty) AS OrderQty from trn.ProductionOrderDetail POD 
                            left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                            left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                            left outer join MasterOrderExchangeRates RT on RT.TransactionId=MO.Id
	                        left JOIN org.Company AS com ON com.Id=mo.CompanyId
                            LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            left outer join [HKP].[Party] p on P.Id=MO.plantID
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            --left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
							LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId
							LEFT OUTER JOIN scs.Currency AS cur ON cur.Id=mo.CurrencyId
                            group by POD.ProductionOrderId,ma.StandardName,cur.Code,uom.UserName--,MO.MasterOrderNo,b.UserName,os.UserName
                            ) AS ORD on ord.ProductionOrderID=PO.Id

                                WHERE WC.EntityId IN (" + entityid + @")
                            order by wc.Code,p1.ProductionDate";

                dtOS2 = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);

            }

        }
        private void getOS2SnapshotQuery(string entityid, string masterid, out DataTable dtOS2)
        {
            dtOS2 = new DataTable();
            try
            {
                string sql = @"select 
                            WC.UserName as WorkCenter,p1.ProductionDate AS TargetDate,po.Id AS ProductionOrderID,
                             p1.Quantity AS PlannedQtyForTheDay,t1.SPT,t1.ProductionPriority,
                              BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=T1.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where T1.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where T1.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                             StyleGroup=STUFF((select distinct ','+xmoi.ProductionGrouping from 
	                                                                             trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                   
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                            MasterOrderNo=STUFF((select distinct ','+XMO.MasterOrderNo from 
	                                                                                trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                                                              
                            ord.FOB,ord.CM,CEILING(t1.TargetPerDay) AS LineTargetPerDay,ord.OrderQty,ord.PlannedQty AS PlanOrderQty,prodWC.ProductionQtyAtWC,prodpr.ProductionQtyAtPR,u.UserName AS Unit,e.UserName AS Entity,
                            ACCI.EmployeeName AS AccountIncharge,acch.EmployeeName AS AccountHolder,ord.FirstDeliveryDate,ord.LastDeliveryDate,ord.ProductionCompletionDate,
                            DATEADD(DD,ISNULL(PRDTIME.LastDayOfProduction,0)*-1, ord.LastDeliveryDate) AS BaseProcessCompletionDate,
                            ps.UserName AS ProductionStatus,

                            ord.CM*p1.Quantity AS BookedCM,t1.NoOfWorkStation,t1.LSD,t1.CommitmentDate,t1.MainRawMaterialInhouseDate,
                            t1.OtherRawMaterialInhouseDate,
                            --ord.OrderStatus,
                            PRODWC.LineStartDateAtWC,PRODWC.LineEndDateAtWC,
                            PRODPR.LineStartDateAtPR,PRODPR.LineEndDateAtPR, 
                            
                            p3.PlanningStartDateAtWC,p3.PlanningEndDateAtWC,
                            p2.PlanningStartDateAtPR,p2.PlanningEndDateAtPR, 
                           
                            
                            
                            --targetdate,earlyStartDate,
                            p1.ProductionHours AS WorkingHours,t1.PlanWorkingHoursPerDay,
                            convert(INT,(ceiling(t1.TargetPerDay/t1.PlanWorkingHoursPerDay))) AS PlanTargetPerHour,
                            ppc.WorkingHours AS CalendarWorkingHours,
                            eff.StandardWorkingHours, eff.StandardWorkingHourCost,wc.DailyFixedCost,wc.VariableCost AS VariableCostPerHour
                            from trn.ProductionOrder PO
                            inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
                            INNER join ProductionPlanningSnapshotType1 p1 on p1.ProductionPlanningSnapshotMasterType1='" + masterid + @"' AND p1.ProductionOrderID=t1.ProductionOrderID and ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)


                             left outer join scs.WorkCenterMaster WC on wc.id=p1.WorkCenterMasterId
                            LEFT OUTER JOIN EmployeeInformation AS ACCI ON ACCI.SystemId=wc.AccountInCharge
                            LEFT OUTER JOIN EmployeeInformation AS ACCH ON ACCH.SystemId=wc.AccountHolder

                            LEFT OUTER JOIN (SELECT MAX(days) AS LastDayOfProduction,pops.ProductionOrderId 
                              FROM trn.ProductionOrderProcessSet AS pops WHERE pops.Symbol='+'
                            GROUP BY pops.ProductionOrderId) AS PRDTIME ON prdtime.ProductionOrderId=po.Id

                            left outer join mst.MaterialMaster mm on mm.id=p1.MaterialMasterId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                            left outer join org.Entity E  on e.Id=p1.EntityID
                            LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
                            left outer join org.Plant PLN on pln.Id=PO.PlantId
							LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId

                            --planning at PR Level
                            left outer join (
				                            SELECT P1.ProductionOrderID,P1.ProcessID,p1.WorkCenterMasterId,
				                            MIN(ProductionDate) AS PlanningStartDateAtPR ,max(ProductionDate) AS PlanningEndDateAtPR 
				                            FROM ProductionPlanningSnapshotType1 p1 where p1.ProductionPlanningSnapshotMasterType1='" + masterid + @"' group by  P1.ProductionOrderID,P1.ProcessID,p1.WorkCenterMasterId) as p2 
				                            on p2.ProductionOrderID=t1.ProductionOrderID  and p1.WorkCenterMasterId=p2.WorkCenterMasterId
				                            and p2.ProcessID=(select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and  ProductionOrderID=p1.ProductionOrderID)
                            --planning at WC Level
                            left outer join (
				                            SELECT P1.ProcessID,p1.WorkCenterMasterId,
				                             MIN(ProductionDate) AS PlanningStartDateAtWC ,max(ProductionDate) AS PlanningEndDateAtWC 
				                            FROM ProductionPlanningSnapshotType1 p1 where p1.ProductionPlanningSnapshotMasterType1='" + masterid + @"' group by P1.ProcessID,p1.WorkCenterMasterId) as p3 
				                            on p1.WorkCenterMasterId=p3.WorkCenterMasterId
				                            and p3.ProcessID=(select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and  ProductionOrderID=p1.ProductionOrderID)

                             --production at WC Level
                            LEFT OUTER JOIN (
				                                  SELECT s.EntityId,s.WorkCenterMasterId,s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtWC,MIN(s.ProductionDate) AS LineStartDateAtWC,MAX(s.ProductionDate) AS LineENDDateAtWC
				                            FROM   trn.ProductionSummary S
				                            GROUP BY  s.ProductionOrderId,s.WorkCenterMasterId,s.EntityId,s.ProcessId
                            ) AS PRODWC ON  p1.EntityID=PRODWC.EntityId AND PRODWC.WorkCenterMasterId=p1.WorkCenterMasterId AND p1.ProcessID=PRODWC.ProcessId AND PRODWC.ProductionOrderId=p1.ProductionOrderID

                            --production at PR Level
                            LEFT OUTER JOIN (
				                           SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS LineStartDateAtPR,MAX(s.ProductionDate) AS LineEndDateAtPR
				                            FROM   trn.ProductionSummary S 
				                            GROUP BY  s.ProductionOrderId,s.ProcessId
                            ) AS PRODPR ON  PRODPR.ProductionOrderId=p1.ProductionOrderID AND p1.ProcessID=PRODPR.ProcessId


                            left outer join (
                            select POD.ProductionOrderId,
                            SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty,
                           
                            --SUM((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100))) AS PlannedQty,
                            min(so.DeliveryDate) AS FirstDeliveryDate,
                            max(so.DeliveryDate) AS LastDeliveryDate,
                            MAX(so.CommitmentDate) AS ProductionCompletionDate,
                            AVG(c.FOB) AS FOB,AVG(c.CM) AS CM,
                            sum(SO.Qty) AS OrderQty from trn.ProductionOrderDetail POD 
                            left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                            left outer join [HKP].[Party] p on P.Id=MO.plantID
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            --left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
                            group by POD.ProductionOrderId--,MO.MasterOrderNo,b.UserName,os.UserName
                            ) AS ORD on ord.ProductionOrderID=PO.Id

                                WHERE WC.EntityId='" + entityid + @"'
                            order by wc.Code,p1.ProductionDate";





                dtOS2 = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);

            }

        }


        #endregion OS2

        #region MASTER ORDER based on base process
        //master order and os3 based on base process, therefore no need to select the process from the screen
        [HttpGet, Authorize]
        public ActionResult OS3xls(string entityid)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");


                Dictionary<string, List<DataRow>> dicProductionQtyDistribution;
                DataTable dt, dtOrderMaster;
                getSalesOrderDistribution(System.DateTime.Now.ToString("dd-MMM-yyyy"), entityid, out dicProductionQtyDistribution, out dt);

                getOrderMaster(entityid, out dtOrderMaster);


                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(5);
                workbook.Worksheets[3].Name = "OS3 Data";
                sheet = workbook.Worksheets[3];


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
                sheet[ROW, COL].Text = "Buyer Order No";
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
                sheet[ROW, COL].Text = "Buyer PO No";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPONo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer PO Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPODate = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Category";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderCategory = COL;    //                        

                COL++;
                sheet[ROW, COL].Text = "Order Status";
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
                COL++;
                sheet[ROW, COL].Text = "Produced Qty";
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

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(150, 250, 150);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                string ExpectedProductionStartDate = "";
                double PRCumulativePlanQty = 0;
                string PRId = "";
                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                {
                    if (dtOrderMaster.Rows[i]["ProductionOrderId"].ToString() == "2087")
                    {

                    }
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

                    sheet[ROW, colOwnItem].Text = dtOrderMaster.Rows[i]["OwnReferenceNo"].ToString();
                    sheet[ROW, colBuyerItem].Text = dtOrderMaster.Rows[i]["BuyerReferenceNo"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = dtOrderMaster.Rows[i]["OwnOrderNo"].ToString();
                    sheet[ROW, colBuyerOrderNo].Text = dtOrderMaster.Rows[i]["BuyerOrderNo"].ToString();
                    sheet[ROW, colSalesOrderDesc].Text = dtOrderMaster.Rows[i]["SODesc"].ToString();

                    sheet[ROW, colMaterialRowId].Text = dtOrderMaster.Rows[i]["MaterialRowId"].ToString();

                    sheet[ROW, colCustomer].Text = dtOrderMaster.Rows[i]["Customer"].ToString();
                    sheet[ROW, colBuyer].Text = dtOrderMaster.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colCommitmentDate].Text = GetDate(dtOrderMaster.Rows[i]["CommitmentDate"].ToString());
                    sheet[ROW, colDeliveryDate].Text = GetDate(dtOrderMaster.Rows[i]["DeliveryDate"].ToString());
                    sheet[ROW, colMasterOrderNo].Text = dtOrderMaster.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colMaterial].Text = dtOrderMaster.Rows[i]["Material"].ToString();
                    sheet[ROW, colOrderCategory].Text = dtOrderMaster.Rows[i]["OrderCategory"].ToString();
                    sheet[ROW, colOrderStatus].Text = dtOrderMaster.Rows[i]["OrderStatus"].ToString();
                    sheet[ROW, colProductionOrderId].Text = dtOrderMaster.Rows[i]["ProductionOrderId"].ToString();
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

                IWorksheet sheet2 = workbook.Worksheets[4];
                sheet2.Name = "OS-W";
                sheet2.ImportDataTable(dt, true, 1, 1);
                int lc = sheet.UsedRange.LastColumn;
                sheet2.Range[1, 1, 1, lc].ColumnWidth = 14;


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
                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMasterOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPRQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPRQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPRPlannedQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPRPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colCummPlannedQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colCummPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPONo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderDesc - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOrderCategory - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOrderStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colproductionStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCommitmentDate - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colExpectedCompletionDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEarlyBy - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colEarlyBy - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colLateBy - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colLateBy - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colDeliveryMonth - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colDeliveryMonth - 1].NumberFormat = clsStaticInfo.NumberFormat();

                pivotTable.Fields[colProductionCompletionMonth - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colCummPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colBuyerOrderNo - 1 || i == colPlant - 1 || i == colEntity - 1)
                        continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }


                //pivotTable.Fields[colSOQty - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colSOQty - 1].Name = "SO Qty";
                //pivotTable.Fields[colSOQty - 1].Subtotals = PivotSubtotalTypes.Sum;
                //pivotTable.Fields[colSOQty - 1].NumberFormat = clsStaticInfo.NumberFormat(0);

                //pivotTable.Fields[colPlannedQty - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colPlannedQty - 1].Name = "Plan Qty";
                //pivotTable.Fields[colPlannedQty - 1].Subtotals = PivotSubtotalTypes.Sum;
                //pivotTable.Fields[colPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat(0);

                //pivotTable.Fields[colAvailableProducedQty - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colAvailableProducedQty - 1].Name = "Available Produced Qty";
                //pivotTable.Fields[colAvailableProducedQty - 1].Subtotals = PivotSubtotalTypes.Sum;
                //pivotTable.Fields[colAvailableProducedQty - 1].NumberFormat = clsStaticInfo.NumberFormat(0);

                IPivotField field = pivotTable.Fields[colSOQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "SO Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colPlannedQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Plan Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colAvailableProducedQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Available Produced Qty", PivotSubtotalTypes.Sum);




                //int totalColumns = pivotTable2_1.RowFields.Count + pivotTable2_1.ColumnFields.Count;
                sheet = workbook.Worksheets[0];
                //int StartFormattingColumn = pivotTable.RowFields.Count + 1;
                //int endFormaatingColumn = StartFormattingColumn + pivotTable.ColumnFields[0].Items.Count + pivotTable.ColumnFields[1].Items.Count;


                pivotTable.ShowDrillIndicators = false;
                //pivotTable.ShowDataFieldInRow = true;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Buyer / Order Wise Planning Status", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion Buyer Summary

                #region OS3- R2
                workbook.Worksheets[1].Name = "OS3- R2";
                sheet = workbook.Worksheets[1];

                pivotSheet = workbook.Worksheets[1];
                pivotTable = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A6"], cache);


                //pivotTable.Fields[colEarlyBy - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colEarlyBy - 1].NumberFormat = clsStaticInfo.NumberFormat();
                //pivotTable.Fields[colLateBy - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colLateBy - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colDeliveryMonth - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colDeliveryMonth - 1].NumberFormat = clsStaticInfo.NumberFormat();

                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMasterOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPONo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderDesc - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOrderCategory - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOrderStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colproductionStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCommitmentDate - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colExpectedCompletionDate - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colProductionCompletionMonth - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colCummPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colDeliveryMonth - 1 || i == colPlant - 1 || i == colEntity - 1)
                        continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }


                //pivotTable.Fields[colSOQty - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colSOQty - 1].Name = "SO Qty";
                //pivotTable.Fields[colSOQty - 1].Subtotals = PivotSubtotalTypes.Sum;
                //pivotTable.Fields[colSOQty - 1].NumberFormat = clsStaticInfo.NumberFormat(0);

                //pivotTable.Fields[colPlannedQty - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colPlannedQty - 1].Name = "Plan Qty";
                //pivotTable.Fields[colPlannedQty - 1].Subtotals = PivotSubtotalTypes.Sum;
                //pivotTable.Fields[colPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat(0);

                //pivotTable.Fields[colAvailableProducedQty - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colAvailableProducedQty - 1].Name = "Available Produced Qty";
                //pivotTable.Fields[colAvailableProducedQty - 1].Subtotals = PivotSubtotalTypes.Sum;
                //pivotTable.Fields[colAvailableProducedQty - 1].NumberFormat = clsStaticInfo.NumberFormat(0);

                pivotTable.Fields[colPRQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPRQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPRPlannedQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPRPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colCummPlannedQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colCummPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colSOQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPRQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPlannedQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPRPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colAvailableProducedQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colCummPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();

                field = pivotTable.Fields[colPRQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "PR Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colPRPlannedQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "PR Plan Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colCummPlannedQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Cumm. Plan Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colSOQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "SO Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colPlannedQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Plan Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colAvailableProducedQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Available Produced Qty", PivotSubtotalTypes.Sum);


                field = pivotTable.Fields[colAvailablePlanQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Available Plan Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colTotalAvailableQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Total Available Qty", PivotSubtotalTypes.Sum);

                //int totalColumns = pivotTable2_1.RowFields.Count + pivotTable2_1.ColumnFields.Count;
                //int StartFormattingColumn = pivotTable.RowFields.Count + 1;
                //int endFormaatingColumn = StartFormattingColumn + pivotTable.ColumnFields[0].Items.Count + pivotTable.ColumnFields[1].Items.Count;


                pivotTable.ShowDrillIndicators = false;
                //pivotTable.ShowDataFieldInRow = true;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Production Completion Month Wise - Order Planning Status", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion Buyer Summary


                #region OS3- R2
                workbook.Worksheets[2].Name = "OS3- R3";
                sheet = workbook.Worksheets[2];

                pivotSheet = workbook.Worksheets[2];
                pivotTable = pivotSheet.PivotTables.Add("PivotTable3", pivotSheet["A6"], cache);

                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colEarlyBy - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colEarlyBy - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colLateBy - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colLateBy - 1].NumberFormat = clsStaticInfo.NumberFormat();

                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMasterOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPRQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPRQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPRPlannedQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colPRPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colCummPlannedQty - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colCummPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.Fields[colPONo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderDesc - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOrderCategory - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOrderStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colproductionStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCommitmentDate - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colExpectedCompletionDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDeliveryMonth - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colDeliveryMonth - 1].NumberFormat = clsStaticInfo.NumberFormat();

                pivotTable.Fields[colProductionCompletionMonth - 1].Axis = PivotAxisTypes.Row; pivotTable.Fields[colCummPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat();

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    //if (i == colBuyerOrderNo - 1)
                    //    continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }


                //pivotTable.Fields[colSOQty - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colSOQty - 1].Name = "SO Qty";
                //pivotTable.Fields[colSOQty - 1].Subtotals = PivotSubtotalTypes.Sum;
                //pivotTable.Fields[colSOQty - 1].NumberFormat = clsStaticInfo.NumberFormat(0);

                //pivotTable.Fields[colPlannedQty - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colPlannedQty - 1].Name = "Plan Qty";
                //pivotTable.Fields[colPlannedQty - 1].Subtotals = PivotSubtotalTypes.Sum;
                //pivotTable.Fields[colPlannedQty - 1].NumberFormat = clsStaticInfo.NumberFormat(0);

                //pivotTable.Fields[colAvailableProducedQty - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colAvailableProducedQty - 1].Name = "Available Produced Qty";
                //pivotTable.Fields[colAvailableProducedQty - 1].Subtotals = PivotSubtotalTypes.Sum;
                //pivotTable.Fields[colAvailableProducedQty - 1].NumberFormat = clsStaticInfo.NumberFormat(0);

                field = pivotTable.Fields[colSOQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "SO Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colPlannedQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Plan Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colAvailableProducedQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Available Produced Qty", PivotSubtotalTypes.Sum);




                //int totalColumns = pivotTable2_1.RowFields.Count + pivotTable2_1.ColumnFields.Count;
                //int StartFormattingColumn = pivotTable.RowFields.Count + 1;
                //int endFormaatingColumn = StartFormattingColumn + pivotTable.ColumnFields[0].Items.Count + pivotTable.ColumnFields[1].Items.Count;


                pivotTable.ShowDrillIndicators = false;
                //pivotTable.ShowDataFieldInRow = true;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Early / Late Base Process  Wise  -  Order  Planning Status", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion Buyer Summary



                string strFileName = "OS3.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                excelEngine.Dispose();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
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

        [HttpGet, Authorize]
        public ActionResult OS4xls(string entityid, string fromDate, string toDate)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");

                if (string.IsNullOrEmpty(fromDate))
                    throw new Exception("Select from date");

                if (string.IsNullOrEmpty(toDate))
                    throw new Exception("Select to date");

                if (bplib.clsWebLib.IsDateOK(fromDate) == false || fromDate == "undefined")
                    throw new Exception("Select from date");

                if (bplib.clsWebLib.IsDateOK(toDate) == false || fromDate == "undefined")
                    throw new Exception("Select to date");

                if (Convert.ToDateTime(fromDate) > Convert.ToDateTime(toDate))
                    throw new Exception("To date cannot be earlier than from date");


                if (Math.Abs(clsStaticInfo.dateDiff(fromDate, toDate)) > 180)
                    throw new Exception("Cannot set date range greater than six months");

                DataTable dtOrderMaster;
                Dictionary<string, List<DataRow>> dicActualData = null;
                getOS4_PlanData(entityid, fromDate, toDate, out dtOrderMaster);
                getOS4_ActualData(entityid, fromDate, toDate, out dicActualData);


                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[1].Name = "OS4 Data";
                sheet = workbook.Worksheets[1];


                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Report Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int colReportDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Snapshot Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSnapshotDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlanDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Work Center";
                sheet[ROW, COL].ColumnWidth = 16;
                int colWorkCenter = COL;
                COL++;
                int colstart = COL;
                sheet[ROW, COL].Text = "Prod. Order No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrderID = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colbuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Orde No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyerOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Order No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOwnOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Item No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Item No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOwnStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Ids";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderIds = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Desc";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderDesc = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 16;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 16;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSAM = COL;
                COL++;
                sheet[ROW, COL].Text = "Work Station";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWorkStation = COL;


                COL++; colstart = COL;
                sheet[ROW, COL].Text = "Plan Hours";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanHours = COL;
                COL++;
                sheet[ROW, COL].Text = "Actual Work Hours";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActualWorkHours = COL;
                COL++;
                //sheet.Range[ROW - 1, colstart, ROW - 1, COL].Merge();
                //sheet.Range[ROW - 1, colstart, ROW - 1, COL].Text = "Plan Data";
                //sheet.Range[ROW - 1, colstart, ROW - 1, COL].CellStyle.Font.Bold = true;
                //sheet.Range[ROW - 1, colstart, ROW - 1, COL].CellStyle.Font.Size = 10f;
                //sheet.Range[ROW - 1, colstart, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[ROW - 1, colstart, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
                //sheet.Range[ROW - 1, colstart, ROW - 1, COL].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(150, 150, 250);
                //sheet.Range[ROW - 1, colstart, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;



                sheet[ROW, COL].Text = "Plan Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlanQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Actual Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualQty = COL;
                COL++; colstart = COL;
                sheet[ROW, COL].Text = "Diff In Prod. Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDifferenceInProduction = COL;

                COL++;
                sheet[ROW, COL].Text = "Plan CM";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlanCM = COL;
                COL++;
                sheet[ROW, COL].Text = "Actual CM";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualCM = COL;
                COL++; colstart = COL;
                sheet[ROW, COL].Text = "Diff In CM";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDifferenceInCM = COL;

                COL++;
                sheet[ROW, COL].Text = "Plan Minutes";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlanMinutes = COL;
                COL++;
                sheet[ROW, COL].Text = "Actual Minutes";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualMinutes = COL;
                COL++; colstart = COL;
                sheet[ROW, COL].Text = "Diff In Minutes";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDifferenceInhours = COL;

                COL++;
                sheet[ROW, COL].Text = "Plan Eff.";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlanEff = COL;
                COL++;
                sheet[ROW, COL].Text = "Actual Eff.";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualEff = COL;
                COL++; colstart = COL;
                sheet[ROW, COL].Text = "Diff In Eff.";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDifferenceInEff = COL;



                #endregion columns


                #region cols
                COL++;
                sheet[ROW, COL].Text = "Workcenter Std Working hour(Workcenter)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colWorkcenterStdWorkinghour = COL;
                COL++;
                sheet[ROW, COL].Text = "No Of Work Station(Workcenter)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colNoOfWorkStationWorkcenter = COL;

                COL++;
                sheet[ROW, COL].Text = "WC Fixed Cost(Workcenter)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colWCStdHourCostWorkCenter = COL;
                COL++;
                sheet[ROW, COL].Text = "Variable Cost per hour(Workcenter)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colVariableCostperhour = COL;
                COL++;
                sheet[ROW, COL].Text = "Actual Workcenter Cost For The Day";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colActualLineCostForTheDay = COL;


                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colCM = COL;

                COL++;
                sheet[ROW, COL].Text = "Workcenter Target Per Day (PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colLineTargetPerDay = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Target Per Hour(PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanTargetPerHourPR = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Plan Hours(PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionPlanHoursPR = COL;

                //COL++;
                //sheet[ROW, COL].Text = "Working Hours(Simulated)";
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet[ROW, COL].ColumnWidth = 10;
                //int colWorkingHours = COL;
                COL++;
                sheet[ROW, COL].Text = "Build Up";
                sheet[ROW, COL].ColumnWidth = 6;
                int colIsBuildup = COL;
                COL++;
                sheet[ROW, COL].Text = "Minimum Workcenter Target (Based on the Workcenter cost)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colBreakEvenQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Profit/Loss";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossPcs = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Loss Due To Workcenter Target";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossLineTargetPcs = COL;

                COL++;
                sheet[ROW, COL].Text = "Production Loss Due To Buildup";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossBuildUpPcs = COL;

                COL++;
                sheet[ROW, COL].Text = "Production Profit/Loss Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossAmount = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Loss Due To Workcenter Target Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossLineTargetAmount = COL;

                COL++;
                sheet[ROW, COL].Text = "Production Loss Due To Buildup Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionProfitLossBuildUpAmount = COL;


                #endregion

                int endCol = COL;

                //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(150, 250, 150);
                //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                //sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                Dictionary<string, DateTime> allDates = new Dictionary<string, DateTime>();
                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                    if (allDates.ContainsKey(dtOrderMaster.Rows[i]["PlanDate"].ToString() + dtOrderMaster.Rows[i]["WorkCenterMasterId"].ToString()) == false)
                        allDates.Add(dtOrderMaster.Rows[i]["PlanDate"].ToString() + dtOrderMaster.Rows[i]["WorkCenterMasterId"].ToString(), Convert.ToDateTime(dtOrderMaster.Rows[i]["PlanDate"].ToString()));

                foreach (KeyValuePair<string, List<DataRow>> item in dicActualData)
                    for (int i = 0; i < item.Value.Count; i++)
                        if (allDates.ContainsKey(item.Value[i]["ActualDate"].ToString() + item.Value[i]["WorkCenterMasterId"].ToString()) == false)
                            allDates.Add(item.Value[i]["ActualDate"].ToString() + item.Value[i]["WorkCenterMasterId"].ToString(), Convert.ToDateTime(item.Value[i]["ActualDate"].ToString()));


                allDates.OrderBy(ee => ee.Value);


                ROW++;

                int startRow = ROW;
                double StandardWS, StandardHours, StandardFixedCost, StandardAdditionalVCPerHour, PlanHour, PlanWS, WSCostPerHour, TotalPlanCost;
                foreach (KeyValuePair<string, DateTime> item in allDates)
                {
                    dtOrderMaster.DefaultView.RowFilter = "(PlanDate + WorkCenterMasterId)='" + item.Key + "'";

                    if (dtOrderMaster.DefaultView.Count > 0)
                    {
                        for (int i = 0; i < dtOrderMaster.DefaultView.Count; i++)
                        {
                            //plan exissts
                            SetDate(sheet[ROW, colPlanDate], dtOrderMaster.DefaultView[i]["PlanDate"].ToString());
                            sheet[ROW, colPlant].Text = dtOrderMaster.DefaultView[i]["Plant"].ToString();
                            sheet[ROW, colEntity].Text = dtOrderMaster.DefaultView[i]["Entity"].ToString();
                            sheet[ROW, colWorkCenter].Text = dtOrderMaster.DefaultView[i]["WorkCenter"].ToString();
                            sheet[ROW, colReportDate].Text = System.DateTime.Now.ToString("dd-MMM-yyyy");
                            sheet[ROW, colSnapshotDate].Text = dtOrderMaster.DefaultView[i]["SnapshotDate"].ToString();

                            sheet[ROW, colProductionOrderID].Number = clsStaticInfo.dbl(dtOrderMaster.DefaultView[i]["ProductionOrderID"].ToString());
                            sheet[ROW, colProductCategory].Text = dtOrderMaster.DefaultView[i]["ProductCategory"].ToString();
                            sheet[ROW, colProduct].Text = dtOrderMaster.DefaultView[i]["Product"].ToString();
                            sheet[ROW, colMaterial].Text = dtOrderMaster.DefaultView[i]["Material"].ToString();
                            sheet[ROW, colArticle].Text = dtOrderMaster.DefaultView[i]["Article"].ToString();
                            sheet[ROW, colCustomer].Text = dtOrderMaster.DefaultView[i]["Customer"].ToString();
                            sheet[ROW, colbuyer].Text = dtOrderMaster.DefaultView[i]["buyer"].ToString();

                            sheet[ROW, colMasterOrderNo].Text = dtOrderMaster.DefaultView[i]["MasterOrderNo"].ToString();
                            sheet[ROW, colBuyerOrderNo].Text = dtOrderMaster.DefaultView[i]["BuyerOrderNo"].ToString();
                            sheet[ROW, colOwnOrderNo].Text = dtOrderMaster.DefaultView[i]["OwnOrderNo"].ToString();
                            sheet[ROW, colStyleNo].Text = dtOrderMaster.DefaultView[i]["StyleNo"].ToString();
                            sheet[ROW, colOwnStyleNo].Text = dtOrderMaster.DefaultView[i]["OwnStyleNo"].ToString();
                            sheet[ROW, colSalesOrderIds].Text = dtOrderMaster.DefaultView[i]["SalesOrderIds"].ToString();
                            sheet[ROW, colSalesOrderDesc].Text = dtOrderMaster.DefaultView[i]["SalesOrderDesc"].ToString();


                            sheet[ROW, colSAM].Number = clsStaticInfo.dbl(dtOrderMaster.DefaultView[i]["SAM"].ToString());
                            sheet[ROW, colWorkStation].Number = clsStaticInfo.dbl(dtOrderMaster.DefaultView[i]["NoOfWorkStation"].ToString());

                            sheet[ROW, colPlanHours].Number = clsStaticInfo.dbl(dtOrderMaster.DefaultView[i]["PlanHours"].ToString());
                            sheet[ROW, colActualWorkHours].Number = clsStaticInfo.dbl(dtOrderMaster.DefaultView[i]["ProductionHours"].ToString());

                            sheet[ROW, colPlanQty].Number = clsStaticInfo.dbl(dtOrderMaster.DefaultView[i]["PlanQty"].ToString());
                            sheet[ROW, colDifferenceInProduction].Formula = clsStaticInfo.GetxlsCol(colActualQty) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanQty) + ROW.ToString();

                            sheet[ROW, colPlanCM].Number = clsStaticInfo.dbl(dtOrderMaster.DefaultView[i]["PlanCM"].ToString());
                            sheet[ROW, colDifferenceInCM].Formula = clsStaticInfo.GetxlsCol(colActualCM) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanCM) + ROW.ToString();


                            //sheet[ROW, colPlanMinutes].Number = clsStaticInfo.dbl(dtOrderMaster.DefaultView[i]["PlanMinutes"].ToString());
                            sheet[ROW, colPlanMinutes].Formula = CellAddr(colPlanQty, ROW) + "*" + CellAddr(colSAM, ROW);
                            sheet[ROW, colDifferenceInhours].Formula = clsStaticInfo.GetxlsCol(colActualMinutes) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanMinutes) + ROW.ToString();


                            //sheet[ROW, colPlanEff].Number = clsStaticInfo.dbl(dtOrderMaster.DefaultView[i]["PlanEfficiency"].ToString());
                            sheet[ROW, colActualEff].Number = 0.0000000000001f;
                            sheet[ROW, colPlanEff].Formula = "IF(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colPlanHours, ROW) + ">0,(" + CellAddr(colPlanMinutes, ROW) + ")/(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colPlanHours, ROW) + "*60),0.0000000000001)";
                            sheet[ROW, colDifferenceInEff].Formula = clsStaticInfo.GetxlsCol(colActualEff) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanEff) + ROW.ToString();





                            string dickey = dtOrderMaster.DefaultView[i]["PlanDate"].ToString() + dtOrderMaster.DefaultView[i]["WorkCenterMasterId"].ToString();
                            if (dicActualData.ContainsKey(dickey))
                            {

                                List<DataRow> dr = dicActualData[dickey].Where(ee => ee["ProductionOrderID"].ToString() == dtOrderMaster.DefaultView[i]["ProductionOrderID"].ToString()).ToList();
                                if (dr == null || dr.Count == 0)
                                {
                                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                                    ROW++;
                                }


                                if (dr != null && dr.Count > 0)
                                {

                                    sheet[ROW, colActualQty].Number = clsStaticInfo.dbl(dr[0]["ActualQty"].ToString());
                                    sheet[ROW, colActualCM].Number = clsStaticInfo.dbl(dr[0]["ActualCM"].ToString());
                                    sheet[ROW, colActualMinutes].Formula = CellAddr(colActualQty, ROW) + "*" + CellAddr(colSAM, ROW);
                                    sheet[ROW, colActualEff].Formula = "IF(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colActualWorkHours, ROW) + ">0,(" + CellAddr(colActualMinutes, ROW) + ")/(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colActualWorkHours, ROW) + "*60),0.0000000000001)";


                                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                                    ROW++;
                                }

                                dr = dicActualData[dickey].Where(ee => ee["ProductionOrderID"].ToString() != dtOrderMaster.DefaultView[i]["ProductionOrderID"].ToString()).ToList();
                                if (dr != null && dr.Count > 0)
                                {
                                    for (int k = 0; k < dr.Count; k++)
                                    {

                                        SetDate(sheet[ROW, colPlanDate], dr[k]["ActualDate"].ToString());
                                        sheet[ROW, colPlant].Text = dr[k]["Plant"].ToString();
                                        sheet[ROW, colEntity].Text = dr[k]["Entity"].ToString();
                                        sheet[ROW, colWorkCenter].Text = dr[k]["WorkCenter"].ToString();
                                        sheet[ROW, colReportDate].Text = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                        sheet[ROW, colSnapshotDate].Text = dr[k]["SnapshotDate"].ToString();

                                        sheet[ROW, colProductionOrderID].Number = clsStaticInfo.dbl(dr[k]["ProductionOrderID"].ToString());
                                        sheet[ROW, colProductCategory].Text = dr[k]["ProductCategory"].ToString();
                                        sheet[ROW, colProduct].Text = dr[k]["Product"].ToString();
                                        sheet[ROW, colMaterial].Text = dr[k]["Material"].ToString();
                                        sheet[ROW, colArticle].Text = dr[k]["Article"].ToString();

                                        sheet[ROW, colbuyer].Text = dr[k]["buyer"].ToString();
                                        sheet[ROW, colCustomer].Text = dr[k]["Customer"].ToString();
                                        sheet[ROW, colMasterOrderNo].Text = dr[k]["MasterOrderNo"].ToString();
                                        sheet[ROW, colBuyerOrderNo].Text = dr[k]["BuyerOrderNo"].ToString();
                                        sheet[ROW, colOwnOrderNo].Text = dr[k]["OwnOrderNo"].ToString();
                                        sheet[ROW, colStyleNo].Text = dr[k]["StyleNo"].ToString();
                                        sheet[ROW, colOwnStyleNo].Text = dr[k]["OwnStyleNo"].ToString();
                                        sheet[ROW, colSalesOrderIds].Text = dr[k]["SalesOrderIds"].ToString();
                                        sheet[ROW, colSalesOrderDesc].Text = dr[k]["SalesOrderDesc"].ToString();


                                        sheet[ROW, colSAM].Number = clsStaticInfo.dbl(dr[k]["SAM"].ToString());
                                        sheet[ROW, colWorkStation].Number = clsStaticInfo.dbl(dr[k]["NoOfWorkStation"].ToString());
                                        sheet[ROW, colPlanHours].Number = clsStaticInfo.dbl(dr[k]["PlanHours"].ToString());
                                        sheet[ROW, colActualWorkHours].Number = clsStaticInfo.dbl(dr[k]["ProductionHours"].ToString());


                                        sheet[ROW, colPlanQty].Number = clsStaticInfo.dbl(dr[k]["PlanQty"].ToString());
                                        sheet[ROW, colPlanCM].Number = clsStaticInfo.dbl(dr[k]["PlanCM"].ToString());
                                        sheet[ROW, colPlanMinutes].Formula = CellAddr(colPlanQty, ROW) + "*" + CellAddr(colSAM, ROW);
                                        sheet[ROW, colPlanEff].Formula = "IF(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colPlanHours, ROW) + ">0,(" + CellAddr(colPlanMinutes, ROW) + ")/(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colPlanHours, ROW) + "*60),0.0000000000001)";


                                        sheet[ROW, colActualQty].Number = clsStaticInfo.dbl(dr[k]["ActualQty"].ToString());
                                        sheet[ROW, colActualCM].Number = clsStaticInfo.dbl(dr[k]["ActualCM"].ToString());
                                        sheet[ROW, colActualMinutes].Formula = CellAddr(colActualQty, ROW) + "*" + CellAddr(colSAM, ROW);
                                        sheet[ROW, colActualEff].Formula = "IF(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colActualWorkHours, ROW) + ">0,(" + CellAddr(colActualMinutes, ROW) + ")/(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colActualWorkHours, ROW) + "*60),0.0000000000001)";




                                        sheet[ROW, colDifferenceInProduction].Formula = clsStaticInfo.GetxlsCol(colActualQty) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanQty) + ROW.ToString();
                                        sheet[ROW, colDifferenceInCM].Formula = clsStaticInfo.GetxlsCol(colActualCM) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanCM) + ROW.ToString();
                                        sheet[ROW, colDifferenceInhours].Formula = clsStaticInfo.GetxlsCol(colActualMinutes) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanMinutes) + ROW.ToString();
                                        sheet[ROW, colDifferenceInEff].Formula = clsStaticInfo.GetxlsCol(colActualEff) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanEff) + ROW.ToString();



                                        //Additional Columns



                                        sheet[ROW, colCM].Number = clsStaticInfo.dbl(dr[k]["CM"].ToString());//from SO
                                        sheet[ROW, colLineTargetPerDay].Number = clsStaticInfo.dbl(dr[k]["LineTargetPerDay"].ToString());//PR
                                                                                                                                         //sheet[ROW, colPlannedQtyForTheDay].Number = clsStaticInfo.dbl(dr[k]["PlannedQtyForTheDay"].ToString());
                                                                                                                                         //sheet[ROW, colWorkingHours].Number = clsStaticInfo.dbl(dr[k]["WorkingHours"].ToString());//PLAN
                                        sheet[ROW, colPlanTargetPerHourPR].Number = clsStaticInfo.dbl(dr[k]["PlanTargetPerHour"].ToString());//PR
                                        sheet[ROW, colProductionPlanHoursPR].Number = clsStaticInfo.dbl(dr[k]["PlanWorkingHoursPerDay"].ToString());//
                                        sheet[ROW, colWorkcenterStdWorkinghour].Number = clsStaticInfo.dbl(dr[k]["StandardWorkingHours"].ToString());//WC
                                        sheet[ROW, colWCStdHourCostWorkCenter].Number = clsStaticInfo.dbl(dr[k]["DailyFixedCost"].ToString());//WC
                                        sheet[ROW, colVariableCostperhour].Number = clsStaticInfo.dbl(dr[k]["VariableCostPerHour"].ToString());//WC




                                        #region Actual Line Cost For the day    

                                        StandardWS = clsStaticInfo.dbl(dr[k]["StandardWorkStations"].ToString());
                                        StandardHours = clsStaticInfo.dbl(dr[k]["StandardWorkingHours"].ToString());
                                        StandardFixedCost = clsStaticInfo.dbl(dr[k]["DailyFixedCost"].ToString());
                                        StandardAdditionalVCPerHour = clsStaticInfo.dbl(dr[k]["VariableCostPerHour"].ToString());

                                        PlanHour = clsStaticInfo.dbl(dr[k]["WorkingHours"].ToString());
                                        PlanWS = clsStaticInfo.dbl(dr[k]["NoOfWorkStation"].ToString());


                                        WSCostPerHour = 0;
                                        if (StandardWS > 0 && StandardHours > 0)
                                            WSCostPerHour = StandardFixedCost / StandardWS;

                                        TotalPlanCost = PlanWS * WSCostPerHour;
                                        if (PlanHour > StandardHours)
                                        {
                                            if (StandardWS > 0 && PlanWS > 0)
                                                TotalPlanCost += (StandardAdditionalVCPerHour / StandardWS * PlanWS) * (PlanHour - StandardHours);
                                            //TotalPlanCost += (PlanHour - StandardHours) * StandardAdditionalVCPerHour;
                                        }


                                        if (bplib.clsWebLib.GetBoolData(dr[k]["isBuildUp"].ToString()) == true)
                                            sheet[ROW, colIsBuildup].Text = "YES";


                                        sheet[ROW, colActualLineCostForTheDay].Number = TotalPlanCost;

                                        if (clsStaticInfo.dbl(dr[k]["CM"].ToString()) > 0)
                                            sheet[ROW, colBreakEvenQty].Formula = string.Concat("IF(", CellAddr(colCM, ROW), ">0,", TotalPlanCost.ToString() + "/" + dr[k]["CM"].ToString(), ",0)");

                                        string BU = CellAddr(colIsBuildup, ROW) + "=\"YES\"";
                                        string A = CellAddr(colBreakEvenQty, ROW);
                                        string B = CellAddr(colPlanTargetPerHourPR, ROW) + "*" + CellAddr(colActualWorkHours, ROW);
                                        string C = CellAddr(colActualQty, ROW);

                                        sheet[ROW, colProductionProfitLossPcs].Formula = C + "-" + A;
                                        string PL = CellAddr(colProductionProfitLossPcs, ROW);


                                        sheet[ROW, colProductionProfitLossLineTargetPcs].Formula = "IF(" + PL + "<0,IF(" + B + "<" + A + ",IF(" + BU + ",(" + A + "-" + B + ")," + A + "-" + C + "),IF(" + BU + ",0," + PL + "*-1)),0)";
                                        sheet[ROW, colProductionProfitLossBuildUpPcs].Formula = "IF(" + PL + "<0,IF(" + B + "<" + A + ",IF(" + BU + ",(" + B + "-" + C + "),0),IF(" + BU + "," + PL + "*-1,0)),0)";


                                        sheet[ROW, colProductionProfitLossAmount].Formula = CellAddr(colProductionProfitLossPcs, ROW) + "*" + CellAddr(colCM, ROW);
                                        sheet[ROW, colProductionProfitLossLineTargetAmount].Formula = CellAddr(colProductionProfitLossLineTargetPcs, ROW) + "*" + CellAddr(colCM, ROW);
                                        sheet[ROW, colProductionProfitLossBuildUpAmount].Formula = CellAddr(colProductionProfitLossBuildUpPcs, ROW) + "*" + CellAddr(colCM, ROW);


                                        #endregion Actual Line Cost For the day












                                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                                        ROW++;
                                    }

                                }

                            }
                            else
                            {
                                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                                ROW++;
                            }
                        }
                    }
                    else
                    {
                        //there is no planning, so print all production data if any
                        string dickey = item.Key;
                        if (dicActualData.ContainsKey(dickey))
                        {

                            List<DataRow> dr = dicActualData[dickey];
                            if (dr != null && dr.Count > 0)
                            {
                                for (int k = 0; k < dr.Count; k++)
                                {

                                    SetDate(sheet[ROW, colPlanDate], dr[k]["ActualDate"].ToString());
                                    sheet[ROW, colPlant].Text = dr[k]["Plant"].ToString();
                                    sheet[ROW, colEntity].Text = dr[k]["Entity"].ToString();
                                    sheet[ROW, colWorkCenter].Text = dr[k]["WorkCenter"].ToString();
                                    sheet[ROW, colReportDate].Text = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                    sheet[ROW, colSnapshotDate].Text = dr[k]["SnapshotDate"].ToString();


                                    sheet[ROW, colProductionOrderID].Number = clsStaticInfo.dbl(dr[k]["ProductionOrderID"].ToString());
                                    sheet[ROW, colProductCategory].Text = dr[k]["ProductCategory"].ToString();
                                    sheet[ROW, colProduct].Text = dr[k]["Product"].ToString();
                                    sheet[ROW, colMaterial].Text = dr[k]["Material"].ToString();
                                    sheet[ROW, colArticle].Text = dr[k]["Article"].ToString();
                                    sheet[ROW, colCustomer].Text = dr[k]["Customer"].ToString();
                                    sheet[ROW, colbuyer].Text = dr[k]["buyer"].ToString();
                                    sheet[ROW, colMasterOrderNo].Text = dr[k]["MasterOrderNo"].ToString();
                                    sheet[ROW, colBuyerOrderNo].Text = dr[k]["BuyerOrderNo"].ToString();
                                    sheet[ROW, colOwnOrderNo].Text = dr[k]["OwnOrderNo"].ToString();
                                    sheet[ROW, colStyleNo].Text = dr[k]["StyleNo"].ToString();
                                    sheet[ROW, colOwnStyleNo].Text = dr[k]["OwnStyleNo"].ToString();
                                    sheet[ROW, colSalesOrderIds].Text = dr[k]["SalesOrderIds"].ToString();
                                    sheet[ROW, colSalesOrderDesc].Text = dr[k]["SalesOrderDesc"].ToString();

                                    sheet[ROW, colSAM].Number = clsStaticInfo.dbl(dr[k]["SAM"].ToString());
                                    sheet[ROW, colWorkStation].Number = clsStaticInfo.dbl(dr[k]["NoOfWorkStation"].ToString());
                                    sheet[ROW, colPlanHours].Number = clsStaticInfo.dbl(dr[k]["PlanHours"].ToString());
                                    sheet[ROW, colActualWorkHours].Number = clsStaticInfo.dbl(dr[k]["ProductionHours"].ToString());


                                    sheet[ROW, colPlanQty].Number = clsStaticInfo.dbl(dr[k]["PlanQty"].ToString());
                                    sheet[ROW, colPlanCM].Number = clsStaticInfo.dbl(dr[k]["PlanCM"].ToString());
                                    sheet[ROW, colPlanMinutes].Formula = CellAddr(colPlanQty, ROW) + "*" + CellAddr(colSAM, ROW);
                                    sheet[ROW, colPlanEff].Formula = "IF(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colPlanHours, ROW) + ">0,(" + CellAddr(colPlanMinutes, ROW) + ")/(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colPlanHours, ROW) + "*60),0.0000000000001)";

                                    sheet[ROW, colActualQty].Number = clsStaticInfo.dbl(dr[k]["ActualQty"].ToString());
                                    sheet[ROW, colActualCM].Number = clsStaticInfo.dbl(dr[k]["ActualCM"].ToString());
                                    sheet[ROW, colActualMinutes].Formula = CellAddr(colActualQty, ROW) + "*" + CellAddr(colSAM, ROW);
                                    sheet[ROW, colActualEff].Formula = "IF(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colActualWorkHours, ROW) + ">0,(" + CellAddr(colActualMinutes, ROW) + ")/(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colActualWorkHours, ROW) + "*60),0.0000000000001)";



                                    sheet[ROW, colDifferenceInProduction].Formula = clsStaticInfo.GetxlsCol(colActualQty) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanQty) + ROW.ToString();
                                    sheet[ROW, colDifferenceInCM].Formula = clsStaticInfo.GetxlsCol(colActualCM) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanCM) + ROW.ToString();
                                    sheet[ROW, colDifferenceInhours].Formula = clsStaticInfo.GetxlsCol(colActualMinutes) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanMinutes) + ROW.ToString();
                                    sheet[ROW, colDifferenceInEff].Formula = clsStaticInfo.GetxlsCol(colActualEff) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colPlanEff) + ROW.ToString();


                                    //Additional Columns



                                    sheet[ROW, colCM].Number = clsStaticInfo.dbl(dr[k]["CM"].ToString());//from SO
                                    sheet[ROW, colLineTargetPerDay].Number = clsStaticInfo.dbl(dr[k]["LineTargetPerDay"].ToString());//PR
                                                                                                                                     //sheet[ROW, colPlannedQtyForTheDay].Number = clsStaticInfo.dbl(dr[k]["PlannedQtyForTheDay"].ToString());
                                                                                                                                     //sheet[ROW, colWorkingHours].Number = clsStaticInfo.dbl(dr[k]["WorkingHours"].ToString());//PLAN
                                    sheet[ROW, colPlanTargetPerHourPR].Number = clsStaticInfo.dbl(dr[k]["PlanTargetPerHour"].ToString());//PR
                                    sheet[ROW, colProductionPlanHoursPR].Number = clsStaticInfo.dbl(dr[k]["PlanWorkingHoursPerDay"].ToString());//
                                    sheet[ROW, colWorkcenterStdWorkinghour].Number = clsStaticInfo.dbl(dr[k]["StandardWorkingHours"].ToString());//WC
                                    sheet[ROW, colWCStdHourCostWorkCenter].Number = clsStaticInfo.dbl(dr[k]["DailyFixedCost"].ToString());//WC
                                    sheet[ROW, colVariableCostperhour].Number = clsStaticInfo.dbl(dr[k]["VariableCostPerHour"].ToString());//WC




                                    #region Actual Line Cost For the day    

                                    StandardWS = clsStaticInfo.dbl(dr[k]["StandardWorkStations"].ToString());
                                    StandardHours = clsStaticInfo.dbl(dr[k]["StandardWorkingHours"].ToString());
                                    StandardFixedCost = clsStaticInfo.dbl(dr[k]["DailyFixedCost"].ToString());
                                    StandardAdditionalVCPerHour = clsStaticInfo.dbl(dr[k]["VariableCostPerHour"].ToString());

                                    PlanHour = clsStaticInfo.dbl(dr[k]["WorkingHours"].ToString());
                                    PlanWS = clsStaticInfo.dbl(dr[k]["NoOfWorkStation"].ToString());


                                    WSCostPerHour = 0;
                                    if (StandardWS > 0 && StandardHours > 0)
                                        WSCostPerHour = StandardFixedCost / StandardWS;

                                    TotalPlanCost = PlanWS * WSCostPerHour;
                                    if (PlanHour > StandardHours)
                                    {
                                        if (StandardWS > 0 && PlanWS > 0)
                                            TotalPlanCost += (StandardAdditionalVCPerHour / StandardWS * PlanWS) * (PlanHour - StandardHours);
                                        //TotalPlanCost += (PlanHour - StandardHours) * StandardAdditionalVCPerHour;
                                    }


                                    if (bplib.clsWebLib.GetBoolData(dr[k]["isBuildUp"].ToString()) == true)
                                        sheet[ROW, colIsBuildup].Text = "YES";


                                    sheet[ROW, colActualLineCostForTheDay].Number = TotalPlanCost;

                                    if (clsStaticInfo.dbl(dr[k]["CM"].ToString()) > 0)
                                        sheet[ROW, colBreakEvenQty].Formula = string.Concat("IF(", CellAddr(colCM, ROW), ">0,", TotalPlanCost.ToString() + "/" + dr[k]["CM"].ToString(), ",0)");

                                    string BU = CellAddr(colIsBuildup, ROW) + "=\"YES\"";
                                    string A = CellAddr(colBreakEvenQty, ROW);
                                    string B = CellAddr(colPlanTargetPerHourPR, ROW) + "*" + CellAddr(colActualWorkHours, ROW);
                                    string C = CellAddr(colActualQty, ROW);

                                    sheet[ROW, colProductionProfitLossPcs].Formula = C + "-" + A;
                                    string PL = CellAddr(colProductionProfitLossPcs, ROW);


                                    sheet[ROW, colProductionProfitLossLineTargetPcs].Formula = "IF(" + PL + "<0,IF(" + B + "<" + A + ",IF(" + BU + ",(" + A + "-" + B + ")," + A + "-" + C + "),IF(" + BU + ",0," + PL + "*-1)),0)";
                                    sheet[ROW, colProductionProfitLossBuildUpPcs].Formula = "IF(" + PL + "<0,IF(" + B + "<" + A + ",IF(" + BU + ",(" + B + "-" + C + "),0),IF(" + BU + "," + PL + "*-1,0)),0)";


                                    sheet[ROW, colProductionProfitLossAmount].Formula = CellAddr(colProductionProfitLossPcs, ROW) + "*" + CellAddr(colCM, ROW);
                                    sheet[ROW, colProductionProfitLossLineTargetAmount].Formula = CellAddr(colProductionProfitLossLineTargetPcs, ROW) + "*" + CellAddr(colCM, ROW);
                                    sheet[ROW, colProductionProfitLossBuildUpAmount].Formula = CellAddr(colProductionProfitLossBuildUpPcs, ROW) + "*" + CellAddr(colCM, ROW);


                                    #endregion Actual Line Cost For the day

                                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                                    ROW++;
                                }

                            }
                        }



                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                        //ROW++;

                    }
                }
                sheet.UsedRange.NumberFormat = "#,##0.00_);(#,##0.00)";
                sheet[startRow, colPlanDate, ROW, colPlanDate].NumberFormat = "dd-MMM-yyyy";
                sheet[startRow, colProductionOrderID, ROW, colProductionOrderID].NumberFormat = "0";
                sheet[startRow, colPlanEff, ROW, colDifferenceInEff].NumberFormat = clsStaticInfo.NumberFormat(2, true);



                sheet.Range[startRow, colPlanTargetPerHourPR, ROW, colPlanTargetPerHourPR].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[startRow, colCM, ROW, colCM].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet.Range[startRow, colActualLineCostForTheDay, ROW, colActualLineCostForTheDay].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[startRow, colBreakEvenQty, ROW, colBreakEvenQty].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[startRow, colProductionProfitLossPcs, ROW, colProductionProfitLossPcs].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[startRow, colProductionProfitLossLineTargetPcs, ROW, colProductionProfitLossLineTargetPcs].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[startRow, colProductionProfitLossBuildUpPcs, ROW, colProductionProfitLossBuildUpPcs].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[startRow, colProductionProfitLossAmount, ROW, colProductionProfitLossAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[startRow, colProductionProfitLossLineTargetAmount, ROW, colProductionProfitLossLineTargetAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[startRow, colProductionProfitLossBuildUpAmount, ROW, colProductionProfitLossBuildUpAmount].NumberFormat = clsStaticInfo.NumberFormat(2);


                IListObject table = sheet.ListObjects.Create("Table1", sheet[clsStaticInfo.GetxlsCol(1) + (6).ToString() + ":" + clsStaticInfo.GetxlsCol(endCol) + (ROW).ToString()]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;


                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange["A7"].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "OS4", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;


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



                string fPath = fPath = HostingEnvironment.MapPath("~/") + "TempReport" + identity.UserId + ".xlsx";


                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }
                #region Performance
                workbook.Worksheets[0].Name = "Performance";
                IWorksheet pivotSheet = workbook.Worksheets[0];

                IPivotCache cache = workbook.PivotCaches.Add(sheet[startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);

                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colWorkCenter - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPlanDate - 1].Axis = PivotAxisTypes.Column;
                pivotTable.Fields[colPlanDate - 1].NumberFormat = "mm/dd";

                IPivotField field = pivotTable.Fields[colSAM - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "SPT", PivotSubtotalTypes.Average);

                field = pivotTable.Fields[colWorkStation - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Work Stations", PivotSubtotalTypes.Sum);


                field = pivotTable.Fields[colPlanQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Plan Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colActualQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Actual Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colDifferenceInProduction - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(0);
                pivotTable.DataFields.Add(field, "Diff In Prd. Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colPlanCM - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(0);
                pivotTable.DataFields.Add(field, "Plan CM", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colActualCM - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(0);
                pivotTable.DataFields.Add(field, "Actual CM", PivotSubtotalTypes.Sum);


                field = pivotTable.Fields[colDifferenceInCM - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(0);
                pivotTable.DataFields.Add(field, "Diff In CM", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colPlanMinutes - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(0);
                pivotTable.DataFields.Add(field, "Plan Minutes", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colActualMinutes - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(0);
                pivotTable.DataFields.Add(field, "Actual Minutes", PivotSubtotalTypes.Sum);


                field = pivotTable.Fields[colDifferenceInhours - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(0);
                pivotTable.DataFields.Add(field, "Diff In Hours", PivotSubtotalTypes.Sum);



                field = pivotTable.Fields[colPlanEff - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(2, true);
                pivotTable.DataFields.Add(field, "Plan Eff.", PivotSubtotalTypes.Average);

                field = pivotTable.Fields[colActualEff - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(2, true);
                pivotTable.DataFields.Add(field, "Actual Eff.", PivotSubtotalTypes.Average);


                field = pivotTable.Fields[colDifferenceInEff - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(2, true);
                pivotTable.DataFields.Add(field, "Diff In Eff.", PivotSubtotalTypes.Sum);


                pivotTable.ShowDrillIndicators = false;
                pivotTable.ShowDataFieldInRow = true;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Plan vs Production Performance ", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion Performance




                string strFileName = "OS4.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                excelEngine.Dispose();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }
        [HttpGet, Authorize]
        public ActionResult ProductionDataXls(string entityid, string fromDate, string toDate)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");

                if (string.IsNullOrEmpty(fromDate))
                    throw new Exception("Select from date");

                if (string.IsNullOrEmpty(toDate))
                    throw new Exception("Select to date");

                if (bplib.clsWebLib.IsDateOK(fromDate) == false || fromDate == "undefined")
                    throw new Exception("Select from date");

                if (bplib.clsWebLib.IsDateOK(toDate) == false || fromDate == "undefined")
                    throw new Exception("Select to date");

                if (Convert.ToDateTime(fromDate) > Convert.ToDateTime(toDate))
                    throw new Exception("To date cannot be earlier than from date");


                if (Math.Abs(clsStaticInfo.dateDiff(fromDate, toDate)) > 180)
                    throw new Exception("Cannot set date range greater than six months");


                Dictionary<string, List<DataRow>> dicActualData = null;
                getProductionData(entityid, fromDate, toDate, out dicActualData);


                if (dicActualData.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Production Data";
                sheet = workbook.Worksheets[0];


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
                sheet[ROW, COL].Text = "From Process/Inventory";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcess = COL;
                COL++;
                sheet[ROW, COL].Text = "From Work Center";
                sheet[ROW, COL].ColumnWidth = 16;
                int colWorkCenter = COL;
                COL++;
                sheet[ROW, COL].Text = "To Process/Inventory";
                sheet[ROW, COL].ColumnWidth = 16;
                int colToProcess = COL;
                COL++;
                sheet[ROW, COL].Text = "To Work Center";
                sheet[ROW, COL].ColumnWidth = 16;
                int colToWorkCenter = COL;
                COL++;
                sheet[ROW, COL].Text = "Shift";
                sheet[ROW, COL].ColumnWidth = 16;
                int colShift = COL;

                COL++;
                sheet[ROW, COL].Text = "Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlanDate = COL;
                COL++;
                int colstart = COL;
                sheet[ROW, COL].Text = "Prod. Order No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrderID = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colbuyer = COL;
                COL++;

                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colCustomer = COL;
                COL++;

                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Orde No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyerOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Order No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOwnOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Item No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Item No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOwnStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Id(Booking)";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderIdBooking = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Desc(Booking)";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderDescBooking = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Ids(PR)";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderIds = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Desc(PR)";
                sheet[ROW, COL].ColumnWidth = 70;
                int colSalesOrderDesc = COL;
                COL++;

                sheet[ROW, COL].Text = "Product Code";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProductCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProduct = COL;
                COL++;

                sheet[ROW, COL].Text = "Production Status";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProductionStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Lot No";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLotNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 32;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 40;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSAM = COL;
                COL++;
                sheet[ROW, COL].Text = "Work Station";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWorkStation = COL;


                COL++;
                sheet[ROW, COL].Text = "Working Hours";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActualWorkHours = COL;
                COL++;

                sheet[ROW, COL].Text = "Production Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualQty = COL;

                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualCM = COL;

                COL++;
                sheet[ROW, COL].Text = "Minutes";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualMinutes = COL;
                COL++;
                sheet[ROW, COL].Text = "Efficiency";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualEff = COL;
               
                
                #endregion columns

                int endCol = COL;

                Dictionary<string, DateTime> allDates = new Dictionary<string, DateTime>();
                foreach (KeyValuePair<string, List<DataRow>> item in dicActualData)
                    for (int i = 0; i < item.Value.Count; i++)
                        if (allDates.ContainsKey(item.Value[i]["ActualDate"].ToString() + item.Value[i]["WorkCenterMasterId"].ToString() + item.Value[i]["Id"].ToString() + item.Value[i]["PartyId"].ToString()) == false)
                            allDates.Add(item.Value[i]["ActualDate"].ToString() + item.Value[i]["WorkCenterMasterId"].ToString() + item.Value[i]["Id"].ToString() + item.Value[i]["PartyId"].ToString(), Convert.ToDateTime(item.Value[i]["ActualDate"].ToString()));


                allDates.OrderBy(ee => ee.Value);


                ROW++;

                int startRow = ROW;
                foreach (KeyValuePair<string, DateTime> item in allDates)
                {

                    //there is no planning, so print all production data if any
                    string dickey = item.Key;
                    if (dicActualData.ContainsKey(dickey))
                    {

                        List<DataRow> dr = dicActualData[dickey];
                        if (dr != null && dr.Count > 0)
                        {
                            for (int k = 0; k < dr.Count; k++)
                            {

                                SetDate(sheet[ROW, colPlanDate], dr[k]["ActualDate"].ToString());
                                sheet[ROW, colPlant].Text = dr[k]["Plant"].ToString();
                                sheet[ROW, colEntity].Text = dr[k]["Entity"].ToString();
                                sheet[ROW, colProcess].Text = dr[k]["Process"].ToString();
                                sheet[ROW, colWorkCenter].Text = dr[k]["WorkCenter"].ToString();

                                sheet[ROW, colToProcess].Text = dr[k]["ToProcess"].ToString();
                                sheet[ROW, colToWorkCenter].Text = dr[k]["ToWorkCenter"].ToString();

                                sheet[ROW, colProductionOrderID].Number = clsStaticInfo.dbl(dr[k]["ProductionOrderID"].ToString());
                                sheet[ROW, colProductCategory].Text = dr[k]["ProductCategory"].ToString();
                                sheet[ROW, colProduct].Text = dr[k]["Product"].ToString();
                                sheet[ROW, colMaterial].Text = dr[k]["Material"].ToString();
                                sheet[ROW, colArticle].Text = dr[k]["Article"].ToString();
                                sheet[ROW, colbuyer].Text = dr[k]["buyer"].ToString();
                                sheet[ROW, colMasterOrderNo].Text = dr[k]["MasterOrderNo"].ToString();
                                sheet[ROW, colBuyerOrderNo].Text = dr[k]["BuyerOrderNo"].ToString();
                                sheet[ROW, colOwnOrderNo].Text = dr[k]["OwnOrderNo"].ToString();
                                sheet[ROW, colStyleNo].Text = dr[k]["StyleNo"].ToString();
                                sheet[ROW, colOwnStyleNo].Text = dr[k]["OwnStyleNo"].ToString();
                                sheet[ROW, colSalesOrderIds].Text = dr[k]["SalesOrderIds"].ToString();
                                sheet[ROW, colSalesOrderDesc].Text = dr[k]["SalesOrderDesc"].ToString();

                                sheet[ROW, colShift].Text = dr[k]["ProductionShift"].ToString();
                                sheet[ROW, colSalesOrderIdBooking].Text = dr[k]["SalesOrderIdBooking"].ToString();
                                sheet[ROW, colSalesOrderDescBooking].Text = dr[k]["SalesOrderDescBooking"].ToString();




                                sheet[ROW, colSAM].Number = clsStaticInfo.dbl(dr[k]["SAM"].ToString());
                                sheet[ROW, colWorkStation].Number = clsStaticInfo.dbl(dr[k]["NoOfWorkStation"].ToString());
                                sheet[ROW, colActualWorkHours].Number = clsStaticInfo.dbl(dr[k]["ProductionHours"].ToString());



                                sheet[ROW, colActualQty].Number = clsStaticInfo.dbl(dr[k]["ActualQty"].ToString());
                                sheet[ROW, colActualCM].Number = clsStaticInfo.dbl(dr[k]["ActualCM"].ToString());
                                sheet[ROW, colActualMinutes].Formula = CellAddr(colActualQty, ROW) + "*" + CellAddr(colSAM, ROW);
                                sheet[ROW, colActualEff].Formula = "IF(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colActualWorkHours, ROW) + ">0,(" + CellAddr(colActualMinutes, ROW) + ")/(" + CellAddr(colWorkStation, ROW) + "*" + CellAddr(colActualWorkHours, ROW) + "*60),0)";

                                sheet[ROW, colProductCode].Text = dr[k]["ProductCode"].ToString();
                                sheet[ROW, colCustomer].Text = dr[k]["Customer"].ToString();
                                sheet[ROW, colProductionStatus].Text = dr[k]["ProductionStatus"].ToString();
                                sheet[ROW, colLotNo].Text = dr[k]["LotNo"].ToString();

                                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                                ROW++;
                            }

                        }
                    }



                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    //ROW++;


                }
                sheet.UsedRange.NumberFormat = "#,##0.00_);(#,##0.00)";
                sheet[startRow, colPlanDate, ROW, colPlanDate].NumberFormat = "dd-MMM-yyyy";
                sheet[startRow, colProductionOrderID, ROW, colProductionOrderID].NumberFormat = "0";


                IListObject table = sheet.ListObjects.Create("Table1", sheet[clsStaticInfo.GetxlsCol(1) + (6).ToString() + ":" + clsStaticInfo.GetxlsCol(endCol) + (ROW).ToString()]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;


                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[7, 7].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Production Data", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;


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
                string strFileName = "Production Data.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                excelEngine.Dispose();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        [HttpGet, Authorize]
        public ActionResult BulletinReport(string entityid)
        {

            try
            {
                Library.Planning.OrderManagement.Bulletin bulletin = new Library.Planning.OrderManagement.Bulletin();
                bulletin.BulletinReport(entityid);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }


        [HttpGet, Authorize]
        public ActionResult OS1xls(string entityid, string fromDate, string toDate,string productionStatusList)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");
                if (string.IsNullOrEmpty(fromDate))
                    throw new Exception("Select from date");

                if (string.IsNullOrEmpty(toDate))
                    throw new Exception("Select to date");

                if (bplib.clsWebLib.IsDateOK(fromDate) == false || fromDate == "undefined")
                    throw new Exception("Select from date");

                if (bplib.clsWebLib.IsDateOK(toDate) == false || fromDate == "undefined")
                    throw new Exception("Select to date");

                if (Convert.ToDateTime(fromDate) > Convert.ToDateTime(toDate))
                    throw new Exception("To date cannot be earlier than from date");
                DataTable dtOrderMaster;
                getOS1(entityid, fromDate, toDate, productionStatusList,out dtOrderMaster);

                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(3);
                workbook.Worksheets[2].Name = "OS1 Detail";
                sheet = workbook.Worksheets[2];


                int ROW = 6; int COL = 1;

                #region columns

                //SalesOrderId Buyer   ResponsiblePerson MasterOrderNo  
                //    Material OrderCategory   OrderStatus Article 
                //    ProductCategory Product BuyerReferenceNo 
                //    OwnReferenceNo  MaterialRowId PONumber    
                //    ProductionOrderId isProductionScheduled   
                //    DeliveryDate CommitmentDate  SOQty PONumber    PODate
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer Group";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomerAccountGroup = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyer = COL;

                COL++;
                sheet[ROW, COL].Text = "Material ROW ID";
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
                sheet[ROW, COL].ColumnWidth = 14;
                int colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 14;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order Creation Date";
                sheet[ROW, COL].ColumnWidth = 14;
                int colMasterOrderCreationDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Order No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyerOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Order No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOwnOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Item No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyerReferenceNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Item No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOwnReferenceNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                int colResponsiblePerson = COL;

                COL++;
                sheet[ROW, COL].Text = "Sales Order Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderId = COL;
                COL++;

                sheet[ROW, COL].Text = "Sales Order Desc";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderDesc = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Added Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSOAddedDate = COL;
                COL++;
                sheet[ROW, COL].Text = "RM Inhouse Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMainRawMaterialInhouseDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Other RM Inhouse Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOtherRawMaterialInhouseDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colFOB = COL;
                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colCM = COL;
                COL++;
                sheet[ROW, COL].Text = "LSD";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLSD = COL;
                COL++;
                sheet[ROW, COL].Text = "Delivery Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Delivery Month";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDeliveryMonth = COL;
                COL++;
                sheet[ROW, COL].Text = "Diff(LSD-Del.Date) in Days";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDiff = COL;
                COL++;

                sheet[ROW, COL].Text = "PO No";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPONo = COL;
                COL++;
                sheet[ROW, COL].Text = "PO Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPODate = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Category";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Category";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSOCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSOStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Commitment Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colCommitmentDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Commitment Month";
                sheet[ROW, COL].ColumnWidth = 12;
                int colCommitmentMonth = COL;
                //SalesOrderId Buyer   ResponsiblePerson MasterOrderNo  
                //    Material OrderCategory   OrderStatus Article 
                //    ProductCategory Product BuyerReferenceNo 
                //    OwnReferenceNo  MaterialRowId PONumber    
                //    ProductionOrderId isProductionScheduled   
                //    DeliveryDate CommitmentDate  SOQty PONumber    PODate
                COL++;
                sheet[ROW, COL].Text = "Expected Completion Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colExpectedCompletionDate = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Distributed Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSODistributedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PR No";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "PR Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrderRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionStatus = COL;

                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSOQty = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Plan Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlannedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Plan Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProcessPlannedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "UOM";
                sheet[ROW, COL].ColumnWidth = 10;
                int colUOM = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colOrderAmount = COL;

                COL++;
                sheet[ROW, COL].Text = "CM Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colCMAmount = COL;
                COL++;
                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCurrency = COL;
                COL++;
                sheet[ROW, COL].Text = "Reason";
                sheet[ROW, COL].ColumnWidth = 60;
                int colReason = COL;
                COL++;
                sheet[ROW, COL].Text = "PR Booked Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPRBookedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Booked Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSOBookedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Total PR Produced Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalPRProducedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Qty(PR)";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPRPlanQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Type";
                sheet[ROW, COL].ColumnWidth = 20;
                int colType = COL;
                COL++;
                sheet[ROW, COL].Text = "Bulletin Id";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBulletinId = COL;
                COL++;
                sheet[ROW, COL].Text = "No Of WS";
                sheet[ROW, COL].ColumnWidth = 10;
                int colNoOfWS = COL;
                COL++;
                sheet[ROW, COL].Text = "Swing SPT";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTotalSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "ContractId";
                sheet[ROW, COL].ColumnWidth = 10;
                int colContractId = COL;
                COL++;
                sheet[ROW, COL].Text = "ContractName";
                sheet[ROW, COL].ColumnWidth = 15;
                int colContractName = COL;
                COL++;
                sheet[ROW, COL].Text = "LCNo";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLCNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Type";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionType = COL;
                COL++;
                sheet[ROW, COL].Text = "Shipment From Stock";
                sheet[ROW, COL].ColumnWidth = 10;
                int colShipmentFromStock = COL;
                COL++;
                sheet[ROW, COL].Text = "Packing Type";
                sheet[ROW, COL].ColumnWidth = 25;
                int colPackingType = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Sequence";
                sheet[ROW, COL].ColumnWidth = 25;
                int colProcessSequence = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Date";
                sheet[ROW, COL].ColumnWidth = 25;
                int colProcessDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Process StartDate";
                sheet[ROW, COL].ColumnWidth = 25;
                int colProcessStartDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Process EndDate";
                sheet[ROW, COL].ColumnWidth = 25;
                int colProcessEndDate = COL;

                #endregion columns

                int endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                {
                    sheet[ROW, colPlant].Text = dtOrderMaster.Rows[i]["Plant"].ToString();
                    sheet[ROW, colEntity].Text = dtOrderMaster.Rows[i]["Entity"].ToString();
                    sheet[ROW, colBuyer].Text = dtOrderMaster.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colCustomer].Text = dtOrderMaster.Rows[i]["Customer"].ToString();
                    sheet[ROW, colCustomerAccountGroup].Text = dtOrderMaster.Rows[i]["CustomerAccountGroup"].ToString();
                    sheet[ROW, colCommitmentDate].Text = GetDate(dtOrderMaster.Rows[i]["CommitmentDate"].ToString());
                    sheet[ROW, colDeliveryDate].Text = GetDate(dtOrderMaster.Rows[i]["DeliveryDate"].ToString());
                    sheet[ROW, colMasterOrderNo].Text = dtOrderMaster.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colMaterial].Text = dtOrderMaster.Rows[i]["Material"].ToString();
                    sheet[ROW, colProductCategory].Text = dtOrderMaster.Rows[i]["ProductCategory"].ToString();
                    sheet[ROW, colProduct].Text = dtOrderMaster.Rows[i]["Product"].ToString();
                    sheet[ROW, colSalesOrderDesc].Text = dtOrderMaster.Rows[i]["SODesc"].ToString();
                    sheet[ROW, colUOM].Text = dtOrderMaster.Rows[i]["UOM"].ToString();
                    sheet[ROW, colCurrency].Text = dtOrderMaster.Rows[i]["Currency"].ToString();
                    sheet[ROW, colMasterOrderCreationDate].Text = dtOrderMaster.Rows[i]["MasterOrderCreationDate"].ToString();

                    sheet[ROW, colExpectedCompletionDate].Text = dtOrderMaster.Rows[i]["ExpectedCompletionDate"].ToString();
                    sheet[ROW, colSODistributedQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["SODistributedQty"].ToString());


                    sheet[ROW, colBulletinId].Text = dtOrderMaster.Rows[i]["BulletinId"].ToString();
                    sheet[ROW, colTotalSPT].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["TotalSPT"].ToString());
                    sheet[ROW, colNoOfWS].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["NoOfWS"].ToString());
                    sheet[ROW, colContractId].Text = dtOrderMaster.Rows[i]["ContractId"].ToString();
                    sheet[ROW, colContractName].Text = dtOrderMaster.Rows[i]["ContractName"].ToString();
                    sheet[ROW, colLCNo].Text = dtOrderMaster.Rows[i]["LCNo"].ToString();
                    sheet[ROW, colProductionType].Text = dtOrderMaster.Rows[i]["ProductionType"].ToString();
                    sheet[ROW, colShipmentFromStock].Text = dtOrderMaster.Rows[i]["ShipmentFromStock"].ToString();
                    sheet[ROW, colPackingType].Text = dtOrderMaster.Rows[i]["PackingType"].ToString();
                    sheet[ROW, colProcessSequence].Text = dtOrderMaster.Rows[i]["ProcessSequence"].ToString();
                    sheet[ROW, colProcessDate].Text = dtOrderMaster.Rows[i]["ProcessDate"].ToString();
                    sheet[ROW, colProcessStartDate].Text = dtOrderMaster.Rows[i]["ProcessStartDate"].ToString();
                    sheet[ROW, colProcessEndDate].Text = dtOrderMaster.Rows[i]["ProcessEndDate"].ToString();


                    sheet[ROW, colArticle].Text = dtOrderMaster.Rows[i]["Article"].ToString();
                    sheet[ROW, colOwnReferenceNo].Text = dtOrderMaster.Rows[i]["OwnReferenceNo"].ToString();
                    sheet[ROW, colBuyerReferenceNo].Text = dtOrderMaster.Rows[i]["BuyerReferenceNo"].ToString();

                    sheet[ROW, colBuyerOrderNo].Text = dtOrderMaster.Rows[i]["BuyerOrderNo"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = dtOrderMaster.Rows[i]["OwnOrderNo"].ToString();


                    sheet[ROW, colMaterialRowId].Text = dtOrderMaster.Rows[i]["MaterialRowId"].ToString();
                    sheet[ROW, colProductionOrderId].Text = dtOrderMaster.Rows[i]["ProductionOrderId"].ToString();

                    sheet[ROW, colProductionOrderRemarks].Text = dtOrderMaster.Rows[i]["Remarks"].ToString();
                    if (dtOrderMaster.Rows[i]["ProductionOrderId"].ToString().Trim() == "")
                        sheet[ROW, colProductionOrderRemarks].Text = "Yet to plan";

                    sheet[ROW, colProductionStatus].Text = dtOrderMaster.Rows[i]["ProductionStatus"].ToString();

                    sheet[ROW, colReason].Text = dtOrderMaster.Rows[i]["Reason"].ToString();


                    sheet[ROW, colOrderCategory].Text = dtOrderMaster.Rows[i]["OrderCategory"].ToString();
                    sheet[ROW, colOrderStatus].Text = dtOrderMaster.Rows[i]["OrderStatus"].ToString();
                    sheet[ROW, colSOCategory].Text = dtOrderMaster.Rows[i]["SOCategory"].ToString();
                    sheet[ROW, colSOStatus].Text = dtOrderMaster.Rows[i]["SOStatus"].ToString();
                    sheet[ROW, colResponsiblePerson].Text = dtOrderMaster.Rows[i]["ResponsiblePerson"].ToString();
                    sheet[ROW, colType].Text = dtOrderMaster.Rows[i]["Type"].ToString();
                    sheet[ROW, colSOQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["SOQty"].ToString());
                    sheet[ROW, colSalesOrderId].Text = dtOrderMaster.Rows[i]["SalesOrderId"].ToString();
                    sheet[ROW, colPONo].Text = dtOrderMaster.Rows[i]["PONumber"].ToString();
                    sheet[ROW, colPODate].Text = dtOrderMaster.Rows[i]["PODate"].ToString();


                    sheet[ROW, colPlannedQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PlannedQty"].ToString());
                    sheet[ROW, colProcessPlannedQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["ProcessPlanQty"].ToString());
                    sheet[ROW, colFOB].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["FOB"].ToString());
                    sheet[ROW, colCM].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["CM"].ToString());
                    sheet[ROW, colDiff].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["Diff"].ToString());

                    sheet[ROW, colOrderAmount].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["OrderAmount"].ToString());
                    sheet[ROW, colCMAmount].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["CMAmount"].ToString());

                    sheet[ROW, colSOAddedDate].Text = dtOrderMaster.Rows[i]["SOAddedDate"].ToString();
                    sheet[ROW, colMainRawMaterialInhouseDate].Text = dtOrderMaster.Rows[i]["MainRawMaterialInhouseDate"].ToString();
                    sheet[ROW, colOtherRawMaterialInhouseDate].Text = dtOrderMaster.Rows[i]["OtherRawMaterialInhouseDate"].ToString();
                    sheet[ROW, colLSD].Text = dtOrderMaster.Rows[i]["LSD"].ToString();

                    //sheet[ROW, colDeliveryMonth].Formula = string.Concat("MONTH(", CellAddr(colDeliveryDate, ROW), ")");
                    //sheet[ROW, colCommitmentMonth].Formula = string.Concat("MONTH(", CellAddr(colCommitmentDate, ROW), ")");


                    sheet[ROW, colDeliveryMonth].Formula = "CONCATENATE(Month(" + CellAddr(colDeliveryDate, ROW) + "),\"/\",Year(" + CellAddr(colDeliveryDate, ROW) + "))";
                    sheet[ROW, colCommitmentMonth].Formula = "CONCATENATE(Month(" + CellAddr(colCommitmentDate, ROW) + "),\"/\",Year(" + CellAddr(colCommitmentDate, ROW) + "))";


                    sheet[ROW, colPRBookedQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PRBookedQuantity"].ToString());
                    sheet[ROW, colSOBookedQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["SOBookedQuantity"].ToString());
                    sheet[ROW, colTotalPRProducedQty].Formula = CellAddr(colPRBookedQty, ROW) + "+" + CellAddr(colSOBookedQty, ROW);
                    sheet[ROW, colPRPlanQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PRPlanQty"].ToString());


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                IListObject table = sheet.ListObjects.Create("Table1", sheet[clsStaticInfo.GetxlsCol(1) + (6).ToString() + ":" + clsStaticInfo.GetxlsCol(endCol) + (ROW).ToString()]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange["A7"].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "OS1", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;


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





                string fPath = fPath = HostingEnvironment.MapPath("~/") + "TempReport" + identity.UserId + ".xlsx";


                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }
                #region Buyer Summary
                workbook.Worksheets[0].Name = "OS1- 1Order Master Report";
                IWorksheet pivotSheet = workbook.Worksheets[0];

                IPivotCache cache = workbook.PivotCaches.Add(sheet[startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);


                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCustomerAccountGroup - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMasterOrderNo - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderDesc - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSOAddedDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSOCategory - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSOStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDeliveryMonth - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMainRawMaterialInhouseDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOtherRawMaterialInhouseDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colLSD - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDiff - 1].Axis = PivotAxisTypes.Row;




                IPivotField field = pivotTable.Fields[colSOQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "SO Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colPlannedQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "SO Planned Qty", PivotSubtotalTypes.Sum);


                field = pivotTable.Fields[colFOB - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "Rate", PivotSubtotalTypes.Average);

                field = pivotTable.Fields[colCM - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "CM Rate", PivotSubtotalTypes.Average);

                field = pivotTable.Fields[colOrderAmount - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(0);
                pivotTable.DataFields.Add(field, "Order Amount", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colCMAmount - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat(0);
                pivotTable.DataFields.Add(field, "CM Amount", PivotSubtotalTypes.Sum);



                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colPlant - 1 || i == colCustomerAccountGroup - 1 || i == colEntity - 1 || i == colBuyer - 1 || i == colCustomer - 1 || i == colBuyerOrderNo - 1)
                        continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }

                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Order Master - Master Order /  Sales  Order Breakdown  with Production Order ", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion Buyer Summary


                #region Order Summary-month wise
                workbook.Worksheets[1].Name = "OS1-2Order Master Summary";
                pivotSheet = workbook.Worksheets[1];
                pivotTable = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A6"], cache);

                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCustomerAccountGroup - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMasterOrderNo - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSOCategory - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSOStatus - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colDeliveryMonth - 1].Axis = PivotAxisTypes.Column;


                field = pivotTable.Fields[colSOQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "SO Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colPlannedQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "SO Planned Qty", PivotSubtotalTypes.Sum);


                field = pivotTable.Fields[colOrderAmount - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Order Amount", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colCMAmount - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "CM Amount", PivotSubtotalTypes.Sum);



                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colPlant - 1 || i == colEntity - 1 || i == colBuyer - 1 || i == colCustomerAccountGroup - 1 || i == colCustomer - 1)
                        continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }

                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[1];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Order Master - Delivery Month Wise Analysis", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion Order Summary-month wise


                string strFileName = "OS1.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                excelEngine.Dispose();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

            return null;
        }

        [HttpGet, Authorize]
        public ActionResult MasterOrder(string entityid, string status)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");


                DataTable dtOrderMaster;

                getOrderMasterMain(entityid, status, out dtOrderMaster);


                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2016;
                workbook = application.Workbooks.Create(3);
                workbook.Worksheets[2].Name = "Master Order Data";
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
                sheet[ROW, COL].Text = "Buyer Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colBuyerOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colOwnOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colOrderQty = COL;
                COL++;
                sheet[ROW, COL].Text = "UOM";
                sheet[ROW, COL].ColumnWidth = 10;
                int colUOM = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Status";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOrderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 22;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 14;
                int colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 14;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Delivery Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Delivery Month";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDeliveryMonth = COL;
                COL++;
                sheet[ROW, COL].Text = "Commitment Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colCommitmentDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 20;
                int colRemarks = COL;

                COL++;
                sheet[ROW, COL].Text = "Type";
                sheet[ROW, COL].ColumnWidth = 20;
                int colType = COL;



                #endregion columns

                int endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                {
                    sheet[ROW, colPlant].Text = dtOrderMaster.Rows[i]["Plant"].ToString();
                    sheet[ROW, colEntity].Text = dtOrderMaster.Rows[i]["Entity"].ToString();
                    sheet[ROW, colBuyer].Text = dtOrderMaster.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colCommitmentDate].Text = GetDate(dtOrderMaster.Rows[i]["CommitmentDate"].ToString());
                    sheet[ROW, colDeliveryDate].Text = GetDate(dtOrderMaster.Rows[i]["DeliveryDate"].ToString());
                    try { sheet[ROW, colDeliveryMonth].Number = Convert.ToDateTime(GetDate(dtOrderMaster.Rows[i]["DeliveryDate"].ToString())).Month; } catch { }
                    sheet[ROW, colMasterOrderNo].Text = dtOrderMaster.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colMaterial].Text = dtOrderMaster.Rows[i]["Material"].ToString();
                    sheet[ROW, colProductCategory].Text = dtOrderMaster.Rows[i]["ProductCategory"].ToString();
                    sheet[ROW, colProduct].Text = dtOrderMaster.Rows[i]["Product"].ToString();
                    sheet[ROW, colOrderStatus].Text = dtOrderMaster.Rows[i]["OrderStatus"].ToString();
                    sheet[ROW, colBuyerOrderNo].Text = dtOrderMaster.Rows[i]["BuyerReferenceNo"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = dtOrderMaster.Rows[i]["OwnReferenceNo"].ToString();
                    sheet[ROW, colOrderQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["TotalQty"].ToString());
                    sheet[ROW, colUOM].Text = dtOrderMaster.Rows[i]["UOM"].ToString();
                    sheet[ROW, colResponsiblePerson].Text = dtOrderMaster.Rows[i]["ResponsiblePerson"].ToString();
                    sheet[ROW, colType].Text = dtOrderMaster.Rows[i]["Type"].ToString();


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.Range[startRow, colOrderQty, ROW, colOrderQty].NumberFormat = clsStaticInfo.NumberFormat();

                IListObject table = sheet.ListObjects.Create("Table1", sheet[clsStaticInfo.GetxlsCol(1) + (6).ToString() + ":" + clsStaticInfo.GetxlsCol(endCol) + (ROW).ToString()]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange["A7"].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Master Order Detail", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;

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


                string fPath = fPath = HostingEnvironment.MapPath("~/") + "TempReport" + identity.UserId + ".xlsx";


                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }
                #region Buyer Summary
                workbook.Worksheets[0].Name = "Summary Buyer Wise";
                IWorksheet pivotSheet = workbook.Worksheets[0];

                IPivotCache cache = workbook.PivotCaches.Add(sheet[startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTableBuyer", pivotSheet["A6"], cache);

                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMasterOrderNo - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOrderStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCommitmentDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colUOM - 1].Axis = PivotAxisTypes.Row;


                pivotTable.Fields[colDeliveryMonth - 1].Axis = PivotAxisTypes.Column;
                IPivotField field = pivotTable.Fields[colOrderQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "", PivotSubtotalTypes.Sum);

                for (int i = 3; i < pivotTable.Fields.Count; i++)
                {
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }


                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Master Order Summary (Buyer Wise)", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                #endregion Buyer Summary


                #region Product Summary
                workbook.Worksheets[1].Name = "Summary Product Wise";
                pivotSheet = workbook.Worksheets[1];
                pivotTable = pivotSheet.PivotTables.Add("PivotTableProduct", pivotSheet["A6"], cache);

                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colBuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMasterOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colUOM - 1].Axis = PivotAxisTypes.Row;


                pivotTable.Fields[colDeliveryMonth - 1].Axis = PivotAxisTypes.Column;
                field = pivotTable.Fields[colOrderQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "", PivotSubtotalTypes.Sum);


                for (int i = 4; i < pivotTable.Fields.Count; i++)
                {
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }


                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                sheet = workbook.Worksheets[1];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Master Order Summary (Product Wise)", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;
                #endregion Buyer Summary





                string strFileName = "Master Order.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                excelEngine.Dispose();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

            return null;
        }

        [HttpGet, Authorize]
        public ActionResult LineBookingStatus(string entityid)
        {

            try
            {
                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Please select entity");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = ProductionOrderReports.LineBookingStatusXls(entityid, identity.PlantId);

                string strFileName = "Workcenter Booking Status.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

            return null;
        }

        private string EntityName(string entityId)
        {
            string entityName = "";
            if (entityId.Contains("'"))
            {

                //DataTable dt = _sqlRepository.GetDataTable("select * from org.entity where id IN (" + entityId + ")");
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    entityName += dt.Rows[0]["UserName"].ToString() + " ";
                //}

            }
            else
            {

                DataTable dt = _sqlRepository.GetDataTable("select * from org.entity where id='" + entityId + "'");
                if (dt.Rows.Count > 0)
                    entityName = "(" + dt.Rows[0]["UserName"].ToString() + ")";
            }
            return entityName;
        }

        private void getOrderMaster(string entityid, out DataTable dtOrderMaster)
        {
            //string sql = @"
            //                SELECT so.Id AS SalesOrderId, b.UserName AS Buyer,ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,mm.UserName AS Material,
            //                POD.ProductionOrderId,OC.UserName AS OrderCategory,os.UserName AS OrderStatus,ps.UserName  AS productionStatus,                          
            //                pc.UserName AS ProductCategory,  pm.UserName AS Product,
            //                so.DeliveryDate,so.CommitmentDate,so.Qty AS SOQty, cp.PONumber,format(cp.PODate,'dd-MMM-yyyy') AS PODate

            //                  FROM trn.MasterOrder MO
            //                left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
            //                INNER join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
            //                LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
            //                LEFT OUTER JOIN TRN.ProductionOrderDetail AS pod ON POD.SalesOrderId=SO.Id
            //                LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
            //                LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId


            //                left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
            //                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
            //                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
            //                left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

            //                left outer join [HKP].[Party] p on P.Id=MO.plantID
            //                left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
            //                left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
            //                left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
            //                left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
            //                left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
            //                left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
            //                left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
            //                left outer join mst.Destination DEST on dest.Id=so.DestinationId
            //                left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
            //                left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
            //                left outer join hkp.Season S on s.id=mo.SeasonId
            //                left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId

            //                WHERE os.Id='"+ Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' AND mo.EntityId='" + entityid + @"'
            //ORDER BY b.UserName,so.DeliveryDate,POD.ProductionOrderId,SO.ID";


            string sql = @" SELECT trkp.UserName AS Plant,trke.UserName AS Entity,trke.Id as EntityId,so.Id AS SalesOrderId, b.UserName AS Buyer,ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,mm.UserName AS Material,
                           OC.UserName AS OrderCategory,os.UserName AS OrderStatus,   MA.StandardName AS Article,                   
                            pc.UserName AS ProductCategory,  pm.UserName AS Product,moi.BuyerReferenceNo,MOI.OwnReferenceNo,
                            mo.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo OwnOrderNo,SO.Description AS SODesc,
                            mm.Id AS MaterialRowId,pod.ProductionOrderId,CASE WHEN isnull(sed.ID,0)<>0 THEN 'YES' ELSE 'NO' END AS isProductionScheduled,
                            so.DeliveryDate,so.CommitmentDate,so.Qty AS SOQty, cp.PONumber,format(cp.PODate,'dd-MMM-yyyy') AS PODate,ps.UserName AS ProductionStatus,
                            CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) AS PlannedQty,0 AS CummPlannedQty,
                           
                            --CEILING((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100))) AS PlannedQty,0 AS CummPlannedQty,
                            PO.Qty AS PRQty,case when isnull(SED.Qty,0)=0 THEN PO.PlannedQty ELSE  SED.Qty END AS PRActualPlannedQty,
                            PO.PlannedQty AS PRPlannedQty,P.UserName AS Customer
                              FROM trn.MasterOrder MO
                            left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
                            INNER join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
                            LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS SED ON sed.ProductionOrderID=pod.ProductionOrderId
                            LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                            LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                           

                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PO.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId

                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
							 --LEFT OUTER JOIN hkp.ProductGroup AS pg ON pg.Id=moi.pro
                           
                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId

                            WHERE PO.Id IN (SELECT DISTINCT p.ProductionOrderId FROM trn.ProductionOrderDetail AS p
                                            JOIN trn.SalesOrder AS so ON so.Id=p.SalesOrderId
                                            WHERE so.OrderStatusId<>'Closed') AND PO.EntityId IN (" + entityid + @")
            ORDER BY trkp.UserName,trke.UserName,trke.Id, pod.ProductionOrderId,so.DeliveryDate,SO.ID";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }
        private void _getOS1(string entityid, out DataTable dtOrderMaster)
        {


            string sql = @"	SELECT so.Id AS SalesOrderId,btn.BulletinId,btn.NoOfWS,btn.TotalSPT,
	con.Id ContractId,PA.UserName ContractName,M.LCRef LCNo,format(XCOM.ExpectedCompletionDate,'dd-MMM-yyyy') AS ExpectedCompletionDate,XCOM.Quantity SODistributedQty,
					format(mo.AddedDate,'dd-MMM-yyyy') AS MasterOrderCreationDate,PO.Remarks, 
					b.UserName AS Buyer,ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,mm.UserName AS Material,
                           OC.UserName AS OrderCategory,os.UserName AS OrderStatus,OC1.UserName AS SOCategory,os1.UserName AS SOStatus,    MA.StandardName AS Article,                   
                            pc.UserName AS ProductCategory,  pm.UserName AS Product,moi.BuyerReferenceNo,MOI.OwnReferenceNo,mo.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo,
                            mm.Id AS MaterialRowId,pod.ProductionOrderId,CASE WHEN isnull(sed.ID,0)<>0 THEN 'YES' ELSE 'NO' END AS isProductionScheduled,
                            so.DeliveryDate,so.CommitmentDate,so.Qty AS SOQty,SO.Reason, cp.PONumber,format(cp.PODate,'dd-MMM-yyyy') AS PODate,ps.UserName AS ProductionStatus
                            ,CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) AS PlannedQty
                            ,FORMAT(SO.AddedDate,'dd-MMM-yyyy') AS SOAddedDate,FORMAT(SO.MainRawMaterialInhouseDate,'dd-MMM-yyyy') AS MainRawMaterialInhouseDate ,FORMAT(SO.OtherRawMaterialInhouseDate,'dd-MMM-yyyy') AS OtherRawMaterialInhouseDate
                            
,CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate ELSE  so.Rate * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END AS FOB,
 CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM ELSE  so.CM * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END AS CM,
 (CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate ELSE  so.Rate * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)*SO.Qty AS OrderAmount,
 (CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM ELSE  so.CM * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)*SO.Qty AS CMAmount,
							                           
                            --,So.Rate*isnull(RT.ExchangeRate,1) AS FOB,
                            --SO.CM*isnull(RT.ExchangeRate,1) AS CM ,
                            --SO.Rate*SO.Qty*isnull(RT.ExchangeRate,1) AS OrderAmount,
                            --SO.CM*isnull(RT.ExchangeRate,1)*SO.Qty AS CMAmount,
                            FORMAT(SO.LSD,'dd-MMM-yyyy') AS LSD,isnull(DATEDIFF(DAY,so.LSD,so.DeliveryDate),0) AS Diff
                            ,uom.UserName AS UOM,so.[Description] AS SODesc,cur.Code AS Currency,trkp.UserName AS Plant,trke.UserName AS Entity,MOI.[Type]
                            ,PRPD.PRBookedQuantity,sopd.SOBookedQuantity,PLN.PRPlanQty,p.UserName AS Customer,PAG.UserName AS CustomerAccountGroup
                            ,SO.ProductionType,ShipmentFromStock=CASE WHEN SO.ShipmentFromStock=1 THEN 'Yes' ELSE 'No' END,PT.UserName PackingType
                              FROM trn.MasterOrder MO
                            left outer join MasterOrderExchangeRates RT on RT.TransactionId=MO.Id
							left JOIN org.Company AS com ON com.Id=mo.CompanyId
                            LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))

                            left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
                            left join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
							left outer join dbo.[Contract] con on con.Id=SO.ContractId
							left outer join HKP.Party PA on PA.Id=con.CustomerId
							left outer join MasterLC M on m.Id=con.MasterLCId
                            left join HKP.PackingType PT ON PT.Id=SO.PackingTypeId
                            left join [ExpectedSOWiseProductionCompletion] XCOM on XCOM.SalesOrderId=SO.Id
                            LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS SED ON sed.ProductionOrderID=pod.ProductionOrderId
                            LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                            LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                            LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId

                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = mo.PlantId
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = mo.EntityId

							LEFT JOIN (SELECT ps.ProductionOrderId,SUM(ps.Quantity) AS  PRBookedQuantity
                                            FROM trn.ProductionSummary AS ps 
                                            WHERE ISNULL(ps.SalesOrderId,'')='' AND ps.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=ps.ProductionOrderId)
                                       GROUP BY ps.ProductionOrderId) AS PRPD ON prpd.ProductionOrderId=pod.ProductionOrderId
                                       
                                        
                            LEFT JOIN (SELECT ps.SalesOrderId,SUM(ps.Quantity) AS  SOBookedQuantity
                                         FROM trn.ProductionSummary AS ps 
                                       WHERE ISNULL(ps.SalesOrderId,'')<>''  AND ps.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=ps.ProductionOrderId)
                                       GROUP BY ps.SalesOrderId) AS SOPD ON SOPD.SalesOrderId=so.Id

                         LEFT JOIN (SELECT ps.ProductionOrderID,SUM(ps.Quantity) AS  PRPlanQty
                                         FROM ProductionPlanningType1 AS ps 
                                       GROUP BY ps.ProductionOrderID) AS PLN ON PLN.ProductionOrderID=pod.ProductionOrderId

                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
							 --LEFT OUTER JOIN hkp.ProductGroup AS pg ON pg.Id=moi.pro
                           
                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
                            LEFT JOIN [HKP].[CompanyParty] AS COMP ON COMP.PartyId=P.Id AND COMP.PartyType='Customer' AND (TRKP.Id=COMP.PlantId OR isnull(COMP.PlantId,'')='')
                            LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=COMP.PartyAccountGroupId

                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join [HKP].[OrderCategory] OC1 on oc1.id=so.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS1 on OS1.id=so.OrderStatusId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
							LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId
							LEFT OUTER JOIN scs.Currency AS cur ON cur.Id=mo.CurrencyId
							left outer join (select pbt.Id BulletinId,pbt.productionOrderId,pbtm.MaxNoOfWS NoOfWS,sum( pbtd.TotalSPT ) TotalSPT from trn.ProductionBulletinTemplate pbt
left outer join trn.ProductionBulletinTemplateMaster pbtm on pbtm.ProductionBulletinTemplateId=pbt.id
left outer join trn.ProductionBulletinTemplateDetail pbtd on pbtd.ProductionBulletinTemplateMasterId=pbtm.Id
AND  pbtm.ProcessId=(select top 1 sx.ProcessId from trn.ProductionOrderProcessSet SX where SX.ProductionOrderId=pbt.productionOrderId and isnull(SX.IsBaseProcess,0)=1)
group by pbt.productionOrderId,pbtm.MaxNoOfWS, pbt.Id ) Btn on Btn.ProductionOrderId=po.Id
--left outer join BOQ on boq.SalesOrderId=so.Id and boq.SalesOrderId=(select top 1 SalesOrderId from boq where SalesOrderId=so.Id)
                            WHERE os.Id='" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' AND mo.EntityId IN (" + entityid + @")
            ORDER BY	trkp.UserName,trke.UserName,PAG.UserName DESC, p.UserName, b.UserName,convert(date,so.DeliveryDate),SO.ID";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }
        private void getOS1(string entityid, string fromDate, string toDate, string productionStatusList, out DataTable dtOrderMaster)
        {
            string orderStatusIds = "'" + productionStatusList.Replace(",", "','") + "'";//replaced with ""

            string sql = @"	SELECT so.Id AS SalesOrderId,btn.BulletinId,btn.NoOfWS,btn.TotalSPT,
	con.Id ContractId,PA.UserName ContractName,M.LCRef LCNo,format(XCOM.ExpectedCompletionDate,'dd-MMM-yyyy') AS ExpectedCompletionDate,XCOM.Quantity SODistributedQty,
					format(mo.AddedDate,'dd-MMM-yyyy') AS MasterOrderCreationDate,PO.Remarks, 
					b.UserName AS Buyer,ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,mm.UserName AS Material,
                           OC.UserName AS OrderCategory,os.UserName AS OrderStatus,OC1.UserName AS SOCategory,os1.UserName AS SOStatus,    MA.StandardName AS Article,                   
                            pc.UserName AS ProductCategory,  pm.UserName AS Product,moi.BuyerReferenceNo,MOI.OwnReferenceNo,mo.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo,
                            mm.Id AS MaterialRowId,pod.ProductionOrderId,CASE WHEN isnull(sed.ID,0)<>0 THEN 'YES' ELSE 'NO' END AS isProductionScheduled,
                            so.DeliveryDate,so.CommitmentDate,so.Qty AS SOQty,SO.Reason, cp.PONumber,format(cp.PODate,'dd-MMM-yyyy') AS PODate,ps.UserName AS ProductionStatus
                            ,CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) AS PlannedQty
                            ,(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))*PPS.Qty) ProcessPlanQty
                            ,FORMAT(SO.AddedDate,'dd-MMM-yyyy') AS SOAddedDate,FORMAT(SO.MainRawMaterialInhouseDate,'dd-MMM-yyyy') AS MainRawMaterialInhouseDate ,FORMAT(SO.OtherRawMaterialInhouseDate,'dd-MMM-yyyy') AS OtherRawMaterialInhouseDate
                            
,CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate ELSE  so.Rate * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END AS FOB,
 CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM ELSE  so.CM * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END AS CM,
 (CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate ELSE  so.Rate * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)*SO.Qty AS OrderAmount,
 (CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM ELSE  so.CM * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)*SO.Qty AS CMAmount,
							                           
                            --,So.Rate*isnull(RT.ExchangeRate,1) AS FOB,
                            --SO.CM*isnull(RT.ExchangeRate,1) AS CM ,
                            --SO.Rate*SO.Qty*isnull(RT.ExchangeRate,1) AS OrderAmount,
                            --SO.CM*isnull(RT.ExchangeRate,1)*SO.Qty AS CMAmount,
                            FORMAT(SO.LSD,'dd-MMM-yyyy') AS LSD,isnull(DATEDIFF(DAY,so.LSD,so.DeliveryDate),0) AS Diff
                            ,uom.UserName AS UOM,so.[Description] AS SODesc,cur.Code AS Currency,trkp.UserName AS Plant,trke.UserName AS Entity,MOI.[Type]
                            ,PRPD.PRBookedQuantity,sopd.SOBookedQuantity,PLN.PRPlanQty,p.UserName AS Customer,PAG.UserName AS CustomerAccountGroup
                            ,SO.ProductionType,ShipmentFromStock=CASE WHEN SO.ShipmentFromStock=1 THEN 'Yes' ELSE 'No' END,PT.UserName PackingType
                            ,PPS.[Sequence] ProcessSequence,FLB.ProcessStartDate,FLB.ProcessEndDate,ProcessDate=FORMAT(DATEADD(DAY, PPS.[Days], FLB.ProcessStartDate),'dd-MMM-yyyy')
                              FROM trn.MasterOrder MO
                            left outer join MasterOrderExchangeRates RT on RT.TransactionId=MO.Id
							left JOIN org.Company AS com ON com.Id=mo.CompanyId
                            LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))

                            left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
                            left join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
                            LEFT JOIN dbo.[Contract]  CON on CON.Id = SO.ContractId
							left outer join HKP.Party PA on PA.Id=con.CustomerId
							left outer join MasterLC M on m.Id=con.MasterLCId
                            left join HKP.PackingType PT ON PT.Id=SO.PackingTypeId
                            left join [ExpectedSOWiseProductionCompletion] XCOM on XCOM.SalesOrderId=SO.Id
                            LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS SED ON sed.ProductionOrderID=pod.ProductionOrderId
                            LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId

                            LEFT OUTER JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderId=PO.Id
							LEFT JOIN (
							Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS ProcessStartDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS ProcessEndDate,ProductionOrderId,ProcessId 
							from TRN.ProductionSummary GROUP BY ProductionOrderId,ProcessId
							) FLB ON FLB.ProductionOrderId=po.Id AND FLB.ProcessId=PPS.ProcessId

                            LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                            LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId

                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = mo.PlantId
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = mo.EntityId

							LEFT JOIN (SELECT ps.ProductionOrderId,SUM(ps.Quantity) AS  PRBookedQuantity
                                            FROM trn.ProductionSummary AS ps 
                                            WHERE ISNULL(ps.SalesOrderId,'')='' AND ps.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=ps.ProductionOrderId)
                                       GROUP BY ps.ProductionOrderId) AS PRPD ON prpd.ProductionOrderId=pod.ProductionOrderId
                                       
                                        
                            LEFT JOIN (SELECT ps.SalesOrderId,SUM(ps.Quantity) AS  SOBookedQuantity
                                         FROM trn.ProductionSummary AS ps 
                                       WHERE ISNULL(ps.SalesOrderId,'')<>''  AND ps.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=ps.ProductionOrderId)
                                       GROUP BY ps.SalesOrderId) AS SOPD ON SOPD.SalesOrderId=so.Id

                         LEFT JOIN (SELECT ps.ProductionOrderID,SUM(ps.Quantity) AS  PRPlanQty
                                         FROM ProductionPlanningType1 AS ps 
                                       GROUP BY ps.ProductionOrderID) AS PLN ON PLN.ProductionOrderID=pod.ProductionOrderId

                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
							 --LEFT OUTER JOIN hkp.ProductGroup AS pg ON pg.Id=moi.pro
                           
                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
                            LEFT JOIN [HKP].[CompanyParty] AS COMP ON COMP.PartyId=P.Id AND COMP.PartyType='Customer' AND (TRKP.Id=COMP.PlantId OR isnull(COMP.PlantId,'')='')
                            LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=COMP.PartyAccountGroupId

                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join [HKP].[OrderCategory] OC1 on oc1.id=so.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS1 on OS1.id=so.OrderStatusId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
							LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId
							LEFT OUTER JOIN scs.Currency AS cur ON cur.Id=mo.CurrencyId
							left outer join (select pbt.Id BulletinId,pbt.productionOrderId,pbtm.MaxNoOfWS NoOfWS,sum( pbtd.TotalSPT ) TotalSPT from trn.ProductionBulletinTemplate pbt
left outer join trn.ProductionBulletinTemplateMaster pbtm on pbtm.ProductionBulletinTemplateId=pbt.id
left outer join trn.ProductionBulletinTemplateDetail pbtd on pbtd.ProductionBulletinTemplateMasterId=pbtm.Id
AND  pbtm.ProcessId=(select top 1 sx.ProcessId from trn.ProductionOrderProcessSet SX where SX.ProductionOrderId=pbt.productionOrderId and isnull(SX.IsBaseProcess,0)=1)
group by pbt.productionOrderId,pbtm.MaxNoOfWS, pbt.Id ) Btn on Btn.ProductionOrderId=po.Id
--left outer join BOQ on boq.SalesOrderId=so.Id and boq.SalesOrderId=(select top 1 SalesOrderId from boq where SalesOrderId=so.Id)
                            WHERE os.Id IN(" + orderStatusIds +@") AND mo.EntityId IN (" + entityid + @") AND mo.AddedDate between '"+fromDate+@"' AND '"+toDate+@"'
            ORDER BY trkp.UserName,trke.UserName,PAG.UserName DESC, p.UserName, b.UserName,convert(date,so.DeliveryDate),SO.ID";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }
        private void getOS4(string entityid, string fromDate, out DataTable dtOrderMaster)
        {
            string sql = @"
                          --plan
SELECT pp.EntityID, PP.ProductionOrderID,wcm.UserName AS WorkCenter, PP.ProductionDate AS PlanDate,pp.Quantity AS PlanQty,ORD.CM*pp.Quantity AS PlanCM,
--additional info
			buyer=STUFF((select distinct ','+XB.UserName from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			
			StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

--actual
PROD.ProductionOrderId AS ActualProductionOrderId,PROD.ProductionDate,PROD.WorkCenterMasterId AS ActualWorkCenterMasterId,Awcm.UserName AS ActualWorkCenter,
prod.buyer AS ActualBuyer,prod.StyleNo AS ActualStyle,
PROD.Quantity AS ProductionQty,PROD.ActualCM,
pt1.NoOfWorkStation,pp.ProductionHours,
--comparison
ISNULL(pp.Quantity,0)-ISNULL(prod.Quantity,0) AS DifferenceInProduction,
ISNULL(prod.Quantity,0)/ISNULL(pp.Quantity,0)*100 AS PerformanceInPercentage,
ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0) AS PlanMinutes,
ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*pp.ProductionHours*60)*100 AS PlanEfficiency,


ISNULL(PROD.Quantity,0)*isnull(pt1.SPT,0) AS ProduceMinutes,
ISNULL(prod.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*pp.ProductionHours*60)*100 AS ProductionEfficiency



FROM ProductionPlanningSnapshot2Type1 AS pp
LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
FULL OUTER JOIN (
	
		SELECT ps.EntityId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,SUM(ps.Quantity) AS Quantity,SUM(ps.Quantity)*ord.CM AS ActualCM,
		--additional info
			buyer=STUFF((select distinct ','+XB.UserName from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			where ps.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			
			StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			where ps.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		FROM trn.ProductionSummary AS ps 
		 left outer join (
                            select POD.ProductionOrderId,
                            SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate* so.Qty ELSE  so.Rate* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(so.Qty) AS FOB,
                            SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE  so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty) AS CM
                            from trn.ProductionOrderDetail POD 
                            left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                            left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
	                        left JOIN org.Company AS com ON com.Id=mo.CompanyId
                            LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
                            group by POD.ProductionOrderId
                            ) AS ORD on ord.ProductionOrderID=ps.ProductionOrderId
		WHERE ps.ProductionDate>='" + fromDate + @"' AND ps.EntityID='" + entityid + @"' AND ps.ProcessId=(select XX.ProcessId from trn.ProductionOrderProcessSet AS XX where XX.IsBaseProcess=1 and XX.ProductionOrderID=ps.ProductionOrderId)
		GROUP BY ps.EntityId,ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ord.CM

) AS PROD ON 
pp.ProductionOrderID=prod.ProductionOrderId  AND pp.EntityID=prod.EntityId
AND FORMAT(pp.ProductionDate,'dd-MMM-yyyy')=format(prod.ProductionDate,'dd-MMM-yyyy') 
AND pp.WorkCenterMasterId=prod.WorkCenterMasterId
LEFT OUTER JOIN scs.WorkCenterMaster AS Awcm ON Awcm.Id=PROD.WorkCenterMasterId

 left outer join (
                            select POD.ProductionOrderId,
                            SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate* so.Qty ELSE  so.Rate* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(so.Qty) AS FOB,
                            SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE  so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty) AS CM
                            from trn.ProductionOrderDetail POD 
                            left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                            LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
                            left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
                            left JOIN org.Company AS com ON com.Id=mo.CompanyId
                            LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            group by POD.ProductionOrderId
                            ) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId

WHERE pp.ProductionDate>='" + fromDate + @"' AND pp.EntityID='" + entityid + @"'

ORDER BY PP.ProductionOrderID, PP.WorkCenterMasterId, PP.ProductionDate,PROD.ProductionOrderID, PROD.WorkCenterMasterId, PROD.ProductionDate";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }

        private void getOS4_PlanData(string entityid, string fromDate, string toDate, out DataTable dtOrderMaster)
        {
            string sql = @"SELECT  trkp.UserName AS Plant,trke.UserName AS Entity,pp.EntityID,pp.WorkCenterMasterId, PP.ProductionOrderID,wcm.UserName AS WorkCenter, 
FORMAT(PP.ProductionDate,'dd-MMM-yyyy') AS PlanDate,pp.Quantity AS PlanQty,ORD.CM*pp.Quantity AS PlanCM,ORD.CM,
pt1.SPT AS SAM,0 AS Manpower,ord.Material,ord.Article,
ord.Product, ord.ProductCategory,Format(pp.AddedDate,'dd-MMM-yyyy') AS SnapshotDate,

 wcm.StandardTimePerDay AS StandardWorkingHours,  wcm.NoOfWorkStation AS StandardWorkStations,wcm.DailyFixedCost,wcm.VariableCost AS VariableCostPerHour,
 PP.ProductionHours AS WorkingHours,PP.isBuildUp,
 pt1.TargetPerDay AS LineTargetPerDay,PT1.TargetPerHour AS PlanTargetPerHour,PT1.PlanWorkingHoursPerDay,
--additional info
			buyer=STUFF((select distinct ','+XB.UserName from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

    Customer=STUFF((select distinct ','+XB.UserName from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			left outer join [HKP].Party XB on XB.Id=XMO.PartyId
			where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

	SalesOrderIds=STUFF((select distinct ','+XSO.Id from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

	SalesOrderDesc=STUFF((select distinct ','+XSO.Description from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

        MasterOrderNo=STUFF((select distinct ','+XMO.Id from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

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

			
		StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
        OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

pt1.NoOfWorkStation,pp.ProductionHours AS PlanHours,0 AS ProductionHours,
ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0) AS PlanMinutes,
ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*pp.ProductionHours*60) AS PlanEfficiency


FROM ProductionPlanningSnapshot2Type1 AS pp
LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId

LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PO.EntityId
LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
 left outer join (
                            select POD.ProductionOrderId,mm.UserName AS Material,MA.StandardName AS Article,PM.UserName AS Product,PC.UserName AS ProductCategory,
                                    SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate* so.Qty ELSE  so.Rate* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(so.Qty) AS FOB,
                                    SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE  so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty) AS CM    
                        from trn.ProductionOrderDetail POD 
                            left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                            left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
                            left JOIN org.Company AS com ON com.Id=mo.CompanyId
                            LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                            LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                            group by mm.UserName,MA.StandardName,PM.UserName,PC.UserName,POD.ProductionOrderId
                            ) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId

WHERE pp.ProductionDate  BETWEEN '" + fromDate + @"' AND '" + toDate + @"'  AND wcm.EntityID in (" + entityid + @")

ORDER BY PP.ProductionDate, PP.WorkCenterMasterId, PP.ProductionOrderID
";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }
        private void getOS4_ActualData(string entityid, string fromDate, string toDate, out Dictionary<string, List<DataRow>> dtOrderMaster)
        {
            string sql = @"SELECT  trkp.UserName AS Plant,trke.UserName AS Entity,pp.EntityID,pp.WorkCenterMasterId, PP.ProductionOrderID,wcm.UserName AS WorkCenter,FORMAT(PP.ProductionDate,'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty,ORD.CM*pp.Quantity AS ActualCM,
                            pt1.SPT AS SAM,ord.Material,ord.Article,
                            ord.Product, ord.ProductCategory,Format(SN.AddedDate,'dd-MMM-yyyy') AS SnapshotDate,
                            sn.Quantity AS PlanQty,ORD.CM*sn.Quantity AS PlanCM,ORD.CM,

 wcm.StandardTimePerDay AS StandardWorkingHours,  wcm.NoOfWorkStation AS StandardWorkStations,wcm.DailyFixedCost,wcm.VariableCost AS VariableCostPerHour,
 PP.ProductionHours AS WorkingHours,SN.isBuildUp,
 pt1.TargetPerDay AS LineTargetPerDay,PT1.TargetPerHour AS PlanTargetPerHour,PT1.PlanWorkingHoursPerDay,
                            --additional info
			                     buyer=STUFF((select distinct ','+XB.UserName from 
			                            trn.SalesOrder XSO 
			                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                            where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                Customer=STUFF((select distinct ','+XB.UserName from 
			                            trn.SalesOrder XSO 
			                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                            left outer join [HKP].Party XB on XB.Id=XMO.PartyId
			                            where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            SalesOrderIds=STUFF((select distinct ','+XSO.Id from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                        where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

	                        SalesOrderDesc=STUFF((select distinct ','+XSO.Description from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                        where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                   
                                  MasterOrderNo=STUFF((select distinct ','+XMO.Id from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

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

			
		                                StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
                                        OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            pt1.NoOfWorkStation,sn.ProductionHours  AS PlanHours,
                            ISNULL(ppt.ProductionHours,ISNULL(ppc.WorkingHours,0)+ISNULL(ppc.OTHours,0)) ProductionHours,
                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0) AS PlanMinutes,
                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*sn.ProductionHours*60) AS PlanEfficiency,

                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0) AS ActualMinutes,
                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*pp.ProductionHours*60) AS ActualEfficiency

                            FROM (SELECT  ps.ProcessId,ps.EntityId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,COUNT(*) AS ProductionHours,SUM(ps.Quantity) AS Quantity
                                    FROM trn.ProductionSummary AS ps 
      		                            WHERE ps.ProductionDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND ps.EntityID in (" + entityid + @") 
      		                            AND ps.ProcessId=(select XX.ProcessId from trn.ProductionOrderProcessSet AS XX where XX.IsBaseProcess=1 and XX.ProductionOrderID=ps.ProductionOrderId)
                                  GROUP BY   ps.ProcessId,  ps.EntityId,ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId
                            ) AS pp
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
                            LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=pp.ProductionOrderId AND sn.ProductionDate=pp.ProductionDate AND sn.WorkCenterMasterId=pp.WorkCenterMasterId AND sn.EntityID=pp.EntityId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
                            left outer join ProductionPlanningType1 AS ppt on ppt.WorkCenterMasterId=PP.WorkCenterMasterId AND  ppt.ProcessId=PP.ProcessId AND ppt.EntityId=pp.EntityId and ppt.ProductionDate=PP.ProductionDate
                            left outer join ProductionPlanningCalendar AS ppc on ppc.ProcessId=PP.ProcessId AND ppc.EntityId=pp.EntityId and PPC.WorkingDate=PP.ProductionDate
                            left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID

                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PO.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
                             left outer join (
                                                        select POD.ProductionOrderId,mm.UserName AS Material,MA.StandardName AS Article,PM.UserName AS Product,PC.UserName AS ProductCategory,
                                                            SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate* so.Qty ELSE  so.Rate* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(so.Qty) AS FOB,
                                                            SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE  so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty) AS CM
                                                            from trn.ProductionOrderDetail POD 
                                                        left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                                                        left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                        left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                                                        left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
                                                        left JOIN org.Company AS com ON com.Id=mo.CompanyId
                                                        LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                                                        LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + entityid + @"))
                                                        LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
                                                        left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                                        LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                        left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                        left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                        left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                                        group by mm.UserName,MA.StandardName,PM.UserName,PC.UserName,POD.ProductionOrderId
                                              ) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId



                            ORDER BY PP.ProductionDate, PP.WorkCenterMasterId, PP.ProductionOrderID
";

            DataTable dt = _sqlRepository.GetDataTable(sql);
            dtOrderMaster = new Dictionary<string, List<DataRow>>();
            List<DataRow> drtemp = new List<DataRow>();
            string _id = "";
            foreach (DataRow item in dt.Rows)
            {
                if (_id != item["ActualDate"].ToString() + item["WorkCenterMasterId"].ToString())
                {
                    drtemp = new List<DataRow>();
                    _id = item["ActualDate"].ToString() + item["WorkCenterMasterId"].ToString();
                    dtOrderMaster.Add(_id, drtemp);


                }

                drtemp.Add(item);
            }




        }
        private void getProductionData(string entityid, string fromDate, string toDate, out Dictionary<string, List<DataRow>> dtOrderMaster)
        {
            try
            {

                string sql = @"SELECT A.* from (SELECT distinct PP.Id, trkp.UserName AS Plant,trke.UserName AS Entity,pp.EntityID,pp.WorkCenterMasterId, PP.ProductionOrderID,wcm.UserName AS WorkCenter,FORMAT(PP.ProductionDate,'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty
							,(select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
							where PO.Id=PoD.ProductionOrderId)*pp.Quantity AS ActualCM
                            ,pt1.SPT AS SAM,pp.ProcessId,isnull(p.UserName,FSFG.UserName) AS Process
							,pops.[Sequence] ProcessSequence
							, BaseProcessFlag = case when pops.IsBaseProcess=0 then 'No' else 'Yes' end
							,isnull(Tp.UserName,TSFG.UserName) AS ToProcess,Twcm.UserName AS ToWorkCenter
							,Material=STUFF((select distinct ','+MA.UserName from
											MST.MaterialMaster MA
											left join TRN.MasterOrderItem moi on moi.MaterialMasterId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
											where PO.Id=PoD.ProductionOrderId for xml path('') ), 1, 1, '')
							,Article=STUFF((select distinct ','+MA.StandardName from
											MST.MaterialMasterArticle MA
											left join TRN.MasterOrderItem moi on moi.ArticleId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
											where PO.Id=PoD.ProductionOrderId for xml path('') ), 1, 1, '')
                          ,Product=STUFF((select distinct ','+PM.UserName from
											MST.MaterialMaster mm
											left join TRN.MasterOrderItem moi on moi.MaterialMasterId=mm.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
											INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
											where PO.Id=PoD.ProductionOrderId for xml path('') ), 1, 1, '')						   
								,ProductCategory=STUFF((select distinct ','+pc.UserName from
								[HKP].[ProductCategory] PC
								left join [MST].[ProductMaster] PM on pc.Id=pm.ProductCategoryId
								left join trn.ProductDefinition AS pd ON pd.ProductMasterId=pm.Id
								left join mst.MaterialMaster mm on mm.id=pd.MaterialMasterId
								left join TRN.MasterOrderItem moi on moi.MaterialMasterId=MM.Id
								left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
								INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
								where PO.Id=PoD.ProductionOrderId for xml path('') ), 1, 1, '')
							,Format(SN.AddedDate,'dd-MMM-yyyy') AS SnapshotDate,
                            sn.Quantity AS PlanQty
							,(select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
							where PO.Id=PoD.ProductionOrderId)*sn.Quantity AS PlanCM

							,CM=(select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
							where PO.Id=PoD.ProductionOrderId)

                             ,CPL.UserName AS ProductionShift,so.Id AS SalesOrderIdBooking,CPL.ShiftDuration ShiftWorkingMin,so.[Description] AS SalesOrderDescBooking,
                             wcm.StandardTimePerDay AS StandardWorkingHours,  wcm.NoOfWorkStation AS StandardWorkStations,wcm.DailyFixedCost,wcm.VariableCost AS VariableCostPerHour,
                             PP.ProductionHours AS WorkingHours,SN.isBuildUp,
                             pt1.TargetPerDay AS LineTargetPerDay,PT1.TargetPerHour AS PlanTargetPerHour,PT1.PlanWorkingHoursPerDay,
                            --additional info
			                     buyer=STUFF((select distinct ','+XB.UserName from 
			                            trn.SalesOrder XSO 
			                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                            where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,LineItemId=STUFF((select distinct ', '+moi.Id from
											TRN.MasterOrderItem moi
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											INNER JOIN TRN.ProductionOrderDetail PD ON pd.SalesOrderId=xp.id
											where pp.ProductionOrderID=PD.ProductionOrderId for xml path('') ), 1, 1, '')
							
							
							,ProductGroup=STUFF((select distinct ', '+moi.ProductionGrouping from
											TRN.MasterOrderItem moi
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											INNER JOIN TRN.ProductionOrderDetail PD ON pd.SalesOrderId=xp.id
											where pp.ProductionOrderID=PD.ProductionOrderId for xml path('') ), 1, 1, '')

							,ProductName=STUFF((select distinct ','+MA.UserName from
												[dbo].[ProductLibrary] MA
												left join TRN.MasterOrderItem moi on moi.ProductLibraryId=MA.Id
												left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
												INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
												where pp.ProductionOrderID=PoD.ProductionOrderId for xml path('') ), 1, 1, '')

							,ProductCode=STUFF((select distinct ','+MA.Code from
												[dbo].[ProductLibrary] MA
												left join TRN.MasterOrderItem moi on moi.ProductLibraryId=MA.Id
												left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
												INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
												where pp.ProductionOrderID=PoD.ProductionOrderId for xml path('') ), 1, 1, '')
                            ,SalesOrderIds=STUFF((select distinct ', '+XSO.Id from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                        where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

	                        SalesOrderDesc=STUFF((select distinct ','+XSO.Description from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                        where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                   
                                  MasterOrderNo=STUFF((select distinct ','+XMO.Id from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

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

			
		                                StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
                                        OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            NULLIF(wcm.NoOfWorkStation,0)NoOfWorkStation,sn.ProductionHours  AS PlanHours,
                            ISNULL(ppt.ProductionHours,0) ProductionHours,
                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0) AS PlanMinutes,
                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0)/(NULLIF(wcm.NoOfWorkStation,0)*sn.ProductionHours*60) AS PlanEfficiency,

                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0) AS ActualMinutes,
                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0)/(NULLIF(wcm.NoOfWorkStation,0)*pp.ProductionHours*60) AS ActualEfficiency
							--,isnull(MMT.[Minute],0) DetentionInMin
,0 Utilization,pp.ProductionOrderId PORefNo,pp.AddedBy EntryBy,UOM.Code UOM,pp.Quantity ProductionQty,pp.Remarks
							,PartyId=STUFF((select distinct ', '+P.Id from HKP.Party P
											left join trn.MasterOrder MO on mo.PartyId=P.Id
											left join  TRN.MasterOrderItem moi on moi.MasterOrderId=mo.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											INNER JOIN TRN.ProductionOrderDetail PD ON pd.SalesOrderId=xp.id
											where pp.ProductionOrderID=PD.ProductionOrderId for xml path('') ), 1, 1, '')
                            ,Customer=STUFF((select distinct ', '+P.UserName from HKP.Party P
											left join trn.MasterOrder MO on mo.PartyId=P.Id
											left join  TRN.MasterOrderItem moi on moi.MasterOrderId=mo.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											INNER JOIN TRN.ProductionOrderDetail PD ON pd.SalesOrderId=xp.id
											where pp.ProductionOrderID=PD.ProductionOrderId for xml path('') ), 1, 1, '')
,PST.UserName ProductionStatus,PP.LotNo
                            FROM (SELECT  ps.Id,ps.ProcessId,ps.LotNumber LotNo,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,COUNT(*) AS ProductionHours,ps.Quantity
									,PS.AddedBy,ps.Remarks,ps.ProductLibraryId
                                    FROM trn.ProductionSummary AS ps 
                                  left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
                                  LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
      		                      WHERE ps.ProductionDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND ps.EntityID in (" + entityid + @") 
                                  GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,PS.AddedBy,ps.Remarks,ps.ProductLibraryId,ps.Quantity,ps.LotNumber
                            ) AS pp

							--left join MachineMasterTransaction MMT on MMT.ProcessId=pp.ProcessId and MMT.ShiftId=pp.ProductionShiftId and MMT.WorkCenterId=pp.WorkCenterMasterId and MMT.[Date]=pp.ProductionDate
                            LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId=pp.ProductionShiftId
							
                            LEFT JOIN trn.SalesOrder AS so ON so.Id=pp.SalesOrderId
							left join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
							left join SCS.UnitOfMeasurement UOM on UOM.Id=MOI.UOMId
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
                            LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=pp.ProductionOrderId AND sn.ProductionDate=pp.ProductionDate AND sn.WorkCenterMasterId=pp.WorkCenterMasterId AND sn.EntityID=pp.EntityId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id=pp.FromSFGInventoryId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id=pp.ToWorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id=pp.ToSFGInventoryId                        
                            left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID=pp.ProductionOrderId AND ppt.WorkCenterMasterId=PP.WorkCenterMasterId AND  ppt.ProcessId=PP.ProcessId AND ppt.EntityId=pp.EntityId and ppt.ProductionDate=PP.ProductionDate
                            left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
left join HKP.ProductionStatus PST on PST.Id=PO.ProductionStatusId
							LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
							LEFT OUTER JOIN hkp.Process AS Tp ON Tp.Id=pp.ToProcessId
							left join trn.productionorderprocessset pops on pops.ProductionOrderId=po.Id AND P.Id=POPS.ProcessId
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
             )A ORDER BY A.ActualDate, A.WorkCenterMasterId, A.ProductionOrderID";

                DataTable dt = _sqlRepository.GetDataTable(sql);
                dtOrderMaster = new Dictionary<string, List<DataRow>>();
                List<DataRow> drtemp = new List<DataRow>();
                string _id = "";
                foreach (DataRow item in dt.Rows)
                {
                    if (_id != item["ActualDate"].ToString() + item["WorkCenterMasterId"].ToString() + item["Id"].ToString() + item["PartyId"].ToString())
                    {
                        drtemp = new List<DataRow>();
                        _id = item["ActualDate"].ToString() + item["WorkCenterMasterId"].ToString() + item["Id"].ToString() + item["PartyId"].ToString();
                        dtOrderMaster.Add(_id, drtemp);


                    }

                    drtemp.Add(item);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }



        }

        private void getOrderMasterMain(string entityid, string status, out DataTable dtOrderMaster)
        {

            if (status.ToUpper() == Library.Model.Enums.OrderStatusEnum.Closed.ToString().ToUpper())
                status = "os.Id='" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"'";
            else
                status = "os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"'";

            string sql = @"  SELECT trkp.UserName AS Plant,trke.UserName AS Entity, b.UserName AS Buyer,ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,mm.UserName AS Material,
                           OC.UserName AS OrderCategory,os.UserName AS OrderStatus, mo.BuyerReferenceNo,MO.OwnReferenceNo,P.UserName AS Customer,                         
                            pc.UserName AS ProductCategory,  pm.UserName AS Product,MO.TotalQty,uom.UserName AS UOM,MO.[Type],
                            MIN(so.DeliveryDate)DeliveryDate,MIN(so.CommitmentDate) AS CommitmentDate

                              FROM trn.MasterOrder MO
                            left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
                            left join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
                            LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                            
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id=mo.PlantId
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id=mo.EntityId


                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
							LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId

                            WHERE " + status + @" AND mo.EntityId IN (" + entityid + @")
                            
                              
                            GROUP BY MO.[Type],P.UserName,MO.TotalQty, trkp.UserName,trke.UserName, b.UserName,ei.EmployeeName,mo.MasterOrderNo,mm.UserName,
                            OC.UserName,os.UserName, mo.BuyerReferenceNo,MO.OwnReferenceNo,                       
                            pc.UserName,  pm.UserName,uom.UserName
                            ORDER BY mo.MasterOrderNo";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }
        private void getOrderMasterWithSalesOrderMain(string entityid, out DataTable dtOrderMaster)
        {
            string sql = @"  SELECT p2.UserName AS Plant,e.UserName AS Entity, b.UserName AS Buyer,ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,mm.UserName AS Material,
                           OC.UserName AS OrderCategory,os.UserName AS OrderStatus, mo.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo,                         
                            pc.UserName AS ProductCategory,  pm.UserName AS Product,MOI.TotalQty AS ItemQty,uom.UserName AS UOM,
                           MOI.Id AS LineItemId,so.Id AS SalesOrderId,so.DeliveryDate, so.DestinationId,dest.UserName AS Destination,
                           so.CommitmentDate, so.ShipmentModeId,smo.UserName AS ShipMode, 
                           so.Qty, so.MainRawMaterialInhouseDate,P.UserName AS Customer,
                           so.OtherRawMaterialInhouseDate, so.PlanExFactoryDate

                              FROM trn.MasterOrder MO
                              LEFT JOIN org.Plant AS p2 ON p2.id=mo.PlantId
                              LEFT JOIN org.Entity AS e ON e.Id=mo.EntityId
                            left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
                            LEFT join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
                            LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId


                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
							LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId

                            WHERE os.Id='" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' AND mo.EntityId='" + entityid + @"'
                            
                          
ORDER BY p2.UserName,e.UserName, mo.MasterOrderNo";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }


        [HttpGet, Authorize]
        public ActionResult getOrderMasterDummy()
        {
            string sql = @"
                            SELECT so.Id AS SalesOrderId, b.UserName AS Buyer,ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,mm.UserName AS Material,
                            POD.ProductionOrderId,OC.UserName AS OrderCategory,os.UserName AS OrderStatus,ps.UserName  AS productionStatus,
                            so.DeliveryDate,so.CommitmentDate,so.Qty AS SOQty,P.UserName AS Customer

                              FROM trn.MasterOrder MO
                            left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
                            INNER join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
                            LEFT OUTER JOIN TRN.ProductionOrderDetail AS pod ON POD.SalesOrderId=SO.Id
                            LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId


                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId

            ORDER BY b.UserName,so.DeliveryDate DESC";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

        }

        private void getSalesOrderDistributionBackup(string date, string entityid, out Dictionary<string, ProductionQtyDistributionSO> dicDistributedSO, out DataTable dt)
        {

            string sql = @"SELECT moi.MasterOrderId, po.Id AS ProductionOrderID,so.Id AS SalesOrderID,ord.OrderQty,PRODPR.ProductionQtyAtPR AS ProductionUptoPreviousDay,
ISNULL(p1.Quantity,0) AS PlanQtyForToday,ISNULL(PRODPRTODAY.ProductionQtyAtPR,0) ProducedQtyToday,

--last day production+today production + today's planning remaining after todays production
so.DeliveryDate, so.Qty AS SOQty,0 AS DistributedQty,0 AS DistributedQtyOnPlan,
CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) AS PlannedQty,
                           
--CEILING((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100))) AS PlannedQty,

(ISNULL(prodpr.ProductionQtyAtPR,0)+ISNULL(PRODPRTODAY.ProductionQtyAtPR,0)) AS CumulativeQty,
ISNULL(PF1.Quantity,0)+
(CASE WHEN ISNULL(p1.Quantity,0)-ISNULL(PRODPRTODAY.ProductionQtyAtPR,0)>0 THEN ISNULL(p1.Quantity,0)-ISNULL(PRODPRTODAY.ProductionQtyAtPR,0) ELSE 0 END) AS FuturePlanQty

from trn.ProductionOrder PO
LEFT OUTER JOIN (SELECT p1.ProductionOrderID,ProcessID,SUM(p1.Quantity) AS Quantity
                   from ProductionPlanningType1 p1 
                 WHERE p1.ProductionDate='" + date + @"' 
                 GROUP BY  ProcessID,p1.ProductionOrderID) AS P1 ON  p1.ProductionOrderID=po.Id AND ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id) 

LEFT OUTER JOIN (SELECT p1.ProductionOrderID,ProcessID,SUM(p1.Quantity) AS Quantity
                   from ProductionPlanningType1 p1 
                 WHERE p1.ProductionDate>'" + date + @"' 
                 GROUP BY  ProcessID,p1.ProductionOrderID) AS PF1 ON  PF1.ProductionOrderID=po.Id AND PF1.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id) 


LEFT OUTER JOIN trn.ProductionOrderDetail POD ON pod.ProductionOrderId=po.Id
left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId

--production at PR Level
LEFT OUTER JOIN (
					SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
				FROM  trn.ProductionSummary S 
					WHERE CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<CONVERT(DATETIME,'" + date + @"')
				GROUP BY  s.ProductionOrderId,s.ProcessId
) AS PRODPR ON  PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
--production at PR Level
LEFT OUTER JOIN (
					SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
				FROM  trn.ProductionSummary S 
				WHERE  CONVERT(DATETIME,format(s.ProductionDate,'dd-MMM-yyyy'))=CONVERT(DATETIME,'" + date + @"')
				GROUP BY  s.ProductionOrderId,s.ProcessId
) AS PRODPRTODAY ON  PRODPRTODAY.ProductionOrderId=po.id AND PRODPRTODAY.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
left outer join (
select POD.ProductionOrderId,
min(so.DeliveryDate) AS FirstDeliveryDate,
max(so.DeliveryDate) AS LastDeliveryDate,
sum(SO.Qty) AS OrderQty from trn.ProductionOrderDetail POD 
left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId

group by POD.ProductionOrderId
) AS ORD on ord.ProductionOrderID=PO.Id

ORDER BY po.Id,so.DeliveryDate,SO.Id

                            ";


            dt = _sqlRepository.GetDataTable(sql);

            Dictionary<string, List<DataRow>> dicActualProduction = new Dictionary<string, List<DataRow>>();
            Dictionary<string, List<DataRow>> dicActualPlan = new Dictionary<string, List<DataRow>>();

            #region date wise production and plan data
            string prodId = "";
            DataTable dtActualProduction = _sqlRepository.GetDataTable(@"SELECT ppt.ProductionOrderID,ppt.ProductionDate,SUM(ppt.Quantity) AS ProducedQty
                                                                          FROM trn.ProductionSummary AS ppt
                                                                        WHERE ppt.ProductionDate<='" + date + @"'
                                                                        GROUP BY ppt.ProductionOrderID,ppt.ProductionDate ORDER BY ppt.ProductionOrderID,ppt.ProductionDate");
            List<DataRow> r = new List<DataRow>();
            for (int i = 0; i < dtActualProduction.Rows.Count; i++)
            {
                if (prodId != dtActualProduction.Rows[i]["ProductionOrderID"].ToString())
                {
                    r = new List<DataRow>();
                    dicActualProduction.Add(dtActualProduction.Rows[i]["ProductionOrderID"].ToString(), r);
                }
                r.Add(dtActualProduction.Rows[i]);

                prodId = dtActualProduction.Rows[i]["ProductionOrderID"].ToString();
            }


            DataTable dtActualPlan = _sqlRepository.GetDataTable(@"SELECT p1.ProductionOrderID,p1.ProductionDate,
                                                                            sum(ISNULL(p1.Quantity,0)+
                                                                            (CASE WHEN ISNULL(p1.Quantity,0)-ISNULL(PRODPRTODAY.ProductionQtyAtPR,0)>0 THEN ISNULL(p1.Quantity,0)-ISNULL(PRODPRTODAY.ProductionQtyAtPR,0) ELSE 0 END)
                                                                            ) AS PlanningQty
                                                                              FROM ProductionPlanningType1 AS p1
                                                                              INNER JOIN  trn.ProductionOrder PO ON p1.ProductionOrderID=po.Id
                                                                            --production at PR Level
                                                                            LEFT OUTER JOIN (
					                                                                            SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
				                                                                            FROM  trn.ProductionSummary S 
				                                                                            WHERE  CONVERT(DATETIME,format(s.ProductionDate,'dd-MMM-yyyy'))=CONVERT(DATETIME,'" + date + @"')
				                                                                            GROUP BY  s.ProductionOrderId,s.ProcessId
                                                                            ) AS PRODPRTODAY ON  PRODPRTODAY.ProductionOrderId=po.id AND PRODPRTODAY.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
  
                                                                            WHERE p1.ProductionDate>='" + date + @"'
                                                                            GROUP BY p1.ProductionOrderID,p1.ProductionDate

                                                                            ORDER BY p1.ProductionOrderId,p1.ProductionDate");

            prodId = "";
            r = new List<DataRow>();
            for (int i = 0; i < dtActualPlan.Rows.Count; i++)
            {
                if (prodId != dtActualPlan.Rows[i]["ProductionOrderID"].ToString())
                {
                    r = new List<DataRow>();
                    dicActualPlan.Add(dtActualPlan.Rows[i]["ProductionOrderID"].ToString(), r);
                }
                r.Add(dtActualPlan.Rows[i]);

                prodId = dtActualPlan.Rows[i]["ProductionOrderID"].ToString();
            }
            #endregion date wise production and plan data


            string id = "";
            dicDistributedSO = new Dictionary<string, ProductionQtyDistributionSO>();
            List<ProductionQtyDistributionSO> prBlock = new List<ProductionQtyDistributionSO>();


            double remainingDistributedQty = 0;
            double remainingFuturePlanQty = 0;
            double plannedProductionQty = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["ProductionOrderID"].ToString() == "2095")
                {


                }

                if (id != dt.Rows[i]["ProductionOrderID"].ToString())
                {

                    //prBlock = new List<ProductionQtyDistributionSO>();
                    //dicDistributedSO.Add(dt.Rows[i]["ProductionOrderID"].ToString(), prBlock);
                    //id = dt.Rows[i]["ProductionOrderID"].ToString();

                    remainingDistributedQty = clsStaticInfo.dbl(dt.Rows[i]["CumulativeQty"].ToString());
                    remainingFuturePlanQty = clsStaticInfo.dbl(dt.Rows[i]["FuturePlanQty"].ToString());
                }
                ProductionQtyDistributionSO dis = new ProductionQtyDistributionSO();

                plannedProductionQty = clsStaticInfo.dbl(dt.Rows[i]["PlannedQty"].ToString());

                dis.MasterOrderId = dt.Rows[i]["MasterOrderId"].ToString();
                dis.ProductionOrderID = dt.Rows[i]["ProductionOrderID"].ToString();
                dis.SalesOrderID = dt.Rows[i]["SalesOrderID"].ToString();
                dis.DeliveryDate = dt.Rows[i]["DeliveryDate"].ToString();

                dis.OrderQty = clsStaticInfo.dbl(dt.Rows[i]["OrderQty"].ToString());
                dis.PlannedQty = clsStaticInfo.dbl(dt.Rows[i]["PlannedQty"].ToString());
                dis.ProductionUptoPreviousDay = clsStaticInfo.dbl(dt.Rows[i]["ProductionUptoPreviousDay"].ToString());
                dis.PlanQtyForToday = clsStaticInfo.dbl(dt.Rows[i]["PlanQtyForToday"].ToString());
                dis.ProducedQtyToday = clsStaticInfo.dbl(dt.Rows[i]["ProducedQtyToday"].ToString());
                dis.CumulativeQty = clsStaticInfo.dbl(dt.Rows[i]["CumulativeQty"].ToString());
                dis.SOQty = clsStaticInfo.dbl(dt.Rows[i]["SOQty"].ToString());


                //distribution for already produced qty
                if (remainingDistributedQty > 0)
                {
                    if (plannedProductionQty <= remainingDistributedQty)
                    {
                        dis.DistributedQty = plannedProductionQty;
                        remainingDistributedQty = remainingDistributedQty - plannedProductionQty;
                        plannedProductionQty = plannedProductionQty - dis.DistributedQty;
                        dt.Rows[i]["DistributedQty"] = dis.DistributedQty;
                    }
                    else
                    {
                        dis.DistributedQty = remainingDistributedQty;
                        plannedProductionQty = plannedProductionQty - dis.DistributedQty;
                        remainingDistributedQty = 0;
                        dt.Rows[i]["DistributedQty"] = dis.DistributedQty;
                    }

                    //DETERMINE when the production ends to meet the remainingDistributedQty
                    double tempDistributedQty = dis.DistributedQty;
                    if (dicActualProduction.ContainsKey(dt.Rows[i]["ProductionOrderID"].ToString()))
                    {
                        List<DataRow> dr = dicActualProduction[dt.Rows[i]["ProductionOrderID"].ToString()];
                        for (int PP = 0; PP < dr.Count; PP++)
                        {
                            if (clsStaticInfo.dbl(dr[PP]["ProducedQty"].ToString()) == 0)
                                continue;

                            if (tempDistributedQty >= clsStaticInfo.dbl(dr[PP]["ProducedQty"].ToString()))
                            {
                                tempDistributedQty = tempDistributedQty - clsStaticInfo.dbl(dr[PP]["ProducedQty"].ToString());
                                dis.LastPlanDateForFullDistribution = Convert.ToDateTime(dr[PP]["ProductionDate"].ToString()).ToString("dd-MMM-yyyy");
                                dr[PP]["ProducedQty"] = 0;
                            }
                            else
                            {
                                dr[PP]["ProducedQty"] = clsStaticInfo.dbl(dr[PP]["ProducedQty"].ToString()) - tempDistributedQty;
                                tempDistributedQty = 0;
                                dis.LastPlanDateForFullDistribution = Convert.ToDateTime(dr[PP]["ProductionDate"].ToString()).ToString("dd-MMM-yyyy");
                                break;
                            }
                        }
                    }
                }

                if (plannedProductionQty > 0 && remainingDistributedQty == 0 && remainingFuturePlanQty > 0)
                {
                    if (plannedProductionQty <= remainingFuturePlanQty)
                    {
                        dis.DistributedQtyForPlan = plannedProductionQty;
                        remainingFuturePlanQty = remainingFuturePlanQty - plannedProductionQty;
                        plannedProductionQty = plannedProductionQty - dis.DistributedQtyForPlan;
                        dt.Rows[i]["DistributedQtyOnPlan"] = dis.DistributedQtyForPlan;
                    }
                    else
                    {
                        dis.DistributedQtyForPlan = remainingFuturePlanQty;
                        plannedProductionQty = plannedProductionQty - dis.DistributedQtyForPlan;
                        remainingFuturePlanQty = 0;
                        dt.Rows[i]["DistributedQtyOnPlan"] = dis.DistributedQtyForPlan;
                    }

                    //DETERMINE when the production ends to meet the remainingDistributedQty
                    double tempDistributedQty = dis.DistributedQtyForPlan;
                    if (dicActualPlan.ContainsKey(dt.Rows[i]["ProductionOrderID"].ToString()))
                    {
                        List<DataRow> dr = dicActualPlan[dt.Rows[i]["ProductionOrderID"].ToString()];
                        for (int PP = 0; PP < dr.Count; PP++)
                        {
                            if (clsStaticInfo.dbl(dr[PP]["PlanningQty"].ToString()) == 0)
                                continue;

                            if (tempDistributedQty >= clsStaticInfo.dbl(dr[PP]["PlanningQty"].ToString()))
                            {
                                tempDistributedQty = tempDistributedQty - clsStaticInfo.dbl(dr[PP]["PlanningQty"].ToString());
                                dis.LastPlanDateForFullDistribution = Convert.ToDateTime(dr[PP]["ProductionDate"].ToString()).ToString("dd-MMM-yyyy");
                                dr[PP]["PlanningQty"] = 0;
                            }
                            else
                            {
                                dr[PP]["PlanningQty"] = clsStaticInfo.dbl(dr[PP]["PlanningQty"].ToString()) - tempDistributedQty;
                                tempDistributedQty = 0;
                                dis.LastPlanDateForFullDistribution = Convert.ToDateTime(dr[PP]["ProductionDate"].ToString()).ToString("dd-MMM-yyyy");
                                break;
                            }
                        }
                    }
                }
                //prBlock.Add(dis);

                dicDistributedSO.Add(dt.Rows[i]["ProductionOrderID"].ToString() + dt.Rows[i]["SalesOrderID"].ToString(), dis);


                id = dt.Rows[i]["ProductionOrderID"].ToString();
            }


        }
        private void getSalesOrderDistribution(string date, string entityid, out Dictionary<string, List<DataRow>> dicDistributedSO, out DataTable dt)
        {

            string sql = @"
                                select D.*,MMN.ProductionStartDate,0 AS CummProductionQty,0 AS CummPlanQty,ISNULL(d.ProductionQty,0)+ISNULL(d.PlanQty,0) AS TotalQty,0 AS CummTotalQty  
                                from (SELECT p1.ProductionOrderID,FORMAT(p1.ProductionDate,'dd-MMM-yyyy')AS ProductionDate,0 AS ProductionQty,SUM(p1.Quantity) AS PlanQty
                                                   from ProductionPlanningType1 p1 
                                                 WHERE p1.ProductionDate>='" + date + @"'  AND P1.EntityId in (" + entityid + @")
                                                 GROUP BY  p1.ProductionDate,p1.ProductionOrderID
                 
                                                 UNION ALL
                 
                                                 SELECT s.ProductionOrderId,FORMAT(s.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,SUM(s.Quantity) AS ProductionQty,0 AS PlanQty
				                                FROM  trn.ProductionSummary S 
					                                WHERE S.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=S.ProductionOrderID) AND  S.EntityId in (" + entityid + @") AND CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<CONVERT(DATETIME,'" + date + @"')
				                                GROUP BY  s.ProductionOrderId,s.ProductionDate
                                ) AS D 
                                left join (
                                   select ProductionOrderID,FORMAT(MIN(ProductionDate),'dd-MMM-yyyy')  AS ProductionStartDate 
                                    from ( SELECT p1.ProductionOrderID,MIN(p1.ProductionDate) AS ProductionDate
                                                   from ProductionPlanningType1 p1 
                                                 WHERE p1.ProductionDate>='" + date + @"'  AND P1.EntityId in (" + entityid + @")
                                                 GROUP BY  p1.ProductionOrderID
                 
                                                 UNION ALL
                 
                                                 SELECT s.ProductionOrderId,MIN(s.ProductionDate) AS ProductionDate
				                                FROM  trn.ProductionSummary S 
					                                WHERE S.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=S.ProductionOrderID) AND  S.EntityId in (" + entityid + @") AND CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<CONVERT(DATETIME,'" + date + @"')
				                                GROUP BY  s.ProductionOrderId) AS K group by ProductionOrderID

                                    ) AS MMN ON MMN.ProductionOrderId=D.ProductionOrderId

                                INNER JOIN trn.ProductionOrder AS po ON po.Id=d.ProductionOrderID
                                INNER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                                WHERE PO.Id IN (SELECT DISTINCT p.ProductionOrderId FROM trn.ProductionOrderDetail AS p
                                            JOIN trn.SalesOrder AS so ON so.Id=p.SalesOrderId
                                            WHERE so.OrderStatusId<>'Closed')
                                ORDER BY D.ProductionOrderID,convert(date,D.ProductionDate)

                            ";


            dt = _sqlRepository.GetDataTable(sql);
            dicDistributedSO = new Dictionary<string, List<DataRow>>();
            List<DataRow> row = new List<DataRow>();

            string Id = ""; double CummProductionQty = 0; double CummPlanQty = 0; double CummTotalQty = 0;
            string ProductionEndDate = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Id != dt.Rows[i]["ProductionOrderID"].ToString())
                {
                    CummProductionQty = 0; CummPlanQty = 0; CummTotalQty = 0;
                    row = new List<DataRow>();
                    dicDistributedSO.Add(dt.Rows[i]["ProductionOrderID"].ToString(), row);

                    ProductionEndDate = dt.Rows[i]["ProductionDate"].ToString();
                }

                dt.Rows[i]["ProductionStartDate"] = ProductionEndDate;

                CummProductionQty += clsStaticInfo.dbl(dt.Rows[i]["ProductionQty"].ToString());
                CummPlanQty += clsStaticInfo.dbl(dt.Rows[i]["PlanQty"].ToString());
                CummTotalQty += clsStaticInfo.dbl(dt.Rows[i]["TotalQty"].ToString());
                ProductionEndDate = dt.Rows[i]["ProductionDate"].ToString();

                dt.Rows[i]["CummProductionQty"] = CummProductionQty;
                dt.Rows[i]["CummPlanQty"] = CummPlanQty;
                dt.Rows[i]["CummTotalQty"] = CummTotalQty;

                row.Add(dt.Rows[i]);

                Id = dt.Rows[i]["ProductionOrderID"].ToString();
            }


        }
        #endregion master order base on base process

        [HttpGet, Authorize]
        public ActionResult LoadPdfDocumentation(string Href)
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT * FROM  mst.MenuMaster WHERE MenuHelpDocInternalName='" + Href + "'");
                PdfLoadedDocument loadedDocument = new PdfLoadedDocument((byte[])dt.Rows[0]["MenuHelpDoc"]);


                loadedDocument.Save(dt.Rows[0]["MenuHelpDocName"].ToString(), HttpContext.ApplicationInstance.Response, Syncfusion.Pdf.HttpReadType.Save);
                return null;

            }
            catch (Exception ex)
            {

                throw;
            }


            return null;
        }

        //Porduction Report + WIP
        [HttpGet, Authorize]
        public ActionResult TNAAuditReport(string MasterOrderId)
        {

            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet = null;



                string sql = AuditString(MasterOrderId);
                DataTable dt = _sqlRepository.GetDataTable(sql);

                if (dt.Rows.Count == 0)
                    throw new Exception("No data found. Make sure that TnA scheduler is running and tasks are generated");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "Audit";
                sheet = workbook.Worksheets[0];

                sheet[1, 1].Text = "TnA Audit for Master Order#" + MasterOrderId;

                int StartRow = 3;
                sheet.ImportDataTable(dt, true, StartRow, 1);

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (dt.Columns[i].ColumnName.ToUpper().Contains("DATE"))
                    {
                        sheet[StartRow, i + 1, sheet.UsedRange.LastRow, i + 1].NumberFormat = "dd-MMM-yyyy";
                    }
                    try
                    {
                        sheet[StartRow, i].Text = Regex.Replace(sheet[StartRow, i].Text, "(\\B[A-Z])", " $1");
                    }
                    catch (Exception ex)
                    {

                    }
                    sheet.AutofitColumn(i + 2);
                }



                IListObject table = sheet.ListObjects.Create("Table1", sheet[clsStaticInfo.GetxlsCol(1) + (StartRow).ToString() + ":" + clsStaticInfo.GetxlsCol(sheet.UsedRange.LastColumn) + (sheet.UsedRange.LastRow).ToString()]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;


                sheet.UsedRange.CellStyle.Font.Size = 8f;
                sheet.IsDisplayZeros = false;
                sheet.IsGridLinesVisible = false;

                string strFileName = "TNA Audit Report " + MasterOrderId + ".xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();


            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

            return null;
        }
        private string AuditString(string MasterOrderId)
        {

            string s = @"SELECT mott.Sequence, mott.TaskDescription,mott.Duration,mott.LagDays AS OwnLagDays,tao.UserName AS TaskAppliedOn,tdd.UserName AS DependentDateType,format(t.DependentDate,'dd-MMM-yyy') AS DependentDate,
Dependency=STUFF((select distinct ','+ CONCAT(XM.TaskDescription,' Type:',xd.Criteria,' LagDays:',xd.LagDays) from 
				                        MasterOrderTaskTemplateDependency    XD 
				                        INNER JOIN MasterOrderTaskTemplate AS XM ON xm.Id=xd.PreTaskTemplateId                                              
				                        where mott.Id=XD.TaskTemplateId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

--format(t.ActualStartDate,'dd-MMM-yyy')DependentStartDate, format(t.ActualEndDate,'dd-MMM-yyy')DependentEndDate, 
--format(t.SequentialStartDate,'dd-MMM-yyy')HierarchicalStartDate, format(t.SequentialEndDate,'dd-MMM-yyy')HierarchicalEndDate,
format(t.OriginalSequentialStartDate,'dd-MMM-yyy')FinalStartDate, format(t.OriginalSequentialEndDate,'dd-MMM-yyy')FinalEndDate,

--format(t.TempStartDate,'dd-MMM-yyy')StartDateShift, format(t.TempEndDate,'dd-MMM-yyy')EndDateShift,
mott.[Active],ei.EmployeeName,tm.ConsiderOffDays,TNA.MasterOrderId, TNA.StyleNo AS LineItem,
TNA.SONo, TNA.PRNo

				                        
				                        
                                      FROM
                                    MasterOrderTaskTemplate AS mott
                                    INNER JOIN TNATasks AS t ON t.TaskTemplateId=mott.Id
                                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=t.EmployeeId
                                    LEFT JOIN hkp.TaskAppliedOn AS tao ON tao.Id=mott.TaskAppliedOnId
                                    LEFT JOIN hkp.TaskDependentDates AS tdd ON tdd.Id=mott.TaskDependentDatesId
                                    LEFT JOIN TaskMaster AS tm ON tm.Id=mott.TaskMasterId
                                    INNER JOIN 
                                     (SELECT TT.Id, 'Order' AS TNAType, b.Id AS BuyerId,tt.TaskTemplateId, b.UserName AS Buyer,mo.Id  AS MasterOrderId,
                                                                        StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
				                                                                            trn.MasterOrderItem XMOI 	                                                   
				                                                                        where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
				
                                                                        SONo=STUFF((select distinct ','+so.Id from 
				                                                                            trn.MasterOrderItem XMOI 	 
				                                                                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id                                                  
				                                                                        where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
				
                                                                        PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
				                                                                            trn.MasterOrderItem XMOI 	 
				                                                                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id   
				                                                                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id                                               
				                                                                        where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				
                                                                            FROM  TNATasks AS TT 
                                                                        INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId AND isnull(t.MasterOrderId,'')<>''
                                                                        INNER JOIN trn.MasterOrder AS mo ON mo.Id=t.MasterOrderId
                                                                        LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId

                                                                        UNION

                                                                        SELECT TT.Id, 'Style' AS TNAType, b.Id AS BuyerId,tt.TaskTemplateId, b.UserName AS Buyer,mo.Id  AS MasterOrderId,
                                                                    StyleNo=MOI.BuyerReferenceNo,
				
                                                                    SONo=STUFF((select distinct ','+so.Id from 
				                                                                     trn.MasterOrderItem XMOI 	 
				                                                                     INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id                                                  
				                                                                    where MOI.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
				
                                                                    PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
				                                                                     trn.MasterOrderItem XMOI 	 
				                                                                     INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id   
				                                                                     INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id                                               
				                                                                    where MOI.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				
                                                                     FROM  TNATasks AS TT 
                                                                    INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId AND isnull(t.MasterOrderItemId,'')<>''
                                                                    inner join trn.MasterOrderItem MOI on MOI.Id=t.MasterOrderItemId
                                                                    INNER JOIN trn.MasterOrder AS mo ON mo.Id=MOI.MasterOrderId
                                                                    LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                
                                                                    UNION

                                                                  SELECT  TT.Id, 'Sales Order' AS TNAType, b.Id AS BuyerId,tt.TaskTemplateId, b.UserName AS Buyer,mo.Id  AS MasterOrderId,
                                                                    StyleNo=MOI.BuyerReferenceNo,
				
                                                                    SONo=so.Id,
				
                                                                    PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
				                                                                      trn.ProductionOrderDetail AS pod                                              
				                                                                    where SO.Id=POD.SalesOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				
                                                                     FROM TaskManagerMaster AS tm
                                                                    INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                                                    INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId AND isnull(t.SalesOrderId,'')<>''
                                                                    INNER JOIN trn.salesorder SO ON so.Id=t.SalesOrderId
                                                                    inner join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
                                                                    INNER JOIN trn.MasterOrder AS mo ON mo.Id=MOI.MasterOrderId
                                                                    LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                
                                                                    UNION

                                                                   SELECT TT.Id, 'Prod. Order' AS TNAType, 
                                                                    PR.BuyerId,tt.TaskTemplateId,   PR.Buyer,
                                                                    PR.MasterOrderId,
                                                                    PR.StyleNo,pr.SONo,
				
				
                                                                    pr.ProductionOrderId AS PRNo
				
                                                                     FROM TaskManagerMaster AS tm
                                                                    INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                                                    INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId  AND isnull(t.ProductionOrderId,'')<>''
                                                                    INNER JOIN trn.ProductionOrder AS po ON PO.Id=t.ProductionOrderId
                                                                    INNER JOIN
                                                                    (
                                			                                    SELECT distinct po.Id AS ProductionOrderId,
                                			                                    b.Id AS BuyerId,b.UserName AS Buyer,
                                			
                                			                                     MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
														                                     trn.MasterOrderItem XMOI 	 
														                                     INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														                                     INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														                                    where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
											 
											                                     ,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
														                                     trn.MasterOrderItem XMOI 	 
														                                     INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														                                     INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														                                    where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                	
                                			                                      ,SONo=STUFF((select distinct ','+sox.Id from 
														                                     trn.MasterOrderItem XMOI 	 
														                                     INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														                                     INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														                                    where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				                                
														                                     FROM trn.ProductionOrder PO
										                                    INNER JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=po.Id AND pod.Id=(SELECT TOP 1 Id FROM trn.ProductionOrderDetail AS px WHERE px.ProductionOrderId=po.Id)
                                		                                    INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                		                                    inner join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
										                                    INNER JOIN trn.MasterOrder AS mo ON mo.Id=MOI.MasterOrderId
										                                    LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                                                    ) AS PR ON pr.ProductionOrderId=po.Id
                                    ) AS TNA ON tna.Id=t.Id


                                    WHERE mott.MasterOrderId='" + MasterOrderId + @"'
                                    ORDER BY mott.Sequence";


            return s;

        }



        [HttpGet, Authorize]
        public ActionResult MasterOrderWithSalesOrder(string EntityId)
        {

            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet = null;



                getOrderMasterWithSalesOrderMain(EntityId, out DataTable dt);

                if (dt.Rows.Count == 0)
                    throw new Exception("No data found.");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "Audit";
                sheet = workbook.Worksheets[0];


                int StartRow = 6;
                sheet.ImportDataTable(dt, true, StartRow, 1);

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (dt.Columns[i].ColumnName.ToUpper().Contains("DATE"))
                    {
                        sheet[StartRow, i + 1, sheet.UsedRange.LastRow, i + 1].NumberFormat = "dd-MMM-yyyy";
                    }
                    try
                    {
                        sheet[StartRow, i].Text = Regex.Replace(sheet[StartRow, i].Text, "(\\B[A-Z])", " $1");
                    }
                    catch (Exception ex)
                    {

                    }
                    sheet.AutofitColumn(i + 2);
                }



                IListObject table = sheet.ListObjects.Create("Table1", sheet[clsStaticInfo.GetxlsCol(1) + (StartRow).ToString() + ":" + clsStaticInfo.GetxlsCol(sheet.UsedRange.LastColumn) + (sheet.UsedRange.LastRow).ToString()]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange["A7"].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Master Order Detail", identity.CompanyId, "", "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet.Range[1, 1, 6, sheet.UsedRange.LastColumn].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;

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
                sheet.UsedRange.CellStyle.Font.Size = 8f;
                sheet.IsDisplayZeros = false;
                sheet.IsGridLinesVisible = false;

                string strFileName = "MasterOrderWithSalesOrder.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();


            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }
        //Porduction Report + WIP
        [HttpGet, Authorize]
        public ActionResult ProductionReport(string entityid, string fromDate, string todate, string ProductionStatus)
        {

            try
            {


                IWorkbook workbook = ProductionOrderReports.ProductionReportXls(entityid, fromDate, todate, ProductionStatus);

                string strFileName = "Production Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();


            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

            return null;
        }

        [HttpGet, Authorize]
        public ActionResult LadderPlanStatus(string entityid)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();

                DataTable dt;
                getLadderPlanStatusQuery(entityid, out dt);
                IWorkbook workbook = LadderPlanStatusFile(excelEngine, entityid, dt);

                string strFileName = "Ladder Plan.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        public IWorkbook LadderPlanStatusFile(ExcelEngine excelEngine, string entityid, DataTable data)
        {
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (data.Rows.Count == 0)
                    throw new Exception("No data found");

                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(3);
                workbook.Worksheets[0].Name = "Ladder Plan";
                sheet = workbook.Worksheets[0];
                application.DefaultVersion = ExcelVersion.Excel2013;
                workbook.Version = ExcelVersion.Excel2013;
                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "PR No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPRNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Work Center";
                sheet[ROW, COL].ColumnWidth = 12;
                int colWorkCenter = COL;
                COL++;

                sheet[ROW, COL].Text = "Plan Target";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanTarget = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Cumulative/ Day ";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanCumilativeDay = COL;
                COL++;
                sheet[ROW, COL].Text = "Work Center Plan Month";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTargetMonth = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Cumulative";
                sheet[ROW, COL].ColumnWidth = 8;
                int colPlanCumilative = COL;
                COL++;

                sheet[ROW, COL].Text = "Production Order Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrderStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Production Quantity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Production Cumulative / Day";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionCumilativeDay = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Cumulative";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionCumilative = COL;
                COL++;
                sheet[ROW, COL].Text = "Diff";
                sheet[ROW, COL].ColumnWidth = 10;
                int colDiff = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColBuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Order No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBuyerOrderNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Buyer Line Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBuyerLineItem = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Order No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOwnOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Line Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOwnLineItem = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Ids";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSalesOrderIds = COL;
                COL++;
                sheet[ROW, COL].Text = "PONo";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPONo = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSOQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Qty";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Total Production";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTotalProduction = COL;
               

                #endregion columns

                int endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                int RowIndex = ROW;
                ROW++;

                int StartRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    //text fields
                    sheet[ROW, colPRNo].Text = data.Rows[i]["PRNo"].ToString();
                    sheet[ROW, colDate].Text = GetDate(data.Rows[i]["Date"].ToString());
                    sheet[ROW, colWorkCenter].Text = data.Rows[i]["WorkCenter"].ToString();

                    sheet[ROW, colPlanTarget].Number = clsStaticInfo.dbl(data.Rows[i]["PlanTarget"].ToString());
                    sheet[ROW, colPlanTarget].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, colPlanTarget].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, colPlanTarget].HorizontalAlignment = ExcelHAlign.HAlignRight;


                    sheet[ROW, colPlanCumilativeDay].Formula = "SUMIFS($" + clsStaticInfo.GetxlsCol(colPlanTarget) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colPlanTarget) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPRNo) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + "," + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colDate) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colDate) + ROW.ToString() + "," + clsStaticInfo.GetxlsCol(colDate) + ROW.ToString() + ")";
                    sheet[ROW, colPlanCumilativeDay].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, colPlanCumilativeDay].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, colPlanCumilativeDay].HorizontalAlignment = ExcelHAlign.HAlignRight;


                    sheet[ROW, colPlanCumilative].Formula = "SUMIFS($" + clsStaticInfo.GetxlsCol(colPlanTarget) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colPlanTarget) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPRNo) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + "," + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + ")";
                    sheet[ROW, colPlanCumilative].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, colPlanCumilative].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, colPlanCumilative].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    
                    sheet[ROW, colProductionOrderStatus].Text = data.Rows[i]["ProductionOrderStatus"].ToString();

                    sheet[ROW, colProductionQty].Number = clsStaticInfo.dbl(data.Rows[i]["ProductionQuantity"].ToString());
                    sheet[ROW, colProductionQty].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, colProductionQty].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, colProductionQty].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    //SUMIFS($H$3:H14,$B$3:B14, B14,$C$3:C14, C14)
                    sheet[ROW, colProductionCumilativeDay].Formula = "SUMIFS($" + clsStaticInfo.GetxlsCol(colProductionQty) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colProductionQty) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPRNo) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + "," + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colDate) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colDate) + ROW.ToString() + "," + clsStaticInfo.GetxlsCol(colDate) + ROW.ToString() + ")";
                    sheet[ROW, colProductionCumilativeDay].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, colProductionCumilativeDay].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, colProductionCumilativeDay].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    //SUMIFS($H$3:H14,$B$3:B14,B14)
                    sheet[ROW, colProductionCumilative].Formula = "SUMIFS($" + clsStaticInfo.GetxlsCol(colProductionQty) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colProductionQty) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPRNo) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + "," + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + ")";
                    sheet[ROW, colProductionCumilative].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, colProductionCumilative].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, colProductionCumilative].HorizontalAlignment = ExcelHAlign.HAlignRight;


                    //SUMIFS($E$3:E14,$B$3:B14, B14,$C$3:C14, C14)
                    sheet[ROW, colDiff].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colPlanTarget) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colProductionQty) + ROW.ToString() + ")";
                    sheet[ROW, colDiff].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, colDiff].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, colDiff].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet[ROW, colCustomer].Text = data.Rows[i]["Customer"].ToString();
                    sheet[ROW, colMasterOrderNo].Text = data.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, ColBuyer].Text = data.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colBuyerOrderNo].Text = data.Rows[i]["BuyerOrder"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = data.Rows[i]["OwnOrder"].ToString();
                    sheet[ROW, colBuyerLineItem].Text = data.Rows[i]["BuyerItem"].ToString();
                    sheet[ROW, colOwnLineItem].Text = data.Rows[i]["OwnItem"].ToString();
                    sheet[ROW, colSalesOrderIds].Text = data.Rows[i]["SOId"].ToString();
                    sheet[ROW, colPONo].Text = data.Rows[i]["PONo"].ToString();

                    sheet[ROW, colSOQty].Number = clsStaticInfo.dbl(data.Rows[i]["SOQty"].ToString());
                    sheet[ROW, colSOQty].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, colSOQty].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, colSOQty].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    //SUMIFS($E$3:$E$14,$B$3:$B$14,B3)
                    sheet[ROW, colPlanQty].Formula = "SUMIFS($" + clsStaticInfo.GetxlsCol(colPlanTarget) + "$" + StartRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPlanTarget) + "$" + LastRow.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPRNo) + "$" + StartRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPRNo) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + ")";
                    sheet[ROW, colPlanQty].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, colPlanQty].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, colPlanQty].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    //SUMIFS($H$3:$H$14,$B$3:$B$14,B3)
                    sheet[ROW, colTotalProduction].Formula = "SUMIFS($" + clsStaticInfo.GetxlsCol(colProductionQty) + "$" + StartRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colProductionQty) + "$" + LastRow.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPRNo) + "$" + StartRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPRNo) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + ")";
                    sheet[ROW, colTotalProduction].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, colTotalProduction].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, colTotalProduction].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                     
                }

                IListObject table = sheet.ListObjects.Create("Table1", sheet[clsStaticInfo.GetxlsCol(1) + (StartRow - 1).ToString() + ":" + clsStaticInfo.GetxlsCol(endCol) + (ROW).ToString()]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;


                sheet.IsDisplayZeros = false;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;


                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "LadderPlan", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;


//#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;



                string fPath = fPath = HostingEnvironment.MapPath("~/") + "LadderPlan" + identity.UserId + ".xlsx";


                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }


                #region PR Wise
                workbook.Worksheets[1].Name = "PRWise";

                IWorksheet pivotSheet = workbook.Worksheets[1];
              
                IPivotCache cache2 = workbook.PivotCaches.Add(sheet[StartRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache2);

                pivotTable.Fields[colPRNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colWorkCenter - 1].Axis = PivotAxisTypes.Row;


                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colPRNo - 1)
                        continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }


                IPivotField field = pivotTable.Fields[colPlanTarget - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Target Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colProductionQty - 1];
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Prod. Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colDiff - 1]; 
                field.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTable.DataFields.Add(field, "Diff", PivotSubtotalTypes.Sum);

                pivotTable.ShowDrillIndicators = false;
                //pivotTable.ShowDataFieldInRow = true;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                reportUtility.CompanyPlantHeaderNew(ref pivotSheet, 1, "Ladder Plan. Last Updated(" + Convert.ToDateTime(data.Rows[0]["Date"].ToString()).ToString("dd-MMM-yyyy") + ")", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref pivotSheet, 6, ExcelPageOrientation.Landscape);
                pivotSheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                pivotSheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                pivotSheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                pivotSheet.IsGridLinesVisible = false;


                #endregion PR Wise

                #region WC Wise

                workbook.Worksheets[2].Name = "WCWise";

                IWorksheet pivotSheetWC = workbook.Worksheets[2];

                IPivotCache cacheWC = workbook.PivotCaches.Add(sheet[StartRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTableWC = pivotSheetWC.PivotTables.Add("PivotTable1", pivotSheetWC["A6"], cacheWC);

                pivotTableWC.Fields[colWorkCenter - 1].Axis = PivotAxisTypes.Row;
                pivotTableWC.Fields[colDate - 1].Axis = PivotAxisTypes.Row;
                pivotTableWC.Fields[colPRNo - 1].Axis = PivotAxisTypes.Row;


                for (int i = 0; i < pivotTableWC.Fields.Count; i++)
                {
                    if (i == colWorkCenter - 1)
                        continue;
                    pivotTableWC.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }


                IPivotField wcField = pivotTableWC.Fields[colPlanTarget - 1];
                wcField.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTableWC.DataFields.Add(wcField, "Target Qty", PivotSubtotalTypes.Sum);

                wcField = pivotTableWC.Fields[colProductionQty - 1];
                wcField.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTableWC.DataFields.Add(wcField, "Prod. Qty", PivotSubtotalTypes.Sum);

                wcField = pivotTableWC.Fields[colDiff - 1];
                wcField.NumberFormat = clsStaticInfo.NumberFormat();
                pivotTableWC.DataFields.Add(wcField, "Diff", PivotSubtotalTypes.Sum);



                pivotTableWC.ShowDrillIndicators = false;
                //pivotTable.ShowDataFieldInRow = true;
                pivotTableWC.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTableWC.Options.NullString = "";
                pivotTableWC.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                reportUtility.CompanyPlantHeaderNew(ref pivotSheetWC, 1, "Ladder Plan. Last Updated(" + Convert.ToDateTime(data.Rows[0]["Date"].ToString()).ToString("dd-MMM-yyyy") + ")", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref pivotSheetWC, 6, ExcelPageOrientation.Landscape);
                pivotSheetWC[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                pivotSheetWC.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                pivotSheetWC.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                pivotSheetWC.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                pivotSheetWC.IsGridLinesVisible = false;

                #endregion WC Wise

                return workbook;



            }
            catch (Exception ex)
            {
                throw ex;

            }



        }

        public ActionResult Snapshot2DataReportXls(string fromDate, string todate)
        {
            try
            {
                IWorkbook workbook = ProductionOrderReports.Snapshot2DataXls(fromDate, todate);
                string strFileName = "Snapshot 2 Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public DataTable getLadderPlanStatusQuery(string entityid, out DataTable ldPlan)
        {
            ldPlan = new DataTable();
            try
            {

                string strSQL = string.Empty;
                strSQL = @"select PPL.*,POS.Quantity AS ProductionQuantity,PRS.UserName ProductionOrderStatus

										,SOQty = (select SUM(SOX.Qty) from trn.SalesOrder AS sox
        								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
        							                                where podx.ProductionOrderId=po.Id)
        								,BuyerOrder = REPLACE(REPLACE(
        										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
        																			trn.MasterOrder XMOI 	 
        								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
        								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
        								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
        							                                where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
        								                		,'&amp;','&'), 'amp;', '')
										,MasterOrderNo = REPLACE(REPLACE(
        										 STUFF((select distinct ','+XMOI.Id from 
        																			trn.MasterOrder XMOI 	 
        								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
        								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
        								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
        							                                where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
        								                		,'&amp;','&'), 'amp;', '')
										,PONo=STUFF((select distinct ','+XPO.PONumber from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
																inner join trn.CustomerPO XPO on xpo.id=sox.CustomerPOId
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
															
										,SOId=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                        ,SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                        ,buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


                                         ,Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			                                                    

                                        ,OwnOrder =REPLACE(REPLACE(

        										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
        																			trn.MasterOrder XMOI 	 
        								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
        								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
        								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
        							                                where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
        									                	,'&amp;','&'), 'amp;', '')
        								 ,BuyerItem=REPLACE(REPLACE(
        										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
        																			trn.MasterOrderItem XMOI 	  
        								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
        								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
        							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
        										                ,'&amp;','&'), 'amp;', '')	                                                
										,OwnItem=REPLACE(REPLACE(
        										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
        																			trn.MasterOrderItem XMOI 	  
        								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
        								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
        							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
        										,'&amp;','&'), 'amp;', '')  from (select P.PRNo,P.EntityID,P.[Date],P.WorkCenter,P.WorkCenterMasterId,sum(P.PlanTarget) PlanTarget 
												from (select PPT.ProductionOrderID PRNo,PPT.EntityID,PPT.ProductionDate [Date],ppt.WorkCenterMasterId,WCM.UserName WorkCenter,SUM(PPT.Quantity) [PlanTarget]
        										
												from ProductionPlanningType1 PPT
        										left join SCS.WorkCenterMaster WCM on WCM.Id=PPT.WorkCenterMasterId
        										group by PPT.ProductionOrderID ,ppt.WorkCenterMasterId,PPT.ProductionDate,WCM.UserName,PPT.EntityID
        										union all
        										select PPS.ProductionOrderID PRNo,PPS.EntityID,PPS.ProductionDate [Date],pps.WorkCenterMasterId,WCM.UserName WorkCenter
        										,SUM(PPS.Quantity) [PlanTarget]
        										from ProductionPlanningSnapshot2Type1 PPS
        										left join SCS.WorkCenterMaster WCM on WCM.Id=PPS.WorkCenterMasterId
        										group by PPS.ProductionOrderID ,PPS.ProductionDate ,pps.WorkCenterMasterId,WCM.UserName,PPS.EntityID
												union all
												 select PRS.ProductionOrderID PRNo,PRS.EntityID,PRS.ProductionDate [Date],PRS.WorkCenterMasterId,WCM.UserName WorkCenter
        										,SUM(0) [PlanTarget] 
												from trn.ProductionSummary PRS 
												join trn.ProductionOrder PO ON PO.id=prs.ProductionOrderId
												join trn.ProductionOrderProcessSet PSS on pss.ProductionOrderId=po.id and pss.IsBaseProcess=1
												left join SCS.WorkCenterMaster WCM on WCM.Id=PRS.WorkCenterMasterId and wcm.ProcessId=pss.ProcessId
												group by   PRS.ProductionOrderID ,PRS.ProductionDate ,PRS.WorkCenterMasterId,WCM.UserName,PRS.EntityID
        										)as P group by P.PRNo,P.EntityID,P.[Date],P.WorkCenter,P.WorkCenterMasterId) PPL 
        										Left join (select ps.ProductionOrderId PRNo,ps.ProductionDate,ps.WorkCenterMasterId,sum(ps.Quantity) Quantity  from trn.ProductionSummary ps 
        										group by ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId
        										) POS on POS.PRNo=PPL.PRNo and pos.ProductionDate=ppl.[Date] and pos.WorkCenterMasterId=ppl.WorkCenterMasterId
        										left join TRN.ProductionOrder PO on PO.Id=PPL.PRNo
												left join HKP.ProductionStatus PRS on PRS.Id=PO.ProductionStatusId
        				
        				

        										where PPL.EntityID='" + entityid + @"' and PRS.UserName in ('Active','Running')";

                return ldPlan = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        private class fields
        {
            public string ArticleID { get; set; } = "";
            public string Attribute1 { get; set; } = "";
            public string attribute1FreeText { get; set; } = "";


        }
        private class ProductionQtyDistributionSO
        {

            public string MasterOrderId { get; set; } = "";
            public string ProductionOrderID { get; set; } = "";
            public string SalesOrderID { get; set; } = "";
            public string DeliveryDate { get; set; } = "";
            public string LastPlanDateForFullDistribution { get; set; } = "";

            public double OrderQty { get; set; } = 0;
            public double PlannedQty { get; set; } = 0;
            public double ProductionUptoPreviousDay { get; set; } = 0;
            public double PlanQtyForToday { get; set; } = 0;
            public double ProducedQtyToday { get; set; } = 0;
            public double CumulativeQty { get; set; } = 0;
            public double SOQty { get; set; } = 0;
            public double DistributedQty { get; set; } = 0;
            public double DistributedQtyForPlan { get; set; } = 0;
            public double DistributedQtyToday { get; set; } = 0;

        }

    }

}