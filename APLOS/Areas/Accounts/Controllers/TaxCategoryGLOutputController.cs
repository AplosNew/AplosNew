using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Taxations;
using Library.Service.Taxations;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class TaxCategoryGLOutputController : BaseController
    {
        private readonly ITaxCategoryGLService _taxCategoryGLService;

        public TaxCategoryGLOutputController(ITaxCategoryGLService taxCategoryGLService)
        {
            _taxCategoryGLService = taxCategoryGLService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/TaxCategoryGLOutput.cshtml");
        }

        [HttpPost]
        public JsonResult UpdateTaxCategoryDeterminate(IEnumerable<TaxCategoryGL> taxCategoryGLOutput)
        {
            _taxCategoryGLService.InsertUpdateTaxCategoryDeterminate(taxCategoryGLOutput);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombine(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            return Json(_taxCategoryGLService.GetSearchWithCombine(parameters, coaId, countryId, inputOutput), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAssing(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            return Json(_taxCategoryGLService.GetSearchWithCombineWithAssing(parameters, coaId, countryId, inputOutput), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineNotAssing(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            return Json(_taxCategoryGLService.GetSearchWithCombineWithNotAssing(parameters, coaId, countryId, inputOutput), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {
            _taxCategoryGLService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}