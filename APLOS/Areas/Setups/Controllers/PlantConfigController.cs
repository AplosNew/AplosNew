using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
    public class PlantConfigController : BaseController
    {
        #region Constructor

        private readonly IPlantConfigService _plantconfigservice;

        public PlantConfigController(IPlantConfigService plantconfigservice)
        {
            _plantconfigservice = plantconfigservice;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetUOMDimensionCbo()
        {
            return Json(new SelectList(_plantconfigservice.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMasterSearchData(GridParameter parameters)
        {
            return Json(_plantconfigservice.GetMasterSearchData(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_plantconfigservice.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPlantConfigDataByPlantId(string plantid)
        {
            return Json(_plantconfigservice.GetPlantConfigByPlant(plantid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPlantList(string CompanyId)
        {
            return Json(_plantconfigservice.GetPlantList(CompanyId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPlantConfigDataById(string Id)
        {
            var masterWithData = _plantconfigservice.GetMasterDataById(Id);
            return Json(new { masterData = masterWithData, Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPlantWiseDuplicateData(string Id, string CompanyGroupId, string CompanyId, string PlantId)
        {
            var dData = _plantconfigservice.GetPlantWiseDuplicateData(Id, CompanyGroupId, CompanyId, PlantId);
            return Json(new { dData, Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProcessList()
        {
            return Json(_plantconfigservice.GetProcessList().Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveMaster(PlantConfig pmaster, IEnumerable<PrdOrdSetting> prdOrdSetting)
        {
            var MasterID = "";
            _plantconfigservice.SaveMaster(pmaster, out MasterID, prdOrdSetting);
            var mData = _plantconfigservice.GetMasterDataById(MasterID);
            return Json(new { MasterData = mData, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Create(PlantConfig PlantConfig)
        {
            _plantconfigservice.InsertGraph(PlantConfig);
            return Json(new { PlantConfig.Id, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PlantConfig PlantConfig)
        {
            _plantconfigservice.UpdateGraph(PlantConfig);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _plantconfigservice.DeleteMaster(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}