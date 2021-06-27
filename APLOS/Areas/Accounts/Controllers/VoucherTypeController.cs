using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Vouchers;
using Library.Service.Vouchers;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class VoucherTypeController : BaseController
    {
        private readonly IVoucherTypeService _voucherTypeService;

        public VoucherTypeController(IVoucherTypeService voucherTypeService)
        {
            _voucherTypeService = voucherTypeService;
        }

        [HttpGet]
        public ActionResult VoucherType()
        {
            return View("~/Areas/Accounts/Views/VoucherType.cshtml");
        }

        [HttpGet]
        public ActionResult VoucherTypeMatrix()
        {
            return View("~/Areas/Accounts/Views/VoucherTypeMatrix.cshtml");
        }

        [HttpGet]
        public ActionResult VoucherTypeConfig()
        {
            return View("~/Areas/Accounts/Views/VoucherTypeConfig.cshtml");
        }

        [Authorize]
        public JsonResult GetVoucherTypeCbo()
        {
            return Json(_voucherTypeService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetVoucherTypeList(GridParameter parameters)
        {
            return Json(_voucherTypeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetVoucherType(string id)
        {
            return Json(_voucherTypeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_voucherTypeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(VoucherType voucherType)
        {
            if (ModelState.IsValid)
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                voucherType.CompanyGroupId = identity.CompanyGroupId;
                _voucherTypeService.Insert(voucherType);
                return Json(new { VoucherType = voucherType, Sequence = _voucherTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(VoucherType voucherType)
        {
            if (ModelState.IsValid)
            {
                _voucherTypeService.Update(voucherType);
                return Json(new { Sequence = _voucherTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _voucherTypeService.Archive(id);
                return Json(new { Sequence = _voucherTypeService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}