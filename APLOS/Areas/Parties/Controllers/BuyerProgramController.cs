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
    public class BuyerProgramController : BaseController
    {
        private readonly IBuyerProgramService _buyerProgramService;

        public BuyerProgramController(IBuyerProgramService buyerProgramService)
        {
            _buyerProgramService = buyerProgramService;
        }

   
        public ActionResult Aplos()
        {
            return View("~/Areas/Parties/Views/BuyerProgram.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string buyerid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerProgramService.GetCbo(buyerid, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetBuyerProgramList(GridParameter parameters, string buyerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerProgramService.Query(parameters, buyerId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAutoSequence(string buyerId)
        {
            return Json(_buyerProgramService.GetAutoSequence(buyerId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BuyerProgram entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            _buyerProgramService.Insert(entity);
            return Json(new { BuyerProgram = entity, Sequence = _buyerProgramService.GetAutoSequence(entity.BuyerId), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BuyerProgram entity)
        {
            _buyerProgramService.Update(entity);
            return Json(new { Sequence = _buyerProgramService.GetAutoSequence(entity.BuyerId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id, string buyerId)
        {
            _buyerProgramService.Delete(id);
            return Json(new { Sequence = _buyerProgramService.GetAutoSequence(buyerId), Message = AplosMessage.Deleted });
        }
    }
}