using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Service.Advances;
using Library.Service.Employees;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class EmployeePayableController : BaseController
    {
        private readonly IEmployeePayableService _employeePayableService;
        private readonly IEmployeePayableWriteOffService _employeePayableWirteOffService;
        
        private readonly IAdvanceWriteOffService _advanceWriteOffService;
        private readonly ISqlRepository _sqlRepository;
        private readonly AccountVoucherReportService _accountVoucherReportService;
        public EmployeePayableController(
            IEmployeePayableService employeePayableService
            , IEmployeePayableWriteOffService employeePayableWirteOffService
            , IAdvanceWriteOffService advanceWriteOffService
           
            , ISqlRepository sqlRepository
            , AccountVoucherReportService accountVoucherReportService)
        {
            _employeePayableService = employeePayableService;
            _employeePayableWirteOffService = employeePayableWirteOffService;
            _advanceWriteOffService = advanceWriteOffService;
            _sqlRepository = sqlRepository;
            _accountVoucherReportService = accountVoucherReportService;
        }

        #region EmployeePayable
        [HttpGet, Authorize]
        public ActionResult EmployeePayable()
        {
            return View("~/Areas/Accounts/Views/EmployeePayable.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeePayableList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeePayableService.GetEmployeePayableList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeePayable), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult InsertEmployeePayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.EmployeePayable.ToString();
            voucherVM.PartyType = PartyType.Employee.ToString();
            foreach (var advanceDetailVM in voucherDetailList)
            {
                if (advanceDetailVM.DrAmount == 0 || advanceDetailVM.DrAmount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _employeePayableService.InsertEmployeePayable(voucherVM, voucherDetailList, taxDetailVMList)) });
        }

        public JsonResult UpdateEmployeePayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.EmployeePayable.ToString();
            voucherVM.PartyType = PartyType.Employee.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _employeePayableService.UpdateEmployeePayable(voucherVM, voucherDetailList)) });
        }

        [HttpPost]
        public JsonResult PostEmployeePayable(string id)
        {
            _employeePayableService.Post(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public ActionResult DeleteEmployeePayable(string employeeBookingId, string voucherId)
        {
            _employeePayableWirteOffService.DeleteEmployeePayable(employeeBookingId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeAvailableInvoiceList(GridParameter parameters, string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountVoucherReportService.GetEmployeeAvailableInvoiceList(parameters, identity.CompanyGroupId, identity.CompanyId,identity.PlantId, employeeId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetMultipleEmployeeList(string column, string value, GridParameter parameters)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = _accountsInvoiceService.GetMultipleEmployeeListQuery(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, parameters);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpGet, Authorize]
        public JsonResult GetMultipleEmployeeAvailableInvoiceList(GridParameter parameters, string employeeId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetMultipleEmployeeAvailableInvoiceList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, employeeId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region EmployeePayment

        public ActionResult EmployeePayment()
        {
            return View("~/Areas/Accounts/Views/EmployeePayment.cshtml");
        }
        public ActionResult MultipleEmployeePayment()
        {
            return View("~/Areas/Accounts/Views/MultipleEmployeePayment.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeePaymentList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeePayableWirteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeListByPlant(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountVoucherReportService.EmployeeListByPayable(parameters, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEmployeeListAllPlant(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountVoucherReportService.EmployeeListAllPlant(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertEmployeePayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.ToCurrencyRate = 1;
            //if(voucherVM.ToCurrencyRate==0)
            //    throw new CustomException("Rate can not 0");
            if ((voucherVM.PaymentSource == PaymentSource.Bank.ToString()) && (voucherVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((voucherVM.PaymentSource == PaymentSource.Cash.ToString()) && (voucherVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            if (voucherDetailVMList == null)
                throw new CustomException("Please Select Payable");
            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _employeePayableWirteOffService.InsertEmployeePayment(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public ActionResult UpdateEmployeePayment()
        {
            return View();
        }

        [HttpPost]
        public JsonResult PostEmployeePayment(string id)
        {
            _employeePayableWirteOffService.Post(id);
            return Json(new { Message = AplosMessage.Posted });
        }
        [HttpPost]
        public ActionResult DeleteEmployeePayment(string employeePayableWriteOffId, string voucherId)
        {
            _employeePayableWirteOffService.DeletePayableWriteOff(employeePayableWriteOffId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet]
        public JsonResult GetEmployeePayableById(string id)
        {
            return Json(_employeePayableService.GetEmployeePayable(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEmployeePayableDetailList(string voucherId)
        {
            return Json(_employeePayableService.GetEmployeePayableDetailList(voucherId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region EmployeeSalaryPayable
        [HttpGet, Authorize]
        public ActionResult EmployeeSalaryPayable()
        {
            return View("~/Areas/Accounts/Views/EmployeeSalaryPayable.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeSalaryPayableList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeePayableService.GetEmployeePayableList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SalaryPayable), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeSalaryPayableDetailList(string voucherId)
        {
            string sql = @"SELECT EP.Id,VD.VoucherId,VD.EmployeePayableDetailId,VD.AdvanceWriteOffDetailId,AWOD.AdvanceId,AWOD.AdvanceDetailId,IVT.InvoiceTaxId,VD.InvoiceTaxDetailId,V.VoucherNo,vd.PartyType,GL.AccountCode GLGeneralInfoCode
                         , B.UserName BudgetName, A.UserName ActivityName, VD.DrAmount,VD.CrAmount, 
                        DrDisable =case when VD.DrAmount > 0 then 0 else 1 end,
                        CrDisable =case when VD.CrAmount > 0 then 0 else 1 end
                          FROM TRN.VoucherDetail VD JOIN TRN.Voucher V ON V.Id = VD.VoucherId
                        LEFT JOIN TRN.EmployeePayable EP ON EP.VoucherId = V.Id
                        LEFT JOIN TRN.AdvanceWriteOff AWO ON AWO.VoucherId = V.Id
                        LEFT JOIN TRN.AdvanceWriteOffDetail AWOD ON AWOD.Id = VD.AdvanceWriteOffDetailId
                        LEFT JOIN TRN.InvoiceTaxDetail IVT ON IVT.Id = VD.InvoiceTaxDetailId
                        LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id = VD.GLGeneralInfoId
                        LEFT JOIN MST.BudgetMaster BM ON BM.Id = VD.BudgetMasterId
                        LEFT JOIN HKP.Budget B ON B.Id = BM.BudgetId
                        LEFT JOIN HKP.Activity A ON A.Id = VD.ActivityId
                        where V.Id='"+ voucherId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertEmployeeSalaryPayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.SalaryPayable.ToString();
            voucherVM.PartyType = PartyType.Employee.ToString();
            if (voucherDetailList.Sum(r => r.DrAmount) != voucherDetailList.Sum(r => r.CrAmount))
                throw new CustomException("Dr and Cr amount is not equal.");

            if (voucherVM.CurrencyId== null)
                throw new CustomException("Please select currency.");
            if (voucherVM.CompanyCurrencyRate == 0 || voucherVM.CompanyCurrencyRate.ToString() == null)
                throw new CustomException("Please input rate.");
            foreach (var advanceDetailVM in voucherDetailList)
            {
                if (advanceDetailVM.BudgetMasterId == null)
                    throw new CustomException("Budget should not null");
                if (advanceDetailVM.ActivityId == null)
                    throw new CustomException("Activity should not null");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertEmployeeSalaryPayable(voucherVM, voucherDetailList)) });
        }
        [HttpPost]
        public JsonResult PostEmployeeSalaryPayable(string id)
        {
            _advanceWriteOffService.PostEmployeeSalaryPayable(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public ActionResult DeleteEmployeeSalaryPayable(string payableId, string voucherId)
        {
            _advanceWriteOffService.DeleteEmployeeSalaryPayable(payableId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}