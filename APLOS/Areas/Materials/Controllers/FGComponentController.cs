#region using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Materials;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class FGComponentController : BaseController
    {
        #region -- Constructor
        private readonly IFGComponentService _fgComponentService;

        public FGComponentController(IFGComponentService fgComponentService)
        {
            _fgComponentService = fgComponentService;
        }
        #endregion

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_fgComponentService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Search for multiple add
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public ActionResult GetFgComponentList(GridParameter parameters, string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fgComponentService.GetFgComponentList(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(id)), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fgComponentService.GetFGComponentCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_fgComponentService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFGComponent()
        {
            return Json(_fgComponentService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFGComponentById(string id)
        {
            return Json(_fgComponentService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FGComponent fgcomponent)
        {
            _fgComponentService.Insert(fgcomponent);
            return Json(new { FGComponent = fgcomponent, Sequence = _fgComponentService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FGComponent fgzone)
        {
            _fgComponentService.Update(fgzone);
            return Json(new { Sequence = _fgComponentService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _fgComponentService.Archive(id);
            return Json(new { Sequence = _fgComponentService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}