#region Using

using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using System.Collections.Generic;
using System.Web.Mvc;
#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionSettingsWithProcessUOMController : Controller
    {
        #region Constructor
        private readonly IProcessCapacityUOMService _processCapacityUOMService;
        private readonly IProductionSettingsService _productionSettingsService;
        public ProductionSettingsWithProcessUOMController(
            IProcessCapacityUOMService processCapacityUOMService
           , IProductionSettingsService productionSettingsService
            )
        {
            _processCapacityUOMService = processCapacityUOMService;
            _productionSettingsService = productionSettingsService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Production settings
        [HttpGet, Authorize]
        public ActionResult ProductionSettingsGetList(string plantId)
        {
            return Json(_productionSettingsService.Query(plantId), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region -- ProcessCapacityUOM
        [HttpGet, Authorize]
        public ActionResult ProcessCapacityUOMGetList(string plantId)
        {
            return Json(_processCapacityUOMService.Query(plantId), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Operations
        [HttpPost]
        public JsonResult Create(ProductionSettings productionSettings, IEnumerable<ProcessCapacityUOM> processCapacityUOM)
        {
            _productionSettingsService.InsertGraph(productionSettings, processCapacityUOM);
            return Json(new { ProductionSettings = productionSettings, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ProductionSettings productionSettings, IEnumerable<ProcessCapacityUOM> processCapacityUOM)
        {
            _productionSettingsService.UpdateGraph(productionSettings, processCapacityUOM);
            return Json(new { ProductionSettings = productionSettings, Message = AplosMessage.Updated });
        }
        public ActionResult Delete(string plantId)
        {
            _productionSettingsService.DeleteGraph(plantId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}