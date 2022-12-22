using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Taxations;
using Library.Service.Organizations;
using Library.Service.Taxations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class TaxCategoryController : BaseController
    {
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ITaxVariantService _taxVariantService;
        private readonly IPlantService _plantService;
        private readonly ISqlRepository _sqlRepository;

        public TaxCategoryController(
              ITaxCategoryService taxCategoryService
            , ITaxVariantService taxVariantService
            , IPlantService plantService
            , ISqlRepository sqlRepository
            )
        {
            _taxCategoryService = taxCategoryService;
            _taxVariantService = taxVariantService;
            _plantService = plantService;
            _sqlRepository = sqlRepository;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/TaxCategory.cshtml");
        }

        #region -- Operations

        [Authorize]
        public JsonResult GetCbo(string countryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(countryId))
            {
                countryId = _plantService.GetPlantCountryId(identity.PlantId);
            }
            return Json(_taxCategoryService.GetCbo(identity.CompanyGroupId, countryId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetTaxCategoryMaterialLevelCbo(string countryId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(countryId))
            {
                countryId = _plantService.GetPlantCountryId(identity.PlantId);
            }
            return Json(_accountsGLService.GetTaxCategoryMaterialLevelCbo(identity.CompanyGroupId, countryId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetTaxCategoryGSTTypeCbo(string countryId)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(countryId))
            {
                countryId = _plantService.GetPlantCountryId(identity.PlantId);
            }
            return Json(_accountsGLService.GetTaxCategoryGSTTypeCbo(identity.CompanyGroupId, countryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string countryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_taxCategoryService.Query(parameters, identity.CompanyGroupId, countryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_taxCategoryService.GetAutoSequence(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(TaxCategory model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.CompanyGroupId = identity.CompanyGroupId;
            _taxCategoryService.Insert(model);
            return Json(new { TaxCategory = model, Sequence = _taxCategoryService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(TaxCategory model)
        {
            _taxCategoryService.Update(model);
            return Json(new { Sequence = _taxCategoryService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _taxCategoryService.Delete(id);
            return Json(new { Sequence = _taxCategoryService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations

        #region -- Tax Variant

        [Authorize]
        public ActionResult TaxVariant()
        {
            return View("~/Areas/Accounts/Views/TaxVariant.cshtml");
        }

        [Authorize]
        public JsonResult GetTaxVariantCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_taxVariantService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxVariantList(GridParameter parameters, string countryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_taxVariantService.Query(parameters, identity.CompanyGroupId, countryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxVariantDetailList(GridParameter parameters, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_taxVariantService.QueryVariantDetailList(parameters, masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetTaxVariantAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_taxVariantService.GetAutoSequence(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateTaxVariant(TaxVariant taxVariant, IEnumerable<TaxVariantDetail> taxVariantDetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            taxVariant.CompanyGroupId = identity.CompanyGroupId;
            _taxVariantService.Insert(taxVariant, taxVariantDetail);
            return Json(new { TaxVariant = taxVariant, Sequence = _taxVariantService.GetAutoSequence(taxVariant.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult EditTaxVariant(TaxVariant taxVariant, IEnumerable<TaxVariantDetail> taxVariantDetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            taxVariant.CompanyGroupId = identity.CompanyGroupId;
            _taxVariantService.Update(taxVariant, taxVariantDetail);
            return Json(new { Sequence = _taxVariantService.GetAutoSequence(taxVariant.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult DeleteTaxVariant(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _taxVariantService.DeleteGraph(id);
            return Json(new { Sequence = _taxVariantService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Deleted });
        }

        public ActionResult DeleteTaxVariantDetail(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _taxVariantService.DeleteGraphDetail(id);
            return Json(new { Sequence = _taxVariantService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Deleted });
        }

        #endregion -- Tax Variant

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string partyPlantId, string hsnCodeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_taxVariantService.GetTaxCategoryList(identity.CompanyGroupId, identity.PlantId, partyPlantId, hsnCodeId), JsonRequestBehavior.AllowGet);
        }
    }
}