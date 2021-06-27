using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Service.Productions.Recipe;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ProductionOrderSubprocessController : BaseController
    {
        #region Constructor
        /// <summary>   The FixedAssetMaster. </summary>
        /// 
        /// 
        //private readonly CompanyGroupFixedAssetCategoryService _fixedassetcategoryservice;
        //private readonly CompanyGroupFixedAssetSubCategoryService _fixedassetsubcategoryservice;
        private readonly IProductionOrderService _productionbatchmasterservice;
        private readonly IProductionOrderSubprocessSetService _productionbatchsubprocesssetservice;
        private readonly IProductionOrderProcessCriteriaService _productionbatchprocesscriteriaservice;
        //private readonly IRecipeWashMasterService _recipemasterservice;
        //private readonly IProductionBatchWorkCenterService _productionbatchworkcenterservice;
        //private readonly ICountryService _countryservice;
        //private readonly IPartyService _partyservice;
        //private readonly IMachineTypeService _machinetypeservice;


        public ProductionOrderSubprocessController(
            //CompanyGroupFixedAssetCategoryService fixedassetcategoryservice,
            IProductionOrderService productionbatchmasterservice,
            IProductionOrderSubprocessSetService productionbatchsubprocesssetmasterservice,
            IProductionOrderProcessCriteriaService productionbatchsubprocesssetdetailservice
            //IRecipeWashMasterService recipemasterservice
            // IProductionBatchWorkCenterService productionbatchworkcenterservice
            //IMachineTypeService machinetypeservice,
            //CompanyGroupFixedAssetSubCategoryService fixedassetsubcategoryservice
            )
        {
            //this._fixedassetcategoryservice = fixedassetcategoryservice;
            this._productionbatchsubprocesssetservice = productionbatchsubprocesssetmasterservice;
            this._productionbatchprocesscriteriaservice = productionbatchsubprocesssetdetailservice;
            this._productionbatchmasterservice = productionbatchmasterservice;
            //this._recipemasterservice = recipemasterservice;
            //this._productionbatchworkcenterservice = productionbatchworkcenterservice;
            //this._partyservice = partyservice;
            //this._machinetypeservice = machinetypeservice;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        [HttpGet]
        public JsonResult GetProcessCbo(string entityid)
        {
            return Json(_productionbatchsubprocesssetservice.GetProcessCbo(entityid), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetCharacteristicsSetting(string entityid, string mmid)
        {
            return Json(_productionbatchsubprocesssetservice.GetCharacteristicsSetting(entityid, mmid), JsonRequestBehavior.AllowGet);
        }

        //[Authorize]
        //[HttpGet]
        //public ActionResult GetSkuAsperConfig(string entityid, string materialmasterid)
        //{
        //    return Json(_recipemasterservice.GetSkuAsperConfig(entityid, materialmasterid), JsonRequestBehavior.AllowGet);
        //}

        [Authorize]
        [HttpGet]
        public ActionResult GetDetailGridData(string id)
        {
            return Json(_productionbatchsubprocesssetservice.GetDetailGridData(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Getsubprocesslist(GridParameter gridparameter, string entityid, string processid, string processtypeid)
        {
            return Json(_productionbatchsubprocesssetservice.GetListProcessAndProcessTypeWise(gridparameter, entityid, processid, processtypeid), JsonRequestBehavior.AllowGet);
        }

        //[Authorize]
        //[HttpGet]
        //public ActionResult LoadBatchList(GridParameter gridparameter, string mmid, string pomid)
        //{
        //    return Json(_productionbatchmasterservice.LoadBatchList(gridparameter, mmid, pomid), JsonRequestBehavior.AllowGet);
        //}
        //[Authorize]
        //[HttpGet]
        //public ActionResult LoadBatchMaster(string id)
        //{
        //    return Json(_productionbatchmasterservice.GetBatchMaster(id), JsonRequestBehavior.AllowGet);
        //}
        //[Authorize]
        //[HttpGet]
        //public ActionResult LoadBatchDetailList(string masterid)
        //{
        //    return Json(_productionbatchmasterservice.GetOrderDetail(masterid), JsonRequestBehavior.AllowGet);
        //}

        [Authorize, HttpGet]
        public ActionResult Getlist(string processid, string processtypeid, string batchid)
        {
            return Json(_productionbatchprocesscriteriaservice.GetList(processid, processtypeid, batchid), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetDetailChildList(string productionBatchProcessCriteriaId, string entityid, string processid, string processtypeid)
        {
            return Json(_productionbatchsubprocesssetservice.GetDetailChildList(productionBatchProcessCriteriaId, entityid, processid, processtypeid), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CreateDetail(ProductionOrderProcessCriteria detail)
        {
            _productionbatchprocesscriteriaservice.SaveDetail(detail);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult SaveDetailChild(string ProductionBatchSubprocessCritariaId, IEnumerable<ProductionOrderSubprocessSet> productionBatchSubprocessSet)
        {
            _productionbatchsubprocesssetservice.SaveProcessSetChild(ProductionBatchSubprocessCritariaId, productionBatchSubprocessSet);
            return Json(new { ProductionBatchSubprocessSet = productionBatchSubprocessSet, Message = AplosMessage.Insert });
        }
        public ActionResult DeleteDetail(string id)
        {
            _productionbatchprocesscriteriaservice.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion
    }
}