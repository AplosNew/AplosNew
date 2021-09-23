using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class CompanyFixedAssetDepreciationRuleController : BaseController
    {
        private readonly ICompanyFixedAssetDepreciationRuleService _companyFixedAssetDepreciationRuleService;
        private readonly ISqlRepository _sqlRepository;

        public CompanyFixedAssetDepreciationRuleController(ICompanyFixedAssetDepreciationRuleService companyFixedAssetDepreciationRuleService
                                                            , ISqlRepository R
                                                            )
        {
            _companyFixedAssetDepreciationRuleService = companyFixedAssetDepreciationRuleService;
            _sqlRepository = R;
        }

        #region Constructor



        #endregion Constructor


        [HttpGet]
        public ActionResult GetList(GridParameter parameters, string companyId)
        {
            return Json(_companyFixedAssetDepreciationRuleService.Query(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }


        [Authorize, HttpGet]
        public JsonResult GetFixedAssetMasterList(GridParameter parameters)
        {
            return Json(_companyFixedAssetDepreciationRuleService.QueryAssetMaster(parameters), JsonRequestBehavior.AllowGet);
        }

        //[Authorize, HttpPost]
        //public ActionResult GetListAssetMaster()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    string sql = @"	select * from mst.FixedAssetMaster";
        //    return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        //}

        [HttpPost, Authorize]
        public ActionResult GetListAssetMaster(string companyId)
        {
            // FixedAssetQueryService fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = _companyFixedAssetDepreciationRuleService.GetListAssetMaster(companyId), Error = false }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFixedAssetDepRuleList()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value, DepreciationRules AS Text FROM mst.FixedAssetDepreciationRule"), JsonRequestBehavior.AllowGet);
        }

        string TableName = "mst.CompanyFixedAssetDepreciationRule";

        [HttpPost]
        public JsonResult Create(List<Dictionary<string, object>> data,string CompanyId)
        {

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where  1=2 ", out dsMaster, false, "1");

                //con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[FinishGoodsBooking] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                // con.OpenDataSetThroughAdapter("SELECT * FROM TRN.InventoryReceive WHERE 1 = 2", out dsInventoryReceive, false, "1");


                //con.OpenDataSetThroughAdapter(@"SELECT * FROM dbo.ItemScanChild WHERE MasterId IN (Select Id from dbo.ItemScan ISN WHERE ISN.WorkDate between '" + data["FromDate"] + "' AND '" + data["ToDate"] + "') AND POId IN (" + pOId + @") AND ProductCode IN (" + productCode + @") AND ISNULL(InventoryReceiveDetailId,'')=''", out dsItemScanChild, false, "1");

                //con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[InventoryMaterial] where MaterialMasterId IN(" + MaterialMasterId + ") and ArticleId IN(" + ArticleId + ")  and CompanyId='" + identity.CompanyId + "' and PlantId='" + identity.PlantId + "'", out dsInventoryMaterial, false, "1");


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                bplib.clsGenID objGenID = new bplib.clsGenID();
                string iID = null;
                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "mst.CompanyFixedAssetDepreciationRule", out iID);
                int i = 0;
                foreach (var item in data)
                {
                    //dsFromDateWiseConsumption.Tables[0].DefaultView.RowFilter = "WorkDate=#" + Convert.ToDateTime(item["WorkDate"].ToString()) + "# AND POId = '" + item["ProductionOrderId"] + "' AND ProductCode = '" + item["ProductCode"] + "'";
                    DataRow drCompanyFADepRule = dsMaster.Tables[0].NewRow();
                    i++;

                    if (item["DepreciationRuleId"] != null)
                    {
                        //if (item["FixedAssetMasterId"] == null)
                       // {
                            drCompanyFADepRule["Id"] = iID + i; ;
                            drCompanyFADepRule["CompanyId"] = CompanyId;
                            drCompanyFADepRule["DepreciationRuleId"] = item["DepreciationRuleId"];
                            drCompanyFADepRule["FixedAssetMasterId"] = item["FixedAssetMasterId"];
                            drCompanyFADepRule["Active"] = true;
                            //drInventoryReceive["EntityId"] = data["ProductionEntityId"].ToString();
                            //drInventoryReceive["GRNDate"] = item["WorkDate"];
                            //drInventoryReceive["EntryDate"] = DateTime.Now;
                            //drInventoryReceive["PODepended"] = false;
                            //drCompanyFADepRule["FinishGoodsBookingId"] = masterId;
                            drCompanyFADepRule["AddedBy"] = identity.Name;
                            drCompanyFADepRule["AddedDate"] = DateTime.Now;
                            drCompanyFADepRule["AddedFromIP"] = identity.IPAddress;
                            dsMaster.Tables[0].Rows.Add(drCompanyFADepRule);
                        //}
                        //else
                        //{
                        //    EditRow(dsMaster.Tables[0].Rows[0], item);
                        //}
                        
                    }
    
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, /*Sequence = GetSequence(),*/ Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }


        //private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    DataRow dr = dt.NewRow();
        //    foreach (var item in sourceData.Keys)
        //    {
        //        try
        //        {
        //            dr[item] = sourceData[item];
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }
        //    dr["AddedBy"] = identity.Name;
        //    dr["AddedDate"] = System.DateTime.Now.ToString();
        //    dr["AddedFromIP"] = identity.IPAddress;
        //    dt.Rows.Add(dr);
        //}



        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }



        [HttpGet]
        public JsonResult GetCompanyDepreciationRuleById(string id)
        {
            return Json(_companyFixedAssetDepreciationRuleService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombine(GridParameter parameters, string companyId)
        {
            return Json(_companyFixedAssetDepreciationRuleService.GetSearchWithCombine(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAll(GridParameter parameters, string companyId, string FixedAssetCategoryIds, string FixedAssetSubCategoryIds)
        {
            return Json(_companyFixedAssetDepreciationRuleService.GetSearchWithCombineAll(parameters, companyId, FixedAssetCategoryIds, FixedAssetSubCategoryIds), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAssing(GridParameter parameters, string companyId, string FixedAssetCategoryIds, string FixedAssetSubCategoryIds)
        {
            return Json(_companyFixedAssetDepreciationRuleService.GetSearchWithCombineWithAssing(parameters, companyId, FixedAssetCategoryIds, FixedAssetSubCategoryIds), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineNotAssing(GridParameter parameters, string companyId, string FixedAssetCategoryIds, string FixedAssetSubCategoryIds)
        {
            return Json(_companyFixedAssetDepreciationRuleService.GetSearchWithCombineWithNotAssing(parameters, companyId, FixedAssetCategoryIds, FixedAssetSubCategoryIds), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult Create(IEnumerable<CompanyFixedAssetDepreciationRule> CompanyDepreciationRule)
        //{
        //    _companyFixedAssetDepreciationRuleService.InsertUpdateCDepreciation(CompanyDepreciationRule);
        //    return Json(new { CompanyDepreciationRule, Message = AplosMessage.Insert });
        //}

        [HttpPost]
        public JsonResult Edit(CompanyFixedAssetDepreciationRule CompanyDepreciationRule)
        {
            _companyFixedAssetDepreciationRuleService.Update(CompanyDepreciationRule);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _companyFixedAssetDepreciationRuleService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}