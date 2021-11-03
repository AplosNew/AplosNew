#region Using
using Library.Core;
using Library.Model.Machines;
using Aplos.Properties;
using Library.Data;
using Library.Service.Machines;

using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    /// <summary>
    /// <remark>Modified:Belayet Hossain;Date:10-Jan-2016;</remark>>
    /// </summary>
    public class MachineSubClassController : Controller
    {
        #region -- Constrator
        private readonly IMachineSubClassService _machineSubClassService;
        public MachineSubClassController(IMachineSubClassService machineSubClassService)
        {
            this._machineSubClassService = machineSubClassService;
        }
        #endregion

        #region -- Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- MachineSubClasss
        [Authorize]
        public JsonResult GetMachineSubClassCbo()
        {
            return Json(new SelectList(_machineSubClassService.GetMachineSubClassList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult GetList(GridParameter parameters)
        //{
        //    return Json(_machineSubClassService.Query(parameters), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult GetMachineSubClass(string id)
        {
            return Json(_machineSubClassService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_machineSubClassService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MachineSubClass machineSubClass)
        {
            if (ModelState.IsValid)
            {
                _machineSubClassService.Insert(machineSubClass);
                return Json(new { MachineSubClass = machineSubClass, Sequence = _machineSubClassService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(MachineSubClass machineSubClass)
        {
            if (ModelState.IsValid)
            {
                _machineSubClassService.Update(machineSubClass);
                return Json(new { Sequence = _machineSubClassService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _machineSubClassService.Archive(id);
                return Json(new { Sequence = _machineSubClassService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}