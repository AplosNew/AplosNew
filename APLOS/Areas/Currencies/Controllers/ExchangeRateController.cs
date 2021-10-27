using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Currencies;
using Library.Service.Currencies;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Currencies.Controllers
{
    public class ExchangeRateController : BaseController
    {
        #region Constructor

        private readonly IExchangeRateService _exchangeRateService;

        public ExchangeRateController(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService;
        }

        #endregion Constructor

        [Authorize, HttpGet]
        public ActionResult GetExchangeRateList(GridParameter parameters, string companyId)
        {
            return Json(_exchangeRateService.Query(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetBaseParallelCurrency(GridParameter parameters, string companyId)
        {
            return Json(_exchangeRateService.BaseParallelCurrency(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult CheckParallelCurrencySet(string companyId)
        {
            return Json(_exchangeRateService.CheckParallelCurrencySet(companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetData(GridParameter parameters, string companyId)
        {
            return Json(_exchangeRateService.GetData(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetDataById(GridParameter parameters, string id)
        {
            return Json(_exchangeRateService.GetDataById(parameters, id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<ExchangeRate> exchangeRate)
        {
            _exchangeRateService.Insert(exchangeRate);
            return Json(new { ExchangeRate = exchangeRate, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Update(ExchangeRate exchangeRate)
        {
            _exchangeRateService.Update(exchangeRate);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _exchangeRateService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [Authorize, HttpGet]
        public ActionResult ParallelExchangeRate(DateTime fromdate, string currencyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_exchangeRateService.ParallelExchangeRate(identity.CompanyId, currencyId, fromdate), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult ReceiveParallelExchangeRate(DateTime fromdate, string currencyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_exchangeRateService.ReceiveParallelExchangeRate(identity.CompanyId, currencyId, fromdate), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCompanyCurrencyExchangeRate(DateTime fromdate, string currencyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_exchangeRateService.GetCompanyCurrencyExchangeRate(identity.CompanyId, currencyId, fromdate), JsonRequestBehavior.AllowGet);
        }

       
    }
}