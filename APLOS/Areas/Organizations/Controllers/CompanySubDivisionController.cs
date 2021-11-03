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
    public class CompanySubDivisionController : BaseController
    {
        #region Constructor

        private readonly ICompanySubDivisionService _companySubDivisionService;

        public CompanySubDivisionController(ICompanySubDivisionService companySubDivisionService)
        {
            _companySubDivisionService = companySubDivisionService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCboList(string companyId)
        {
            return Json(new SelectList(_companySubDivisionService.GetCboList(companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyId)
        {
            return Json(new SelectList(_companySubDivisionService.GetCboList(companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanySubDivision> companySubDivision)
        {
            _companySubDivisionService.InsertRange(companySubDivision);
            return Json(new { CompanySubDivision = companySubDivision, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _companySubDivisionService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCompany(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companySubDivisionService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }
    }
}