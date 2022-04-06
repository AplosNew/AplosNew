using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class VoucherGlUpdateController : BaseController
    {
        private readonly IVoucherService _voucharService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public VoucherGlUpdateController(
             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/VoucherGlUpdate/Aplos.cshtml");
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
            accountsCommonService.UpdateVoucherGl(voucherDetailVMList);

            return Json(new { Message = AplosMessage.Updated });
        }

    }
}