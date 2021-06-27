using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Service.Reports;
using Library.Service.SecurityDeposits;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class SecurityDepositController : BaseController
    {
        private readonly ISecurityDepositService _securityDepositService;
        private readonly ISecurityDepositWriteOffService _securityDepositWriteOffService;
        private readonly ISecurityDepositReportService _securityDepositReportService;

        public SecurityDepositController(
            ISecurityDepositService securityDepositService
            , ISecurityDepositWriteOffService securityDepositWriteOffService
            , ISecurityDepositReportService securityDepositReportService)
        {
            _securityDepositService = securityDepositService;
            _securityDepositWriteOffService = securityDepositWriteOffService;
            _securityDepositReportService = securityDepositReportService;
        }

        [Authorize, HttpGet]
        public JsonResult GetById(string id)
        {
            return Json(_securityDepositService.GetById(id), JsonRequestBehavior.AllowGet);
        }

       
        public ActionResult SecurityDeposit()
        {
            return View("~/Areas/Accounts/Views/SecurityDeposit.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetSecurityDepositList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_securityDepositService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SecurityDeposit), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertSecurityDeposit(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.SecurityDeposit.ToString();
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Input Amount !");
            if (voucherVM.CompanyCurrencyRate < 0 || voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.PartyType == PartyType.Customer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            if (voucherVM.PartyType == PartyType.Vendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            if (voucherVM.PaymentSource!=PaymentSource.GL.ToString() && voucherVM.FinancingTypeId ==  null)
                throw new CustomException("Please Select Transaction Type!");
            if (voucherVM.PaymentSource == PaymentSource.GL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please Select GL!");
            if (voucherVM.PaymentSource == PaymentSource.GL.ToString() && voucherDetailVMList != null)
            {
                if(voucherVM.Amount!= voucherDetailVMList.Sum(r=>r.Amount))
                throw new CustomException("Amount and Total GL Amount is not equal!");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _securityDepositService.SaveSecurityDeposit(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateSecurityDeposit(VoucherViewModel voucherVM)
        {
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Input Amount !");
            if (voucherVM.CompanyCurrencyRate < 0 || voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");

            if (voucherVM.PartyType == PartyType.Customer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            if (voucherVM.PartyType == PartyType.Vendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            if (voucherVM.FinancingTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult PostSecurityDeposit(string securityDepositId)
        {
            _securityDepositService.Post(securityDepositId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet, Authorize]
        public ActionResult ReportSecurityDeposit(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _securityDepositReportService.GetSecurityDepositTakenReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        
        public ActionResult SecurityDepositWriteOff()
        {
            return View("~/Areas/Accounts/Views/SecurityDepositWriteOff.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetSecurityDepositWriteOffList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_securityDepositWriteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SecurityDeposit), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertSecurityDepositWriteOff()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult UpdateSecurityDepositWriteOff()
        {
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult PostSecurityDepositWriteOff()
        {
            return Json(new { Message = AplosMessage.Updated });
        }
    }
}