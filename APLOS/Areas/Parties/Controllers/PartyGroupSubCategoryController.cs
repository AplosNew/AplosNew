using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyGroupSubCategoryController : Controller
    {
        private readonly IPartyGroupSubCategoryService _partyGroupSubCategoryService;

        public PartyGroupSubCategoryController(IPartyGroupSubCategoryService partyGroupSubCategoryService)
        {
            _partyGroupSubCategoryService = partyGroupSubCategoryService;
        }

        [Authorize]
        public JsonResult GetPartyGroupSubCategoryCbo()
        {
            return Json(new SelectList(_partyGroupSubCategoryService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_partyGroupSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPartyGroupSubCategory(string id)
        {
            return Json(_partyGroupSubCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_partyGroupSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PartyGroupSubCategory PartyGroupSubCategory)
        {
            _partyGroupSubCategoryService.Insert(PartyGroupSubCategory);
            return Json(new { PartyGroupSubCategory, Sequence = _partyGroupSubCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PartyGroupSubCategory PartyGroupSubCategory)
        {
            _partyGroupSubCategoryService.Update(PartyGroupSubCategory);
            return Json(new { Sequence = _partyGroupSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyGroupSubCategoryService.Archive(id);
            return Json(new { Sequence = _partyGroupSubCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}