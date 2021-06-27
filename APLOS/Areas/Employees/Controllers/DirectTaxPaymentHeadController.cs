#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class DirectTaxPaymentHeadController : BaseController
    {
        #region Constructor
        private readonly IDirectTaxPaymentHeadService _directTaxPaymentHeadService;
        public DirectTaxPaymentHeadController(
              IDirectTaxPaymentHeadService directTaxPaymentHeadService
            )
        {
            _directTaxPaymentHeadService = directTaxPaymentHeadService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_directTaxPaymentHeadService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_directTaxPaymentHeadService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_directTaxPaymentHeadService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(DirectTaxPaymentHead directTaxPaymentHead)
        {
            _directTaxPaymentHeadService.Insert(directTaxPaymentHead);
            return Json(new { DirectTaxPaymentHead = directTaxPaymentHead, Sequence = _directTaxPaymentHeadService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(DirectTaxPaymentHead directTaxPaymentHead)
        {
            _directTaxPaymentHeadService.Update(directTaxPaymentHead);
            return Json(new { Sequence = _directTaxPaymentHeadService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _directTaxPaymentHeadService.Delete(id);
            return Json(new { Sequence = _directTaxPaymentHeadService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}