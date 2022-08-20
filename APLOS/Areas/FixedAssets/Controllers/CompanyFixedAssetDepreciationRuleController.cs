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
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value, Description AS Text FROM mst.FixedAssetDepreciationRule"), JsonRequestBehavior.AllowGet);
        }

        string TableName = "mst.CompanyFixedAssetDepreciationRule";

        [HttpPost]
        public JsonResult Create(List<Dictionary<string, object>> data, string CompanyId)
        {

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string IdList = "";
                foreach (var item in data)
                {
                    if (item["Id"] != null)
                    {
                        if (IdList == "")
                        {
                            IdList = "'" + item["Id"] + "'";
                        }
                        else
                        {
                            IdList += ",'" + item["Id"] + "'";
                        }
                    }
                }

                if (IdList != "")
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id in (" + IdList + ") ", out dsMaster, false, "1");
                else
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1=2 ", out dsMaster, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                string iID = null;
                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "mst.CompanyFixedAssetDepreciationRule", out iID);
                int i = 0;
                foreach (var item in data)
                {

                    if (item["DepreciationRuleId"] != null)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "Id='" + item["Id"] + "'";
                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow drCompanyFADepRule = dsMaster.Tables[0].NewRow();
                            i++;
                            drCompanyFADepRule["Id"] = iID + i; ;
                            drCompanyFADepRule["CompanyId"] = CompanyId;
                            drCompanyFADepRule["DepreciationRuleId"] = item["DepreciationRuleId"];
                            drCompanyFADepRule["FixedAssetMasterId"] = item["FixedAssetMasterId"];

                            drCompanyFADepRule["Active"] = true;
                            drCompanyFADepRule["AddedBy"] = identity.Name;
                            drCompanyFADepRule["AddedDate"] = DateTime.Now;
                            drCompanyFADepRule["AddedFromIP"] = identity.IPAddress;
                            dsMaster.Tables[0].Rows.Add(drCompanyFADepRule);
                        }
                        else
                        {
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            dr["DepreciationRuleId"] = item["DepreciationRuleId"];
                            dr["FixedAssetMasterId"] = item["FixedAssetMasterId"];

                            dr["Active"] = true;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }


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