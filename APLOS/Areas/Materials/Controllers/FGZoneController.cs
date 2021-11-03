#region using
using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Data;
using Library.Service.Materials;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class FGZoneController : BaseController
    {
        #region -- Constructor
        private readonly IFGZoneService _fgzoneService;

        public FGZoneController(IFGZoneService fgzoneService)
        {
            this._fgzoneService = fgzoneService;
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
            return Json(_fgzoneService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_fgzoneService.GetFGZoneCbo(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_fgzoneService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFGZone()
        {
            return Json(_fgzoneService.Query().Select(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFGZoneById(string id)
        {
            return Json(_fgzoneService.Find(id), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult Create(FGZone fgzone)
        {
            _fgzoneService.Insert(fgzone);
            return Json(new { FGZone = fgzone, Sequence = _fgzoneService.GetAutoSequence(), Message = AplosMessage.Insert });
        }


        [HttpPost]
        public JsonResult Edit(FGZone fgzone)
        {
            _fgzoneService.Update(fgzone);
            return Json(new { Sequence = _fgzoneService.GetAutoSequence(), Message = AplosMessage.Updated });
        }


        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _fgzoneService.Archive(id);
                return Json(new { Sequence = _fgzoneService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}