using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Currencies;
using Library.Service.Currencies;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Currencies.Controllers
{
    public class CurrencyController : BaseController
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [Authorize]
        public JsonResult GetParallelCurrency1List()
        {
            return Json(new SelectList(_currencyService.GetParallelCurrency1List(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetParallelCurrency1(string basecurrency)
        {
            return Json(new SelectList(_currencyService.GetParallelCurrency1(basecurrency), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetHardCurrencyList(string param1, string param2)
        {
            return Json(new SelectList(_currencyService.GetHardCurrencyList(param1, param2), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CompanyGroupCurrency()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CompanyParallelCurrency()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ExchangeRate()
        {
            return View();
        }

        [HttpGet]
        public ActionResult TransactionCurrency()
        {
            return View();
        }

        [Authorize]
        public JsonResult GetCurrencyCbo()
        {
            return Json(new SelectList(_currencyService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCurrencyList(GridParameter parameters)
        {
            return Json(_currencyService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchCurrencyList(GridParameter parameters, string currencyIds)
        {
            return Json(_currencyService.SearchCurrencyList(parameters, new JavaScriptSerializer().Deserialize<string[]>(currencyIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCurrency(string id)
        {
            return Json(_currencyService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_currencyService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Currency currency)
        {
            if (ModelState.IsValid)
            {
                _currencyService.Insert(currency);
                return Json(new { Currency = currency, Sequence = _currencyService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(Currency currency)
        {
            if (ModelState.IsValid)
            {
                _currencyService.Update(currency);
                return Json(new { Sequence = _currencyService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _currencyService.Archive(id);
                return Json(new { Sequence = _currencyService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}