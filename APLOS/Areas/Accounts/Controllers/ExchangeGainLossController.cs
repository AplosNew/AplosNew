using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Currencies;
using Library.Model.Finances;
using Library.Service.Currencies;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class ExchangeGainLossController : BaseController
    {
        private readonly IExchangeGainLossService _exchangeGainLossService;

        public ExchangeGainLossController(IExchangeGainLossService exchangeGainLossService)
        {
            _exchangeGainLossService = exchangeGainLossService;
        }

        [HttpGet]
        public ActionResult ExchangeGainLoss()
        {
            return View("~/Areas/Accounts/Views/ExchangeGainLoss.cshtml");
        }

        [HttpGet]
        public ActionResult ExchangeGain(GridParameter parameters, string coaId, FinancingTypeEnum sourceType)
        {
            return Json(_exchangeGainLossService.ExchangeGain(parameters, coaId, sourceType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ExchangeLoss(GridParameter parameters, string coaId, FinancingTypeEnum sourceType)
        {
            return Json(_exchangeGainLossService.ExchangeLoss(parameters, coaId, sourceType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult ExchangeGainCompanyWise(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_exchangeGainLossService.ExchangeGainCompanyWise(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult ExchangeLossCompanyWise(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_exchangeGainLossService.ExchangeLossCompanyWise(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<ExchangeGainLossGL> exchangeGainLoss)
        {
            IEnumerable<ExchangeGainLossGL> exchangeGainLosses = exchangeGainLoss.ToList();
            _exchangeGainLossService.Save(exchangeGainLosses);
            return Json(new { ExchangeGainLossGL = exchangeGainLosses, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ExchangeGainLossGL exchangeGainLoss)
        {
            _exchangeGainLossService.Update(exchangeGainLoss);
            return Json(new { Message = AplosMessage.Updated });
        }

        [Authorize, HttpGet]
        public JsonResult GetExchangeGainLoss(FinancingTypeEnum sourceType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_exchangeGainLossService.GetExchangeGainLoss(identity.CompanyId, sourceType), JsonRequestBehavior.AllowGet);
        }
    }
}