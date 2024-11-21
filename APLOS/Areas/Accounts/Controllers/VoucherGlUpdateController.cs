using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Service.Extension.Accounts;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class VoucherGlUpdateController : BaseController
    {
        private readonly IVoucherService _voucharService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        public VoucherGlUpdateController(
             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IVoucherService voucherService
            )
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _voucherService = voucherService;
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/VoucherGlUpdate/Aplos.cshtml");
        }
        public ActionResult CustomerConfirmation()
        {
            return View("~/Areas/Accounts/Views/VoucherGlUpdate/CustomerConfirmation.cshtml");
        }


        [HttpPost, Authorize]
        public ActionResult GetVoucherDataList(string voucherNo)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsCommonService.getVoucherGLDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherNo), Error = false }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult Data(string voucherId)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            return Json(accountsCommonService.getVoucherData(voucherId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateVoucherGl(IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
            var voucher = _voucherService.FindVoucher(voucherDetailVMList.FirstOrDefault().VoucherId);
            _accountsCommonService.CheckingFiscalYearClose(voucher);

            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            foreach (var voucherDetailVM in voucherDetailVMList)
            {
                if (voucherDetailVM.DrAmount > 0)
                {
                    DataSet dsCapitalizationMaster = null;
                    string capitalizationMastersql = @"SELECT * FROM [TRN].[CapitalizationMasterDetail] where VoucherDetailId = '" + voucherDetailVM.Id + "' ";
                    con.OpenDataSetThroughAdapter(capitalizationMastersql, out dsCapitalizationMaster, false, "1");
                    if (dsCapitalizationMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher GL update not allowed, Voucher No. '" + voucher.VoucherNo + "'  Activity: '" + voucherDetailVM.ActivityName + "' already Capitalized!");
                    }
                }
            }

            accountsCommonService.UpdateVoucherGl(voucherDetailVMList);

            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public ActionResult GetcustomerInvoiceList(string[] CustomerSelectedList, string fromDate, string toDate, string paymentStatus)
        {
            string customerSelectedList = "";

            foreach (var item in CustomerSelectedList)
            {
                if (string.IsNullOrEmpty(customerSelectedList))
                {
                    customerSelectedList += "''," + item;
                }
                else
                {
                    customerSelectedList += "," + item;
                }

            }
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsCommonService.GetcustomerInvoiceList(identity.CompanyGroupId, identity.CompanyId,identity.PlantId, customerSelectedList, fromDate, toDate, paymentStatus), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
        [HttpPost]
        public JsonResult UpdateInvoice(IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            accountsCommonService.UpdateInvoiceforConfirm(voucherDetailVMList);

            return Json(new { Message = AplosMessage.Updated });
        }

    }
}