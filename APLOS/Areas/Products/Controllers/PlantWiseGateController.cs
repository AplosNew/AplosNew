#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Products;
using Library.Model.Products;

#endregion

namespace Aplos.Areas.Products.Controllers
{
    public class PlantWiseGateController : BaseController
    {
        #region Constructor
        private readonly IPlantWiseGateService _plantWiseGateService;
        public PlantWiseGateController(
              IPlantWiseGateService plantWiseGateService
            )
        {
            _plantWiseGateService = plantWiseGateService;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetCbo(string plantId)
        {
            return Json(new SelectList(_plantWiseGateService.GetCbo(plantId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetList(GridParameter parameters,string plantId)
        {
            return Json(_plantWiseGateService.Query(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetGateData(GridParameter parameters)
        {
            return Json(_plantWiseGateService.GetGateData(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUserGateList(string userId)
        {
            return Json(_plantWiseGateService.GetUserGateList(userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_plantWiseGateService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PlantWiseGate plantWiseGate)
        {
            _plantWiseGateService.Insert(plantWiseGate);
            return Json(new { PlantWiseGate = plantWiseGate, Sequence = _plantWiseGateService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(PlantWiseGate plantWiseGate)
        {
            _plantWiseGateService.Update(plantWiseGate);
            return Json(new { Sequence = _plantWiseGateService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _plantWiseGateService.Delete(id);
            return Json(new { Sequence = _plantWiseGateService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}