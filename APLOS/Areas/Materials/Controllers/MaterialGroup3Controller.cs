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
    public class MaterialGroup3Controller : BaseController
    {
        #region -- Constructor
        private readonly IMaterialGroup3Service _materialGroup3Service;

        public MaterialGroup3Controller(IMaterialGroup3Service materialGroup3Service)
        {
            this._materialGroup3Service = materialGroup3Service;
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
            return Json(_materialGroup3Service.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_materialGroup3Service.GetCboList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_materialGroup3Service.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialGroup3()
        {
            return Json(_materialGroup3Service.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialGroup3ById(string id)
        {
            return Json(_materialGroup3Service.Find(id), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult Create(MaterialGroup3 materialGroup3)
        {
            _materialGroup3Service.Insert(materialGroup3);
            return Json(new { MaterialGroup3 = materialGroup3, Sequence = _materialGroup3Service.GetAutoSequence(), Message = AplosMessage.Insert });
        }
        
        [HttpPost]
        public JsonResult Edit(MaterialGroup3 materialGroup3)
        {
            _materialGroup3Service.Update(materialGroup3);
            return Json(new { Sequence = _materialGroup3Service.GetAutoSequence(), Message = AplosMessage.Updated });
        }
        
        [HttpPost]
        public JsonResult Delete(string id)
        {
            _materialGroup3Service.Archive(id);
            return Json(new { Sequence = _materialGroup3Service.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}