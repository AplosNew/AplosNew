#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class AnnualNonCashController : BaseController
    {
        #region Constructor

        private readonly IAnnualNonCashService _annualNonCashService;

        public AnnualNonCashController(
              IAnnualNonCashService annualNonCashService
            )
        {
            _annualNonCashService = annualNonCashService;
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

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_annualNonCashService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_annualNonCashService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_annualNonCashService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(AnnualNonCash model)
        {
            _annualNonCashService.Insert(model);
            return Json(new { AnnualNonCash = model, Sequence = _annualNonCashService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(AnnualNonCash model)
        {
            _annualNonCashService.Update(model);
            return Json(new { Sequence = _annualNonCashService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _annualNonCashService.Delete(id);
            return Json(new { Sequence = _annualNonCashService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}