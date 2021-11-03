#region using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Materials;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialTypeController : BaseController
    {
        #region Constructor
        private readonly IMaterialTypeService _materialTypeService;
        public MaterialTypeController(IMaterialTypeService materialTypeService)
        {
            _materialTypeService = materialTypeService;
        }
        #endregion

        #region -- Pages
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize, HttpGet]
        public JsonResult GetCboByMaterialMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_materialTypeService.GetCboByMaterialMaster(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_materialTypeService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboFilterBySFG()
        {
            return Json(new SelectList(_materialTypeService.GetCboFilterBySFG(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetMaterialTypeNatureListCbo()
        {
            return Json(_materialTypeService.GetMaterialTypeNatureListCbo(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_materialTypeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_materialTypeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialType(string id)
        {
            return Json(_materialTypeService.Find(id), JsonRequestBehavior.AllowGet);
        }
        //[HttpGet, Authorize]
        //public JsonResult GetMaterialTypeNatureList(string masterId)
        //{
        //    return Json(_materialTypeService.GetMaterialTypeNatureList(masterId), JsonRequestBehavior.AllowGet);
        //}
        [HttpPost]
        public JsonResult Create(MaterialType materialType)
        {
            _materialTypeService.Insert(materialType);
            return Json(new { MaterialType = materialType, Sequence = _materialTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(MaterialType materialType)
        {
            _materialTypeService.Update(materialType);
            return Json(new { Sequence = _materialTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _materialTypeService.Delete(id);
            return Json(new { Sequence = _materialTypeService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}