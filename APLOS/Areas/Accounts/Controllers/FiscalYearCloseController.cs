using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Calendars;
using Library.Service.Calendars;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class FiscalYearCloseController : BaseController
    {
        private readonly IFiscalYearService _fiscalYearService;
        

        public FiscalYearCloseController(
            IFiscalYearService fiscalYearService
            )
        {
            _fiscalYearService = fiscalYearService;
            
        }

        [HttpGet]
        public ActionResult FiscalYearClose()
        {
            return View("~/Areas/Accounts/Views/FiscalYearClose.cshtml");
        }


        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_fiscalYearService.GetCboFiscalYearList(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fiscalYearService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFiscalYearClose(string id)
        {
            return Json(_fiscalYearService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FiscalYear fiscalYear)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            fiscalYear.CompanyGroupId = identity.CompanyGroupId;
            _fiscalYearService.Insert(fiscalYear);
            return Json(new { FiscalYear = fiscalYear, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FiscalYear fiscalYear)
        {
            _fiscalYearService.Update(fiscalYear);
            return Json(new { Message = AplosMessage.Success });
        }

        //[HttpPost]
        //public ActionResult Delete(string id)
        //{
        //    if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
        //    if (_fiscalYearService.UsingCheck(id))
        //        throw new CustomException("This Fiscal Year is already in used...");
        //    _fiscalYearPeriodService.DeleteFiscalYearById(id);
        //    _fiscalYearService.Delete(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}

        
    }
}