using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyGroupClassController : Controller
    {
        private readonly IPartyGroupClassService _partyGroupClassService;

        public PartyGroupClassController(IPartyGroupClassService partyGroupClassService)
        {
            _partyGroupClassService = partyGroupClassService;
        }

        [Authorize]
        public JsonResult GetPartyGroupClassCbo()
        {
            return Json(new SelectList(_partyGroupClassService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_partyGroupClassService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPartyGroupClass(string id)
        {
            return Json(_partyGroupClassService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_partyGroupClassService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PartyGroupClass PartyGroupClass)
        {
            _partyGroupClassService.Insert(PartyGroupClass);
            return Json(new { PartyGroupClass, Sequence = _partyGroupClassService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PartyGroupClass PartyGroupClass)
        {
            _partyGroupClassService.Update(PartyGroupClass);
            return Json(new { Sequence = _partyGroupClassService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyGroupClassService.Archive(id);
            return Json(new { Sequence = _partyGroupClassService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}