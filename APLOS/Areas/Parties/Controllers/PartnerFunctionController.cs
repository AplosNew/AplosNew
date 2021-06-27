using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartnerFunctionController : BaseController
    {
        private readonly IPartnerFunctionService _partnerFunctionService;

        public PartnerFunctionController(IPartnerFunctionService partnerFunctionService)
        {
            _partnerFunctionService = partnerFunctionService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult GetList(GridParameter parameters, string accountType)
        {
            return Json(_partnerFunctionService.Query(parameters, accountType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPartnerFunctionList(GridParameter parameters)
        {
            return Json(_partnerFunctionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPartnerFunctionByAccountType(GridParameter parameters, string accountType)
        {
            return Json(_partnerFunctionService.GetPartnerFunctionByAccountType(parameters, accountType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_partnerFunctionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PartnerFunction partnerFunction)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            partnerFunction.CompanyGroupId = identity.CompanyGroupId;
            _partnerFunctionService.Insert(partnerFunction);
            return Json(new { PartnerFunction = partnerFunction, Sequence = _partnerFunctionService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PartnerFunction partnerFunction)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            partnerFunction.CompanyGroupId = identity.CompanyGroupId;
            _partnerFunctionService.Update(partnerFunction);
            return Json(new { Sequence = _partnerFunctionService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partnerFunctionService.Archive(id);
            return Json(new { Sequence = _partnerFunctionService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}