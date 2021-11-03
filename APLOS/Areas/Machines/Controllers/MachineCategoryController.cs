#region Using
using Library.Core;
using Library.Model.Machines;
using Aplos.Properties;
using Library.Data;
using Library.Service.Machines;

using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class MachineCategoryController : Controller
    {
        #region -- Constrator
        private readonly IMachineCategoryService _machineCategoryService;
        public MachineCategoryController(IMachineCategoryService machineCategoryService)
        {
            this._machineCategoryService = machineCategoryService;
        }
        #endregion

        #region -- Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- MachineCategorys
        [Authorize]
        public JsonResult GetMachineCategoryCbo()
        {
            return Json(new SelectList(_machineCategoryService.GetmachineCategoryList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_machineCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMachineCategory(string id)
        {
            return Json(_machineCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_machineCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MachineCategory machineCategory)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            machineCategory.CompanyGroupId = identity.CompanyGroupId;
            if (ModelState.IsValid)
            {
                _machineCategoryService.Insert(machineCategory);
                return Json(new { MachineCategory = machineCategory, Sequence = _machineCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(MachineCategory machineCategory)
        {
            if (ModelState.IsValid)
            {
                _machineCategoryService.Update(machineCategory);
                return Json(new { Sequence = _machineCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _machineCategoryService.Archive(id);
                return Json(new { Sequence = _machineCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}