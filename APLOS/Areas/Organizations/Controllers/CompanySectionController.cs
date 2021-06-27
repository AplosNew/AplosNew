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
    public class CompanySectionController : BaseController
    {
        #region Constructor

        private readonly ICompanySectionService _companySectionService;

        public CompanySectionController(ICompanySectionService companySectionService)
        {
            _companySectionService = companySectionService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCboList(string companyId)
        {
            return Json(new SelectList(_companySectionService.GetCboList(companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyId)
        {
            return Json(new SelectList(_companySectionService.GetCboList(companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanySection> companySection)
        {
            _companySectionService.InsertRange(companySection);
            return Json(new { CompanySection = companySection, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _companySectionService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet]
        public ActionResult GetListWithCompany(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companySectionService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }
    }
}