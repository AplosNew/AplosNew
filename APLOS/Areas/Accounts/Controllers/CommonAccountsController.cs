using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Enums;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CommonAccountsController : BaseController
    {
        private readonly ICommonAccountsSetOffService _commonAccountsSetOffService;

        public CommonAccountsController(ICommonAccountsSetOffService commonAccountsSetOffService)
        {
            _commonAccountsSetOffService = commonAccountsSetOffService;
        }

        [HttpPost, Authorize]
        public JsonResult InsertDebitNoteAdvanceSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
             , IEnumerable<VoucherDetailViewModel> voucherDetailInvoiceList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.DebitNoteSetOff.ToString();
            voucherVM.PaymentSource = voucherVM.SettlementType;
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate <= 0)
                throw new CustomException("Please Input Rate.");
            if ((voucherVM.PaymentSource == "Bank") && (voucherVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((voucherVM.PaymentSource == "Cash") && (voucherVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
                voucherVM.EntityId = advanceDetailVM.EntityId;
            }
                voucherVM.Amount = voucherDetailInvoiceList.Sum(r => r.Amount);
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _commonAccountsSetOffService.InsertDebitNoteAdvanceSetOff(voucherVM, voucherDetailVMList, voucherDetailInvoiceList)) });
           
        }
    }
} 