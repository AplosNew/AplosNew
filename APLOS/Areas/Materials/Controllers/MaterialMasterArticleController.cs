using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Materials;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialMasterArticleController : BaseController
    {
        #region -- Constructor

        private readonly IMaterialMasterArticleService _baseService;
        private readonly IMaterialMasterAttributeValueService _valueService;
        private readonly ISqlRepository _sqlRepository;

        public MaterialMasterArticleController(IMaterialMasterArticleService baseService, IMaterialMasterAttributeValueService valueService, ISqlRepository sqlRepository)
        {
            _baseService = baseService;
            _valueService = valueService;
            _sqlRepository = sqlRepository;
        }

        #endregion -- Constructor

        #region Pages


        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult ProcessConstraint()
        {
            return View();
        }


        #endregion Pages

        #region List

        [HttpGet, Authorize]
        public JsonResult GetList(string materialMasterId)
        {
            return Json(_baseService.Query(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialArticle(GridParameter parameters, string materialMasterId)
        {
            return Json(_baseService.GetMaterialArticle(parameters, materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetMaterialArticle(string materialMasterId, string materialType)
        {
            return Json(_baseService.GetMaterialArticle(materialMasterId, new JavaScriptSerializer().Deserialize<string[]>(materialType)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialArticleValue(string articleId)
        {
            return Json(_baseService.GetMaterialArticleValue(articleId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// use : Product definition,bulletin
        /// </summary>
        /// <param name="materialMasterId"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public JsonResult GetArticlListByMaterialMaster(string materialMasterId)
        {
            return Json(_baseService.GetArticlListByMaterialMaster(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAttributeValueList(GridParameter parameters, string assignment, string materialMasterId, string attributeId)
        {
            return Json(_valueService.GetAttributeValueList(parameters, assignment, materialMasterId, attributeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetArticlValueHead(string materialMasterId)
        {
            return Json(_baseService.GetArticlValueHead(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetArticleValueList(string materialMasterId)
        {
            return Json(_baseService.GetAttributeValueList(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetMaterialMasterWithArticlePopUpData(string column, string value, string type)
        {
            MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           // return Json(materialCommonService.GetMaterialMasterWithArticlePopUpData(column, value, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);

            var jsondata = Json(materialCommonService.GetMaterialMasterWithArticlePopUpData(column, value, identity.CompanyGroupId, type), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public JsonResult GetMaterialArticlePopUpData(string column, string value, string type)
        {
            MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // return Json(materialCommonService.GetMaterialMasterWithArticlePopUpData(column, value, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);

            var jsondata = Json(materialCommonService.GetMaterialArticlePopUpData(column, value), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #endregion List

        #region -- Operations
        [HttpPost, Authorize]
        public JsonResult Comapre(List<MaterialMasterArticleNew> allArticles, List<MaterialMasterArticleValue> currentArticles)
        {
            try
            {
                _baseService.Comapare(allArticles, currentArticles);
                return Json(new { Error = false, Message = "" });
            }
            catch (System.Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        //[HttpPost]
        //public JsonResult Create(IEnumerable<MaterialMasterArticle> articles, string materialCode)
        //{
        //    _baseService.InsertOrUpdateGraph(articles, materialCode);
        //    return Json(new { Message = AplosMessage.Insert });
        //}

        [HttpPost]
        public JsonResult Create(string articles, string materialCode)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<MaterialMasterArticle> article = JsonConvert.DeserializeObject<List<MaterialMasterArticle>>(articles, settings);

            _baseService.InsertOrUpdateGraph(article, materialCode);
            return Json(new { Message = AplosMessage.Insert });
        }


        [HttpPost, Authorize]
        public JsonResult Delete(string id)
        {
            _baseService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost]
        public JsonResult DeleteMaster(string id)
        {
            _baseService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult XCreateArticleAlias(Dictionary<string, object> datas)
        {
            try
            {
                SaveArticleAlias(datas);

                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public void SaveArticleAlias(Dictionary<string, object> datas)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM ArticleAlias WHERE Id ='" + datas["Id"] + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ArticleAlias", out sID);
                    dr["Id"] = sID;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateArticleAlias(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM ArticleAlias where ArticleId='" + data["ArticleId"] + "' AND MasterOrderItemId='" + data["MasterOrderItemId"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ArticleAlias", out _Id);

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


                return Json(new { Error = false, Id = _Id, Message = AplosMessage.Updated });

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


        [HttpGet, Authorize]
        public ActionResult getArticleAliaslist(string articleId, string masterOrderItemId)
        {
            try
            {
                return Json(_baseService.getArticleAliaslist(articleId, masterOrderItemId), JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult deleteArticleAliasData(string Id)
        {
            _baseService.deleteArticleAliasData(Id);
            return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations

        #region ProductionGrouping
        string TableName = "[HKP].[ProductionGrouping]";

        [HttpPost]
        public ActionResult GetPGList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetProductionGroupingCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreateProductionGrouping(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

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

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult DeleteProductionGrouping(string id)
        {
            string sql = @"select * from '" + TableName + "' where Id = '" + id + "'";

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }
      
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        #endregion

        #region ProcessConstraint
        string TableName1 = "[HKP].[ProcessConstraint]";

        [HttpPost]
        public ActionResult GetPCList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName1 + ") AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoPCSequence()
        {
            return Json(GetPCSequence(), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult CreateProcessConstraint(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where ProcessId='" + data["ProcessId"] + "' AND  Id<>'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Process already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName1, out _Id);

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

                return Json(new { Error = false, Data = data, Sequence = GetPCSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult DeleteProcessConstraint(string id)
        {
            string sql = @"select * from '" + TableName1 + "' where Id = '" + id + "'";

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetPCSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        private double GetPCSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName1 + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        #endregion

        #region ProcessConstraintValue
        string TableName2 = "[HKP].[ProcessConstraintValue]";

        [HttpPost,Authorize]
        public ActionResult GetPCVList(string masterId)
        {
            string sql = @"SELECT * FROM " + TableName2 + "  Where ProcessConstraintId='"+ masterId + "' order by sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoPCVSequence(string masterId)
        {
            return Json(GetPCVSequence(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateProcessConstraintValue(Dictionary<string, object> data,string masterId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "' AND ProcessConstraintId='"+masterId+"'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "' AND ProcessConstraintId='" + masterId + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where Id='" + data["Id"] + "' AND ProcessConstraintId='" + masterId + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName2, out _Id);

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

                return Json(new { Error = false, Data = data, Sequence = GetPCVSequence(masterId), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult DeleteProcessConstraintValue(string id,string masterId)
        {
            string sql = @"select * from '" + TableName2 + "' where Id = '" + id + "'";

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName2 + " where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetPCVSequence(masterId), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        private double GetPCVSequence(string masterId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName2 + " Where ProcessConstraintId='"+masterId+"'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        #endregion
    }
}