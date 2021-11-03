#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Aplos.Controllers;
using Library.Data.Sql;
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ReportCurrencyExchangeController : BaseController
    {
        #region Constructor
        /// <summary>   The skillCategoryService service. </summary>
        private readonly SqlRepository _sqlRepository;

        public ReportCurrencyExchangeController()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetRelativeCurrencyMatrix(string PlantId, string BaseCurrencyId)
        {

            Library.General.Conversions.CurrencyConversions currency = new Library.General.Conversions.CurrencyConversions("");
            return Json(new
            {
                Matrix = currency.GetRelativeCurrencyMatrix(PlantId, BaseCurrencyId),
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetReportCurrency(string PlantId)
        {
            Library.General.Conversions.CurrencyConversions currency = new Library.General.Conversions.CurrencyConversions("");
            return Json(new
            {
                BaseCurrency = currency.GetReportBaseCurrency(PlantId),
                TransactionCurrencyList = currency.GetAllTransactionCurrency(PlantId)
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveReportCurrencyConversion(string PlantId, string BaseCurrencyId, List<Dictionary<string, object>> data)
        {
            Library.General.Conversions.CurrencyConversions currency = new Library.General.Conversions.CurrencyConversions("");
            try
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (OTSBD.clsStaticInfo.dbl(data[i]["ExchangeRate"]) == 0)
                        throw new System.Exception("Please enter exchange rate");

                }

                currency.SaveReportCurrencyConversion(PlantId, BaseCurrencyId, data);

                return Json(new
                {
                    Error = false,
                    Message = "Data updated successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }


        #endregion
    }
}