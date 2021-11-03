#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class CompanyServiceMasterController : BaseController
    {
        #region Constructor

        private readonly ICompanyServiceMasterService _serviceMasterCompanyExtensionService;

        public CompanyServiceMasterController(ICompanyServiceMasterService serviceMasterCompanyExtensionService)
        {
            _serviceMasterCompanyExtensionService = serviceMasterCompanyExtensionService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCboList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_serviceMasterCompanyExtensionService.GetCboByCompany(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboServiceList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_serviceMasterCompanyExtensionService.GetCboService(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanyServiceMaster> serviceMasterCompanyExtension, string companyId)
        {
            _serviceMasterCompanyExtensionService.InsertOrUpdate(serviceMasterCompanyExtension, companyId);
            return Json(new { ServiceMasterCompanyExtension = serviceMasterCompanyExtension, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _serviceMasterCompanyExtensionService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCompany(string companyId)
        {
            return Json(_serviceMasterCompanyExtensionService.Query(companyId), JsonRequestBehavior.AllowGet);
        }
    }
}