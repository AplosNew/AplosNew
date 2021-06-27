using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyCategoryController : BaseController
    {
        private readonly IPartyCategoryService _partyCategoryService;

        public PartyCategoryController(IPartyCategoryService partyCategoryService)
        {
            _partyCategoryService = partyCategoryService;
        }

        [Authorize]
        public JsonResult GetPartyCategoryCbo()
        {
            return Json(new SelectList(_partyCategoryService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_partyCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPartyCategory(string id)
        {
            return Json(_partyCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_partyCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PartyCategory partyCategory)
        {
            _partyCategoryService.Insert(partyCategory);
            return Json(new { PartyCategory = partyCategory, Sequence = _partyCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PartyCategory partyCategory)
        {
            _partyCategoryService.Update(partyCategory);
            return Json(new { Sequence = _partyCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyCategoryService.Archive(id);
            return Json(new { Sequence = _partyCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}