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
    public class PlantWiseTermsAndConditionsController : BaseController
    {
        #region Constructor

        private readonly IPlantWiseTermsAndConditionsService _plantWiseTermsAndConditionsService;

        public PlantWiseTermsAndConditionsController(IPlantWiseTermsAndConditionsService plantWiseTermsAndConditionsService)
        {
            _plantWiseTermsAndConditionsService = plantWiseTermsAndConditionsService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters, string plantId)
        {
            return Json(_plantWiseTermsAndConditionsService.Query(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PlantWiseTermsAndConditions plantWiseTermsAndConditions)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantWiseTermsAndConditions.CompanyGroupId = identity.CompanyGroupId;
            _plantWiseTermsAndConditionsService.Insert(plantWiseTermsAndConditions);
            return Json(new { PlantWiseTermsAndConditions = plantWiseTermsAndConditions, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PlantWiseTermsAndConditions plantWiseTermsAndConditions)
        {
            _plantWiseTermsAndConditionsService.Update(plantWiseTermsAndConditions);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _plantWiseTermsAndConditionsService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetTermsAndConditionsByPreRecruitmentEmployee(string preRecruitmentEmployeeId)
        {
            return Json(_plantWiseTermsAndConditionsService.GetTermsAndConditionsByPreRecruitmentEmployee(preRecruitmentEmployeeId), JsonRequestBehavior.AllowGet);
        }
    }
}