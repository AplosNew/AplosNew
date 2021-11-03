using Library.Model.External;
using Library.Service.External;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class AplosEmpFieldTagController : BaseController
    {
        private readonly IAplosEmpFieldTagService _aplosEmpFieldTagService;
        public AplosEmpFieldTagController(
              IAplosEmpFieldTagService aplosEmpFieldTagService
            )
        {
            _aplosEmpFieldTagService = aplosEmpFieldTagService;
        }
        public ActionResult Aplos()
        {
            ViewBag.ControllerName = "AplosEmpFieldTagController";
            return View();
        }
        [HttpGet]
        public JsonResult GetCompanyGroupCbo()
        {
            return Json(_aplosEmpFieldTagService.GetCompanyGroupCbo().Rows, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetList(GridParameter parameters, int companyGroupId)
        {
            return Json(_aplosEmpFieldTagService.Query(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(IEnumerable<AplosEmpFieldTag> model)
        {
            _aplosEmpFieldTagService.Insert(model);
            return Json(new { AplosEmpField = model, Message = "Data saved successfully" });
        }
    }
}