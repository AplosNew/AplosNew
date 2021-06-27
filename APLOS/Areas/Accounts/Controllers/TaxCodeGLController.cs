using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Taxations;
using Library.Service.Taxations;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class TaxCodeGLController : BaseController
    {
        private readonly ITaxCodeGLService _taxCodeGLService;

        public TaxCodeGLController(ITaxCodeGLService taxCodeGLService)
        {
            _taxCodeGLService = taxCodeGLService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/TaxCodeGL.cshtml");
        }

        [HttpPost]
        public JsonResult UpdateTaxCodeDeterminate(IEnumerable<TaxCodeGL> taxCodeGL)
        {
            _taxCodeGLService.InsertUpdateTaxCodeDeterminate(taxCodeGL);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombine(GridParameter parameters, string countryId, string coaId)
        {
            return Json(_taxCodeGLService.GetSearchWithCombine(parameters, countryId, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAssing(GridParameter parameters, string countryId, string coaId)
        {
            return Json(_taxCodeGLService.GetSearchWithCombineWithAssing(parameters, countryId, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineNotAssing(GridParameter parameters, string countryId, string coaId)
        {
            return Json(_taxCodeGLService.GetSearchWithCombineWithNotAssing(parameters, countryId, coaId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {
            _taxCodeGLService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}