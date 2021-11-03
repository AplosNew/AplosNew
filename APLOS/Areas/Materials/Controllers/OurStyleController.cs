#region using
using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Service.Materials;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class OurStyleController : BaseController
    {
        #region -- Constructor
        private readonly IOurStyleService _materialGroup1Service;

        public OurStyleController(IOurStyleService materialGroup1Service)
        {
            this._materialGroup1Service = materialGroup1Service;
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
            return Json(_materialGroup1Service.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_materialGroup1Service.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_materialGroup1Service.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetOurStyle()
        {
            return Json(_materialGroup1Service.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetOurStyleById(string id)
        {
            return Json(_materialGroup1Service.Find(id), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(OurStyle ourStyle)
        {
            _materialGroup1Service.Insert(ourStyle);
            return Json(new { OurStyle = ourStyle, Sequence = _materialGroup1Service.GetAutoSequence(), Message = AplosMessage.Insert });
        }


        [HttpPost]
        public JsonResult Edit(OurStyle ourStyle)
        {
            _materialGroup1Service.Update(ourStyle);
            return Json(new { Sequence = _materialGroup1Service.GetAutoSequence(), Message = AplosMessage.Updated });
        }


        [HttpPost]
        public JsonResult Delete(string id)
        {
            _materialGroup1Service.Delete(id);
            return Json(new { Sequence = _materialGroup1Service.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}