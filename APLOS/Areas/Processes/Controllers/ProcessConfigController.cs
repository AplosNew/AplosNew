#region Using
using Aplos.Controllers;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;
using System.Collections.Generic;
using System.Web.Mvc;
#endregion

namespace Aplos.Areas.Processes.Controllers
{
    public class ProcessConfigController : BaseController
    {
        #region --Constructor
        private readonly IProcessConfigService _processConfigService;

        public ProcessConfigController(IProcessConfigService processConfigService)
        {
            this._processConfigService = processConfigService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region DDl
        [Authorize]
        public JsonResult GetProcessConfigBomOrRecipeCbo()
        {
            return Json(_processConfigService.GetProcessConfigBomOrRecipeCbo(), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetProcessConfigLevelCbo()
        {
            return Json(_processConfigService.GetProcessConfigLevelCbo(), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetProcessConfigMaterialTaggingTypeCbo()
        {
            return Json(_processConfigService.GetProcessConfigMaterialTaggingTypeCbo(), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region -- Operations

        [Authorize]
        [HttpGet]
        public JsonResult GetList(string materialMasterId)
        {
            return Json(_processConfigService.Query(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetCharacteristicsName(string materialMasterId)
        {
            return Json(_processConfigService.GetCharacteristicsName(materialMasterId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(IEnumerable<ProcessConfig> processConfig)
        {
            _processConfigService.Insert(processConfig);
            return Json(new {Message = AplosMessage.Insert });
        }

        public ActionResult Delete(IEnumerable<ProcessConfig> processConfig)
        {
            _processConfigService.Archive(processConfig);
            return Json(new {Message = AplosMessage.Deleted });
        }
        #endregion
    }
}