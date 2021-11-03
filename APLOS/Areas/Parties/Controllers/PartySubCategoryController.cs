using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartySubCategoryController : BaseController
    {
        private readonly IPartySubCategoryService _partySubCategoryService;

        public PartySubCategoryController(IPartySubCategoryService partySubCategoryService)
        {
            _partySubCategoryService = partySubCategoryService;
        }

        [Authorize]
        public JsonResult GetPartySubCategoryCboList()
        {
            return Json(_partySubCategoryService.GetCboList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_partySubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_partySubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(PartySubCategory partySubCategory)
        {
            _partySubCategoryService.Insert(partySubCategory);
            return Json(new { PartySubCategory = partySubCategory, Sequence = _partySubCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PartySubCategory partySubCategory)
        {
            _partySubCategoryService.Update(partySubCategory);
            return Json(new { PartySubCategory = partySubCategory, Sequence = _partySubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partySubCategoryService.Archive(id);
            return Json(new { Sequence = _partySubCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}