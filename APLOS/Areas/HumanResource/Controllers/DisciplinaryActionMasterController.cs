#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Employees;
using Library.Service.HumanResources;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class DisciplinaryActionMasterController : BaseController
    {
        #region Constructor

        private readonly IDisciplinaryActionMasterService _disciplinaryActionMasterService;
        private readonly IDisciplinaryActionCriticalityService _disciplinaryActionCriticalityService;
        private readonly IDisciplinaryActionService _disciplinaryActionService;

        public DisciplinaryActionMasterController(
              IDisciplinaryActionMasterService disciplinaryActionMasterService
            , IDisciplinaryActionCriticalityService disciplinaryActionCriticalityService
            , IDisciplinaryActionService disciplinaryActionService
            )
        {
            _disciplinaryActionMasterService = disciplinaryActionMasterService;
            _disciplinaryActionCriticalityService = disciplinaryActionCriticalityService;
            _disciplinaryActionService = disciplinaryActionService;
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
        public ActionResult GetList(GridParameter parameters, string EmpSystemId)
        {
            return Json(_disciplinaryActionMasterService.Query(parameters, EmpSystemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeDataList(GridParameter parameters, string plantId, string empId)
        {
            return Json(_disciplinaryActionMasterService.GetEmployeeData(parameters, plantId, empId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(DisciplinaryActionMaster model)
        {
            _disciplinaryActionMasterService.Insert(model);
            return Json(new { DisciplinaryActionMaster = model, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(DisciplinaryActionMaster model)
        {
            _disciplinaryActionMasterService.Update(model);
            return Json(new { DisciplinaryActionMaster = model, Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            //if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _disciplinaryActionMasterService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}