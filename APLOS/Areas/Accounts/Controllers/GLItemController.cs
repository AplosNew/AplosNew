using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.ChartOfAccounts;
using Library.Model.Enums;
using Library.Service.ChartOfAccounts;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Materials;
using Library.Service.Vouchers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class GLItemController : BaseController
    {
        private readonly IGLGeneralInfoService _glGeneralInfoService;
        private readonly IGLCompanyInfoService _glCompanyInfoService;
        private readonly IGLAccountTypeService _glAccountTypeService;
        private readonly IVoucherReportService _voucharReportService;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly AccountVoucherReportService _accountVoucherReportService;
        private readonly ISqlRepository _sqlRepository;

        public GLItemController(
            IGLGeneralInfoService glGeneralInfoService
            , IGLCompanyInfoService glCompanyInfoService
            , IGLAccountTypeService glAccountTypeService
            , IVoucherReportService voucharReportService
            , IMaterialMasterService materialMasterService
            , IEmployeePayableService employeePayableService
            , AccountVoucherReportService accountVoucherReportService
            , ISqlRepository sqlRepository
            )
        {
            _glGeneralInfoService = glGeneralInfoService;
            _glCompanyInfoService = glCompanyInfoService;
            _glAccountTypeService = glAccountTypeService;
            _voucharReportService = voucharReportService;
            _materialMasterService = materialMasterService;
            _employeePayableService = employeePayableService;
            _accountVoucherReportService = accountVoucherReportService;
            _sqlRepository = sqlRepository;
        }

        [Authorize, HttpGet]
        public JsonResult GetCompanyGLCboList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glCompanyInfoService.GetCompanyGLCboList(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult GLCompanyInfo()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult GLMapping()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(GLGeneralInfo glGeneralInfo, IEnumerable<GLAccountType> glAccountType)
        {
            _glGeneralInfoService.Insert(glGeneralInfo, glAccountType);
            return Json(new { GLGeneralInfo = glGeneralInfo, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(GLGeneralInfo glGeneralInfo, IEnumerable<GLAccountType> glAccountType)
        {
            _glGeneralInfoService.Update(glGeneralInfo, glAccountType);
            return Json(new { GLGeneralInfo = glGeneralInfo, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _glGeneralInfoService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult GlcompanyinfoInsert(string glcominfolist, string companyId)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<GLCompanyInfo> gLCompanyInfoList = JsonConvert.DeserializeObject<List<GLCompanyInfo>>(glcominfolist, settings);

            //var gLCompanyInfoList = glcominfolist.ToList();
            _glGeneralInfoService.InsertGlCompany(gLCompanyInfoList, companyId);
            return Json(new { GLGeneralInfo = gLCompanyInfoList, Message = AplosMessage.Success });
        }

        [Authorize, HttpGet]
        public JsonResult GetCboList()
        {
            return Json(new SelectList(_glGeneralInfoService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence(string coaId)
        {
            return Json(_glGeneralInfoService.GetAutoSequence(coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetGlComInfoSequence()
        {
            return Json(_glCompanyInfoService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetGLGeneralInfoList(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.Query(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetGLLevelByLevel(GridParameter parameters, string level)
        {
            return Json(_glGeneralInfoService.GetGLLevelByLevel(parameters, level), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGLList(GridParameter parameters, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyGroupId))
                companyGroupId = identity.CompanyGroupId;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetGLList(parameters, companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGLListLedgerReport(GridParameter parameters, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyGroupId))
                companyGroupId = identity.CompanyGroupId;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetGLListLedgerReport(parameters, companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGLListWithBudget(GridParameter parameters, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyGroupId))
                companyGroupId = identity.CompanyGroupId;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetGLListWithBudget(parameters, companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAllGLList(GridParameter parameters, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyGroupId))
                companyGroupId = identity.CompanyGroupId;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetGLListWithBudget(parameters, companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAllGLListSetup(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetAllGLList(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetGlCompanyConfigList(string companyId)
        {
            return Json(_glGeneralInfoService.GetGlCompanyConfigList(companyId), JsonRequestBehavior.AllowGet);
        }

        [Obsolete]
        [Authorize, HttpGet]
        public ActionResult GetVendorInvoiceGLList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetVendorInvoiceGLList(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetVendorInvoiceGLBudgetList(GridParameter parameters)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetInvoiceGLBudgetList(parameters, identity.CompanyGroupId, identity.CompanyId, AccountTypeEnum.Expense.ToString()), JsonRequestBehavior.AllowGet);
            //return Json(_glGeneralInfoService.GetInvoiceGLBudgetList(parameters, identity.CompanyGroupId, identity.CompanyId, AccountTypeEnum.Expense.ToString()), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetOtherVendorChargesGLBudgetList(GridParameter parameters,string serviceMasterId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetOtherVendorChargesGLBudgetList(parameters, identity.CompanyGroupId, identity.CompanyId, serviceMasterId, AccountTypeEnum.Expense.ToString()), JsonRequestBehavior.AllowGet);
            //return Json(_glGeneralInfoService.GetInvoiceGLBudgetList(parameters, identity.CompanyGroupId, identity.CompanyId, AccountTypeEnum.Expense.ToString()), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetExpenseTypeGLBudgetActivityList(GridParameter parameters)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetGLBudgetActivityList(parameters, identity.CompanyGroupId, identity.CompanyId, AccountTypeEnum.Expense), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetGLControlList(GridParameter parameters,string companyId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetInvoiceGLBudgetList(parameters, identity.CompanyGroupId, companyId, AccountTypeEnum.Expense.ToString()), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetIssuePostingGLBudgetActivityList(GridParameter parameters)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetIssuePostingGLBudgetActivityList(parameters, identity.CompanyGroupId, identity.CompanyId, AccountTypeEnum.Expense), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetNonReconAssetLiabilityGLBudgetActivityList(GridParameter parameters)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetNonReconAssetLiabilityGLBudgetActivityList(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetGLBudgetActivityForEmployeeSalaryPayable(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetGLBudgetActivityForEmployeeSalaryPayable(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult GetAllGLBudgetActivityExceptRecon(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetAllGLBudgetActivityExceptRecon(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAllGLBudgetActivityByCompnay(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetAllGLBudgetActivityPostingAutomaticOnly(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetAllGLBudgetActivityPostingAutomaticOnly(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetAllGLBudgetActivityPostingAutomaticOnly(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAllGLBudgetActivityForAdvanceJournal(GridParameter parameters)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetAllGLBudgetActivityForAdvanceJournal(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAllLiabilityGLBudgetActivity(GridParameter parameters)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetAllLiabilityGLBudgetActivity(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetInterTransactionGLBudgetActivity(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetInterTransactionGLBudgetActivity(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetGLBudgetActivityList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetGLBudgetActivityList(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAllGLBudgetActivityList(GridParameter parameters)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetAllGLBudgetActivity(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetExpenseRevenueGLBudgetActivity(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetExpenseRevenueGLBudgetActivity(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetReconMaterialTypeMaterialMasterGL(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetReconMaterialTypeMaterialMasterGL(parameters,  identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCustomerInvoiceGLList2()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetCustomerInvoiceGLList(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCustomerInvoiceGLBudgetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetInvoiceGLBudgetList(parameters, identity.CompanyGroupId, identity.CompanyId, AccountTypeEnum.Revenue.ToString()), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCustomerInvoiceGLBudgetListWithCompany(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetInvoiceGLBudgetList(parameters, identity.CompanyGroupId, companyId, AccountTypeEnum.Revenue.ToString()), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGLByAccountCode(string accountcode)
        {
            return Json(_glGeneralInfoService.GetGLByAccountCode(accountcode).Rows, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult CheckAccountCode(string accountCode)
        {
            return Json(_glGeneralInfoService.AccountCodeChecking(accountCode), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGLAccountCode(GridParameter parameters)
        {
            return Json(_glGeneralInfoService.GetGLAccountCode(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetBankGLAccountCode(GridParameter parameters, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyGroupId))
                companyGroupId = identity.CompanyGroupId;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetBankGLAccountCode(parameters, companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCreditableGL(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_glGeneralInfoService.GetCreditableGL(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCreditableGLCOAWise(GridParameter parameters, string coaId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetCreditableGLTaxRecon(parameters, coaId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetNonCreditableGL(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetExpensesGL(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetNonCreditableGLCOAWise(GridParameter parameters, string coaId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetExpensesGLSetup(parameters, coaId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetWithHoldGL(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetWithHoldGL(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetWithHoldGLCOAWise(GridParameter parameters, string coaId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetWithHoldGLSetupTaxRecon(parameters, coaId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Obsolete]
        [Authorize, HttpGet]
        public JsonResult GetVendorAdditionalGLList(GridParameter parameters, string partyId, string companyId)
        {
            return Json(_glGeneralInfoService.GetVendorAdditionalGLList(parameters, partyId, companyId), JsonRequestBehavior.AllowGet);
        }

        [Obsolete]
        [Authorize, HttpGet]
        public JsonResult GetCustomerAdditionalGLList(GridParameter parameters, string partyId)
        {
            return Json(_glGeneralInfoService.GetCustomerAdditionalGLList(parameters, partyId), JsonRequestBehavior.AllowGet);
        }

        #region Customer GL

        [Authorize, HttpGet]
        public JsonResult GetCustomerReconeGLCOAWise(GridParameter parameters, string coaId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            return Json(_accountsGLService.GetGLListByCOA(parameters, coaId, AccountTypeEnum.Asset, ReconcileAccountEnum.Customer), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerReconeGLPartyAccountGroup(GridParameter parameters, string coaId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            return Json(_accountsGLService.GetReconeGLPartyAccountGroup(parameters, coaId, AccountTypeEnum.Asset, ReconcileAccountEnum.Customer), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPartyCreditGLAccountCode(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetPartyDRGLAccountCode(parameters, identity.CompanyGroupId, companyId, ReconcileAccountEnum.Vendor, AccountTypeEnum.Liability), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerDownpaymentGLCOAWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetGLListByCOA(parameters, coaId, AccountTypeEnum.Liability, ReconcileAccountEnum.Customer), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerDownpaymentGL(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetPartyDRGLAccountCode(parameters, identity.CompanyGroupId, companyId, ReconcileAccountEnum.Customer, AccountTypeEnum.Liability), JsonRequestBehavior.AllowGet);
        }

        #endregion Customer GL

        #region Vendor GL

        [Authorize, HttpGet]
        public JsonResult GetVendorReconeGLCOAWise(GridParameter parameters, string coaId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            return Json(_accountsGLService.GetGLListByCOA(parameters, coaId, AccountTypeEnum.Liability, ReconcileAccountEnum.Vendor), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetVendorReconeGLPartyAccountGroup(GridParameter parameters, string coaId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            return Json(_accountsGLService.GetReconeGLPartyAccountGroup(parameters, coaId, AccountTypeEnum.Liability, ReconcileAccountEnum.Vendor), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPartyDebitGLAccountCode(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetPartyDRGLAccountCode(parameters, identity.CompanyGroupId, companyId, ReconcileAccountEnum.Customer, AccountTypeEnum.Asset), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetVendorDownpaymentGLCOAWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetGLListByCOA(parameters, coaId, AccountTypeEnum.Asset, ReconcileAccountEnum.Vendor), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetVendorDownpaymentGL(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetPartyDRGLAccountCode(parameters, identity.CompanyGroupId, companyId, ReconcileAccountEnum.Vendor, AccountTypeEnum.Asset), JsonRequestBehavior.AllowGet);
        }

        #endregion Vendor GL

        [Authorize, HttpGet]
        public JsonResult GetClearingAccountGL(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetClearingAccountGL(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAssetCOAWise(GridParameter parameters, string coaId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);

            return Json(_accountsGLService.GetAssetCOAWise(parameters, coaId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetAssetCOAWiseIncentive(GridParameter parameters, string coaId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);

            return Json(_accountsGLService.GetAssetCOAWiseIncentive(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAssetBudgetActivityCOAWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetAssetBudgetActivityCOAWise(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetReconAssetTypeGL(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetReconAssetTypeGL(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEquityGLCOAWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetEquityGLList(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEquityLiabilityGLCOAWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetEquityLiabilityGLList(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetLiabilityCOAWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetLiabilityGLList(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetLiabilityCOAWiseExceptRecon(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetLiabilityGLListExceptRecon(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAssetCOAWiseExceptRecon(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetAssetGLListExceptRecon(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAssetLiabilityGLListExceptRecon(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetAssetLiabilityGLListExceptRecon(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAssetGLCOAWiseTaxRecon(GridParameter parameters, string coaId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            return Json(_accountsGLService.GetAssetLiabilityGLListTaxRecon(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetLiabilityGLCOAWiseTaxRecon(GridParameter parameters, string coaId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            return Json(_accountsGLService.GetAssetLiabilityGLListTaxRecon(parameters, coaId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetExpenseGLCOAWiseTaxRecon(GridParameter parameters, string coaId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            return Json(_accountsGLService.GetExpenseGLTaxRecon(parameters, coaId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetAccomultateDepriciationGL(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetAccomultateDepriciationGL(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDepriciationExpensesGL(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetDepriciationExpensesGL(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeReconAssetWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetEmployeeReconAssetWise(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeReconLiabilityCOAWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetEmployeeReconLiabilityCOAWise(parameters, coaId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEmployeeReconGLBudgetActivity(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeePayableService.GetEmployeeReconGLBudgetActivity(parameters,identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEmployeeReconAssetGLBudgetActivity(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeePayableService.GetEmployeeReconAssetGLBudgetActivity(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetAssetLiabilityGL(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetAssetLiabilityGL(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAUCGL(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetAUCGL(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetExpensesGL(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetExpensesGL(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetExpenseGLCOAWise(GridParameter parameters, string coaId)
        {
            AccountsGLService accountsGLService = new AccountsGLService(_sqlRepository);

            return Json(accountsGLService.GetExpenseGLList(parameters, coaId), JsonRequestBehavior.AllowGet);
            //return Json(_glGeneralInfoService.GetExpenseGLList(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetRevenueGLCOAWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetRevenueGLList(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetRevenueExpensesGLCOAWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetRevenueExpensesGLCOAWise(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetRevenueExpensesGLBudgetCOAWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetRevenueExpensesBudgetCOAWise(parameters, coaId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetExpenseGLBudgetActivityCOAWise(GridParameter parameters, string coaId)
        {
            AccountsGLService accountsGLService = new AccountsGLService(_sqlRepository);
            return Json(accountsGLService.GetExpensesGLBudgetActivityCOAWise(parameters, coaId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetBalanceSheetGLCOAWise(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetBalanceSheetGLCOAWise(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetBalanceSheetGLAssetRecon(GridParameter parameters, string coaId)
        {
            return Json(_glGeneralInfoService.GetBalanceSheetGLAssetRecon(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetEquityGLList(GridParameter parameters, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyGroupId))
                companyGroupId = identity.CompanyGroupId;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetEquityGLList(parameters, companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCashGL(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetCashGL(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetATypeAssetAndReconAssetGL(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetATypeAssetAndReconAssetGL(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetATypeAssetAndReconAssetGLWithFixedAssetMaster(GridParameter parameters, string companyId, string fixedAssetMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_materialMasterService.GetATypeAssetAndReconAssetGLWithFixedAssetMaster(parameters, companyId, fixedAssetMasterId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFixedAssetMasterGL(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetFixedAssetMasterGL(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFixedAssetAccDepGL(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetFixedAssetAccDepGL(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFixedAssetAUCGL(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetFixedAssetAUCGL(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetATypeExpenseGL(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_glGeneralInfoService.GetATypeExpenseGL(parameters, companyId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetExpenseGLBudgetActivity(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            
            return Json(_glGeneralInfoService.GetExpenseGLBudgetActivity(parameters,  identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetRevenueGLBudgetActivity(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            
            return Json(_glGeneralInfoService.GetRevenueGLBudgetActivity(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetGLCompanyInfoList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glCompanyInfoService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetGLCompanyInfoListByGLId(GridParameter parameters, string glId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glCompanyInfoService.GetGLCompanyInfoListByGLId(parameters, glId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetGLAccountTypeByGLId(GridParameter parameters, string glId)
        {
            return Json(_glAccountTypeService.Query(parameters, glId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetGLGeneralInfoById(string id)
        {
            return Json(_glGeneralInfoService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetIssueAUCGLBudgetActivity(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetIssueAUCGLBudgetActivity(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAssetMasterGLBudget(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetAssetMasterGLBudget(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetRevenueExpenseGLBudget(GridParameter parameters)
        {
            AccountsGLService accountsGLService = new AccountsGLService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsGLService.GetRevenueExpenseGLBudget(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCurrentAssetRevenueExpenseGLBudget(GridParameter parameters)
        {
            AccountsGLService accountsGLService = new AccountsGLService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsGLService.GetCurrentAssetRevenueExpenseGLBudget(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAssetMasterGLBudgetActivity(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glGeneralInfoService.GetAssetMasterGLBudgetActivity(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GeneralLedgerListReport()
        {
            return View("~/Areas/Accounts/Views/GeneralLedgerListReport.cshtml");
        }

        [HttpGet]
        public ActionResult GetGeneralLedgerListReport(ReportFormat reportFormat, string coaId)
        {

            var workbook = _accountVoucherReportService.GetGLReport(coaId);
            var reportFileName = "GL Master";
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
    }
}