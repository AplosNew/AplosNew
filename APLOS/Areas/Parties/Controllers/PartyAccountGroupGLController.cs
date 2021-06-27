using Aplos.Controllers;
using Aplos.Properties;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyAccountGroupGLController : BaseController
    {
        private readonly IPartyAccountGroupGLService _partyAccountGroupGLService;

        public PartyAccountGroupGLController(IPartyAccountGroupGLService partyAccountGroupGLService)
        {
            _partyAccountGroupGLService = partyAccountGroupGLService;
        }

        [HttpGet, Authorize]
        public JsonResult GetList(string coaId)
        {
            return Json(_partyAccountGroupGLService.GetList(coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<PartyAccountGroupGL> partyAccountGroupGLList)
        {
            _partyAccountGroupGLService.InsertOrUpdateGraph(partyAccountGroupGLList);
            return Json(new { PartyAccountGroupGL = partyAccountGroupGLList, Message = AplosMessage.Insert });
        }

        public ActionResult Delete(string id)
        {
            _partyAccountGroupGLService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}