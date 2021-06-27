using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Employees;
using Library.Service.Setups;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class SalutationController : BaseController
    {
        #region Constructor

        private readonly ISalutationService _SalutaionService;

        public SalutationController(ISalutationService SalutaionService)
        {
            _SalutaionService = SalutaionService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous, Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(_SalutaionService.GetCbo(companyGroupId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_SalutaionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_SalutaionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Salutation salutation)
        {
           // salutation.CompanyGroupId = null;
            _SalutaionService.Insert(salutation);
            return Json(new { Salutation = salutation, Sequence = _SalutaionService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(Salutation salutation)
        {
            _SalutaionService.Update(salutation);
            return Json(new { Sequence = _SalutaionService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _SalutaionService.Delete(id);
            return Json(new { Sequence = _SalutaionService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}