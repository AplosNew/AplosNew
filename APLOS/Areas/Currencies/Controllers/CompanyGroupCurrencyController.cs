using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Currencies;
using Library.Service.Currencies;
using Library.Service.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Currencies.Controllers
{
    public class CompanyGroupCurrencyController : BaseController
    {
        #region Constructor

        private readonly ICompanyGroupCurrencyService _companyGroupCurrencyService;

        public CompanyGroupCurrencyController(ICompanyGroupCurrencyService companyGroupCurrencyService)
        {
            _companyGroupCurrencyService = companyGroupCurrencyService;
        }

        #endregion Constructor

        [Authorize, HttpGet]
        public JsonResult GetCbo(string companyGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyGroupId))
            {
                companyGroupId = identity.CompanyGroupId;
            }
            if (string.IsNullOrEmpty(companyGroupId))
                throw new CustomException(ResourcesCore.InvalidCompanyGroup);
            return Json(new SelectList(_companyGroupCurrencyService.GetCbo(companyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(IEnumerable<CompanyGroupCurrency> groupCurrency)
        {
            _companyGroupCurrencyService.InsertOrUpdateGraph(groupCurrency);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult NeglateDecimal(string id, bool select)
        {
            _companyGroupCurrencyService.NeglateDecimal(id, select);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public JsonResult GetCurrencySearchData()
        {
            return Json(_companyGroupCurrencyService.GetCurrencySearchData().ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCurrencySearch(GridParameter parameters, string comanyGroupId)
        {
            return Json(_companyGroupCurrencyService.Query(parameters, comanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}