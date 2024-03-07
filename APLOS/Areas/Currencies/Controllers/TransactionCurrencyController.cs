using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Currencies;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Currencies.Controllers
{
    public class TransactionCurrencyController : BaseController
    {
        private readonly ICurrencyTransactionService _currencyTransactionService;
        private readonly ISqlRepository _sqlRepository;

        public TransactionCurrencyController(ICurrencyTransactionService currencyTransactionService, ISqlRepository sqlRepository)
        {
            _currencyTransactionService = currencyTransactionService;
            _sqlRepository = sqlRepository;
        }

        [HttpGet, Authorize]
        public JsonResult GetCboAllCompanyTransactionList()
        {
            return Json(_currencyTransactionService.GetCboAllCompanyTransactionList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboCurrencyTransaction(string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId) || companyId=="null")
            {
                companyId = identity.CompanyId;
            }

            var sql = @"SELECT CT.CurrencyId AS Value, C.Code AS Text, CT.CurrencyId, C.Code, IsBaseCurrency=CASE WHEN CT.CurrencyId=CO.BaseCurrencyId THEN 1 ELSE 0 END
                            FROM [SCS].[CurrencyTransaction] AS CT
                            LEFT JOIN [SCS].[Currency] AS C ON CT.CurrencyId=C.Id
                            LEFT JOIN [ORG].[Company] AS CO ON CO.Id=CT.CompanyId
                            WHERE CT.CompanyId='" + companyId + "' ORDER BY C.Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboCurrencyTransactionForPotal(string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId) || companyId == "null")
            {
                companyId = identity.CompanyId;
            }
            return Json(GetCboForPotal(companyId), JsonRequestBehavior.AllowGet);
        }

        public IEnumerable<object> GetCboForPotal(string companyId)
        {
            try
            {
                var sql = @"SELECT CT.CurrencyId AS Value, C.Code AS Text, CT.CurrencyId, C.Code, IsBaseCurrency=CASE WHEN CT.CurrencyId=CO.BaseCurrencyId THEN 1 ELSE 0 END
                            FROM [SCS].[CurrencyTransaction] AS CT
                            LEFT JOIN [SCS].[Currency] AS C ON CT.CurrencyId=C.Id
                            LEFT JOIN [ORG].[Company] AS CO ON CO.Id=CT.CompanyId
                            WHERE CT.CompanyId='" + companyId + "' ORDER BY C.Code";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Currency.ToString()));
            }
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