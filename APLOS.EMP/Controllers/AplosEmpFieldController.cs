#region Using
using Library.Model.External;
using Library.Service.External;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Controllers
{
    public class AplosEmpFieldController : BaseController
    {
        #region Constructor
        private readonly IAplosEmpFieldService _aplosEmpFieldService;
        public AplosEmpFieldController(
              IAplosEmpFieldService aplosEmpFieldService
            )
        {
            _aplosEmpFieldService = aplosEmpFieldService;
        }
        #endregion

        #region -- Pages
        public ActionResult Aplos()
        {
            ViewBag.ControllerName = "AplosEmpFieldController";
            return View();
        }
        #endregion
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_aplosEmpFieldService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(AplosEmpField model)
        {
            _aplosEmpFieldService.Insert(model);
            return Json(new { AplosEmpField = model, Message = "Success" });
        }
        [HttpPost]
        public JsonResult Edit(AplosEmpField model)
        {
            _aplosEmpFieldService.Update(model);
            return Json(new {  Message = "Success" });
        }
        public ActionResult Delete(int id)
        {
            _aplosEmpFieldService.Delete(id);
            return Json(new { Message = "Success" });
        }
        #region -- Operations
        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_aplosEmpFieldService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}