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
    public class StateController : BaseController
    {
        private readonly IStateService _stateService;

        public StateController(IStateService stateService)
        {
            _stateService = stateService;
        }

        [HttpGet]
        public ActionResult State()
        {
            return View("~/Areas/Addresses/Views/State.cshtml");
        }

        [Authorize]
        public JsonResult GetStateCbo()
        {
            return Json(new SelectList(_stateService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, AllowAnonymous]
        public JsonResult GetStateCboByCountry(string countryId)
        {
            return Json(new SelectList(_stateService.GetCbo(countryId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetStateList(GridParameter parameters)
        {
            return Json(_stateService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetState(string id)
        {
            return Json(_stateService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_stateService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(State state, IEnumerable<LocalLanguage> localLanguages)
        {
            _stateService.Insert(state, localLanguages);
            return Json(new { State = state, Sequence = _stateService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(State State, IEnumerable<LocalLanguage> localLanguages)
        {
            _stateService.Update(State, localLanguages);
            return Json(new { Sequence = _stateService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _stateService.Delete(id);
            return Json(new { Sequence = _stateService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}