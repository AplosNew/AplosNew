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
    public class CompanyDesignationController : BaseController
    {
        #region -- Constructor

        private readonly ICompanyDesignationService _companyDesignationService;

        public CompanyDesignationController(ICompanyDesignationService companyDesignationService)
        {
            _companyDesignationService = companyDesignationService;
        }

        #endregion -- Constructor

        #region -- Pages

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_companyDesignationService.GetCbo(identity.CompanyGroupId, companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyDesignationService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanyDesignation> designationMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            foreach (var item in designationMaster)
            {
                item.CompanyGroupId = identity.CompanyGroupId;
            }
            _companyDesignationService.Insert(designationMaster);
            return Json(new { DesignationMasterCompanyWise = designationMaster, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _companyDesignationService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}