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
    public class DesignationGroupController : BaseController
    {
        #region -- Constructor

        private readonly IDesignationGroupService _designationGroupService;
        private readonly ICompanyGroupDesignationGroupService _companyGroupDesignationGroupService;
        private readonly ISqlRepository _sqlRepository;

        public DesignationGroupController(IDesignationGroupService designationGroupService,
            ICompanyGroupDesignationGroupService companyGroupDesignationGroupService,ISqlRepository R)
        {
            _companyGroupDesignationGroupService = companyGroupDesignationGroupService;
            _designationGroupService = designationGroupService;
            _sqlRepository = R;

        }

        #endregion -- Constructor

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_designationGroupService.GetCboList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(new SelectList(_companyGroupDesignationGroupService.GetCboByCompanyGroup(companyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_designationGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetDesignationGroup()
        {
            return Json(_designationGroupService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupDesignationGroupService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetDGList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 1000 * from (SELECT * FROM HKP.DesignationGroup) AS TEMP WHERE " + strkey + " order by sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(DesignationGroup designationGroup, IEnumerable<LocalLanguage> localLanguages)
        {
            _designationGroupService.Insert(designationGroup, localLanguages);
            return Json(new { Sequence = _designationGroupService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(DesignationGroup DesignationGroup, IEnumerable<LocalLanguage> localLanguages)
        {
            _designationGroupService.Update(DesignationGroup, localLanguages);
            return Json(new { Sequence = _designationGroupService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _designationGroupService.Delete(id);
            return Json(new { Sequence = _designationGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}