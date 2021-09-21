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
    public class FixedAssetDepreciationRuleController : BaseController
    {
        private readonly IFixedAssetDepreciationRuleService _fixedAssetDepreciationRuleService;
        private readonly ICompanyFixedAssetDepreciationRuleService _companyFixedAssetDepreciationRuleService;

        private readonly ISqlRepository _sqlRepository;

        public FixedAssetDepreciationRuleController(
            IFixedAssetDepreciationRuleService fixedAssetDepreciationRuleService
            , ICompanyFixedAssetDepreciationRuleService companyFixedAssetDepreciationRuleService
            , ISqlRepository R)
        {
            _fixedAssetDepreciationRuleService = fixedAssetDepreciationRuleService;
            _companyFixedAssetDepreciationRuleService = companyFixedAssetDepreciationRuleService;
            _sqlRepository = R;
        }

        #region Constructor
        #endregion Constructor


        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_fixedAssetDepreciationRuleService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_fixedAssetDepreciationRuleService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetDepreciationRule(GridParameter parameters)
        {
            return Json(_fixedAssetDepreciationRuleService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDepreciationRuleById(string id)
        {
            return Json(_fixedAssetDepreciationRuleService.Find(id), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult Create(FixedAssetDepreciationRule DepreciationRule)
        //{
        //    _fixedAssetDepreciationRuleService.Insert(DepreciationRule);
        //    return Json(new { DepreciationRule, Message = AplosMessage.Insert });
        //}

        string TableName = "[MST].[FixedAssetDepreciationRule]";

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code already exists!!!");

                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "' AND  PlantId='" + data["PlantId"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same user name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "DEP" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                    
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, /*Sequence = GetSequence(),*/ Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();
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
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dt.Rows.Add(dr);
        }

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


        [HttpPost]
        public JsonResult Edit(FixedAssetDepreciationRule DepreciationRule)
        {
            _fixedAssetDepreciationRuleService.Update(DepreciationRule);
            return Json(new { Message = AplosMessage.Updated });
        }

        //[HttpPost]
        //public JsonResult Delete(string id)
        //{
        //    _fixedAssetDepreciationRuleService.DeleteGraph(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}

        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, /*Sequence = GetSequence(),*/ Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //private double GetSequence()
        //{
        //    DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
        //    if (dt.Rows.Count > 0)
        //        return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
        //    return 1;
        //}



    }
}