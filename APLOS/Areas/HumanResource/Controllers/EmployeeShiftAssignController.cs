using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeeShiftAssignController : BaseController
    {
        #region Constructor

        private readonly IEmployeeShiftAssignService _employeeShiftAssignService;

        public EmployeeShiftAssignController(
              IEmployeeShiftAssignService employeeShiftAssignService
            )
        {
            _employeeShiftAssignService = employeeShiftAssignService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string plantId, string date)
        {
            return Json(_employeeShiftAssignService.Query(parameters, plantId, date), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetRoasterCboByPlant(string plantId)
        {
            return Json(_employeeShiftAssignService.GetRoasterCboByPlant(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetRosterWiseShiftName(string plantId, string roasterId)
        {
            return Json(_employeeShiftAssignService.GetRosterWiseShiftName(plantId, roasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(EmployeeShiftAssign model, EmployeeWeekOffByDay employeeWeekOffByDay)
        {
            _employeeShiftAssignService.Insert(model, employeeWeekOffByDay);
            return Json(new { EmployeeShiftAssign = model, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(EmployeeShiftAssign model)
        {
            _employeeShiftAssignService.Update(model);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _employeeShiftAssignService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}