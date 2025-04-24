using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Model.Enums;
using Library.Model.ManagementChartOfAccounts;
using Library.Service.ManagementChartOfAccounts;
using Library.Service.Vouchers;
using Library.ViewModel.ManagementChartOfAccounts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Linq;
using Library.Accounting.Accounts;
using System.Data;
using Library.Security.Core;
using Library.Data.Sql;
using Syncfusion.XlsIO;
using System.IO;
using Library.Data;
using Library.Service.Helpers;

namespace Aplos.Areas.Accounts.Controllers
{
    public class BudgetMasterController : BaseController
    {
        private readonly IVoucherReportService _voucharReportService;
        private readonly IBudgetMasterService _budgetMasterService;
        private readonly AccountVoucherReportService _accountVoucherReportService;
        private readonly IBudgetMasterActivityService _budgetMasterActivityService;
        private readonly IRepositoryAsync<BudgetMaster> _budgetMasterRepository;
        private readonly ISqlRepository _sqlRepository;
        public BudgetMasterController(
             IVoucherReportService voucharReportService
            , IBudgetMasterService budgetMasterService
            , IBudgetMasterActivityService budgetMasterActivityService
            , IRepositoryAsync<BudgetMaster> budgetMasterRepository
            , AccountVoucherReportService accountVoucherReportService
            , ISqlRepository R)
        {
            _voucharReportService = voucharReportService;
            _budgetMasterService = budgetMasterService;
            _budgetMasterActivityService = budgetMasterActivityService;
            _budgetMasterRepository = budgetMasterRepository;
            _accountVoucherReportService = accountVoucherReportService;
            _sqlRepository = R;
        }

        [HttpGet, Authorize]
        public ActionResult GetBudgetMasterActivityCbo(string budgetMasterId)
        {
            return Json(_budgetMasterActivityService.GetBudgetMasterActivityCbo(budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBudgetMasterActivityLevelEmployeeCbo(string budgetMasterId, string level, string employeeId)
        {

            return Json(_budgetMasterActivityService.GetBudgetMasterActivityCbo(budgetMasterId, level, employeeId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetBudgetMasterActivityLevelPotalCbo(string budgetMasterId, string level, string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(employeeId))
                employeeId = identity.EmployeeId;
            return Json(_budgetMasterActivityService.GetBudgetMasterActivityCbo(budgetMasterId, level, employeeId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetBudgetMasterActivityLevelCbo(string budgetMasterId, string level)
        {
            return Json(_budgetMasterActivityService.GetBudgetMasterActivityCbo(budgetMasterId, level, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetAllBudgetMasterActivityCbo(string budgetMasterId)
        {
            return Json(GetAllBudgetMasterActivityCboData(budgetMasterId), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetAllBudgetMasterActivityCboData(string budgetMasterId)
        {
            var sql = @"SELECT BMA.BudgetMasterId, BMA.ActivityId, A.UserName AS ActivityName, A.FALinked,A.IsOrderSpecific,A.ActivityOrderType,BMA.IsServiceApplicable,BMA.ActivityOrderType
                        FROM [MST].[BudgetMasterActivity] AS BMA
                        JOIN [HKP].[Activity] AS A ON A.Id=BMA.ActivityId
                        WHERE BMA.BudgetMasterId='" + budgetMasterId + "' ORDER BY A.Code, A.UserName";
            return _sqlRepository.GetDataCollection(sql);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboActivityForSetup(string coaId, string glId, string budgetId)
        {
            return Json(_budgetMasterActivityService.GetCboActivityForSetup(coaId, glId, budgetId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboBudgetForSetup(string coaId, string glId)
        {
            return Json(_budgetMasterService.GetCboBudgetForSetup(coaId, glId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterCboList(string glId)
        {
            return Json(_budgetMasterService.GetBudgetMasterCboList(glId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterCboByCOAAndGLId(string coaId, string glId)
        {
            return Json(_budgetMasterService.GetBudgetMasterCboByCOAAndGLId(coaId, glId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterCboByCompanyAndGLId(string companyId, string glId)
        {
            return Json(_budgetMasterService.GetBudgetMasterCboByCompanyAndGLId(companyId, glId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboBudgetMasterForSetup()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterService.GetCboBudgetMasterForSetup(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetCategoryCbo()
        {
            return Json(_budgetMasterService.GetBudgetCategoryCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetSubCategoryCboByCategory(string categoryId)
        {
            return Json(_budgetMasterService.GetBudgetSubCategoryCboByCategory(categoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetCboBySubCategory(string subCategoryId)
        {
            return Json(_budgetMasterService.GetBudgetCboBySubCategory(subCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCboEmployeeBudgetList(string employeeId)
        {
            if (!string.IsNullOrEmpty(employeeId))
                return Json(_budgetMasterService.GetCboEmployeeBudgetList(employeeId), JsonRequestBehavior.AllowGet);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(identity.EmployeeId))
                employeeId = identity.EmployeeId;
            return Json(_budgetMasterService.GetCboEmployeeBudgetList(employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboEmployeeBudgetPopUpListByEmployeeId(GridParameter parameters, string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterService.GetCboEmployeeBudgetPopUpList(parameters, employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCboBudgetCategorySubCategoryActivityPopUpList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterService.GetCboBudgetCategorySubCategoryActivityPopUpList(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterPopUpList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterService.GetBudgetMasterPopUpList(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboEmployeeBudgetPopUpList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterService.GetCboEmployeeBudgetPopUpList(parameters, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCboEmployeeBudgetActivityList(string employeeId, string budgetMasterId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return Json(_budgetMasterService.GetCboEmployeeBudgetActivityList(employeeId, budgetMasterId),
                    JsonRequestBehavior.AllowGet);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(identity.EmployeeId))
                employeeId = identity.EmployeeId;
            return Json(_budgetMasterService.GetCboEmployeeBudgetActivityList(employeeId, budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCboEmployeeBudgetActivityPhoneList(string employeeId, string budgetId, string activityId)
        {
            if (!string.IsNullOrEmpty(employeeId))
                return Json(
                    new SelectList(
                        _budgetMasterService.GetCboEmployeeBudgetActivityPhoneList(employeeId, budgetId, activityId),
                        "Value", "Text"), JsonRequestBehavior.AllowGet);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(identity.EmployeeId))
                employeeId = identity.EmployeeId;
            return Json(new SelectList(_budgetMasterService.GetCboEmployeeBudgetActivityPhoneList(employeeId, budgetId, activityId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetFALinkedList(string budgetMasterId, string activityId, string faLinked)
        {
            return Json(_budgetMasterService.GetFALinkedList(budgetMasterId, activityId, faLinked), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBudgetCboByGL(string glgeneralInfoId)
        {
            return Json(new SelectList(_budgetMasterService.GetBudgetCboByGL(glgeneralInfoId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBudgetCboByEmployeeActivity(string employeeId, string activityId)
        {
            if (!string.IsNullOrEmpty(employeeId))
                return Json(
                    new SelectList(_budgetMasterService.GetBudgetCboByEmployeeActivity(employeeId, activityId), "Value",
                        "Text"), JsonRequestBehavior.AllowGet);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(identity.EmployeeId))
                employeeId = identity.EmployeeId;
            return Json(new SelectList(_budgetMasterService.GetBudgetCboByEmployeeActivity(employeeId, activityId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        #region Budget Responsible Person
        [HttpGet, Authorize]
        public ActionResult GetAllEmployee(GridParameter parameters, string plantId)
        {
            return Json(_budgetMasterService.GetAllEmployee(parameters, plantId), JsonRequestBehavior.AllowGet);
        }
        #endregion


        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult Budget()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult BudgetCategory()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult BudgetClass()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult BudgetGroup()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult BudgetSubCategory()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult FARegister()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult FiscalYearBudget()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult BudgetControl()
        {
            return View();
        }

        [HttpPost]
        public JsonResult FARegisterUpdate(BudgetMaster budgetMaster, IEnumerable<ActivityViewModel> budgetActivities)
        {
            _budgetMasterService.UpdateFARegister(budgetMaster, budgetActivities);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string coaId)
        {
            return Json(_budgetMasterService.Query(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public int GetMaxRefNo()
        {
            int budMaxRefNo = _budgetMasterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RefNo AS INT)), 0) RefNo FROM [MST].[BudgetMaster] ").First();
            budMaxRefNo++;
            return budMaxRefNo;
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetActivityList(string budgetMasterId)
        {
            return Json(_budgetMasterActivityService.Query(budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFAMasterLinkList(string budgetMasterId)
        {
            return Json(_budgetMasterActivityService.GetFAMasterLinkList(budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFARegisterLinkList(string budgetMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterActivityService.GetFARegisterLinkList(identity.CompanyId, budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterById(string id)
        {
            return Json(_budgetMasterService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetBudgetPaymentTypeList(string budgetmasterId)
        {
            return Json(_budgetMasterService.GetBudgetPaymentTypeList(budgetmasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BudgetMaster budgetmaster, IEnumerable<ActivityViewModel> budgetActivities,
            IEnumerable<BudgetMasterPaymentTerm> budgetMasterPaymentTypeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetMasterService.Insert(budgetmaster, budgetActivities, budgetMasterPaymentTypeList, identity.CompanyGroupId);
            return Json(new { BudgetMaster = budgetmaster, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BudgetMaster budgetMaster, IEnumerable<ActivityViewModel> budgetActivities, IEnumerable<BudgetMasterPaymentTerm> budgetMasterPaymentTypeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetMasterService.Update(budgetMaster, budgetActivities, budgetMasterPaymentTypeList, identity.CompanyGroupId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _budgetMasterService.DeleteMaster(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public bool CheckUsingActivityInTransaction(string id)
        {
            try
            {
                var sql = @"IF EXISTS(select 1 SELECT top(1) ActivityId FROM  trn.VoucherDetail where ActivityId='" + id + @"'
                            )x WHERE x.ActivityId='" + id + @"') SELECT 1 ELSE SELECT 0 RETURN ";
                return Convert.ToBoolean(_budgetMasterRepository.SqlQuery<int>(sql).Single());
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpGet]
        public ActionResult BudgetMasterReport()
        {
            return View("~/Areas/Accounts/Views/BudgetMasterReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetBudgetMasterReport(ReportFormat reportFormat, string coaId, bool isActivityLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetBudgetMasterReport(identity.CompanyGroupId, coaId, isActivityLevel);
            var reportFileName = "Budget Master";
            switch (reportFormat)
            {
                //case ReportFormat.Pdf:
                //    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        #region FiscalYearBudget
        [HttpGet, Authorize]
        public ActionResult GetFiscalYearBudgetReport(ReportFormat reportFormat, string fiscalYearPeriodId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var workbook = _voucharReportService.GetFiscalYearBudgetReport(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fiscalYearPeriodId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Monthly  Budget";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }



        #endregion FiscalYearBudget


        #region BudgetControl
        [HttpPost]
        public JsonResult CreateBudgetControl(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.BudgetControlHeader where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from dbo.BudgetControlHeader where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from dbo.BudgetControlHeader where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("BudgetControlHeader", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult GetBudgetControlList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT TOP 100 * FROM (SELECT B.*,E.EmployeeName ApproveBy FROM dbo.BudgetControlHeader B
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=B.ApproveById) AS TEMP WHERE " + strkey + " order by UserName";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteBudgetControl(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("Delete From dbo.BudgetControlHeader where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
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
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat,string entityids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            IWorkbook workbook = accountsInventoryPayableReportService.GetSampleFileBudgetControlChild(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, entityids);
            var reportFileName = "Budget Control Data upload Sample File";

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }

        [HttpPost]
        public JsonResult CreateBudgetControlChild(List<Dictionary<string, object>> data, string headerId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsId;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.BudgetControlChild where  BudgetControlId='" + headerId + "'", out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT Count(Id)Id FROM dbo.BudgetControlChild where  BudgetControlId='" + headerId + "'", out dsId, false, "1");
                var count = 0;
                if (dsId.Tables[0].Rows.Count > 0)
                {
                    count = Convert.ToInt32(dsId.Tables[0].Rows[0]["Id"].ToString());
                }
                if (data != null)
                {
                    foreach (var item in data)
                    {

                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            count++;
                            item["Id"] = headerId + "-" + count;
                            item["BudgetControlId"] = headerId;
                            if (string.IsNullOrEmpty(item["ResponsiblePersonId"].ToString()))
                            {
                                item["ResponsiblePersonId"] = DBNull.Value;
                            }
                            if (string.IsNullOrEmpty(item["ActionById"].ToString()))
                            {
                                item["ActionById"] = DBNull.Value;
                            }
                            if (string.IsNullOrEmpty(item["UoMId"].ToString()))
                            {
                                item["UoMId"] = DBNull.Value;
                            }
                            if (string.IsNullOrEmpty(item["IsLinear"].ToString()))
                            {
                                item["IsLinear"] = "0";
                            }
                            else
                            {
                                item["IsLinear"] = "1";
                            }
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }



                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult ImportData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<BudgetControlChildTemplate> data = new List<BudgetControlChildTemplate>();

                var file = Request.Files["file"];

                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {

                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
                    try
                    {
                        string connString = string.Empty;
                        ExcelEngine excelEngine = null;
                        IApplication application = null;
                        IWorkbook workbook = null;

                        excelEngine = new ExcelEngine();
                        application = excelEngine.Excel;
                        workbook = excelEngine.Excel.Workbooks.Open(path);

                        DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                        DataSet dsExcel = new DataSet();
                        dsExcel.Tables.Add(dt);


                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            exception += "\r\nTrying to delete";
                            docFile.Delete();
                        }

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                BudgetControlChildTemplate vm = new BudgetControlChildTemplate();

                                
                                vm.EntityId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.BudgetMasterActivityId = dsExcel.Tables[0].Rows[i][12].ToString().Trim();
                                vm.IsLinear = dsExcel.Tables[0].Rows[i][18].ToString();
                                vm.CurrentValue =Convert.ToDecimal(dsExcel.Tables[0].Rows[i][19].ToString().Trim());
                                vm.LastValue = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][20].ToString().Trim());
                                vm.UoMId = dsExcel.Tables[0].Rows[i][21].ToString().Trim();
                                vm.ResponsiblePersonId = dsExcel.Tables[0].Rows[i][22].ToString().Trim();
                                vm.ActionById = dsExcel.Tables[0].Rows[i][23].ToString().Trim();
                                vm.Remarks = dsExcel.Tables[0].Rows[i][24].ToString().Trim();
                               
                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
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
                catch (Exception ex)
                {
                    //throw ex;
                }
                finally
                {
                }
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetBudgetControlChildList(string headerId)
        {
            return Json(_sqlRepository.GetDataCollection(@"SELECT * FROM[dbo].[BudgetControlChild] Where BudgetControlId = '"+headerId+"'", null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEntityList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_sqlRepository.GetDataCollection(@"SELECT CAST(0 as bit) Flag,Id, rd.UserName AS [EntityName], PlantId, (SELECT UserName FROM  [ORG].[Plant] WHERE Id=rd.PlantId) AS [Plant], DivisionId, (SELECT UserName FROM  [ORG].[Division] WHERE Id=rd.DivisionId) AS [Division], SubDivisionId, (SELECT UserName FROM  [ORG].[SubDivision] WHERE Id=rd.SubDivisionId) AS [SubDivision], UnitId, (SELECT UserName FROM  [ORG].[Unit] WHERE Id=rd.UnitId) AS [Unit], REPLACE(CONVERT(CHAR(11), EffectiveDate, 106),' ','-') AS [EffectiveDate], REPLACE(CONVERT(CHAR(11), EffectiveDateUpTo, 106),' ','-') AS [EffectiveDate UpTo] FROM  [ORG].[Entity] as rd WHERE Archive=0 AND rd.Active=1 AND CompanyId='" + identity.CompanyId+ @"' 
ORDER BY [EntityName], [Plant], [Division], [SubDivision], [Unit] asc", null), JsonRequestBehavior.AllowGet);
        }
        #endregion BudgetControl

    }
    public class BudgetControlChildTemplate
    {

        public string Id { get; set; }
        public string BudgetControlId { get; set; }
        public string BudgetMasterActivityId { get; set; }
        public string IsLinear { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal LastValue { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string ActionById { get; set; }
        public string UoMId { get; set; }
        public string Remarks { get; set; }
        public string EntityId { get; set; }
 
    }
}