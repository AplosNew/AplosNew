using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Setups;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class PlantDesignationGroupSalaryRuleController : BaseController
    {
        #region -- Constructor

        private readonly IPlantDesignationGroupSalaryRuleService _plantDesignationGroupSalaryRuleService;

        public PlantDesignationGroupSalaryRuleController(IPlantDesignationGroupSalaryRuleService plantDesignationGroupSalaryRuleService)
        {
            _plantDesignationGroupSalaryRuleService = plantDesignationGroupSalaryRuleService;
        }

        #endregion -- Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetDesignationGroupWithoutExistingId(GridParameter parameters, string designationIds, string salaryRuleMasterId)
        {
            return Json(_plantDesignationGroupSalaryRuleService.QueryDesignationWithoutExisting(parameters, designationIds, salaryRuleMasterId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPlantDesignationGroupSalaryRule(string plantId, string salaryRuleMasterId)
        {
            return Json(_plantDesignationGroupSalaryRuleService.QueryGraph(plantId, salaryRuleMasterId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPlantDesignationGroupSalaryRuleById(string id)
        {
            return Json(_plantDesignationGroupSalaryRuleService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryRuleMasterWithPlantCbo(string plantId)
        {
            return Json(_plantDesignationGroupSalaryRuleService.GetSalaryRuleMasterWithPlantCbo(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<PlantDesignationGroupSalaryRule> plantDesignationGroupSalaryRule)
        {
            _plantDesignationGroupSalaryRuleService.InsertORUpdate(plantDesignationGroupSalaryRule);
            return Json(new { PlantDesignationGroupSalaryRule = plantDesignationGroupSalaryRule, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string plantId, string salaryRuleMasterId)
        {
            _plantDesignationGroupSalaryRuleService.DeleteGraph(plantId, salaryRuleMasterId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations

        public ActionResult DesignationMasterReport(string plantId)
        {
            var fileName = "Designation Master Report " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _plantDesignationGroupSalaryRuleService.GetDesignationMaster(plantId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
    }
}