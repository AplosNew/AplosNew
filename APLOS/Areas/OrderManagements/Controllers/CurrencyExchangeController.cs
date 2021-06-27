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

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class CurrencyExchangeController : BaseController
    {
        #region Constructor
        /// <summary>   The skillCategoryService service. </summary>
        private readonly SqlRepository _sqlRepository;

        public CurrencyExchangeController(IDestinationService skillCategoryService, ICompanyGroupDestinationService companyGroupDestinationService)
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
        public ActionResult GetExchangeRates(string TransactionId, string TableName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.General.Conversions.CurrencyConversions currency = new Library.General.Conversions.CurrencyConversions(TableName);
            return Json(currency.GetExchangeRates(TransactionId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }



        #endregion
    }
}