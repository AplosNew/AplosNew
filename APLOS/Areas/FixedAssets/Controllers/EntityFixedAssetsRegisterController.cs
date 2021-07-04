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

        string TableName = "dbo.FinalSettlementDeductionHead";
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

        public JsonResult Create(string entityId, string departmentId, IEnumerable<FixedAssetRegister> entityFixedAssetList)
        {
            try
            {
                //DataSet dsMaster;
                //ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "' AND  PlantId='" + data["PlantId"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same code already exists!!!");

                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "' AND  PlantId='" + data["PlantId"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same user name already exists!!!");


                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                // string entityFixedAssetList = "";

                //foreach (var item in entityFixedAssetList)
                //{
                //    if (string.IsNullOrEmpty(entityFixedAssetList))
                //    {
                //        entityFixedAssetList += "''," + item;
                //    }
                //    else
                //    {
                //        entityFixedAssetList += "," + item;
                //    }

                //}


               // string _Id = "";

                //#region data update
                //if (dsMaster.Tables[0].Rows.Count == 0)
                //{
                //    bplib.clsGenID genid = new bplib.clsGenID();
                //    genid.GenID(TableName, out _Id);

                //    data["Id"] = "FDH" + _Id;
                //    AddNewRow(dsMaster.Tables[0], data);
                //}
                //else
                //{
                //    _Id = data["Id"].ToString();
                //    EditRow(dsMaster.Tables[0].Rows[0], data);
                //}
                //#endregion data update

                //clsStaticInfo _info = new clsStaticInfo();
                //_info.SaveDataSets(dsMaster);

                return Json(new { Error = false, /*Data = data, Sequence = GetSequence(),*/ Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }




    }
}