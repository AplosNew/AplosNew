#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class DesignationController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        private readonly IDesignationService _designationService;
        private readonly ICompanyGroupDesignationService _companyGroupDesignationService;

        public DesignationController(
            IDesignationService designationService,
            ICompanyGroupDesignationService companyGroupDesignationService
            , ISqlRepository sqlRepository)
        {
            _companyGroupDesignationService = companyGroupDesignationService;
            _designationService = designationService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetLowerDesignationCbo(string id)
        {
            return Json(new SelectList(_designationService.GetCboList(id), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetUpperDesignationCbo(string id)
        {
            return Json(new SelectList(_designationService.GetCboUpperList(id), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(_companyGroupDesignationService.GetCbo(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_designationService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetbyDesignationMasterCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_sqlRepository.GetDataCollection(@"select Id [Value],UserName [Text] from HKP.Designation where Id IN(Select DesignationId from MST.DesignationMaster)"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupDesignationService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetDList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 1000 * from (SELECT * FROM HKP.Designation) AS TEMP WHERE " + strkey + " order by sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDesignation(string id)
        {
            return Json(_designationService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_designationService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Designation Designation, IEnumerable<LocalLanguage> localLanguages)
        {
            _designationService.Insert(Designation, localLanguages);
            return Json(new { Designation, Sequence = _designationService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Designation Designation, IEnumerable<LocalLanguage> localLanguages)
        {
            _designationService.Update(Designation, localLanguages);
            return Json(new { Sequence = _designationService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _designationService.Delete(id);
            return Json(new { Sequence = _designationService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}