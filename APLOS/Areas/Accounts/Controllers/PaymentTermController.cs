using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Payments;
using Library.Service.Payments;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class PaymentTermController : BaseController
    {
        private readonly IPaymentTermService _paymentTermService;
        private readonly IPaymentTermDetailService _paymentTermDetailService;

        public PaymentTermController(
            IPaymentTermService paymentTermService
            , IPaymentTermDetailService paymentTermDetailService
            )
        {
            _paymentTermService = paymentTermService;
            _paymentTermDetailService = paymentTermDetailService;
        }

        [Authorize, HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/PaymentTerm.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_paymentTermService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerCbo()
        {
            return Json(_paymentTermService.GetCustomerCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetVendorCbo()
        {
            return Json(_paymentTermService.GetVendorCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetPaymentTermList(GridParameter parameters)
        {
            return Json(_paymentTermService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_paymentTermDetailService.GetList(id, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPaymentTermCondition(GridParameter parameters, string paymentTermId)
        {
            return Json(_paymentTermDetailService.GetPaymentTermCondition(parameters, paymentTermId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPaymentTerm(string id)
        {
            return Json(_paymentTermService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PaymentTerm paymentTerm, IEnumerable<PaymentTermDetail> paymentTermDetail)
        {
            _paymentTermService.Insert(paymentTerm, paymentTermDetail);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PaymentTerm paymentTerm, IEnumerable<PaymentTermDetail> paymentTermDetail)
        {
            _paymentTermService.Update(paymentTerm, paymentTermDetail);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _paymentTermService.Delete(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}