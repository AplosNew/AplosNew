#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Banks;
using Library.Model.Vouchers;
using Library.Service.Banks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Banks.Controllers
{
    public class BankReconciliationController : BaseController
    {
        #region Constructor

        private readonly IBankReconciliationService _bankReconciliationService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IBankReportService _bankReportService;

        public BankReconciliationController(IBankReconciliationService bankReconciliationService, ISqlRepository sqlRepository, IBankReportService bankReportService)
        {
            _bankReconciliationService = bankReconciliationService;
            _bankReportService = bankReportService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Aplos


        public ActionResult BankReconciliation()
        {
            return View("~/Areas/Banks/Views/BankReconciliation.cshtml");
        }

        #endregion Aplos

        #region Operation
        [Authorize, HttpGet]
        public JsonResult GetBankreconciliationList(GridParameter parameters)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankReconciledList(DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankReconciledList(identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetIssuedNotPresentList(GridParameter parameters, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetIssuedNotPresentList(parameters, identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetReceivedNotPresentList(GridParameter parameters, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetReceivedNotPresentList(parameters, identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCrReconList(GridParameter parameters, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankCrReconList(parameters, identity.CompanyGroupId, identity.CompanyId, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetBankCrReconListSyncfusion(string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsBankReconcilliationService.GetBankCrReconListSyncfusion(identity.CompanyGroupId, identity.CompanyId, bankMasterId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetBankDrReconListSyncfusion(DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsBankReconcilliationService.GetBankDrReconListSyncfusion(identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [HttpGet, Authorize]
        public JsonResult GetBankDrReconList(GridParameter parameters, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankDrReconList(parameters, identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankReconLastDate(string bankMasterId)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankReconLastDate(identity.CompanyGroupId, identity.CompanyId, bankMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankReconDrCrTotalAmount(string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankReconDrCrTotalAmount(identity.CompanyGroupId, identity.CompanyId, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(BankReconciliation bankReconciliation, List<GLTransactionDetail> tempList)
        {
            _bankReconciliationService.InsertBankReconciliation(bankReconciliation, tempList);
            return Json(new { Message = AplosMessage.Insert });
        }


        [HttpGet, Authorize]
        public ActionResult CRReconcileReport(string BankMasterID,string fromDate,string toDate)
        {
            try
            {
                _bankReportService.CRReconcileReport(BankMasterID, fromDate, toDate);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult DRReconcileReport(string BankMasterID, string fromDate, string toDate,string cutOffDate)
        {
            try
            {
                _bankReportService.DRReconcileReport(BankMasterID, fromDate, toDate, cutOffDate);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpPost]
        public JsonResult DeleteBankreconciliation(string bankReconciliationId)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);

            accountsBankReconcilliationService.DeleteBankreconciliation(bankReconciliationId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Operation
    }
}