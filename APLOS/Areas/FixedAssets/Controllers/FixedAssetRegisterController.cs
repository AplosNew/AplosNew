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
using Library.ViewModel.Accounts;
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
    public class FixedAssetRegisterController : BaseController
    {
        private readonly IInventoryPayableService _inventoryPayableService;
        private readonly IFixedAssetRegisterService _fixedAssetRegisterService;
        private readonly IFixedAssetRegisterCharacteristicsValueService _fixedAssetRegisterCharacteristicsValueService;
        private readonly ISqlRepository _sqlRepository;
        

        public FixedAssetRegisterController(
             IFixedAssetRegisterService fixedAssetRegisterService
            , IInventoryPayableService inventoryPayableService
            , IFixedAssetRegisterCharacteristicsValueService fixedAssetRegisterCharacteristicsValueService
            , ISqlRepository sqlRepository
            )
        {
            _fixedAssetRegisterService = fixedAssetRegisterService;
            _inventoryPayableService = inventoryPayableService;
            _fixedAssetRegisterCharacteristicsValueService = fixedAssetRegisterCharacteristicsValueService;
            _sqlRepository = sqlRepository;
        }

       
        public ActionResult Aplos()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetRegister.cshtml");
        }
        public ActionResult FARegister()
        {
            return View("~/Areas/FixedAssets/Views/FARegister.cshtml");
        }
        #region GL vs Fa
        public ActionResult GLvsFA()
        {
            return View("~/Areas/FixedAssets/Views/GLvsFA.cshtml");
        }


        [HttpGet, Authorize]
        public ActionResult GLVSfaReport(string MasterLCList)
        {
            try
            {
                //if (string.IsNullOrEmpty(MasterLCList))
                //    throw new Exception("Please select at least one master LC");

                ExcelEngine excelEngine = new ExcelEngine();
                FixedAssetReportService _fixedAssetReportService = new FixedAssetReportService(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //IWorkbook workbook = _fixedAssetReportService.GetMasterLCReport(excelEngine, MasterLCList);
                //return Json(_fixedAssetReportService.GetMasterLCReport(excelEngine, MasterLCList), JsonRequestBehavior.AllowGet);
                IWorkbook workbook = _fixedAssetReportService.GLVSfaReport(excelEngine, identity.CompanyId, identity.PlantId);

                string strFileName = "GLVSFA.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }
        #endregion GL vs Fa
        public ActionResult FixedAssetRegisterExpenseReport()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetRegisterExpenseReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetCbo()
        {

            return Json(new SelectList(_fixedAssetRegisterService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetRegisterList(string registerid)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);

            return Json(_fixedAssetQueryService.GetFixedAssetList(registerid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetRegisterLists(GridParameter parameters, string fixedAssetRegisterIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetRegisterService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, new JavaScriptSerializer().Deserialize<string[]>(fixedAssetRegisterIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetRegisterInfoWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string companyId)
        {
            return Json(_fixedAssetRegisterService.GetRegisterInfoWithFAMId(assetMasterId, budgetMasterId, assetGLId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAssetItemList(GridParameter parameters)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetAssetItemList(parameters, EnumMaterialTypeNatureList.Asset.ToString(), EnumMaterialTypeNatureList.Consumable.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOpeningBalanceInfoWithFAMId(string assetMasterId, string companyId)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);

            return Json(_fixedAssetQueryService.GetOpeningBalanceInfoWithFAMId(assetMasterId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOpeningBalanceInfoWithMaterialMasterId(string assetGLId, string assetBudgetId, string assetActivityId, string companyId)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetOpeningBalanceInfoWithAssetItemId(assetGLId, assetBudgetId, assetActivityId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOpeningBalanceInfoWithBudgetMasterId(string assetBudgetId, string assetActivityId, string companyId)
        {
            var fixedAssetMasterGL = _sqlRepository.GetModelCollection<FixedAssetMasterGL>(@"SELECT FGL.AccumulatedDepreciationBudgetMasterId,FGL.AccumulatedDepreciationActivityId,FGL.FixedAssetMasterId  
		                                FROM HKP.FixedAssetMasterBudgetTag TAG 
		                                LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=TAG.FixedAssetMasterId 
		                                LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=FAM.Id WHERE TAG.BudgetMasterId='" + assetBudgetId + "'").FirstOrDefault();

            return Json(_fixedAssetRegisterService.GetOpeningBalanceInfoWithBudgetMasterId(assetBudgetId, assetActivityId, companyId, fixedAssetMasterGL.AccumulatedDepreciationBudgetMasterId, fixedAssetMasterGL.AccumulatedDepreciationActivityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetRegisterSavedTotalRowWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string companyId)
        {
            return Json(_fixedAssetRegisterService.GetRegisterSavedTotalRowWithFAMId(assetMasterId, budgetMasterId, assetGLId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAssetRegisterIdList(string AssetRegisterIdList)
        {
            return Json(_fixedAssetRegisterService.GetSavedListById(AssetRegisterIdList), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPriceAndCurrencyById(string id)
        {
            return Json(_fixedAssetRegisterService.GetPriceAndCurrencyById(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter gridparameter)//, string ids
        {
            return Json(_fixedAssetRegisterService.GetSearchData(gridparameter,null), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpGet, Authorize]
        public ActionResult GetOBFixedAssetRegisterList(GridParameter gridparameter)//, string ids
        {
            return Json(_fixedAssetRegisterService.GetOBFARegisterData(gridparameter, null), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }
        [HttpGet, Authorize]
        public ActionResult GetRegisterByMaterialMaster(GridParameter gridparameter, string materialMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetRegisterService.GetRegisterByMaterialMaster(gridparameter, identity.CompanyId, materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListForBudgetMaster(GridParameter gridparameter, string ids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetRegisterService.GetListForBudgetMaster(gridparameter, identity.CompanyId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAttribute(string fixedAssetRegisterId, string assetItemId)
        {
            return Json(_fixedAssetRegisterService.QueryForAttribute(fixedAssetRegisterId, assetItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetShortList(GridParameter gridparameter)
        {
            return Json(_fixedAssetRegisterService.GetSearch(gridparameter), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSkuWithRegister(string materialMasterId, string registerId)
        {
            return Json(_fixedAssetRegisterService.GetSkuWithRegister(materialMasterId, registerId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMaterialMasterCharacteristicsWithValueFreeText(string materialMasterId, string registerid)
        {
            return Json(_fixedAssetRegisterCharacteristicsValueService.GetMaterialMasterCharacteristicsList(materialMasterId, registerid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTotalAccDepRegister(string budgetMasterId)
        {
            string sql = @"DECLARE @budgetMasterId varchar(20)= (SELECT FGL.AccumulatedDepreciationBudgetMasterId
		FROM HKP.FixedAssetMasterBudgetTag TAG 
		LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=TAG.FixedAssetMasterId WHERE TAG.BudgetMasterId='"+ budgetMasterId + @"')
		PRINT @budgetMasterId


        DECLARE @activityId varchar(10)= (SELECT FGL.AccumulatedDepreciationActivityId
		FROM HKP.FixedAssetMasterBudgetTag TAG 
		LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=TAG.FixedAssetMasterId WHERE TAG.BudgetMasterId='"+ budgetMasterId + @"')
		
		            SELECT SUM(FAR.ADBaseAmount) ADBaseAmount,GL.UserName GLName,B.UserName BugetName
		            ,A.UserName ActivityName,FAM.UserName,FAR.FixedAssetMasterId
		            FROM  TRN.FixedAssetRegister FAR 
		            LEFT JOIN MST.FixedAssetMaster FAM ON FAR.FixedAssetMasterId=FAM.Id
		            LEFT JOIN MST.BudgetMaster BM ON BM.Id=FAR.ADBudgetMasterId
		            LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=BM.GLGeneralInfoId
		            LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
		            LEFT JOIN HKP.Activity A ON A.Id=FAR.ADActivityId
		            --LEFT JOIN TRN.OpeningBalanceDetail OBD ON  OBD.BudgetMasterId=FAR.ADBudgetMasterId AND OBD.FAType='AccDept'
		            WHERE FAR.ADBudgetMasterId=@budgetMasterId AND FAR.ADActivityId=@activityId
		              AND FAR.IsOpeningBalance=1
		              group by FAR.FixedAssetMasterId,GL.UserName ,B.UserName 
		            ,A.UserName ,FAM.UserName";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTotalAccDepOB(string budgetMasterId)
        {
            string sql = @"DECLARE @budgetMasterId varchar(20)= (SELECT FGL.AccumulatedDepreciationBudgetMasterId
		                    FROM HKP.FixedAssetMasterBudgetTag TAG 
		                    LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=TAG.FixedAssetMasterId WHERE TAG.BudgetMasterId='"+ budgetMasterId + @"')

                DECLARE @activityId varchar(10)= (SELECT FGL.AccumulatedDepreciationActivityId
                		FROM HKP.FixedAssetMasterBudgetTag TAG 
                		LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=TAG.FixedAssetMasterId WHERE TAG.BudgetMasterId='" + budgetMasterId + @"')
                
                		SELECT  SUM(OBD.CrAmount) Amount,GL.UserName GLName,B.UserName BugetName
                		,A.UserName ActivityName,OBD.Id
                		FROM  TRN.OpeningBalanceDetail OBD 
                		LEFT JOIN(select distinct AccumulatedDepreciationBudgetMasterId,AccumulatedDepreciationActivityId 
                		from  HKP.FixedAssetMasterGL 
                		WHERE AccumulatedDepreciationBudgetMasterId=@budgetMasterId AND AccumulatedDepreciationActivityId=@activityId) 
                		FGL ON FGL.AccumulatedDepreciationBudgetMasterId=OBD.BudgetMasterId
                		LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=OBD.GLGeneralInfoId
                		LEFT JOIN MST.BudgetMaster BM ON BM.Id=OBD.BudgetMasterId
                		LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                		LEFT JOIN HKP.Activity A ON A.Id=OBD.ActivityId
                		WHERE FGL.AccumulatedDepreciationBudgetMasterId=@budgetMasterId AND FGL.AccumulatedDepreciationActivityId=@activityId
                		   AND FAType='AccDept'
                		  GROUP BY GL.UserName ,B.UserName 
                		,A.UserName ,OBD.Id";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateRegister(FixedAssetRegister register, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int NumberOfQuantity, string CompanyCurrencyCode, string CompanyGroupCurrencyCode, string HardCurrencyCode, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
            , string assetGLId, string assetBudgetId, string assetActivityId)
        {
            var registerid = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            register.CompanyGroupId = identity.CompanyGroupId;
            register.CompanyId = identity.CompanyId;
            register.PlantId = identity.PlantId;
            register.IsOpeningBalance = true;
            _fixedAssetRegisterService.InsertORUpdateItem(register, subFixedAssetRegister, NumberOfQuantity, CompanyCurrencyCode, CompanyGroupCurrencyCode, HardCurrencyCode, out registerid
                , assetItemValue, fixedAssetRegisterSkuValue, assetGLId, assetBudgetId, assetActivityId);
            return Json(new { id = registerid, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsValueList(GridParameter parameters, string assignment, string mMasterId, string characteristicsId)
        {
            return Json(_fixedAssetRegisterCharacteristicsValueService.GetCharacteristicsValueList(parameters, assignment, mMasterId, characteristicsId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteRegister(string registerid)
        {
            _fixedAssetRegisterService.DeleteItem(registerid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult CheckMasterIsRegisterApplyByAssetId(string assetMasterId)
        {
            return Json(_fixedAssetRegisterService.CheckMasterIsRegisterApplyByMaterialMasterId(assetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetGLFAListList()
        {
            try
            {
                FixedAssetReportService _fixedAssetReportService = new FixedAssetReportService(_sqlRepository);
                return Json(_fixedAssetReportService.getGLVSfaListSql(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


        }

        


        #region FixedAsset Register JV OB
        public ActionResult FixedAssetRegisterJVOB()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetRegisterJVOB.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult CheckFixedMasterIsRegisterApplyByOBJV(string assetMasterId)
        {
            return Json(_fixedAssetRegisterService.CheckFixedMasterIsRegisterApplyByOBJV(assetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetJVOpeningBalanceFixedAssetItem(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId)
        {
            return Json(_fixedAssetRegisterService.GetJVOpeningBalanceFixedAssetItem(fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, companyId), JsonRequestBehavior.AllowGet);
        }

       
        [HttpGet, Authorize]
        public ActionResult GetJVOBRegisterInfoWithFAMId(string fixedAssetMasterId, string budgetMasterId, string assetGLId, string activityId, string companyId)
        {
            return Json(_fixedAssetRegisterService.GetJVOBRegisterInfoWithFAMId(fixedAssetMasterId, budgetMasterId, assetGLId, activityId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOBFixedAssetList()//, string ids
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetRegisterService.GetOBFixedAssetList(identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpPost]
        public JsonResult CreateRegisterJVOB(FixedAssetRegister register, int NumberOfQuantity, string CompanyCurrencyCode, string CompanyGroupCurrencyCode, string HardCurrencyCode, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
           ,string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId)
        {
            var registerid = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            register.CompanyGroupId = identity.CompanyGroupId;
            register.CompanyId = identity.CompanyId;
            register.PlantId = identity.PlantId;
            register.IsOpeningBalance = true;
            register.IsAUC = false;
            _fixedAssetRegisterService.InsertORUpdateItemJVOB(register, NumberOfQuantity, CompanyCurrencyCode, CompanyGroupCurrencyCode, HardCurrencyCode, out registerid
                , assetItemValue, fixedAssetRegisterSkuValue, fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId);
            return Json(new { id = registerid, Message = AplosMessage.Insert });
        }


        [Authorize, HttpGet]
        public JsonResult getDepreciationRulelist()
        {
            return Json(_sqlRepository.GetDataCollection("select distinct CFADR.DepreciationRuleId AS Value, FADR.Description as Text from mst.CompanyFixedAssetDepreciationRule CFADR left  join mst.FixedAssetDepreciationRule FADR ON CFADR.DepreciationRuleId = FADR.Id"), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region FixedAsset Register JV 
        public ActionResult FixedAssetRegisterJV()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetRegisterJV.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetJVFixedAssetRegisterList(GridParameter gridparameter)//, string ids
        {
            return Json(_fixedAssetRegisterService.GetJVFixedAssetRegisterList(gridparameter, null), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpGet, Authorize]
        public ActionResult GetRegisterListJV(string registerid)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);

            return Json(_fixedAssetQueryService.GetFixedAssetListJV(registerid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetJVFixedAssetItem(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId)
        {
            return Json(_fixedAssetRegisterService.GetJVFixedAssetItem(fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, companyId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetJVRegisterInfoWithFAMId(string fixedAssetMasterId, string budgetMasterId, string assetGLId, string activityId, string companyId)
        {
            return Json(_fixedAssetRegisterService.GetJVRegisterInfoWithFAMId(fixedAssetMasterId, budgetMasterId, assetGLId, activityId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult CheckFixedMasterIsRegisterApplyByJV(string assetMasterId)
        {
            return Json(_fixedAssetRegisterService.CheckFixedMasterIsRegisterApplyByJV(assetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetJVSubAssetList(string fixedAssetRegisterId)
        {
            return Json(_fixedAssetRegisterService.GetJVSubAssetList(fixedAssetRegisterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateRegisterJV(FixedAssetRegister register, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int NumberOfQuantity, string CompanyCurrencyCode, string CompanyGroupCurrencyCode, string HardCurrencyCode, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
           ,string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId)
        {
            var registerid = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            register.CompanyGroupId = identity.CompanyGroupId;
            register.CompanyId = identity.CompanyId;
            register.PlantId = identity.PlantId;
            register.IsOpeningBalance = false;
            register.IsAUC = false;
            _fixedAssetRegisterService.InsertORUpdateItemJV(register, subFixedAssetRegister, NumberOfQuantity, CompanyCurrencyCode, CompanyGroupCurrencyCode, HardCurrencyCode, out registerid
                , assetItemValue, fixedAssetRegisterSkuValue, fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId);
            return Json(new { id = registerid, Message = AplosMessage.Insert });
        }
        #endregion

        #region FixedAsset Register AUC JV 
        public ActionResult FixedAssetRegisterAUCJV()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetRegisterAUCJV.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetAUCJVFixedAssetRegisterList(GridParameter gridparameter)//, string ids
        {
            return Json(_fixedAssetRegisterService.GetAUCJVFixedAssetRegisterList(gridparameter, null), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpGet, Authorize]
        public ActionResult GetAUCJVFixedAssetItem(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId)
        {
            return Json(_fixedAssetRegisterService.GetAUCJVFixedAssetItem(fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, companyId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetAUCJVRegisterInfoWithFAMId(string fixedAssetMasterId, string budgetMasterId, string assetGLId, string activityId, string companyId)
        {
            return Json(_fixedAssetRegisterService.GetAUCJVRegisterInfoWithFAMId(fixedAssetMasterId, budgetMasterId, assetGLId, activityId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult CheckFixedMasterIsRegisterApplyByAUCJV(string assetMasterId)
        {
            return Json(_fixedAssetRegisterService.CheckFixedMasterIsRegisterApplyByAUCJV(assetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAUCJVSubAssetList(string fixedAssetRegisterId)
        {
            return Json(_fixedAssetRegisterService.GetAUCJVSubAssetList(fixedAssetRegisterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateRegisterAUCJV(FixedAssetRegister register, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int NumberOfQuantity, string CompanyCurrencyCode, string CompanyGroupCurrencyCode, string HardCurrencyCode, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
           , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId)
        {
            var registerid = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            register.CompanyGroupId = identity.CompanyGroupId;
            register.CompanyId = identity.CompanyId;
            register.PlantId = identity.PlantId;
            register.IsOpeningBalance = false;
            register.IsAUC = true;
            _fixedAssetRegisterService.InsertORUpdateItemAUCJV(register, subFixedAssetRegister, NumberOfQuantity, CompanyCurrencyCode, CompanyGroupCurrencyCode, HardCurrencyCode, out registerid
                , assetItemValue, fixedAssetRegisterSkuValue, fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId);
            return Json(new { id = registerid, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult GetRegisterAUCList(string registerid)
        {
            return Json(_fixedAssetRegisterService.GetAUCList(registerid), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region FixedAssets AUC Capitalize GRN Bass

        public ActionResult FixedAssetAUCCapitalizeGRNBass()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetAUCCapitalizeGRNBass.cshtml");
        }

        [HttpGet]
        public ActionResult GetGRNFixedAssetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetRegisterService.GetGRNFixedAssetList(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGRNCapitalizeFixedAssetGL(string issueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetRegisterService.GetGRNCapitalizeFixedAssetGL(identity.CompanyId, issueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFixedAssetCapitalizeJournalData(GridParameter gridParameter)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetRegisterService.GetFixedAssetCapitalizeJournalData(gridParameter,identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertGRNFixedAssetCapitalizeJournal(string issueId, string voucherTypeId, decimal ToCurrencyRate, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var voucherVM = new VoucherViewModel
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                VoucherTypeId = voucherTypeId,
                CompanyCurrencyRate = ToCurrencyRate,
                PostingDate = DateTime.Now
            };

            foreach (var item in voucherDetailVMList)
            {
                if (item.GLGeneralInfoId == null)
                    throw new CustomException("GL is Not Mapped !");
                if (item.BudgetMasterId == null)
                    throw new CustomException("Budget is Not Mapped !");
                if (item.ActivityId == null)
                    throw new CustomException("Activity is Not Mapped!");

            }
            if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not equal");
            _inventoryPayableService.InsertGRNFixedAssetCapitalizeJournal(issueId, voucherVM, voucherDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult GetFixedAssetCapitalizeJournalReport(ReportFormat reportFormat, string voucherId,string sourceType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _fixedAssetRegisterService.GetFixedAssetCapitalizeJournalReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, sourceType);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        #endregion

        #region Issue AUC Capitalize 

       
        public ActionResult IssueAUCCapitalize()
        {
            return View("~/Areas/FixedAssets/Views/IssueAUCCapitalize.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetIssueAUCList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetRegisterService.GetIssueAssetAUCList(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetIssueInventoryAUCList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_fixedAssetRegisterService.GetIssueInventoryAUCList(identity.PlantId), JsonRequestBehavior.AllowGet);
             jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public ActionResult GetPostedAUCList()
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_fixedAssetQueryService.GetPostedAUCList(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [Authorize, HttpGet]
        public JsonResult GetIssueCapitalizeFixedAssetGL(string issueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetRegisterService.GetGRNCapitalizeFixedAssetGL(identity.CompanyId, issueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetIssueFixedAssetCapitalizeJournalData(GridParameter gridParameter)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetRegisterService.GetFixedAssetCapitalizeJournalData(gridParameter, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertIssueFixedAssetCapitalizeJournal(string issueId, DateTime postingDate, string voucherTypeId,string currencyId, decimal ToCurrencyRate
            , IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InventoryMaterialViewModel> invIssueDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var voucherVM = new VoucherViewModel
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                VoucherTypeId = voucherTypeId,
                CurrencyId = currencyId,
                CompanyCurrencyRate = ToCurrencyRate,
                PostingDate = postingDate
            };

            foreach (var item in voucherDetailVMList)
            {
                if (item.GLGeneralInfoId == null)
                    throw new CustomException("GL is Not Mapped !");
                if (item.BudgetMasterId == null)
                    throw new CustomException("Budget is Not Mapped !");
                if (item.ActivityId == null)
                    throw new CustomException("Activity is Not Mapped!");

            }
            if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not equal");
            _inventoryPayableService.InsertIssueFixedAssetCapitalizeJournal(issueId, voucherVM, voucherDetailVMList, invIssueDetailList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult InsertIssueInventoryCapitalizeJournal(string issueId, string postingDate,string voucherTypeId, string currencyId, decimal ToCurrencyRate
           , IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InventoryMaterialViewModel> invIssueDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var voucherVM = new VoucherViewModel
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                VoucherTypeId = voucherTypeId,
                CompanyCurrencyRate = ToCurrencyRate,
                PostingDate = postingDate.ToDateTime("dd-MMM-yyyy")
            };

            foreach (var item in voucherDetailVMList)
            {
                if (item.GLGeneralInfoId == null)
                    throw new CustomException("GL is Not Mapped !");
                if (item.BudgetMasterId == null)
                    throw new CustomException("Budget is Not Mapped !");
                if (item.ActivityId == null)
                    throw new CustomException("Activity is Not Mapped!");

            }
            if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not equal");
            _inventoryPayableService.InsertIssueInventoryCapitalizeJournal(issueId, voucherVM, voucherDetailVMList, invIssueDetailList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult InsertExpensesCapitalizeJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            foreach (var item in voucherDetailVMList)
            {
                if (item.GLGeneralInfoId == null)
                    throw new CustomException("GL is Not Mapped !");
                if (item.BudgetMasterId == null)
                    throw new CustomException("Budget is Not Mapped !");
                if (item.ActivityId == null)
                    throw new CustomException("Activity is Not Mapped!");

            }
            
            _inventoryPayableService.InsertExpensesCapitalizeJournal(voucherVM, voucherDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }



        [HttpGet, Authorize]
        public ActionResult GetIssueFixedAssetCapitalizeJournalReport(ReportFormat reportFormat, string voucherId,string sourceType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _fixedAssetRegisterService.GetFixedAssetCapitalizeJournalReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, sourceType);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        #endregion

        #region Capitalized FixedAsset Register JV 
        public ActionResult CapitalizedFixedAssetRegister()
        {
            return View("~/Areas/FixedAssets/Views/CapitalizedFixedAssetRegister.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetCapitalizedFixedAssetRegister(string registerid)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetCapitalizedFixedAssetRegister(registerid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCapitalizedFixedAssetRegisterList(GridParameter gridparameter)//, string ids
        {
            return Json(_fixedAssetRegisterService.GetJVFixedAssetRegisterList(gridparameter, null), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpGet, Authorize]
        public ActionResult GetCapitalizedAssetItem(GridParameter gridparameter, string faType)//, string ids
        {
            return Json(_fixedAssetRegisterService.GetCapitalizeAssetItem(gridparameter, faType), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpGet, Authorize]
        public ActionResult GetCapitalizeAssetItemValue(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId)
        {
            return Json(_fixedAssetRegisterService.GetCapitalizeAssetItemValue(fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, companyId), JsonRequestBehavior.AllowGet);
        }
        

        [HttpGet, Authorize]
        public ActionResult GetCapitalizedRegisterInfoWithFAMId(string fixedAssetMasterId, string budgetMasterId, string assetGLId, string activityId, string companyId)
        {
            return Json(_fixedAssetRegisterService.GetJVRegisterInfoWithFAMId(fixedAssetMasterId, budgetMasterId, assetGLId, activityId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult CheckFixedMasterIsRegisterApplyByCapitalized(string assetMasterId)
        {
            return Json(_fixedAssetRegisterService.CheckFixedMasterIsRegisterApplyByJV(assetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCapitalizedSubAssetList(string fixedAssetRegisterId)
        {
            return Json(_fixedAssetRegisterService.GetJVSubAssetList(fixedAssetRegisterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateRegisterCapitalized(FixedAssetRegister register, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, decimal NumberOfQuantity, string CompanyCurrencyCode, string CompanyGroupCurrencyCode, string HardCurrencyCode, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
           , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, IEnumerable<FixedAssetRegisterDetail> fixedAssetRegisterDetail)
        {
            var registerid = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            register.CompanyGroupId = identity.CompanyGroupId;
            register.CompanyId = identity.CompanyId;
            register.PlantId = identity.PlantId;
            register.IsOpeningBalance = false;
            register.IsAUC = false;
            _fixedAssetRegisterService.InsertORUpdateCapitalizeAsset(register, subFixedAssetRegister, NumberOfQuantity, CompanyCurrencyCode, CompanyGroupCurrencyCode, HardCurrencyCode, out registerid
                , assetItemValue, fixedAssetRegisterSkuValue, fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, fixedAssetRegisterDetail);
            return Json(new { id = registerid, Message = AplosMessage.Insert });
        }
        #endregion

        #region Non Asset Register
        public ActionResult NonAssetRegister()
        {
            return View("~/Areas/FixedAssets/Views/NonAssetRegister.cshtml");
        }
        [HttpGet, Authorize]
        public ActionResult GetNonAssetItem(string faType)//, string ids
        {
            return Json(_fixedAssetRegisterService.GetNonAssetItem(faType), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpPost]
        public JsonResult CreateRegisterIndividualNonAsset(FixedAssetRegister register, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int NumberOfQuantity, string CompanyCurrencyCode, string CompanyGroupCurrencyCode, string HardCurrencyCode, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
           , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, IEnumerable<FixedAssetRegisterDetail> fixedAssetRegisterDetail)
        {
            var registerid = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            register.CompanyGroupId = identity.CompanyGroupId;
            register.CompanyId = identity.CompanyId;
            register.PlantId = identity.PlantId;
            register.IsOpeningBalance = false;
            register.IsAUC = false;
            _fixedAssetRegisterService.InsertORUpdateCapitalizeNonAsset(register, subFixedAssetRegister, NumberOfQuantity, CompanyCurrencyCode, CompanyGroupCurrencyCode, HardCurrencyCode, out registerid
                , assetItemValue, fixedAssetRegisterSkuValue, fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, fixedAssetRegisterDetail);
            return Json(new { id = registerid, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult GetAssetRegisterItemForSubAsset()//, string ids
        {
            return Json(_fixedAssetRegisterService.GetAssetRegisterItemForSubAsset(), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpPost]
        public JsonResult CreateRegisterSubAsset(string registerid, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, IEnumerable<FixedAssetRegisterDetail> fixedAssetRegisterDetail)
        {
            
            _fixedAssetRegisterService.InsertORUpdateCapitalizeSubAsset(subFixedAssetRegister, fixedAssetRegisterDetail);
            return Json(new { id = registerid, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult GetExpensesRegisterItem(string faType)//, string ids
        {
            return Json(_fixedAssetRegisterService.GetExpensesRegisterItem(faType), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        #endregion

        #region Expenses Capitalize
        
        public ActionResult ExpensesCapitalized()
        {
            return View("~/Areas/FixedAssets/Views/ExpensesCapitalized.cshtml");
        }

     
        [HttpPost, Authorize]
        public JsonResult GetExpensesCapitalizedList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetExpensesCapitalizedList(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetExpensesCapitalizedList(string column, string value, string plantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select top 100 * from (SELECT  V.Id ,V.VoucherNo ,V.SourceType,REPLACE(Convert(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                                    ,VD.Amount Amount
                                    ,CU.Code CurrencyCode,V.CurrencyId
                                    ,REPLACE(Convert(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                                    FROM  TRN.Voucher V 
									LEFT JOIN (select VoucherId,SUM(DrAmount) Amount from  TRN.VoucherDetail where DrAmount>0 group by VoucherId) VD ON VD.VoucherId=V.Id
                                    LEFT JOIN [SCS].Currency CU ON CU.Id=V.CurrencyId
                                    WHERE V.Archive=0 AND V.SourceType='ExpensesCapitalizeJournal' AND V.PlantId='" + plantId + @"'
                                    ) AS TEMP WHERE " + strkey + " order by PostingDate DESC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Fixed Asset Dispose
        public ActionResult FixedAssetDispose()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetDispose/FixedAssetDispose.cshtml");
        }
        public ActionResult FixedAssetDisposePost()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetDispose/FixedAssetDisposePost.cshtml");
        }
        public ActionResult FixedAssetDepreciationPost()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetDepreciationPost.cshtml");
        }
        [HttpPost, Authorize]
        public ActionResult GetFixedAssetRegisterPopUpList(string column, string value, string companyId)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (companyId == null)
                companyId = identity.CompanyId;
                return Json(_fixedAssetQueryService.GetFixedAssetRegisterPopUpList(column, value, companyId), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }


        [HttpGet, Authorize]
        public ActionResult GetFixedAssetRegisterDisposeEditList(string fixedAssetRegisterDisposeId,  string companyId)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (companyId == null)
                companyId = identity.CompanyId;
            return Json(_fixedAssetQueryService.GetFixedAssetRegisterDisposeEditList(fixedAssetRegisterDisposeId, companyId), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [Authorize, HttpGet]
        public JsonResult GetFixedAssetAccDepGL(GridParameter parameters, string companyId)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId))
                companyId = identity.CompanyId;
            return Json(_fixedAssetQueryService.GetFixedAssetAccDepGL(parameters, companyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetFixedAssetLostList(string column, string value,string companyId)
        {
            FixedAssetDisposeService _fixedAssetDisposeService = new FixedAssetDisposeService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (companyId == null)
                companyId = identity.CompanyId;
            return Json(_fixedAssetDisposeService.GetFixedAssetDisposeList(column, value, companyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CreateFixedAssetLost(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegister> fixedAssetRegister)
        {
            _fixedAssetRegisterService.InsertFixedAssetLost(fixedAssetDisposed, fixedAssetRegister);
            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost]
        public JsonResult UpdateFixedAssetLost(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegisterDisposedDetail> fixedAssetRegister)
        {
            _fixedAssetRegisterService.EditFixedAssetLost(fixedAssetDisposed, fixedAssetRegister);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult CreateFixedAssetSales(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegister> fixedAssetRegister)
        {
            _fixedAssetRegisterService.InsertFixedAssetSales(fixedAssetDisposed, fixedAssetRegister);
            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost]
        public JsonResult UpdateFixedAssetSales(string status, FixedAssetRegisterDisposed disposeVM, IEnumerable<FixedAssetRegisterDisposedDetail> fixedAssetRegister)
        {
            _fixedAssetRegisterService.EditFixedAssetSales( status,  disposeVM, fixedAssetRegister);
            return Json(new { Message = AplosMessage.Updated });
        }
       

        [HttpPost]
        public JsonResult CreateFixedAssetScrap(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegister> fixedAssetRegister)
        {
            _fixedAssetRegisterService.InsertFixedAssetScrap(fixedAssetDisposed, fixedAssetRegister);
            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost]
        public JsonResult UpdateFixedAssetScrap(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegisterDisposedDetail> fixedAssetRegister)
        {
            _fixedAssetRegisterService.EditFixedAssetScrap(fixedAssetDisposed, fixedAssetRegister);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public ActionResult GetFixedAssetRegisterDisposePopUpList(string column, string value, string companyId)
        {
            FixedAssetDisposeService _fixedAssetDisposeService = new FixedAssetDisposeService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (companyId == null)
                companyId = identity.CompanyId;
            return Json(_fixedAssetDisposeService.GetFixedAssetDisposeListForPosting(column, value, companyId), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpPost, Authorize]
        public ActionResult GetFixedAssetLostByDisposeIdList(string id)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
           
            return Json(_fixedAssetQueryService.GetFixedAssetLostByDisposeIdList(id), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpPost, Authorize]
        public ActionResult GetFixedAssetLostJVList(string fixedAssetDisposeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);

            return Json(_fixedAssetQueryService.GetFixedAssetLostJVList(fixedAssetDisposeId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpPost, Authorize]
        public ActionResult GetFixedAssetSalesSingleJVList(string fixedAssetDisposeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetFixedAssetSalesSingleJVList(fixedAssetDisposeId,identity.CompanyId,identity.PlantId), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpPost, Authorize]
        public ActionResult GetFixedAssetSalesBookAsSalesJV1List(string fixedAssetDisposeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetFixedAssetSalesBookAsSalesJV1List(fixedAssetDisposeId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpPost, Authorize]
        public ActionResult GetFixedAssetSalesBookAsSalesJV2List(string fixedAssetDisposeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetFixedAssetSalesBookAsSalesJV2List(fixedAssetDisposeId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }

        [HttpPost, Authorize]
        public ActionResult GetFixedAssetScrapSingleJVList(string fixedAssetDisposeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetFixedAssetScrapSingleJVList(fixedAssetDisposeId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);//, new JavaScriptSerializer().Deserialize<string[]>(ids)
        }
        [HttpPost, Authorize]
        public ActionResult GetFixedAssetTheftSingleJVList(string fixedAssetDisposeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetFixedAssetTheftSingleJVList(fixedAssetDisposeId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetFixedAssetDepreciationSingleJVList(string fixedAssetMasterId, DateTime depreciationProcessDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetFixedAssetDepreciationSingleJVList(fixedAssetMasterId, depreciationProcessDate, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetFixedAssetDisposePostedList(string column, string value, string companyId)
        {
            FixedAssetDisposeService _fixedAssetDisposeService = new FixedAssetDisposeService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (companyId == null)
                companyId = identity.CompanyId;
            return Json(_fixedAssetDisposeService.GetFixedAssetDisposePostedList(column, value, companyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CreateFixedAssetDisposePost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<FixedAssetRegisterDisposedDetail> farDisposeDetailList
            , IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist)
        {
            FixedAssetDisposeService _fixedAssetDisposeService = new FixedAssetDisposeService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (voucherVM.Status == "Sales")
            {
            _fixedAssetDisposeService.InsertFixedAssetDisposeSalesPosting(voucherVM, voucherDetailVMList, farDisposeDetailList, advanceSalarySchedulelist);

            }
            else 
            {
                _fixedAssetDisposeService.InsertFixedAssetDisposeScrapPosting(voucherVM, voucherDetailVMList, farDisposeDetailList, advanceSalarySchedulelist);

            }
            return Json(new { Message = AplosMessage.Insert });
        }


        //Fixed Assets Dispose Post Report

        //[HttpGet, Authorize]
        //public ActionResult PabyableJournal(ReportFormat reportFormat, string inventoryReceiveId, string employeeId, bool isReversCharge)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var reportFileName = "GRN";
        //    var workbook = _inventoryReportService.PabyableJournal(identity.CompanyId, identity.PlantId, inventoryReceiveId, employeeId, isReversCharge, reportFileName);
        //    switch (reportFormat)
        //    {
        //        case ReportFormat.Pdf:
        //            return RenderReportAsPdf(workbook, reportFileName);

        //        case ReportFormat.Excel:
        //            return RenderReportAsExcel(workbook, reportFileName);

        //        default:
        //            return View();
        //    }
        //}

        [HttpGet, Authorize]
        public ActionResult FixedAssetsDisposePost(ReportFormat reportFormat, string disposedVoucherId)
        {
            FixedAssetDisposeService _fixedAssetDisposeService =new FixedAssetDisposeService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            
            var workbook = _fixedAssetDisposeService.FixedAssetsDisposePostReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, disposedVoucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName,false);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }
        [HttpGet, Authorize]
        public ActionResult FixedAssetsDepreciationPostReport(ReportFormat reportFormat, string depreciationVoucherId)
        {
            FixedAssetDisposeService _fixedAssetDisposeService = new FixedAssetDisposeService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var workbook = _fixedAssetDisposeService.FixedAssetsDepreciationPostReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, depreciationVoucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName, false);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }


        //[HttpGet, Authorize]
        //public ActionResult GetVendorInvoiceChargeWriteOffReport(ReportFormat reportFormat, string voucherId)
        //{
        //    AccountsInvoiceReportService _accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var workbook = _accountsInvoiceReportService.GetVendorInvoiceChargeReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
        //    switch (reportFormat)
        //    {
        //        case ReportFormat.Pdf:
        //            return RenderReportAsPdf(workbook, reportFileName);

        //        case ReportFormat.Excel:
        //            return RenderReportAsExcel(workbook, reportFileName);

        //        default:
        //            return RenderReportAsExcel(workbook, reportFileName);
        //    }
        //}
        #endregion

        #region Report

        public ActionResult FixedAssetsRegisterReport()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetsRegisterReport.cshtml");
        }
        public ActionResult FixedAssetsRegisterDisposeReport()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetsRegisterDisposeReport.cshtml");
        }

        [Authorize]
        public ActionResult FixedAssetRegisterReportExcel(string MaterialMasterId, string MaterialMasterArticleId, string fixedAssetMasterId, string vendorId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = _fixedAssetRegisterService.FixedAssetRegisterList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, MaterialMasterId, MaterialMasterArticleId, fixedAssetMasterId, vendorId);

                string strFileName = "Fixed Assets Register Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }
        [Authorize]
        public ActionResult FixedAssetRegisterDisposedReportExcel(string fromDate, string toDate, string nonPosted, string posted, string disposeStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();
                string DisposeStatus = "'" + disposeStatus.Replace(",", "','") + "'";//replaced with ""
                IWorkbook workbook = _fixedAssetRegisterService.FixedAssetRegisterDisposedList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,  fromDate,  toDate,  nonPosted,  posted,  DisposeStatus);

                string strFileName = "Fixed Assets Register Disposed Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }


        // [HttpGet, Authorize]
        //public ActionResult FixedAssetRegisterReportPdf( string MaterialMasterId,string MaterialMasterArticleId,string fixedAssetMasterId, string vendorId)
        //{
        //    //string PartyType, string PartyId, string MaterialMasterId, string FixedAssetsId, string FromDate, string ToDate
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    try
        //    {
        //        // if (string.IsNullOrEmpty(MasterLCList))
        //        //   throw new Exception("Please select at least one master Order");

        //        ExcelEngine excelEngine = new ExcelEngine();

        //        IWorkbook workbook =_fixedAssetRegisterService.FixedAssetRegisterList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, MaterialMasterId, MaterialMasterArticleId, fixedAssetMasterId, vendorId);
        //       // string strFileName = "Fixed Assets Register Report.pdf";
        //        string strFileName = "Fixed Assets Register Report.xlsx";
        //        ExcelToPdfConverter convert = new ExcelToPdfConverter(workbook);
        //        PdfDocument pdfDoc = convert.Convert();
        //        workbook.Close();
        //        pdfDoc.Save(strFileName, System.Web.HttpContext.Current.Response, HttpReadType.Save);
        //        //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.Message, JsonRequestBehavior.AllowGet);

        //    }
        //    return null;
        //}

        [HttpGet, Authorize]
        public ActionResult GetBulletinTamplateIndexReport(ReportFormat reportFormat, string fixedAssetRegisterDisposeId)
        {

           
            var reportFileName = "Fixed Asset Disposed";
            FixedAssetReportService fixedAssetReportService = new FixedAssetReportService(_sqlRepository);
            var workbook = fixedAssetReportService.FixedAssetDisposedReportWorkSheet( fixedAssetRegisterDisposeId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetFixedAssetDisposePdfReport(ReportFormat reportFormat, string fixedAssetRegisterDisposeId)
        {
            FixedAssetReportService fixedAssetReportService = new FixedAssetReportService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            fixedAssetReportService.FixedAssetDisposed(fixedAssetRegisterDisposeId);

            return null;
        }

        [HttpGet, Authorize]
        public ActionResult FixedAssetDisposedTemplate(ReportFormat reportFormat, string fixedAssetRegisterDisposeId)
        {


            var reportFileName = "Fixed Asset Disposed";
            FixedAssetReportService fixedAssetReportService = new FixedAssetReportService(_sqlRepository);
            var workbook = fixedAssetReportService.FixedAssetDisposedReportWorkSheet(fixedAssetRegisterDisposeId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }




        #endregion

        #region Elastis Search

        [HttpPost, Authorize]
        public ActionResult GetFixedAssetRegisterElasticSearchDataList(string materialMasterId, string materialMasterArticleId, string fixedAssetMasterId, string vendorId, string isAsset, string machine, string fromDate, string toDate)
        {
            FixedAssetQueryService fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = fixedAssetQueryService.GetFixedAssetRegisterElasticSearchDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, materialMasterId, materialMasterArticleId, fixedAssetMasterId, vendorId, isAsset, machine, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetFixedAssetRegisterDisposedElasticSearchDataList(string fromDate, string toDate, string nonPosted, string posted, string disposeStatus)
        {
            try
            {
                string DisposeStatus = "'" + disposeStatus.Replace(",", "','") + "'";//replaced with ""
                FixedAssetQueryService fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(new { DATA = fixedAssetQueryService.GetFixedAssetRegisterDisposedElasticSearchDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate, nonPosted, posted, DisposeStatus), Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
        }
        #endregion Elastis Search

        #region Fixed Asset Depreciation Process
        public ActionResult FixedAssetDepreciationProcess()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetDepreciationProcess.cshtml");
        }
        [HttpPost, Authorize]
        public ActionResult GetfixedAssetMastersListForProcess( string fiscalYearId, string toDate, string startDate)
        {

            FixedAssetQueryService fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = fixedAssetQueryService.GetfixedAssetMastersListForProcess(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fiscalYearId, toDate, startDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
        [HttpPost]
        public JsonResult FixedAssetDepreciationProcess(string[] selectedAssetMastersList, string fiscalYearId, string toDate)
        {
            string selectedAssetMastersLists = "";

            foreach (var item in selectedAssetMastersList)
            {
                if (string.IsNullOrEmpty(selectedAssetMastersLists))
                {
                    selectedAssetMastersLists +=   item ;
                }
                else
                {
                    selectedAssetMastersLists += "," + item ;
                }

            }
            FixedAssetQueryService fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            fixedAssetQueryService.FixedAssetDepreciationProcess(selectedAssetMastersLists, fiscalYearId, toDate);

            return Json(new { Message = AplosMessage.Insert });
        }
        #endregion
        #region Fixed Asset Depreciation Process
        [HttpPost, Authorize]
        public ActionResult GetFixedAssetDepreciationListForPosting(string column, string value)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           
            return Json(_fixedAssetQueryService.GetFixedAssetDepreciationListForPosting(column, value, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetFixedAssetDepreciationPostedList(string column, string value)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           
            return Json(_fixedAssetQueryService.GetFixedAssetDepreciationPostedList(column, value, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CreateFixedAssetDepreciationPost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<FixedAssetDepreciationProcessVM> fixedAssetDepreciationList)
        {
            FixedAssetDisposeService _fixedAssetDisposeService = new FixedAssetDisposeService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            _fixedAssetDisposeService.InsertFixedAssetDepreciationPosting(voucherVM, voucherDetailVMList, fixedAssetDepreciationList);

            return Json(new { Message = AplosMessage.Insert });
        }
        #endregion

    }
}