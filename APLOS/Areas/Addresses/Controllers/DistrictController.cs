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
    public class DistrictController : BaseController
    {
        private readonly IDistrictService _districtService;

        public DistrictController(IDistrictService districtService)
        {
            _districtService = districtService;
        }

        [HttpGet]
        public ActionResult District()
        {
            return View("~/Areas/Addresses/Views/District.cshtml");
        }

        [AllowAnonymous]
        public JsonResult GetDistrictCbo()
        {
            return Json(new SelectList(_districtService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetDistrictCboByStateChange(string stateId)
        {
            return Json(_districtService.GetDistrictCboList(stateId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDistrictList(GridParameter parameters)
        {
            return Json(_districtService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDistrict(string id)
        {
            return Json(_districtService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_districtService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(District district, IEnumerable<LocalLanguage> localLanguages)
        {
            _districtService.Insert(district, localLanguages);
            return Json(new { District = district, Sequence = _districtService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(District district, IEnumerable<LocalLanguage> localLanguages)
        {
            _districtService.Update(district, localLanguages);
            return Json(new { District = district, Sequence = _districtService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _districtService.Delete(id);
            return Json(new { Sequence = _districtService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}