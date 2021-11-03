using Aplos.Controllers;
using Aplos.Helpers;
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
    public class CompanyParallelCurrencyController : BaseController
    {
        #region Constructor

        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;

        public CompanyParallelCurrencyController(
            ICompanyParallelCurrencyService companyParallelCurrencyService)
        {
            _companyParallelCurrencyService = companyParallelCurrencyService;
        }

        #endregion Constructor

        public ActionResult GetCompanyParallelCurrency(string companyId)
        {
            return Json(_companyParallelCurrencyService.GetAllCompanyLocal(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(string comcurType, string comcurId, string comparaId, string GroCurType, string groupcurId, string groupparaId, string hardcurType,
            string hardcurId, string hardparaId, string CompanyId, bool GroupParrallelCurrencyDel, bool HardParrallelCurrencyDel)
        {
            try
            {
                var comparallellist = new List<CompanyParallelCurrency>();
                {
                    comparallellist.Add(
                        new CompanyParallelCurrency
                        {
                            Id = comparaId,
                            CompanyId = CompanyId,
                            ParallelCurrencyType = comcurType,
                            CurrencyId = comcurId,
                            Active = true,
                        });
                    if (GroCurType != "" && groupcurId != "")
                    {
                        comparallellist.Add(
                       new CompanyParallelCurrency
                       {
                           Id = groupparaId,
                           CompanyId = CompanyId,
                           ParallelCurrencyType = GroCurType,
                           CurrencyId = groupcurId,
                           Active = true,
                           GroupParrallelCurrencyDel = GroupParrallelCurrencyDel,
                       });
                    }

                    if (hardcurType != "" && hardcurId != "")
                    {
                        comparallellist.Add(
                       new CompanyParallelCurrency
                       {
                           Id = hardparaId,
                           CompanyId = CompanyId,
                           ParallelCurrencyType = hardcurType,
                           CurrencyId = string.IsNullOrEmpty(hardcurId) ? null : hardcurId,
                           Active = true,
                           HardParrallelCurrencyDel = HardParrallelCurrencyDel
                       });
                    }

                    _companyParallelCurrencyService.InsertRange(comparallellist);
                }
                return Json(new { Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult GetSearchGridData(GridParameter parameters)
        {
            return new CustomJsonResult { Data = _companyParallelCurrencyService.Query(parameters) };
        }

        [Authorize, HttpGet]
        public ActionResult CurrencyCheckParallel(GridParameter parameters, string currencyId)
        {
            return Json(_companyParallelCurrencyService.CurrencyCheckParallel(parameters, currencyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult CurrencyParallel(string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyId = identity.CompanyId;
            }
            return Json(_companyParallelCurrencyService.GetParallelCurrency(companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult CheckParallelCurrencyExceptBase(string currencyId)
        {
            return Json(_companyParallelCurrencyService.CheckParallelCurrencyExceptBase(currencyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult CheckParallelCurrencyBySelecteCurrency(string currencyId)
        {
            return Json(_companyParallelCurrencyService.CheckParallelCurrencyBySelecteCurrency(currencyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult CboParallelCurrency()
        {
            
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyParallelCurrencyService.GetCbo(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCurrencyPrecision(string currencyId)
        {
            return Json(_companyParallelCurrencyService.GetCurrencyPrecision(currencyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult ParallelExchangeRate(DateTime fromdate, string currencyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyParallelCurrencyService.ParallelExchangeRate(identity.CompanyId, currencyId, fromdate), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult ExceptParallelExchangeRate(DateTime fromdate, string currencyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyParallelCurrencyService.ExceptParallelExchangeRate(identity.CompanyId, currencyId, fromdate), JsonRequestBehavior.AllowGet);
        }
    }
}