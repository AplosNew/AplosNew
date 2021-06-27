using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class DesignationMasterConfigurationController : BaseController
    {
        #region -- Constructor

        private readonly IDesignationMasterConfigurationService _DesignationMasterConfigurationService;

        public DesignationMasterConfigurationController(IDesignationMasterConfigurationService DesignationMasterConfigurationService)
        {
            _DesignationMasterConfigurationService = DesignationMasterConfigurationService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_DesignationMasterConfigurationService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetLeavePolicyCboList(string plantId)
        {
            return Json(_DesignationMasterConfigurationService.GetLeavePolicyCbo(plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetBonusPolicyMasterCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_DesignationMasterConfigurationService.GetBonusPolicyMasterCbo(plantId,identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAttdnBonusPmtPolicyMasterCbo(string plantId)
        {
            return Json(_DesignationMasterConfigurationService.GetAttdnBonusPmtPolicyMasterCbo(plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetBonusPolicyMonthlyRetainMasterCbo(string plantId)
        {
            return Json(_DesignationMasterConfigurationService.GetBonusPolicyMonthlyRetainMasterCbo(plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPFPolicyMasterCbo(string plantId)
        {
            return Json(_DesignationMasterConfigurationService.GetPFPolicyMasterCbo(plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetESICPolicyMasterCbo(string plantId)
        {
            return Json(_DesignationMasterConfigurationService.GetESICPolicyMasterCbo(plantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetLegalDesignationbyDesignationMaster(string designationMasterId)
        {
            return Json(_DesignationMasterConfigurationService.GetLegalDesignationbyDesignationMaster(designationMasterId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult OverTimePmtPolicyMasterCbo(string plantId)
        {
            return Json(_DesignationMasterConfigurationService.OverTimePmtPolicyMasterCbo(plantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetDesignationListWithDesignationGroup(string designationGroupId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_DesignationMasterConfigurationService.QueryDesignation(designationGroupId, plantId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDesignationMasterConfiguration(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_DesignationMasterConfigurationService.QueryGraph(plantId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDesignationMasterConfigurationById(string id)
        {
            return Json(_DesignationMasterConfigurationService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryRuleMstHead()
        {
            return Json(_DesignationMasterConfigurationService.GetSalaryRuleMstHead(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAttdnBonusPmtPolicyHead()
        {
            return Json(_DesignationMasterConfigurationService.GetAttdnBonusPmtPolicyHead(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<DesignationMasterConfiguration> DesignationMasterConfiguration)
        {
            _DesignationMasterConfigurationService.InsertORUpdate(DesignationMasterConfiguration);
            return Json(new { DesignationMasterConfiguration, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(DesignationMasterConfiguration DesignationMasterConfiguration)
        {
            _DesignationMasterConfigurationService.Update(DesignationMasterConfiguration);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _DesignationMasterConfigurationService.Delete(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations
    }
}