using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyGroupCategoryController : BaseController
    {
        private readonly IPartyGroupCategoryService _partyGroupCategoryService;

        public PartyGroupCategoryController(IPartyGroupCategoryService partyGroupCategoryService)
        {
            _partyGroupCategoryService = partyGroupCategoryService;
        }

        [Authorize]
        public JsonResult GetPartyGroupCategoryCbo()
        {
            return Json(new SelectList(_partyGroupCategoryService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_partyGroupCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPartyGroupCategory(string id)
        {
            return Json(_partyGroupCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_partyGroupCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PartyGroupCategory partyGroupCategory)
        {
            _partyGroupCategoryService.Insert(partyGroupCategory);
            return Json(new { PartyGroupCategory = partyGroupCategory, Sequence = _partyGroupCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PartyGroupCategory partyGroupCategory)
        {
            _partyGroupCategoryService.Update(partyGroupCategory);
            return Json(new { Sequence = _partyGroupCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyGroupCategoryService.Archive(id);
            return Json(new { Sequence = _partyGroupCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}