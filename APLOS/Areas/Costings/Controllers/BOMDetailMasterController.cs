#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.OrderManagement.Production;
using Library.OrderManagement.Sales;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Costings.Controllers
{
    public class BOMDetailMasterController : BaseController
    {
        string TableName = "dbo.BOMDetailMaster";
        #region Constructor
        clsSales clsSales = new clsSales();
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly ISqlRepository _sqlRepository;
        public BOMDetailMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT B.[Id]
      ,B.[UserCode]
      ,B.[UserName]
      ,B.[ResponsiblePersonId]
      ,B.[Remarks]
      ,B.[AddedBy]
      ,B.[AddedDate]
      ,B.[AddedFromIP]
      ,B.[UpdatedBy]
      ,B.[UpdatedDate]
      ,B.[UpdatedFromIP]
	  ,RP.EmployeeName ResponsiblePerson
  FROM [dbo].[BOMDetailMaster] B
  LEFT JOIN dbo.EmployeeInformation RP ON RP.SystemId=B.ResponsiblePersonId) AS TEMP WHERE " + strkey + "";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.BOMDetailMaster where UserCode='" + data["UserCode"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from dbo.BOMDetailMaster where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from dbo.BOMDetailMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;
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

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[BOMSODetail] Where BOMDetailChild1Id IN(Select Id from BOMDetailChild1 Where BOMDetailMasterId='"+id+"')");
                con.executeQuery("delete from BOMDetailChild1 where BOMDetailMasterId='" + id + "'");
                con.executeQuery("delete from BOMDetailChild2 where BOMDetailMasterId='" + id + "'");
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        [HttpPost, Authorize]
        public ActionResult DeleteChild1(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[BOMSODetail] Where BOMDetailChild1Id IN('" + id + "')");
                con.executeQuery("delete from BOMDetailChild1 where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        [HttpPost, Authorize]
        public ActionResult DeleteChildSO(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[BOMSODetail] Where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        [HttpPost, Authorize]
        public ActionResult DeleteChild2(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from BOMDetailChild2 where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

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

        [Authorize, HttpGet]
        public ActionResult GetProductLibrary()
        {
            try
            {
                string sql = @"Select PL.Id,PL.Code,PL.ShortName,PL.StandardName, UserName=CASE WHEN PL.RecipeOrProductionGroup = 'Recipe' THEN RGM.UserName+' ('+PL.RecipeOrProductionGroup+')' ELSE PL.ProductionGroup+' ('+PL.RecipeOrProductionGroup+')' END
FROM dbo.ProductLibrary PL
LEFT JOIN[TRN].[RecipeGlobalMaster] RGM ON RGM.Id = PL.RecipeId WHERE PL.Active =1";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateChild1Data(Dictionary<string, object> data)

        {
            try
            {
                if (data != null)
                {

                    string _Id;
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("select * from dbo.BOMDetailChild1 where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("BOMDetailChild1", out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data update

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetChild1Data(string masterid)
        {
            string sql = @"Select BC.*,P.Code PartyCode,P.UserName PartyName, PL.Code,PL.UserName ProductCode from [dbo].[BOMDetailChild1] BC
LEFT JOIN HKP.Party P ON P.id=BC.CustomerId
LEFT JOIN dbo.ProductLibrary PL ON PL.id=BC.ProductCodeId
Where BOMDetailMasterId='" + masterid + "'";

            JsonResult json = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetSOData()
        {
            try
            {
                JsonResult json = Json(_productionSummaryData.GetSOData(), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateSOData(Dictionary<string, object> data)

        {
            try
            {
                if (data != null)
                {

                    string _Id;
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("select * from dbo.BOMSODetail where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("BOMSODetail", out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data update

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedSOData(string masterid)
        {
            string sql = @"select * from  BOMSODetail Where BOMDetailChild1Id='" + masterid + "'";

            JsonResult json = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpGet, Authorize]
        public ActionResult GetCostingItemData()
        {
            try
            {
                JsonResult json = Json(_productionSummaryData.GetCostingItemData(), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetFirstSKUCbo()
        {
            try
            {
                JsonResult json = Json(_productionSummaryData.GetFirstSKUCbo(), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetSecondSKUCbo()
        {
            try
            {
                JsonResult json = Json(_productionSummaryData.GetSecondSKUCbo(), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateChild2Data(Dictionary<string, object> data)

        {
            try
            {
                if (data != null)
                {

                    string _Id;
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("select * from dbo.BOMDetailChild2 where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("BOMDetailChild2", out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data update

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetChild2Data(string masterid)
        {
            string sql = @"Select B.*,CI.UserName CostingItem,C1.UserName SKU1,C2.UserName SKU2,MM.UserName MaterialMaster,MM.Code MaterialCode,MMA.Code ArticleCode,MMA.StandardName Article,P.UserName VendorName,P.Code VendorCode from dbo.BOMDetailChild2 B
LEFT JOIN HKP.CostingItem CI ON CI.Id=B.CostingItemId
LEFT JOIN HKP.CharacteristicsValue C1 ON C1.Id=B.FirstCharacteristicsValueId
LEFT JOIN HKP.CharacteristicsValue C2 ON C2.Id=B.SecondCharacteristicsValueId
LEFT JOIN MST.MaterialMaster MM ON MM.Id=B.MaterialMasterId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=B.ArticleId
LEFT JOIN HKP.Party P ON P.Id=B.VendorId
Where B.BOMDetailMasterId='" + masterid + "'";

            JsonResult json = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetSODataList(string masterid)
        {
            try
            {
                string sql = @"Select * from [dbo].[BOMSODetail] Where BOMDetailChild1Id IN(Select ID from BOMDetailChild1 Where BOMDetailMasterId='" + masterid + "')";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetBOMDetailData(string masterid)
        {
            try
            {
                string sql = @"SELECT C.CostingItemId,C.FirstCharacteristicsValueId,C.SecondCharacteristicsValueId,C.BOMMaterialDetail
,C.MaterialMasterId,C.ArticleId,C.VendorId,B.CustomerRefNo,B.VendorRefNo,B.OwnRefNo
,PB.Code PartyCode,PB.UserName PartyName, PL.Code,PL.UserName ProductCode
,CI.UserName CostingItem,C1.UserName SKU1,C2.UserName SKU2,MM.UserName MaterialMaster
,MM.Code MaterialCode,MMA.Code ArticleCode,MMA.StandardName Article,P.UserName VendorName,P.Code VendorCode
FROM [dbo].[BOMDetailMaster] A
LEFT JOIN [dbo].BOMDetailChild1 B ON B.BOMDetailMasterId=A.Id
LEFT JOIN [dbo].BOMDetailChild2 C ON C.BOMDetailMasterId=A.Id
LEFT JOIN HKP.CostingItem CI ON CI.Id=C.CostingItemId
LEFT JOIN HKP.CharacteristicsValue C1 ON C1.Id=C.FirstCharacteristicsValueId
LEFT JOIN HKP.CharacteristicsValue C2 ON C2.Id=C.SecondCharacteristicsValueId
LEFT JOIN MST.MaterialMaster MM ON MM.Id=C.MaterialMasterId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=C.ArticleId
LEFT JOIN HKP.Party P ON P.Id=C.VendorId
LEFT JOIN HKP.Party PB ON PB.id=B.CustomerId
LEFT JOIN dbo.ProductLibrary PL ON PL.id=B.ProductCodeId
WHERE A.Id='" + masterid + "'";
                JsonResult json = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}