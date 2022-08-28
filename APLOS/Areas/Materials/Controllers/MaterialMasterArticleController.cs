using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
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

        public MaterialMasterArticleController(IMaterialMasterArticleService baseService, IMaterialMasterAttributeValueService valueService)
        {
            _baseService = baseService;
            _valueService = valueService;
        }

        #endregion -- Constructor

        #region Pages

      
        public ActionResult Aplos()
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

        #endregion List

        #region -- Operations
        [HttpPost,Authorize]
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

        [HttpPost,Authorize]
        public ActionResult CreateArticleAlias(Dictionary<string, object> datas)
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

        #endregion -- Operations
    }
}