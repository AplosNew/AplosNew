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
    public class BuyerController : BaseController
    {
        private readonly IBuyerService _buyerService;

        public BuyerController(IBuyerService buyerService)
        {
            _buyerService = buyerService;
        }

      
        public ActionResult Aplos()
        {
            return View("~/Areas/Parties/Views/Buyer.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_buyerService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBuyer()
        {
            return Json(_buyerService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Buyer buyer)
        {
            _buyerService.Insert(buyer);
            return Json(new { Buyer = buyer, Sequence = _buyerService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Buyer buyer)
        {
            _buyerService.Update(buyer);
            return Json(new { Sequence = _buyerService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _buyerService.Archive(id);
            return Json(new { Sequence = _buyerService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}