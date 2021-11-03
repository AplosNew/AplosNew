using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.Parties;
using Library.Service.Finances;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class FinancingTypeController : BaseController
    {
        private readonly IFinancingTypeGLService _financingTypeGLService;
        private readonly IFinancingTypeService _financingTypeService;

        public FinancingTypeController(
             IFinancingTypeService financingTypeService
            , IFinancingTypeGLService financingTypeGLService
            )
        {
            _financingTypeService = financingTypeService;
            _financingTypeGLService = financingTypeGLService;
        }

        [HttpGet]
        public ActionResult GetFinancingType(string id)
        {
            return Json(_financingTypeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFinancingTypeAutoSequence(string type)
        {
            return Json(_financingTypeService.GetAutoSequence(type), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteFinancingType(string id)
        {
            _financingTypeService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetFinancingTypeGL(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeGLService.GetGL(identity.CompanyId, id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveFinancingTypeGL(IEnumerable<FinancingTypeGL> financingTypeGLList)
        {
            _financingTypeGLService.InsertOrUpdate(financingTypeGLList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult DeleteFinancingTypeGL(string id)
        {
            _financingTypeGLService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [Authorize, HttpGet]
        public JsonResult GetCboInterCompanyFinancingType(FinancingTypeEnum sourceType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetCboInterCompany(identity.CompanyId, sourceType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInterCompanyAssetLiabilityType()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetInterCompanyAssetLiabilityType(identity.CompanyId, SourceType.InterTransaction), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboInterPlantFinancingType(FinancingTypeEnum sourceType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetCboInterPlant(identity.CompanyId, sourceType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboOtherFinancingType(FinancingTypeEnum sourceType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetCboOther(identity.CompanyId, sourceType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboFinancingType(FinancingTypeEnum sourceType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetCboOther(identity.CompanyId, sourceType), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCboAssetLiabilityTranType()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // This cbo will return with both payable and advance gl list;
            return Json(_financingTypeService.GetCboAssetLiabilityTranType(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetCboFinanceTypeForAdvanceJournal()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // This cbo will return with both payable and advance gl list;
            return Json(_financingTypeService.GetCboFinanceTypeForAdvanceJournal(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        #region -- InvoiceDeduction

        [HttpGet]
        public ActionResult PaymentDeduction()
        {
            return View("~/Areas/Accounts/Views/PaymentDeduction.cshtml");
        }

        [HttpGet]
        public JsonResult GetPaymentDeductionTypeAutoSequence()
        {
            return Json(_financingTypeService.GetAutoSequence(FinancingTypeEnum.InvoiceDeduction.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPaymentDeductionList(GridParameter parameters)
        {
            return Json(_financingTypeService.Query(parameters, FinancingTypeEnum.InvoiceDeduction), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreatePaymentDeduction(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.InvoiceDeduction.ToString();
            _financingTypeService.Insert(financingType);
            return Json(new { ModelData = financingType, Sequence = _financingTypeService.GetAutoSequence(financingType.SourceType), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditPaymentDeduction(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.InvoiceDeduction.ToString();
            _financingTypeService.Update(financingType);
            return Json(new { Sequence = _financingTypeService.GetAutoSequence(financingType.SourceType), Message = AplosMessage.Updated });
        }

        #endregion -- InvoiceDeduction

        #region -- Investment

        [HttpGet]
        public ActionResult InvestmentType()
        {
            return View("~/Areas/Accounts/Views/InvestmentType.cshtml");
        }

        [HttpGet]
        public ActionResult GetInvestmentTypeList(GridParameter parameters)
        {
            return Json(_financingTypeService.Query(parameters, FinancingTypeEnum.Investment), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateInvestmentType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.Investment.ToString();
            _financingTypeService.Insert(financingType);
            return Json(new { ModelData = financingType, Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditInvestmentType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.Investment.ToString();
            _financingTypeService.Update(financingType);
            return Json(new { Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        #endregion -- Investment

        #region -- LoanType

        [HttpGet]
        public ActionResult LoanType()
        {
            return View("~/Areas/Accounts/Views/LoanType.cshtml");
        }

        [HttpGet]
        public ActionResult GetLoanTypeList(GridParameter parameters)
        {
            return Json(_financingTypeService.Query(parameters, FinancingTypeEnum.Loan), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateLoanType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.Loan.ToString();
            _financingTypeService.Insert(financingType);
            return Json(new { ModelData = financingType, Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditLoanType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.Loan.ToString();
            _financingTypeService.Update(financingType);
            return Json(new { Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        #endregion -- LoanType

        #region -- Security

        [HttpGet]
        public ActionResult SecurityType()
        {
            return View("~/Areas/Accounts/Views/SecurityType.cshtml");
        }

        [HttpGet]
        public ActionResult GetSecurityTypeList(GridParameter parameters)
        {
            return Json(_financingTypeService.Query(parameters, FinancingTypeEnum.Security), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateSecurityType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.Security.ToString();
            _financingTypeService.Insert(financingType);
            return Json(new { ModelData = financingType, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditSecurityType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.Security.ToString();
            _financingTypeService.Update(financingType);
            return Json(new { Message = AplosMessage.Updated });
        }

        [Authorize, HttpGet]
        public JsonResult GetCboSecurityFinancingTypeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetCboOther(identity.CompanyId, FinancingTypeEnum.Security), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Security

        #region -- InvestmentTypeGL

        [HttpGet]
        public ActionResult InvestmentTypeGL()
        {
            return View("~/Areas/Accounts/Views/InvestmentTypeGL.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetInvestmentTypeGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllList(parameters, coaId, FinancingTypeEnum.Investment), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetInvestmentTypeGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAssingList(parameters, coaId, FinancingTypeEnum.Investment), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetInvestmentTypeGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetNotAssingList(parameters, coaId, FinancingTypeEnum.Investment), JsonRequestBehavior.AllowGet);
        }

        #endregion -- InvestmentTypeGL

        #region -- LoanTypeGL

        [HttpGet]
        public ActionResult LoanTypeGL()
        {
            return View("~/Areas/Accounts/Views/LoanTypeGL.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetLoanTypeGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllList(parameters, coaId, FinancingTypeEnum.Loan), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLoanTypeGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAssingList(parameters, coaId, FinancingTypeEnum.Loan), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLoanTypeGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetNotAssingList(parameters, coaId, FinancingTypeEnum.Loan), JsonRequestBehavior.AllowGet);
        }

        #endregion -- LoanTypeGL

        #region -- InvoiceDeductionGL

        [HttpGet]
        public ActionResult PaymentDeductionGL()
        {
            return View("~/Areas/Accounts/Views/PaymentDeductionGL.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetPaymentDeductionGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllList(parameters, coaId, FinancingTypeEnum.InvoiceDeduction), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPaymentDeductionGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAssingRevenueList(parameters, coaId, FinancingTypeEnum.InvoiceDeduction), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPaymentDeductionGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetNotAssingRevenueList(parameters, coaId, FinancingTypeEnum.InvoiceDeduction), JsonRequestBehavior.AllowGet);
        }

        #endregion -- InvoiceDeductionGL

        #region -- SecurityTypeGL

        [HttpGet]
        public ActionResult SecurityTypeGL()
        {
            return View("~/Areas/Accounts/Views/SecurityTypeGL.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetSecurityTypeGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllList(parameters, coaId, FinancingTypeEnum.Security), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSecurityTypeGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAssingList(parameters, coaId, FinancingTypeEnum.Security), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSecurityTypeGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetNotAssingList(parameters, coaId, FinancingTypeEnum.Security), JsonRequestBehavior.AllowGet);
        }

        #endregion -- SecurityTypeGL

        #region -- InterTransactionType

        [HttpGet]
        public ActionResult InterTransactionType()
        {
            return View("~/Areas/Accounts/Views/InterTransactionType.cshtml");
        }

        [HttpGet]
        public ActionResult GetInterTransactionTypeList(GridParameter parameters)
        {
            return Json(_financingTypeService.Query(parameters, FinancingTypeEnum.InterTransaction), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateInterTransactionType(FinancingType financingType)
        {
            financingType.IsInterCompany = true;
            financingType.IsInterPlant = true;
            financingType.IsOthers = true;
            financingType.SourceType = SourceType.InterTransaction.ToString();
            _financingTypeService.Insert(financingType);
            return Json(new { ModelData = financingType, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditInterTransactionType(FinancingType financingType)
        {
            financingType.IsInterCompany = true;
            financingType.IsInterPlant = true;
            financingType.IsOthers = true;
            financingType.SourceType = SourceType.InterTransaction.ToString();
            _financingTypeService.Update(financingType);
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion -- InterTransactionType

        #region -- InterTransactionTypeGL

        [HttpGet]
        public ActionResult InterTransactionTypeGL()
        {
            return View("~/Areas/Accounts/Views/InterTransactionTypeGL.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetInterTransactionTypeGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllList(parameters, coaId, FinancingTypeEnum.InterTransaction), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetInterTransactionTypeGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAssingList(parameters, coaId, FinancingTypeEnum.InterTransaction), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetInterTransactionTypeGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetNotAssingList(parameters, coaId, FinancingTypeEnum.InterTransaction), JsonRequestBehavior.AllowGet);
        }

        #endregion -- InterTransactionTypeGL

        #region -- CreditNote

        [HttpGet]
        public ActionResult CreditNoteType()
        {
            return View("~/Areas/Accounts/Views/CreditNoteType.cshtml");
        }

        [HttpGet]
        public ActionResult GetCreditNoteTypeList(GridParameter parameters)
        {
            return Json(_financingTypeService.Query(parameters, FinancingTypeEnum.CreditNote), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboCreditNoteTypeList(PartyType partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetDebitCreditNoteTranType(identity.CompanyId, FinancingTypeEnum.CreditNote, partyType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateCreditNoteType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.CreditNote.ToString();
            financingType.AssetUserName = financingType.LiabilityUserName;
            _financingTypeService.Insert(financingType);
            return Json(new { ModelData = financingType, Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditCreditNoteType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.CreditNote.ToString();
            financingType.AssetUserName = financingType.LiabilityUserName;
            _financingTypeService.Update(financingType);
            return Json(new { Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpGet]
        public ActionResult CreditNoteTypeGL()
        {
            return View("~/Areas/Accounts/Views/CreditNoteTypeGL.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetCreditNoteTypeGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllList(parameters, coaId, FinancingTypeEnum.CreditNote), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCreditNoteTypeGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAssingList(parameters, coaId, FinancingTypeEnum.CreditNote), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCreditNoteTypeGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetNotAssingList(parameters, coaId, FinancingTypeEnum.CreditNote), JsonRequestBehavior.AllowGet);
        }

        #endregion -- CreditNote

        #region -- DebitNote

        [HttpGet]
        public ActionResult DebitNoteType()
        {
            return View("~/Areas/Accounts/Views/DebitNoteType.cshtml");
        }

        [HttpGet]
        public ActionResult GetDebitNoteTypeList(GridParameter parameters)
        {
            return Json(_financingTypeService.Query(parameters, FinancingTypeEnum.DebitNote), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboDebitNoteTypeList(PartyType partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetDebitCreditNoteTranType(identity.CompanyId, FinancingTypeEnum.DebitNote, partyType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateDebitNoteType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.DebitNote.ToString();
            financingType.AssetUserName = financingType.LiabilityUserName;
            _financingTypeService.Insert(financingType);
            return Json(new { ModelData = financingType, Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditDebitNoteType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.DebitNote.ToString();
            financingType.AssetUserName = financingType.LiabilityUserName;
            _financingTypeService.Update(financingType);
            return Json(new { Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpGet]
        public ActionResult DebitNoteTypeGL()
        {
            return View("~/Areas/Accounts/Views/DebitNoteTypeGL.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetDebitNoteTypeGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllList(parameters, coaId, FinancingTypeEnum.DebitNote), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDebitNoteTypeGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAssingList(parameters, coaId, FinancingTypeEnum.DebitNote), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDebitNoteTypeGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetNotAssingList(parameters, coaId, FinancingTypeEnum.DebitNote), JsonRequestBehavior.AllowGet);
        }

        #endregion -- DebitNote

        #region -- Customer Type

        [HttpGet]
        public ActionResult CustomerTranType()
        {
            return View("~/Areas/Accounts/Views/CustomerTranType.cshtml");
        }

        [HttpGet]
        public ActionResult GetCustomerTranTypeList(GridParameter parameters)
        {
            return Json(_financingTypeService.Query(parameters, FinancingTypeEnum.Customer), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboCustomerTranTypeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetCustomerVendorTranType(identity.CompanyId, FinancingTypeEnum.Customer, PartyType.Customer), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateCustomerTranType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.Customer.ToString();
            financingType.AssetUserName = financingType.AssetUserName;
            _financingTypeService.Insert(financingType);
            return Json(new { ModelData = financingType, Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditCustomerTranType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.Customer.ToString();
            financingType.AssetUserName = financingType.AssetUserName;
            _financingTypeService.Update(financingType);
            return Json(new { Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpGet]
        public ActionResult CustomerTranTypeGL()
        {
            return View("~/Areas/Accounts/Views/CustomerTranTypeGL.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetCustomerTranTypeGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllList(parameters, coaId, FinancingTypeEnum.Customer), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCustomerTranTypeGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAssingList(parameters, coaId, FinancingTypeEnum.Customer), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCustomerTranTypeGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetNotAssingList(parameters, coaId, FinancingTypeEnum.Customer), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Customer Type

        #region -- Vendor Type

        [HttpGet]
        public ActionResult VendorTranType()
        {
            return View("~/Areas/Accounts/Views/VendorTranType.cshtml");
        }

        [HttpGet]
        public ActionResult GetVendorTranTypeList(GridParameter parameters)
        {
            return Json(_financingTypeService.Query(parameters, FinancingTypeEnum.Vendor), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVendorTranTypeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_financingTypeService.GetCustomerVendorTranType(identity.CompanyId, FinancingTypeEnum.Vendor, PartyType.Vendor), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateVendorTranType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.Vendor.ToString();
            financingType.LiabilityUserName = financingType.LiabilityUserName;
            _financingTypeService.Insert(financingType);
            return Json(new { ModelData = financingType, Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditVendorTranType(FinancingType financingType)
        {
            financingType.SourceType = FinancingTypeEnum.Vendor.ToString();
            financingType.LiabilityUserName = financingType.LiabilityUserName;
            _financingTypeService.Update(financingType);
            return Json(new { Sequence = _financingTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpGet]
        public ActionResult VendorTranTypeGL()
        {
            return View("~/Areas/Accounts/Views/VendorTranTypeGL.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetVendorTranTypeGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllList(parameters, coaId, FinancingTypeEnum.Vendor), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetVendorTranTypeGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAssingList(parameters, coaId, FinancingTypeEnum.Vendor), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetVendorTranTypeGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetNotAssingList(parameters, coaId, FinancingTypeEnum.Vendor), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Vendor Type

        #region -- Discount

       

        [HttpGet]
        public ActionResult DiscountTypeGL()
        {
            return View("~/Areas/Accounts/Views/DiscountTypeGL.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetDiscountTypeGLTypeGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllDiscountList(parameters, coaId), JsonRequestBehavior.AllowGet);
        }


        #endregion -- DebitNote
        [HttpGet]
        public ActionResult RoundingGL()
        {
            return View("~/Areas/Accounts/Views/RoundingGL.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetRoundingGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAllList(parameters, coaId, FinancingTypeEnum.Rounding), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetRoundingGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetAssingList(parameters, coaId, FinancingTypeEnum.Rounding), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetRoundingGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_financingTypeGLService.GetNotAssingList(parameters, coaId, FinancingTypeEnum.Rounding), JsonRequestBehavior.AllowGet);
        }
    }
}