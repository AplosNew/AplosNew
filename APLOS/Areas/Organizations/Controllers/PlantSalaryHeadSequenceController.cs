using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Payrolls;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class PlantSalaryHeadSequenceController : BaseController
    {
        #region -- Constructor

        private readonly IPlantSalaryHeadSequenceService _PlantSalaryHeadSequenceService;

        public PlantSalaryHeadSequenceController(IPlantSalaryHeadSequenceService PlantSalaryHeadSequenceService)
        {
            _PlantSalaryHeadSequenceService = PlantSalaryHeadSequenceService;
        }

        #endregion -- Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_PlantSalaryHeadSequenceService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPlantSalaryHeadSequence(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_PlantSalaryHeadSequenceService.QueryGraph(plantId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPlantSalaryHeadSequenceById(string id)
        {
            return Json(_PlantSalaryHeadSequenceService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryHead()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_PlantSalaryHeadSequenceService.GetSalaryHead(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<PlantSalaryHeadSequence> PlantSalaryHeadSequence)
        {
            _PlantSalaryHeadSequenceService.InsertORUpdate(PlantSalaryHeadSequence);
            return Json(new { PlantSalaryHeadSequence, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PlantSalaryHeadSequence PlantSalaryHeadSequence)
        {
            _PlantSalaryHeadSequenceService.Update(PlantSalaryHeadSequence);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _PlantSalaryHeadSequenceService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}