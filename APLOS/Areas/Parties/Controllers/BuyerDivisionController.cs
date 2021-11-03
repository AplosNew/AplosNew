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
    public class BuyerDivisionController : BaseController
    {
        private readonly IBuyerDivisionService _buyerDivisionService;

        public BuyerDivisionController(IBuyerDivisionService buyerDivisionService)
        {
            _buyerDivisionService = buyerDivisionService;
        }

      
        public ActionResult Aplos()
        {
            return View("~/Areas/Parties/Views/BuyerDivision.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string buyerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerDivisionService.GetCbo(buyerId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence(string buyerId)
        {
            return Json(_buyerDivisionService.GetAutoSequence(buyerId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBuyerDivisionList(GridParameter parameters, string buyerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerDivisionService.Query(parameters, buyerId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBuyerDivisionById(string id)
        {
            return Json(_buyerDivisionService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BuyerDivision buyerDivision)
        {
            _buyerDivisionService.Insert(buyerDivision);
            return Json(new { BuyerDivision = buyerDivision, Sequence = _buyerDivisionService.GetAutoSequence(buyerDivision.BuyerId), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BuyerDivision buyerDivision)
        {
            _buyerDivisionService.Update(buyerDivision);
            return Json(new { Sequence = _buyerDivisionService.GetAutoSequence(buyerDivision.BuyerId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id, string buyerId)
        {
            _buyerDivisionService.DeleteBuyerDivision(id);
            return Json(new { Sequence = _buyerDivisionService.GetAutoSequence(buyerId), Message = AplosMessage.Deleted });
        }
    }
}