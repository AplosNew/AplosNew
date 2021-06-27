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
    public class AlternativeGLController : BaseController
    {
        private readonly IAlternativeGLService _alternativeGLService;

        public AlternativeGLController(IAlternativeGLService alternativeGLService)
        {
            _alternativeGLService = alternativeGLService;
        }

        public ActionResult AlternativeGL()
        {
            return View("~/Areas/Accounts/Views/AlternativeGL.cshtml");
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters, string alternativeCoaId)
        {
            return Json(_alternativeGLService.GetSearchData(parameters, alternativeCoaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_alternativeGLService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string acoaid)
        {
            return Json(_alternativeGLService.GetAutoSequence(acoaid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAlternativeGL()
        {
            return Json(_alternativeGLService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAlternativeGLId(string id)
        {
            return Json(_alternativeGLService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetVendorAlternativeGLData(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_alternativeGLService.GetVendorAlternativeGLData(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(AlternativeGL alternativeGL)
        {
            _alternativeGLService.Insert(alternativeGL);
            return Json(new { AlternativeGL = alternativeGL, Sequence = _alternativeGLService.GetAutoSequence(alternativeGL.AlternativeCOAId), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(AlternativeGL alternativeGL)
        {
            _alternativeGLService.Update(alternativeGL);
            return Json(new { Sequence = _alternativeGLService.GetAutoSequence(alternativeGL.AlternativeCOAId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id, string acoaId)
        {
            _alternativeGLService.Archive(id);
            return Json(new { Sequence = _alternativeGLService.GetAutoSequence(acoaId), Message = AplosMessage.Deleted });
        }
    }
}