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
    public class BuyerDepartmentController : BaseController
    {
        private readonly IBuyerDepartmentService _buyerDepartmentService;

        public BuyerDepartmentController(IBuyerDepartmentService buyerDepartmentService)
        {
            _buyerDepartmentService = buyerDepartmentService;
        }

       
        public ActionResult Aplos()
        {
            return View("~/Areas/Parties/Views/BuyerDepartment.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string buyerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerDepartmentService.GetCbo(buyerId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence(string buyerId)
        {
            return Json(_buyerDepartmentService.GetAutoSequence(buyerId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBuyerDepartmentList(GridParameter parameters, string buyerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerDepartmentService.Query(parameters, buyerId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBuyerDepartmentById(string id)
        {
            return Json(_buyerDepartmentService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BuyerDepartment buyerDepartment)
        {
            _buyerDepartmentService.Insert(buyerDepartment);
            return Json(new { BuyerDepartment = buyerDepartment, Sequence = _buyerDepartmentService.GetAutoSequence(buyerDepartment.BuyerId), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BuyerDepartment buyerDepartment)
        {
            _buyerDepartmentService.Update(buyerDepartment);
            return Json(new { Sequence = _buyerDepartmentService.GetAutoSequence(buyerDepartment.BuyerId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id, string buyerId)
        {
            _buyerDepartmentService.DeleteBuyerDepartment(id);
            return Json(new { Sequence = _buyerDepartmentService.GetAutoSequence(buyerId), Message = AplosMessage.Deleted });
        }
    }
}