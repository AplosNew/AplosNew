using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Finances;
using Library.Service.Finances;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class BankChargeTypeController : BaseController
    {
        private readonly IFinancingTypeService _financingTypeService;
        private readonly IFinancingTypeGLService _financingTypeGLService;

        public BankChargeTypeController(
             IFinancingTypeService financingTypeService
            , IFinancingTypeGLService financingTypeGLService
            )
        {
            _financingTypeService = financingTypeService;
            _financingTypeGLService = financingTypeGLService;
        }

        [HttpGet]
        public ActionResult BankChargeType()
        {
            return View("~/Areas/Banks/Views/BankChargeType.cshtml");
        }

        [HttpGet]
        public JsonResult GetBankChargeTypeAutoSequence()
        {
            return Json(_financingTypeService.GetAutoSequence(FinancingTypeEnum.BankCharge.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetBankChargeTypeList(GridParameter parameters)
        {
            return Json(_financingTypeService.Query(parameters, FinancingTypeEnum.BankCharge), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateBankChargeType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.BankCharge.ToString();
            _financingTypeService.Insert(financingType);
            return Json(new { ModelData = financingType, Sequence = _financingTypeService.GetAutoSequence(financingType.SourceType), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditBankChargeType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.BankCharge.ToString();
            _financingTypeService.Update(financingType);
            return Json(new { Sequence = _financingTypeService.GetAutoSequence(financingType.SourceType), Message = AplosMessage.Updated });
        }

        [HttpGet]
        public ActionResult BankChargeTypeGL()
        {
            return View("~/Areas/Banks/Views/BankChargeTypeGL.cshtml");
        }

        [HttpGet]
        public ActionResult GetBankChargeTypeGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllList(parameters, coaId, FinancingTypeEnum.BankCharge), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetBankChargeTypeGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAssingExpensesList(parameters, coaId, FinancingTypeEnum.BankCharge), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetBankChargeTypeGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetNotAssingExpensesList(parameters, coaId, FinancingTypeEnum.BankCharge), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveBankChargeTypeGL(IEnumerable<FinancingTypeGL> financingTypeGLList)
        {
            _financingTypeGLService.InsertOrUpdate(financingTypeGLList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult DeleteBankChargeTypeGL(string id)
        {
            _financingTypeGLService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public JsonResult GetCboBankChargeTypeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetCboBankChargeTypeList(identity.CompanyId, 0), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboBankChargeTypeSourceDeductionList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetCboBankChargeTypeList(identity.CompanyId, 1), JsonRequestBehavior.AllowGet);
        }
    }
}