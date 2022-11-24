using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Vouchers;
using Library.Service.Vouchers;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class VoucherTypeConfigController : BaseController
    {
        private readonly IVoucherTypeConfigService _voucherTypeConfigService;

        public VoucherTypeConfigController(IVoucherTypeConfigService voucherTypeConfigService)
        {
            _voucherTypeConfigService = voucherTypeConfigService;
        }

        [HttpGet]
        public ActionResult VoucherTypeConfig()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetVoucherTypeConfigList(GridParameter parameters, string companyId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeConfigService.Query(parameters, identity.CompanyGroupId, companyId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateVoucherTypeConfig(VoucherTypeConfig voucherTypeConfige)
        {
            if (voucherTypeConfige.CompanyGroupId == null || voucherTypeConfige.CompanyGroupId == "")
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                voucherTypeConfige.CompanyGroupId = identity.CompanyGroupId;
            }
            _voucherTypeConfigService.Insert(voucherTypeConfige);
            return Json(new { VoucherTypeConfig = voucherTypeConfige, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult UpdateVoucherTypeConfig(VoucherTypeConfig voucherTypeConfige)
        {
            if(voucherTypeConfige.CompanyGroupId==null|| voucherTypeConfige.CompanyGroupId == "")
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                voucherTypeConfige.CompanyGroupId = identity.CompanyGroupId;
            }
            _voucherTypeConfigService.Update(voucherTypeConfige);
            return Json(new { VoucherTypeConfig = voucherTypeConfige, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult DeleteVoucherTypeConfig(int id)
        {
            _voucherTypeConfigService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}