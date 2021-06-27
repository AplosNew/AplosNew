using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Data.Sql;
using Library.Model.Taxations;
using Library.Service.Taxations;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class TaxCategoryGLController : BaseController
    {
        private readonly ITaxCategoryGLService _taxCategoryGLService;
        private readonly ISqlRepository _sqlRepository;

        public TaxCategoryGLController(ITaxCategoryGLService taxCategoryGLService, ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
            _taxCategoryGLService = taxCategoryGLService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/TaxCategoryGL.cshtml");
        }

        #region -- Operations
        [HttpPost]
        public JsonResult UpdateTaxCategoryDeterminate(IEnumerable<TaxCategoryGL> taxCategoryGL)
        {
            _taxCategoryGLService.InsertUpdateTaxCategoryDeterminate(taxCategoryGL);
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

        #endregion -- Operations

        #region TaxCategoryRCM

        [Authorize]
        public ActionResult TaxCategoryRCM()
        {
            return View("~/Areas/Accounts/Views/TaxCategoryRCM.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineRCM(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            return Json(_taxCategoryGLService.GetSearchWithCombineRCM(parameters, coaId, countryId, inputOutput), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAssingRCM(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            return Json(_taxCategoryGLService.GetSearchWithCombineWithAssingRCM(parameters, coaId, countryId, inputOutput), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineNotAssingRCM(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            return Json(_taxCategoryGLService.GetSearchWithCombineWithNotAssingRCM(parameters, coaId, countryId, inputOutput), JsonRequestBehavior.AllowGet);
        }

        #endregion TaxCategoryRCM




        #region TaxCategoryRCMOutput

        [Authorize]
        public ActionResult TaxCategoryRCMOutput()
        {
            return View("~/Areas/Accounts/Views/TaxCategoryRCMOutput.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineExcludedOutputTax(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            TaxReportService _taxReportService = new TaxReportService(_sqlRepository);
            return Json(_taxReportService.GetListWithCombineExcludedOutputTax(parameters, coaId, countryId, inputOutput), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAssingExcludedOutput(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            TaxReportService _taxReportService = new TaxReportService(_sqlRepository);

            return Json(_taxReportService.GetListWithCombineAssingExcludedOutput(parameters, coaId, countryId, inputOutput), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineNotAssingExcludedOutput(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            TaxReportService _taxReportService = new TaxReportService(_sqlRepository);

            return Json(_taxReportService.GetListWithCombineNotAssingExcludedOutput(parameters, coaId, countryId, inputOutput), JsonRequestBehavior.AllowGet);
        }

        #endregion TaxCategoryRCMOutput


    }
}