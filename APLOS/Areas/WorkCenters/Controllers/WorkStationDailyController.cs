using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Employees;
using Library.Service.WorkCenters;
using System.Web.Mvc;

namespace Aplos.Areas.WorkCenters.Controllers
{
    public class WorkStationDailyController : BaseController
    {
        #region Constructor

        private readonly IWorkStationDailyService _workStationDailyService;

        public WorkStationDailyController(IWorkStationDailyService workStationDailyService)
        {
            _workStationDailyService = workStationDailyService;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpGet]
        public JsonResult GetWorkStation(string entityId, string workcenterId)
        {
            return Json(_workStationDailyService.GetWorkStation(entityId, workcenterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters, string entityId, string workCenterId, string entryDate)
        {
            return Json(_workStationDailyService.GetList(parameters, entityId, workCenterId, entryDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMachineList(GridParameter parameters, string operationId, string processId)
        {
            return Json(_workStationDailyService.GetMachineList(parameters, operationId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetOperationList(GridParameter parameters, string entityId, string processId)
        {
            return Json(_workStationDailyService.GetOperationList(parameters, entityId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(WorkStationDaily workStationDaily)
        {
            _workStationDailyService.Insert(workStationDaily);
            return Json(new { WorkStationDaily = workStationDaily, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(WorkStationDaily workStationDaily)
        {
            _workStationDailyService.Update(workStationDaily);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _workStationDailyService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}