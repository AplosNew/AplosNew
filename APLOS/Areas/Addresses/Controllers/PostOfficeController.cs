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
    public class PostOfficeController : BaseController
    {
        private readonly IPostOfficeService _postOfficeService;

        public PostOfficeController(IPostOfficeService postOfficeService)
        {
            _postOfficeService = postOfficeService;
        }

        [HttpGet]
        public ActionResult PostOffice()
        {
            return View("~/Areas/Addresses/Views/PostOffice.cshtml");
        }

        [AllowAnonymous]
        public JsonResult GetPostOfficeCbo()
        {
            return Json(new SelectList(_postOfficeService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetPostOfficeCboByDistrictChange(string districtId)
        {
            return Json(_postOfficeService.CboList(districtId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPostOfficeList(GridParameter parameters)
        {
            return Json(_postOfficeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPostOffice(string id)
        {
            return Json(_postOfficeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_postOfficeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PostOffice postOffice, IEnumerable<LocalLanguage> localLanguages)
        {
            _postOfficeService.Insert(postOffice, localLanguages);
            return Json(new { PostOffice = postOffice, Sequence = _postOfficeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PostOffice postOffice, IEnumerable<LocalLanguage> localLanguages)
        {
            _postOfficeService.Update(postOffice, localLanguages);
            return Json(new { PostOffice = postOffice, Sequence = _postOfficeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _postOfficeService.Delete(id);
            return Json(new { Sequence = _postOfficeService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}