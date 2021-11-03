using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Banks;
using Library.Service.Banks;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class CashMasterController : BaseController
    {
        private readonly ICashMasterService _cashMasterService;

        public CashMasterController(ICashMasterService cashMasterService)
        {
            _cashMasterService = cashMasterService;
        }

        [HttpGet]
        public ActionResult CashMaster()
        {
            return View("~/Areas/Banks/Views/CashMaster.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetCashMasterGL(string cashMasterId)
        {
            return Json(_cashMasterService.GetCashMasterGL(cashMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_cashMasterService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCashMasterCboList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_cashMasterService.GetCashMasterCboList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCashMasterCboListByEntity(string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_cashMasterService.GetCashMasterCboListByEntity(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_cashMasterService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCashMasterVoucher(GridParameter parameters, string id, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_cashMasterService.GetCashMasterList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, id, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCashMasterVoucherPayment(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_cashMasterService.GetCashMasterVoucherPayment(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCashMasterByGL(string glGeneralInfoId)
        {
            return Json(_cashMasterService.GetCashMasterByGL(glGeneralInfoId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCashMasterlist(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_cashMasterService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetBankMasterById(string id)
        {
            return Json(_cashMasterService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CashMaster cashMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            cashMaster.CompanyGroupId = identity.CompanyGroupId;
            _cashMasterService.Insert(cashMaster);
            return Json(new { BankMaster = cashMaster, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(CashMaster cashMaster)
        {
            _cashMasterService.Update(cashMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _cashMasterService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}