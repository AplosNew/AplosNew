#region Using
using Aplos.Controllers;
using Library.Model.Machines;
using Aplos.Properties;
using Library.Service.Machines;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class MachineClassController : BaseController
    {
        #region -- Constrator
        private readonly IMachineClassService _machineClassService;
        private readonly ICompanyGroupMachineClassService _companyGroupMachineClassService;
        public MachineClassController(IMachineClassService machineClassService, ICompanyGroupMachineClassService companyGroupMachineClassService)
        {
            _machineClassService = machineClassService;
            _companyGroupMachineClassService = companyGroupMachineClassService;
        }
        #endregion

        #region -- Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- MachineClasss
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupMachineClassService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupMachineClassService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMachineClass(string id)
        {
            return Json(_machineClassService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_machineClassService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MachineClass machineClass)
        {
            _machineClassService.Insert(machineClass);
            return Json(new { MachineClass = machineClass, Sequence = _machineClassService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(MachineClass machineClass)
        {
            _machineClassService.Update(machineClass);
            return Json(new { Sequence = _machineClassService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _machineClassService.DeleteGraph(id);
            return Json(new { Sequence = _machineClassService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}