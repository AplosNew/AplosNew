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
    public class MaterialGroup2Controller : BaseController
    {
        #region -- Constructor
        private readonly IMaterialGroup2Service _materialGroup2Service;

        public MaterialGroup2Controller(IMaterialGroup2Service materialGroup2Service)
        {
            this._materialGroup2Service = materialGroup2Service;
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
            return Json(_materialGroup2Service.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_materialGroup2Service.GetCboList(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_materialGroup2Service.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMaterialGroup2()
        {
            return Json(_materialGroup2Service.Query().Select(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMaterialGroup2ById(string id)
        {
            return Json(_materialGroup2Service.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MaterialGroup2 materialGroup2)
        {
            _materialGroup2Service.Insert(materialGroup2);
            return Json(new { MaterialGroup2 = materialGroup2, Sequence = _materialGroup2Service.GetAutoSequence(), Message = AplosMessage.Insert });
        }
        
        [HttpPost]
        public JsonResult Edit(MaterialGroup2 materialGroup2)
        {
            _materialGroup2Service.Update(materialGroup2);
            return Json(new { Sequence = _materialGroup2Service.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _materialGroup2Service.Archive(id);
            return Json(new { Sequence = _materialGroup2Service.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}