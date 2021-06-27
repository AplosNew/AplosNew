#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class CompanyDivisionController : BaseController
    {
        #region Constructor

        private readonly ICompanyDivisionService _companyDivisionService;

        public CompanyDivisionController(ICompanyDivisionService companyDivisionService)
        {
            _companyDivisionService = companyDivisionService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCboList(string companyId)
        {
            return Json(new SelectList(_companyDivisionService.GetCboList(companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyId)
        {
            return Json(new SelectList(_companyDivisionService.GetCboList(companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanyDivision> companyDivision)
        {
            _companyDivisionService.InsertRange(companyDivision);
            return Json(new { CompanyDivision = companyDivision, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _companyDivisionService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCompany(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyDivisionService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }
    }
}