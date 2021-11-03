using Library.Service.Productions;
using System.Web.Mvc;
using Aplos.Controllers;
using System;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Productions.Controllers
{
    public class MainProcessPlanningController : BaseController
    {

        #region Constructor
        /// <summary>   The OperationProductionOrderController service. </summary>
        private readonly IMainProcessPlanningService _mainProcessPlaningservice;

        public MainProcessPlanningController(
            IMainProcessPlanningService mainProcessPlaningservice,
            CustomerPOService customerposervice)
        {
            this._mainProcessPlaningservice = mainProcessPlaningservice;

        }
        #endregion

        #region -- Pages
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        [Authorize]
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public ActionResult GetList(string plantId, DateTime toDate, string companyId, string processId)
        {
            return Json(_mainProcessPlaningservice.GetList(plantId, toDate, companyId, processId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult Process(string plantId, DateTime toDate, string companyId, string processId)
        {
            return Json(_mainProcessPlaningservice.Process(plantId, toDate, companyId, processId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveFreezing(string ids)
        {
            _mainProcessPlaningservice.SaveFreezing(new JavaScriptSerializer().Deserialize<string[]>(ids));
            return Json(new { Message = "Data freezing successfully." });
        }
        #endregion
    }
}