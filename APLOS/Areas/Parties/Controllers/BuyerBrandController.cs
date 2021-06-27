using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class BuyerBrandController : BaseController
    {
        private readonly IBuyerBrandService _buyerBrandService;

        public BuyerBrandController(IBuyerBrandService buyerBrandService)
        {
            _buyerBrandService = buyerBrandService;
        }

    
        public ActionResult Aplos()
        {
            return View("~/Areas/Parties/Views/BuyerBrand.cshtml");
        }

        [AllowAnonymous]
        public JsonResult GetCbo(string buyerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_buyerBrandService.GetCbo(buyerId, identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult GetCboAll()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_buyerBrandService.GetCbo(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string buyerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerBrandService.Query(parameters, buyerId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string buyerId)
        {
            return Json(_buyerBrandService.GetAutoSequence(buyerId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BuyerBrand model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _buyerBrandService.Insert(model, identity.CompanyGroupId);
            return Json(new { BuyerBrand = model, Sequence = _buyerBrandService.GetAutoSequence(model.BuyerId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(BuyerBrand model)
        {
            _buyerBrandService.Update(model);
            return Json(new { Sequence = _buyerBrandService.GetAutoSequence(model.BuyerId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id, string buyerId)
        {
            _buyerBrandService.DeleteGraph(id);
            return Json(new { Sequence = _buyerBrandService.GetAutoSequence(buyerId), Message = AplosMessage.Deleted });
        }
    }
}