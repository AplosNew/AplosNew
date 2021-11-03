using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Taxations;
using Library.Service.Taxations;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
    public class HSNTaxPercentageController : BaseController
    {
        #region Constructor

        private readonly IHSNTaxPercentageService _hSNTaxPercentageService;

        public HSNTaxPercentageController(IHSNTaxPercentageService hSNTaxPercentageService)
        {
            _hSNTaxPercentageService = hSNTaxPercentageService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters, string countryId)
        {
            return Json(_hSNTaxPercentageService.GetList(parameters, countryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetHNSList(GridParameter parameters, string countryId)
        {
            return Json(_hSNTaxPercentageService.GetHSNList(parameters, countryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<HSNTaxPercentage> hSNTaxPercentage, string countryId)
        {
            _hSNTaxPercentageService.InsertOrUpdate(hSNTaxPercentage, countryId);
            return Json(new { HSNTaxPercentage = hSNTaxPercentage, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(HSNTaxPercentage hSNTaxPercentage)
        {
            _hSNTaxPercentageService.Update(hSNTaxPercentage);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _hSNTaxPercentageService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}