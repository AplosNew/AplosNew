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
    public class MaterialGroup4Controller : BaseController
    {
        #region -- Constructor
        private readonly IMaterialGroup4Service _materialGroup4Service;

        public MaterialGroup4Controller(IMaterialGroup4Service materialGroup4Service)
        {
            this._materialGroup4Service = materialGroup4Service;
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
            return Json(_materialGroup4Service.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_materialGroup4Service.GetCboList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_materialGroup4Service.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialGroup4()
        {
            return Json(_materialGroup4Service.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialGroup4ById(string id)
        {
            return Json(_materialGroup4Service.Find(id), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult Create(MaterialGroup4 materialGroup4)
        {
            if (ModelState.IsValid)
            {
                _materialGroup4Service.Insert(materialGroup4);
                return Json(new { MaterialGroup4 = materialGroup4, Sequence = _materialGroup4Service.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }
        
        [HttpPost]
        public JsonResult Edit(MaterialGroup4 materialGroup4)
        {
            if (ModelState.IsValid)
            {
                _materialGroup4Service.Update(materialGroup4);
                return Json(new { Sequence = _materialGroup4Service.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _materialGroup4Service.Archive(id);
                return Json(new { Sequence = _materialGroup4Service.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}