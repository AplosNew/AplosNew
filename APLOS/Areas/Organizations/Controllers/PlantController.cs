using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Addresses;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class PlantController : BaseController
    {
        private readonly IPlantService _plantService;

        public PlantController(IPlantService plantService)
        {
            _plantService = plantService;
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            // Get all plant list.
            return Json(_plantService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPlantPrefix(string plantId)
        {
            return Json(_plantService.GetPlantPrefix(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            // Get plant list by companyGroupId.
            if (string.IsNullOrEmpty(companyGroupId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyGroupId = identity.CompanyGroupId;
            }
            return Json(_plantService.GetCboByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCboByCompany(string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyId = identity.CompanyId;
            }
            // Get plant list by companyId.
            return Json(_plantService.GetCboByCompany(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_plantService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPlantList(GridParameter parameters, string companyGroupId, string companyId)
        {
            return Json(_plantService.Query(parameters, companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _plantService.Delete(id);
            return Json(new {Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult Create(Plant plant, AddressMaster addressMaster, IEnumerable<ContactMaster> contactMaster, IEnumerable<LocalLanguage> localLanguages)
        {
            _plantService.Insert(plant, addressMaster, contactMaster, localLanguages);
            return Json(new { Plant = plant,Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Plant plant, AddressMaster addressMaster, IEnumerable<ContactMaster> contactMaster, IEnumerable<LocalLanguage> localLanguages)
        {
            _plantService.Update(plant, addressMaster, contactMaster, localLanguages);
            return Json(new {Message = AplosMessage.Updated });
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_plantService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence(string companyId)
        {
            return Json(_plantService.GetAutoSequence(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult UpdatePlant()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetCboInterPlant(string companyGroupId, string companyId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyGroupId))
            {
                companyGroupId = identity.CompanyGroupId;
            }
            if (string.IsNullOrEmpty(companyId))
            {
                companyId = identity.CompanyId;
            }
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            return Json(_plantService.GetCboInterPlant(companyGroupId, companyId, plantId), JsonRequestBehavior.AllowGet);
        }
    }
}