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
    public class MaterialGroup1Controller : BaseController
    {
        #region -- Constructor
        private readonly IMaterialGroup1Service _materialGroup1Service;

        public MaterialGroup1Controller(IMaterialGroup1Service materialGroup1Service)
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
            return Json(_materialGroup1Service.GetCboList(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_materialGroup1Service.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMaterialGroup1()
        {
            return Json(_materialGroup1Service.Query().Select(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMaterialGroup1ById(string id)
        {
            return Json(_materialGroup1Service.Find(id), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(MaterialGroup1 materialGroup1)
        {
            if (ModelState.IsValid)
            {
                _materialGroup1Service.Insert(materialGroup1);
                return Json(new { MaterialGroup1 = materialGroup1, Sequence = _materialGroup1Service.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }


        [HttpPost]
        public JsonResult Edit(MaterialGroup1 materialGroup1)
        {
            if (ModelState.IsValid)
            {
                _materialGroup1Service.Update(materialGroup1);
                return Json(new { Sequence = _materialGroup1Service.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }


        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _materialGroup1Service.Archive(id);
                return Json(new { Sequence = _materialGroup1Service.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}