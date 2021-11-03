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
    public class CompanyCostCenterController : BaseController
    {
        #region Constructor

        private readonly ICompanyCostCenterService _companyCostCenterService;

        public CompanyCostCenterController(ICompanyCostCenterService companyCostCenterService)
        {
            _companyCostCenterService = companyCostCenterService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCboList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyCostCenterService.GetCboByCompany(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanyCostCenter> companyCostCenter, string companyId)
        {
            _companyCostCenterService.InsertOrUpdate(companyCostCenter, companyId);
            return Json(new { companyCostCenter, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _companyCostCenterService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCompany(GridParameter parameters, string companyId)
        {
            return Json(_companyCostCenterService.QueryWithCompany(parameters, companyId), JsonRequestBehavior.AllowGet);
        }
    }
}