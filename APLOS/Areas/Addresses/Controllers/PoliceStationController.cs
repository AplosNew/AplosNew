using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Addresses;
using Library.Model.Setups;
using Library.Service.Addresses;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Addresses.Controllers
{
    public class PoliceStationController : BaseController
    {
        private readonly IPoliceStationService _policeStationService;

        public PoliceStationController(IPoliceStationService policeStationService)
        {
            _policeStationService = policeStationService;
        }

        [HttpGet]
        public ActionResult PoliceStation()
        {
            return View("~/Areas/Addresses/Views/PoliceStation.cshtml");
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetPoliceStationCbo()
        {
            return Json(_policeStationService.GetCboList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetPoliceStationCboByDistrictChange(string districtId)
        {
            return Json(_policeStationService.GetCboList(districtId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPoliceStationList(GridParameter parameters)
        {
            return Json(_policeStationService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPoliceStation(string id)
        {
            return Json(_policeStationService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_policeStationService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PoliceStation policeStation, IEnumerable<LocalLanguage> localLanguages)
        {
            _policeStationService.Insert(policeStation, localLanguages);
            return Json(new { PoliceStation = policeStation, Sequence = _policeStationService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PoliceStation policeStation, IEnumerable<LocalLanguage> localLanguages)
        {
            _policeStationService.Update(policeStation, localLanguages);
            return Json(new { PoliceStation = policeStation, Sequence = _policeStationService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _policeStationService.Delete(id);
            return Json(new { Sequence = _policeStationService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}