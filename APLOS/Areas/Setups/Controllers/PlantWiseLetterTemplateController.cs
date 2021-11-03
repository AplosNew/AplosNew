#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class PlantWiseLetterTemplateController : BaseController
    {
        #region Constructor

        private readonly IPlantWiseLetterTemplateService _plantWiseLetterTemplateService;

        public PlantWiseLetterTemplateController(IPlantWiseLetterTemplateService plantWiseLetterTemplateService)
        {
            _plantWiseLetterTemplateService = plantWiseLetterTemplateService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters, string plantId,string letterType)
        {
            return Json(_plantWiseLetterTemplateService.Query(parameters, plantId, letterType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PlantWiseLetterTemplate plantWiseLetterTemplate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantWiseLetterTemplate.CompanyGroupId = identity.CompanyGroupId;
            _plantWiseLetterTemplateService.Insert(plantWiseLetterTemplate);
            return Json(new { plantWiseLetterTemplate = plantWiseLetterTemplate, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PlantWiseLetterTemplate plantWiseLetterTemplate)
        {
            _plantWiseLetterTemplateService.Update(plantWiseLetterTemplate);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _plantWiseLetterTemplateService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetTermsAndConditionsByPreRecruitmentEmployee(string preRecruitmentEmployeeId)
        {
            return Json(_plantWiseLetterTemplateService.GetTermsAndConditionsByPreRecruitmentEmployee(preRecruitmentEmployeeId), JsonRequestBehavior.AllowGet);
        }
    }
}