#region using

using Aplos.Properties;
using Aplos.Controllers;
using Library.Model.Materials;
using Library.Service.Materials;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialAttributeMasterController : BaseController
    {
        #region -- Constructor

        private readonly IMaterialAttributeMasterService _materialAttributeMasterService;

        public MaterialAttributeMasterController(
            IMaterialAttributeMasterService materialAttributeMasterService)
        {
            this._materialAttributeMasterService = materialAttributeMasterService;
        }

        #endregion -- Constructor

        #region --pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion --pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(string materialGroupMasterId)
        {
            return Json(_materialAttributeMasterService.Query(materialGroupMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetListForMaterialMaster(string materialGroupMasterId)
        {
            return Json(_materialAttributeMasterService.QueryForMaterialMaster(materialGroupMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialAttributeMasterId(string id)
        {
            return Json(_materialAttributeMasterService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<MaterialAttributeMaster> materialAttributeMasters)
        {
            _materialAttributeMasterService.Save(materialAttributeMasters);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(MaterialAttributeMaster materialAttributeMaster)
        {
            _materialAttributeMasterService.Update(materialAttributeMaster);
            return Json(new { MaterialAttributeMaster = materialAttributeMaster, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _materialAttributeMasterService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}