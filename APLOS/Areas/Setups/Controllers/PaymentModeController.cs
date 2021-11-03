#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Payments;
using Library.Model.Setups;
using Library.Service.Payments;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class PaymentModeController : BaseController
    {
        #region -- Constructor

        private readonly IPaymentModeService _paymentModeService;

        public PaymentModeController(IPaymentModeService paymentModeService)
        {
            this._paymentModeService = paymentModeService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_paymentModeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_paymentModeService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_paymentModeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPaymentMode()
        {
            return Json(_paymentModeService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPaymentModeById(string id)
        {
            return Json(_paymentModeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PaymentMode paymentMode)
        {
            _paymentModeService.Insert(paymentMode);
            return Json(new { PaymentMode = paymentMode, Sequence = _paymentModeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PaymentMode paymentMode)
        {
            _paymentModeService.Update(paymentMode);
            return Json(new { Sequence = _paymentModeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _paymentModeService.Delete(id);
                return Json(new { Sequence = _paymentModeService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations
    }
}