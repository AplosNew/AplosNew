using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class AlternativeCOAController : BaseController
    {
        private readonly IAlternativeCOAService _alternativeCOAService;

        public AlternativeCOAController(IAlternativeCOAService alternativeCOAService)
        {
            _alternativeCOAService = alternativeCOAService;
        }

        [HttpGet, Authorize]
        public JsonResult GetAlternativeCOAListCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_alternativeCOAService.GetCboAlternativeCOAList(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetLengthOfGLCbo(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_alternativeCOAService.GetCboLengthOfGL(identity.CompanyGroupId, id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AlternativeCOA()
        {
            return View("~/Areas/Accounts/Views/AlternativeCOA.cshtml");
        }

        [HttpGet]
        public ActionResult GetAlternativeCOAList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_alternativeCOAService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAGLAlternativeCOA(string acoaId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_alternativeCOAService.GetAGLAlternativeCOA(identity.CompanyGroupId, acoaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAlternativeCOAById(string id)
        {
            return Json(_alternativeCOAService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(AlternativeCOA entity)
        {
            _alternativeCOAService.Insert(entity);
            return Json(new { AlternativeCOA = entity, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(AlternativeCOA coa)
        {
            _alternativeCOAService.Update(coa);
            return Json(new { Message = AplosMessage.Success });
        }

        public ActionResult Delete(string id)
        {
            _alternativeCOAService.Archive(id);
            return Json(new { Message = AplosMessage.Success });
        }
    }
}