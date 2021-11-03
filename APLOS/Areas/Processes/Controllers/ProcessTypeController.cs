#region Using
using Aplos.Controllers;
using Library.Core;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;

using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
    public class ProcessTypeController : BaseController
    {
        #region Constructor
        /// <summary>   The processTypeService service. </summary>
        private readonly IProcessTypeService _processTypeService;

        public ProcessTypeController(IProcessTypeService processTypeService)
        {
            this._processTypeService = processTypeService;
        }
        #endregion

        #region Aplos
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion     

        #region GetSearchGridData
        public ActionResult GetProcessTypeList(GridParameter parameters, string processId)
        {
            return Json(_processTypeService.GetProcessTypeList(parameters, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbobyProcess(string processId)
        {
            return Json(_processTypeService.GetCbobyProcess(processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProcessTypeCbo()
        {
            return Json(_processTypeService.GetProcessType(), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region -- Operations
        public JsonResult GetProcessType()
        {
            return Json(_processTypeService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_processTypeService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProcessType processType)
        {
            _processTypeService.Insert(processType);
            return Json(new { ProcessType = processType, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ProcessType processType)
        {
            _processTypeService.Update(processType);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _processTypeService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion
    }
}