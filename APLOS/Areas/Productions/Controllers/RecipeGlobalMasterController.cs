using Aplos.Controllers;
using Library.Model.Productions.Recipe;
using Aplos.Properties;
using Library.Service.Materials;
using Library.Service.Processes;
using Library.Service.Productions.Recipe;
using Library.Service.Setups;
using Library.Core;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Service.OrderManagements;
using Library.Data;
using System;
using Syncfusion.XlsIO;
using Library.Model.Enums;

namespace Aplos.Areas.Productions.Controllers
{
    public class RecipeGlobalMasterController : BaseController
    {
        #region Constructor

        /// <summary>   The RecipeMaster. </summary>
        ///
        ///
        private readonly IProcessService _processService;

        private readonly ISubProcessService _subprocessservice;
        private readonly IMaterialMasterService _materialmasterservice;
        private readonly IUnitOfMeasurementService _unitofmeasurementservice;

        private readonly IRecipeGlobalMasterService _recipemasterservice;
        private readonly IRecipeGlobalSubprocessService _recipesubprocessservice;
        private readonly IRecipeGlobalOperationService _recipeGlobaloperationservice;
        private readonly IRecipeGlobalRawMaterialService _reciperawmaterialservice;
        private readonly IRecipeGlobalUtilityService _recipeGlobalutilityservice;

        private readonly ICharacteristicsValueService _characteristicsvalueservice;
        private readonly IProductionOrderSubprocessSetService _productionbatchsubprocesssetservice;

        public RecipeGlobalMasterController(
            IRecipeGlobalRawMaterialService reciperawmaterialservice,
        ISubProcessService subprocessservice,
            IRecipeGlobalSubprocessService recipesubprocessservice,
            IProcessService processservice,
            IRecipeGlobalUtilityService recipeGlobalutilityservice,
            ICharacteristicsValueService characteristicsvalueservice,
            IRecipeGlobalOperationService recipeGlobaloperationservice,
            IMaterialMasterService materialmasterservice,
            IUnitOfMeasurementService unitofmeasurementservice,
            IProductionOrderSubprocessSetService productionbatchsubprocesssetservice,
        IRecipeGlobalMasterService recipemasterservice)
        {
            this._reciperawmaterialservice = reciperawmaterialservice;
            this._recipesubprocessservice = recipesubprocessservice;
            this._subprocessservice = subprocessservice;
            this._processService = processservice;
            this._recipeGlobalutilityservice = recipeGlobalutilityservice;
            this._recipeGlobaloperationservice = recipeGlobaloperationservice;
            this._productionbatchsubprocesssetservice = productionbatchsubprocesssetservice;
            this._characteristicsvalueservice = characteristicsvalueservice;
            this._materialmasterservice = materialmasterservice;
            this._unitofmeasurementservice = unitofmeasurementservice;
            this._recipemasterservice = recipemasterservice;
        }

        #endregion Constructor

        #region -- Pages
        
       
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations


        [HttpGet, Authorize]
        public ActionResult GetReport(ReportFormat reportFormat, string mmId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _recipemasterservice.GetRecipeReport(out string reportFileName, mmId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
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


        [HttpGet, Authorize]
        public JsonResult RecipeDetailsUsedListList(string recipemasterId)
        {
            return Json(_recipemasterservice.RecipeDetailsUsedListList(recipemasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialArticle(GridParameter parameters, string materialMasterId)
        {
            return Json(_recipemasterservice.GetMaterialArticle(parameters, materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetRecipeRawMaterialList(string masterId)
        {
            return Json(_recipemasterservice.GetRecipeRawMaterialList(masterId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEntityProductionProcessCbo(string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipemasterservice.GetEntityProductionProcessCbo(identity.IsControlAdmin, identity.IsSysAdmin, identity.UserId, entityId).Rows, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMaterialAttributeCbo()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipemasterservice.GetMaterialMasterCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsCbo()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipemasterservice.GetMaterialMasterCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterCbo()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipemasterservice.GetMaterialMasterCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetRecipeOperationCbo(string processId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipemasterservice.GetRecipeOperationCbo(processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetUnitOfMeasurementCbo()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipemasterservice.GetUnitOfMeasurementCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMeasurementCbo(string materialMasterId)
        {
            return Json(_recipemasterservice.GetMeasurementCbo(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetRecipeMaterialGroupingMasterMeasurementCbo(string recipeMaterialGroupingMasterId)
        {
            return Json(_recipemasterservice.GetRecipeMaterialGroupingMasterMeasurementCbo(recipeMaterialGroupingMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetRecipeCbo(string entityId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipemasterservice.GetRecipeCbo(entityId), JsonRequestBehavior.AllowGet);
        }

        [Authorize,HttpGet]
        public ActionResult GetMasterList(string masterid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipemasterservice.GetList(masterid,identity.CompanyGroupId,identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetListOnChange(string processId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipemasterservice.GetRecipeConfigData(identity.PlantId , processId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
		public JsonResult GetRecipeByPOCbo(string pomid)
        {
            return Json(_recipemasterservice.GetRecipeByPOCbo(pomid), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public JsonResult GetGlobalOperationCbo(string recipeGlobalsubprocessid)
        {
            return Json(_recipeGlobaloperationservice.GetGlobalOperationCbo(recipeGlobalsubprocessid), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetList(GridParameter parameters, string MaterialMasterId)
        {
            return Json(_recipemasterservice.GetListByMMId(parameters, MaterialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult RecipeGlobalMasterList(GridParameter parameters, string entityId, string processId)
        {
            return Json(_recipemasterservice.RecipeGlobalMasterList(parameters, entityId, processId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
		public ActionResult GetDetailList(string masterid)
        {
            return Json(_recipesubprocessservice.GetList(masterid), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetUtilityList(string recipeGlobalsubprocessid)
        {
            return Json(_recipeGlobalutilityservice.GetList(recipeGlobalsubprocessid), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetOperationList(string subprocessid)
        {
            return Json(_recipeGlobaloperationservice.GetList(subprocessid), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetOperation(string rwoid)
        {
            return Json(_recipeGlobaloperationservice.GetOperation(rwoid), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetUtility(string recipeGlobalutilityid)
        {
            return Json(_recipeGlobalutilityservice.GetUtility(recipeGlobalutilityid), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetDetail(string id)
        {
            return Json(_recipesubprocessservice.GetDetailById(id), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetDetailChildList(string detailid)
        {
            return Json(_reciperawmaterialservice.GetList(detailid), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetRecipeMaterialGroup()
        {
            return Json(_recipemasterservice.GetRecipeMaterialGroup(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetRecipeGlobalMaterialGroup(string recipeGlobalSubprocessId)
        {
            return Json(_recipemasterservice.GetRecipeGlobalMaterialGroup(recipeGlobalSubprocessId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
		public ActionResult GetDetailChild(string id)
        {
            return Json(_reciperawmaterialservice.GetDetailById(id), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetSubprocess(string processid)
        {
            return Json(_subprocessservice.GetCbo(processid).Rows, JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetRawMaterial(string mmid)
        {
            return Json(_reciperawmaterialservice.GetMaterialMaster(mmid), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetMaterialMasterList(GridParameter parameters)
        {
            return Json(JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetSkuAsperConfig(string entityid, string materialmasterid)
        {
            return Json(_recipemasterservice.GetSkuAsperConfig(entityid, materialmasterid), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetMMUOMCbo(string materialmasterid)
        {
            return Json(_unitofmeasurementservice.GetCbo(materialmasterid).Rows, JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GetCharacteristicsValueList(GridParameter parameters, string CharacteristicsId, string ids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_characteristicsvalueservice.Query(parameters, identity.CompanyGroupId, CharacteristicsId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipemasterservice.GetProcessCriteriaCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSubProcessCbo(string processId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipemasterservice.GetSubProcessCbo(identity.CompanyGroupId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateMaster(RecipeGlobalMaster master)
        {
            var masterid = string.Empty;
            _recipemasterservice.InsertORUpdateMaster(master, out masterid);
            return Json(new { id = masterid, Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult CreateDetail(RecipeGlobalUtility recipeutility)
        {
            _recipemasterservice.InsertORUpdateDetail(recipeutility);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult CreateRecipeSubprocess(RecipeGlobalSubprocess recipeSubprocess)
        {
            _recipesubprocessservice.CreateRecipeSubprocess(recipeSubprocess);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult CreateRecipeOperation(RecipeGlobalOperation recipeoperation)
        {
            _recipeGlobaloperationservice.CreateRecipeOperation(recipeoperation);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult CreateDetailChild()
        {
            //_recipemasterservice.InsertORUpdateDetailChild(detailchild);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult CreateRawMaterial(RecipeGlobalRawMaterial RecipeGlobalRawMaterial)
        {
            var IsDuplicateEntryAllowed = _reciperawmaterialservice.ShouldValidation(RecipeGlobalRawMaterial.RecipeGlobalMasterId, RecipeGlobalRawMaterial.MaterialMasterId, RecipeGlobalRawMaterial.ArticleId, RecipeGlobalRawMaterial.RecipeGlobalSubprocessId);
            if (IsDuplicateEntryAllowed)
            {
                _reciperawmaterialservice.CreateRecipeRawMaterial(RecipeGlobalRawMaterial);
            }
            else
            {
                //if (!string.IsNullOrEmpty(RecipeGlobalRawMaterial.ArticleId))
                //    throw new CustomException("Selected Material has no attribute and Article has attribute, so it can not be added again...");
                //else
                    throw new CustomException("Selected Material/Article already exists...");
            }

            
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult CreateRecipeGlobalMaterialGroup(RecipeGlobalMaterialGroup recipeGlobalMaterialGroup)
        {
            var IsDuplicateEntryAllowed = _reciperawmaterialservice.RecipeGlobalMaterialGroupValidation(recipeGlobalMaterialGroup.RecipeGlobalSubprocessId, recipeGlobalMaterialGroup.RecipeMaterialGroupingMasterId);
            if (IsDuplicateEntryAllowed)
            {
                _reciperawmaterialservice.CreateRecipeGlobalMaterialGroup(recipeGlobalMaterialGroup);
            }
            else
            {
                throw new CustomException("Selected Group already exists...");
            }
            _reciperawmaterialservice.CreateRecipeGlobalMaterialGroup(recipeGlobalMaterialGroup);
            return Json(new { Message = AplosMessage.Insert });
        }


        [HttpPost, Authorize]
        public JsonResult DeleteRecipeGlobalMaterialGroup(string id)
        {
            _reciperawmaterialservice.DeleteRecipeGlobalMaterialGroup(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult DeleteMaster(string masterid)
        {
            _recipemasterservice.DeleteRecipe(masterid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteDetail(string detailid)
        {
            _recipesubprocessservice.DeleteDetail(detailid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteRecipeOperation(string operationid)
        {
            _recipeGlobaloperationservice.DeleteOperation(operationid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteRecipeUtility(string utilityid)
        {
            _recipeGlobalutilityservice.DeleteUtility(utilityid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteRawMaterial(string rawmaterialid)
        {
            _reciperawmaterialservice.DeleteRawMaterial(rawmaterialid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}