#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
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

        public BankReconciliationController(IBankReconciliationService bankReconciliationService)
        {
            _bankReconciliationService = bankReconciliationService;
        }

        #endregion Constructor

        #region Aplos

        [Authorize]
        public ActionResult BankReconciliation()
        {
            return View("~/Areas/Banks/Views/BankReconciliation.cshtml");
        }

        #endregion Aplos

        #region Operation

        [HttpGet, Authorize]
        public JsonResult GetBankReconciledList(DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankReconciliationService.GetBankReconciledList(identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetIssuedNotPresentList(GridParameter parameters, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankReconciliationService.GetIssuedNotPresentList(parameters, identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetReceivedNotPresentList(GridParameter parameters, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankReconciliationService.GetReceivedNotPresentList(parameters, identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCrReconList(GridParameter parameters, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankReconciliationService.GetBankCrReconList(parameters, identity.CompanyGroupId, identity.CompanyId, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankDrReconList(GridParameter parameters, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankReconciliationService.GetBankDrReconList(parameters, identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankReconLastDate(string bankMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankReconciliationService.GetBankReconLastDate(identity.CompanyGroupId, identity.CompanyId,bankMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BankReconciliation bankReconciliation, List<GLTransactionDetail> tempList)
        {
            _bankReconciliationService.InsertBankReconciliation(bankReconciliation, tempList);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion Operation
    }
}