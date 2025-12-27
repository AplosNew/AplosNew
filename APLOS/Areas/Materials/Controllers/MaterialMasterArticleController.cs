using Aplos.Areas.Setups.Controllers;
using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

            var jsondata = Json(materialCommonService.GetMaterialMasterWithArticlePopUpData(column, value, identity.CompanyGroupId, type), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public JsonResult GetMaterialMasterWithArticleDataByProductMaster(string column, string value, string ProductMasterId)
        {
            MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var jsondata = Json(materialCommonService.GetMaterialMasterWithArticleDataByProductMaster(column, value, identity.CompanyGroupId, ProductMasterId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [HttpPost, Authorize]
        public JsonResult GetMaterialMasterWithArticleForProcessConstraintPopUpData(string column, string value, string type)
        {
            MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var jsondata = Json(materialCommonService.GetMaterialMasterWithArticleForProcessConstraintPopUpData(column, value, identity.CompanyGroupId, type), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public JsonResult GetSavedMaterialMasterWithArticlerProcessConstraintPopUpData(string column, string value, string type,string processConstraintId)
        {
            MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var jsondata = Json(materialCommonService.GetSavedMaterialMasterWithArticlerProcessConstraintPopUpData(column, value, identity.CompanyGroupId, type, processConstraintId), JsonRequestBehavior.AllowGet);
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
        public JsonResult CreateMOIArticleAlias(Dictionary<string, object> data)
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

        [HttpPost, Authorize]
        public JsonResult CreateArticleAlias(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM ArticleAlias where ArticleId='" + data["ArticleId"] + "'", out dsMaster, false, "1");

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

        #region Parameter
        string ParameterTableName = "[HKP].[ArticleParameterType]";

        [HttpPost]
        public ActionResult GetParameterList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + ParameterTableName + ") AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetParameterAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetParameterCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + ParameterTableName + ""), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreateParameter(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + ParameterTableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + ParameterTableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + ParameterTableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(ParameterTableName, out _Id);

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

                return Json(new { Error = false, Data = data, Sequence = GetParameterSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult DeleteParameter(string id)
        {
            string sql = @"select * from '" + ParameterTableName + "' where Id = '" + id + "'";

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + ParameterTableName + " where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetParameterSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        private double GetParameterSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + ParameterTableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        #endregion
        #region Parameter Upload
        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = GetSampleFileServiceMaster(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Article Parameter Data upload Sample File";

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


        public DataTable GetServiceMasterGLData()
        {
            var cmdText = @"SELECT APU.*,APT.UserName ArticleParameterType,MMA.StandardName Article 
                            FROM [HKP].[ArticleParameterUpload] APU
                            LEFT JOIN [HKP].[ArticleParameterType] APT ON APT.Id=APU.ArticleParameterTypeId
                            LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id=APU.ArticleId";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetSampleFileServiceMaster(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName)
        {
            #region declare
            OTSBD.clsReport objRpt = null;
            OTSBD.clsStaticInfo objStatic = null;
            objStatic = new OTSBD.clsStaticInfo();
            string OTConsiderOn = string.Empty;

            #endregion
            try
            {
                ReportUtility ru = new ReportUtility();

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = ru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new OTSBD.clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;

                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];
                xlsRow = 1;

                #region ------------------Column Header------------------


                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ArticleParameterTypeId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20; int colArticleParameterTypeId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ArticleId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 11; int colArticleId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sequence"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15; int colSequence = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "StandardValue"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15; int colStandardValue = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IntermidiateValue"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20; int colIntermidiateValue = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IsProduction"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 17; int colIsProduction = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remark"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30; int colRemark = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ArticleParameterType"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 22; int colArticleParameterType = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Article"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20; int colArticle = xlsCol; xlsCol += 1;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                xlsRow++;

                sheet1.Range[xlsRow, colIsProduction, xlsRow, colIsProduction].DataValidation.AllowType = ExcelDataType.Integer;
                //sheet1.Range[xlsRow, colSalesApplicable, xlsRow, colSalesApplicable].DataValidation.AllowType = ExcelDataType.Integer;
                //sheet1.Range[xlsRow, colIndependentApplicable, xlsRow, colIndependentApplicable].DataValidation.AllowType = ExcelDataType.Integer;

                #endregion ------------------Column Header------------------

                DataTable dtData = GetServiceMasterGLData();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    sheet1[xlsRow, colArticleParameterTypeId].Text = dtData.Rows[i]["ArticleParameterTypeId"].ToString();
                    sheet1[xlsRow, colArticleId].Text = dtData.Rows[i]["ArticleId"].ToString();
                    sheet1[xlsRow, colSequence].Text = dtData.Rows[i]["Sequence"].ToString();
                    if (dtData.Rows[i]["StandardValue"].ToString() == null)
                    {
                        sheet1[xlsRow, colStandardValue].Text = "0";
                    }
                    else
                    {
                        sheet1[xlsRow, colStandardValue].Text = dtData.Rows[i]["StandardValue"].ToString();
                    }
                    if (dtData.Rows[i]["IntermidiateValue"].ToString() == null)
                    {
                        sheet1[xlsRow, colIntermidiateValue].Text = "0";
                    }
                    else
                    {
                        sheet1[xlsRow, colIntermidiateValue].Text = dtData.Rows[i]["IntermidiateValue"].ToString();
                    }
                    if (dtData.Rows[i]["IsProduction"].ToString() == "False")
                    {
                        sheet1[xlsRow, colIsProduction].Text = "0";
                    }
                    else
                    {
                        sheet1[xlsRow, colIsProduction].Text = "1";
                    }
                    
                    sheet1[xlsRow, colRemark].Text = dtData.Rows[i]["Remark"].ToString();
                    sheet1[xlsRow, colArticleParameterType].Text = dtData.Rows[i]["ArticleParameterType"].ToString();
                    sheet1[xlsRow, colArticle].Text = dtData.Rows[i]["Article"].ToString();

                    xlsRow++;
                }


                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Sheet1";
                #endregion Page Setup

                #endregion  Lunch Out

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult ImportData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<ArticleParameterUploadedDataViewModel> data = new List<ArticleParameterUploadedDataViewModel>();

                var file = Request.Files["file"];

                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {

                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
                    try
                    {
                        string connString = string.Empty;
                        ExcelEngine excelEngine = null;
                        IApplication application = null;
                        IWorkbook workbook = null;

                        excelEngine = new ExcelEngine();
                        application = excelEngine.Excel;
                        workbook = excelEngine.Excel.Workbooks.Open(path);

                        DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                        DataSet dsExcel = new DataSet();
                        dsExcel.Tables.Add(dt);


                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            exception += "\r\nTrying to delete";
                            docFile.Delete();
                        }

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                ArticleParameterUploadedDataViewModel vm = new ArticleParameterUploadedDataViewModel();

                                vm.ArticleParameterTypeId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.ArticleId = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                vm.Sequence = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                vm.StandardValue = dsExcel.Tables[0].Rows[i][3].ToString().Trim();
                                vm.IntermidiateValue = dsExcel.Tables[0].Rows[i][4].ToString().Trim();
                                vm.IsProduction = dsExcel.Tables[0].Rows[i][5].ToString().Trim();
                                vm.Remark = dsExcel.Tables[0].Rows[i][6].ToString().Trim();

                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
                        }
                    }
                    catch (Exception ex)
                    {

                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            docFile.Delete();
                        }
                        throw (ex);
                    }

                }
                catch (Exception ex)
                {
                    //throw ex;
                }
                finally
                {
                }
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveUploadedData(List<Dictionary<string, object>> data)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsBC, dsDD;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                string strSQL = "Delete FROM [HKP].[ArticleParameterUpload]";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [HKP].[ArticleParameterUpload] where 1=2", out dsBC, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + Convert.ToInt64(item["Id"]) + "'";

                        if (dv.Count == 0)
                        {
                            if (item["ArticleParameterTypeId"] == null || item["ArticleParameterTypeId"] == "")
                            {
                                item["ArticleParameterTypeId"] = null;
                            }
                            if (item["ArticleId"] == null || item["ArticleId"] == "")
                            {
                                item["ArticleId"] = null;
                            }

                            AddNewRow(dsBC.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }


                }
                #endregion
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsBC);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


     
        private class ArticleParameterUploadedDataViewModel
        {
            public string ArticleParameterTypeId { get; set; }
            public string ArticleId { get; set; }
            public string Sequence { get; set; }
            public string StandardValue { get; set; }
            public string IntermidiateValue { get; set; }
            public string IsProduction { get; set; }
            public string Remark { get; set; }

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

        [HttpPost,Authorize]
        public JsonResult UpdateArticle(List<Dictionary<string, object>> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsArticle;
            string id = "";
            try
            {
                foreach (var item in data)
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }

                    objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("Select * From MST.MaterialMasterArticle Where Id IN ("+id+")", out dsArticle, false, "1");
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsArticle.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        
                        if (dv.Count > 0)
                        {
                            
                            DataRow drmo = dv[0].Row;
                            item["ProcessConstraintId"] = masterId;
                            EditRow(drmo, item);
                        }
                    }
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsArticle);
                return Json(new { Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion
    }
}