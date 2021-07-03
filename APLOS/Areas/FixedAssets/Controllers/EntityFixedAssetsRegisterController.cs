using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.FixedAssets;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.FixedAsset;
using Library.Model.FixedAssets;
using Library.Model.Inventory;
using Library.Model.Materials;
using Library.Service.Enums;
using Library.Service.FixedAssets;
using Library.Service.Helpers;
using Library.Service.Invoices;
using Library.ViewModel.Materials;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class EntityFixedAssetsRegisterController : BaseController
    {
        private readonly IInventoryPayableService _inventoryPayableService;
        private readonly IFixedAssetRegisterService _fixedAssetRegisterService;
        private readonly IFixedAssetRegisterCharacteristicsValueService _fixedAssetRegisterCharacteristicsValueService;
        private readonly ISqlRepository _sqlRepository;
        //private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;


        public EntityFixedAssetsRegisterController(
             IFixedAssetRegisterService fixedAssetRegisterService
            , IInventoryPayableService inventoryPayableService
            , IFixedAssetRegisterCharacteristicsValueService fixedAssetRegisterCharacteristicsValueService
            , ISqlRepository sqlRepository
            //, ICompanyParallelCurrencyService companyParallelCurrencyService
            )
        {
            _fixedAssetRegisterService = fixedAssetRegisterService;
            _inventoryPayableService = inventoryPayableService;
            _fixedAssetRegisterCharacteristicsValueService = fixedAssetRegisterCharacteristicsValueService;
            _sqlRepository = sqlRepository;
            //_companyParallelCurrencyService = companyParallelCurrencyService;
        }

       
        public ActionResult Aplos()
        {
            return View("~/Areas/FixedAssets/Views/EntityFixedAssetsRegister/Aplos.cshtml");
        }


        [HttpPost, Authorize]
        public ActionResult GetEntityFixedAssetRegisterDataList()
        {
            FixedAssetQueryService fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = fixedAssetQueryService.GetEntityFixedAssetRegisterDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        }




       

    }
}