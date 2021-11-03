#region Using
using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Service.Materials;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialGridController : BaseController
    {
        #region Constructor
        /// <summary>   The IMaterialGridService service. </summary>
        private readonly IMaterialGridService _materialGridService;
        private readonly IMaterialGridCharacteristicsService _materialGridCharacteristicsService;

        public MaterialGridController(
            IMaterialGridService materialGridService,
            IMaterialGridCharacteristicsService materialGridCharacteristicsService
            )
        {
            this._materialGridService = materialGridService;
            this._materialGridCharacteristicsService = materialGridCharacteristicsService;
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
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_materialGridService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet,Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_materialGridService.GetMaterialGridList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_materialGridService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetMaterialGridListWithoutExisting(GridParameter parameters, string companyGroupId, string ids)
        {
            return Json(_materialGridService.GetMaterialGridListWithoutExisting(parameters, companyGroupId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(MaterialGrid materialgrid,
            IEnumerable<MaterialGridCharacteristics> materialGridCharacteristics, string[] deletedItems)
        {
            _materialGridService.Insert(materialgrid, materialGridCharacteristics, deletedItems);
            return Json(new { MaterialGrid = materialgrid, Sequence = _materialGridService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(MaterialGrid materialgrid)
        {
            _materialGridService.Update(materialgrid);
            return Json(new { Sequence = _materialGridService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _materialGridService.DeleteGraph(id);
            return Json(new { Sequence = _materialGridService.GetAutoSequence(),Message = AplosMessage.Deleted });
        }
        #endregion

        #region MaterialGridCharacteristics

        [HttpGet, Authorize]
        public ActionResult GetMaterialgridCharacteristics(string materialGridId)
        {
            return Json(_materialGridCharacteristicsService.Query(materialGridId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetByMatrialGridList(string materialGridId)
        {
            return Json(_materialGridCharacteristicsService.GetByMatrialGridList(materialGridId), JsonRequestBehavior.AllowGet);
        }


        #endregion
    }
}