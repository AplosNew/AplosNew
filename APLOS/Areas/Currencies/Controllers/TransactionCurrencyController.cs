using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Currencies;
using Library.Service.Currencies;
using System.Web.Mvc;

namespace Aplos.Areas.Currencies.Controllers
{
    public class TransactionCurrencyController : BaseController
    {
        private readonly ICurrencyTransactionService _currencyTransactionService;

        public TransactionCurrencyController(ICurrencyTransactionService currencyTransactionService)
        {
            _currencyTransactionService = currencyTransactionService;
        }

        [HttpGet, Authorize]
        public JsonResult GetCboAllCompanyTransactionList()
        {
            return Json(_currencyTransactionService.GetCboAllCompanyTransactionList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboCurrencyTransaction(string companyId)
        {
            return Json(_currencyTransactionService.GetCboCurrencyTransaction(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetExchangeCurrencyCbo(string companyId)
        {
            return Json(_currencyTransactionService.GetExchangeCurrencyCbo(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTransactionCurrencyList(GridParameter parameters, string companyId)
        {
            return Json(_currencyTransactionService.Query(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTransactionCurrency(string id)
        {
            return Json(_currencyTransactionService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CurrencyTransaction currencyTransaction)
        {
            _currencyTransactionService.Insert(currencyTransaction);
            return Json(new { CurrencyTransaction = currencyTransaction, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(CurrencyTransaction currencyTransaction)
        {
            _currencyTransactionService.Update(currencyTransaction);
            return Json(new { CurrencyTransaction = currencyTransaction, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id, string companyId)
        {
            _currencyTransactionService.Delete(id, companyId);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}