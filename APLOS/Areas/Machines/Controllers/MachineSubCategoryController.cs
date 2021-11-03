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
    /// <summary>
    /// <remarks>Modified:Belayet Hossain Date:29-12-2015.</remarks>
    /// </summary>
    public class MachineSubCategoryController : Controller
    {
        #region -- Constrator
        private readonly IMachineSubCategoryService _machineSubCategoryService;
        public MachineSubCategoryController(IMachineSubCategoryService machineSubCategoryService)
        {
            this._machineSubCategoryService = machineSubCategoryService;
        }
        #endregion

        #region -- Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- MachineSubCategorys
        [Authorize]
        public JsonResult GetMachineSubCategoryCbo()
        {
            return Json(new SelectList(_machineSubCategoryService.GetMachineSubCategoryList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_machineSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMachineSubCategory(string id)
        {
            return Json(_machineSubCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_machineSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MachineSubCategory machineSubCategory)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            machineSubCategory.CompanyGroupId = identity.CompanyGroupId;
            if (ModelState.IsValid)
            {
                _machineSubCategoryService.Insert(machineSubCategory);
                return Json(new { MachineSubCategory = machineSubCategory, Sequence = _machineSubCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(MachineSubCategory machineSubCategory)
        {
            if (ModelState.IsValid)
            {
                _machineSubCategoryService.Update(machineSubCategory);
                return Json(new { Sequence = _machineSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _machineSubCategoryService.Archive(id);
                return Json(new { Sequence = _machineSubCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}