#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Materials;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Accounts.Controllers
{
    public class GeneralAccountDeterminateController : BaseController
    {
        string TableName = "hkp.GeneralAccountDeterminate";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IMaterialMasterService _materialMasterService;

        public GeneralAccountDeterminateController(IMaterialMasterService materialMasterService, ISqlRepository R)
        {
            _materialMasterService = materialMasterService;
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GlControl()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetList(string column, string value, string COAId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT TOP 100 * FROM (
                            SELECT GAD.*,C.UserName COA,(GL.AccountCode +'-'+ GL.UserName) GLGeneralInfo,B.UserName Budget,A.UserName Activity
                            FROM [HKP].[GeneralAccountDeterminate] GAD
                            LEFT JOIN [HKP].[COA] C ON C.Id=GAD.COAId
                            LEFT JOIN [HKP].[GLGeneralInfo] GL ON GL.Id=GAD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] BM ON BM.Id=GAD.BudgetMasterId
                            LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                            LEFT JOIN [HKP].[Activity] A ON A.Id=GAD.ActivityId
                            WHERE GAD.COAId='" + COAId + @"'
                            ) AS TEMP WHERE " + strkey + "";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {


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


                return Json(new { Error = false, Message = AplosMessage.Updated });

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
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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

        #region GL Control
        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [MST].[GLControlMaster]"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CreateGlControl(Dictionary<string, object> data,string materialId, List<Dictionary<string, object>> materialList)
        {
            try
            {
                DataSet dsMaster, dsMaterial;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [MST].[GLControlMaster] where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from [MST].[GLControlMaster] where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from [MST].[GLControlMaster] where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "' AND  Sequence<>'" + data["Sequence"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Sequence already exists!!!");

                con.OpenDataSetThroughAdapter("select * from [MST].[GLControlMaster] where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string materialsql = "SELECT * FROM [MST].[MaterialMaster] WHERE Id  in ('" + materialId + "')";
                con.OpenDataSetThroughAdapter(materialsql, out dsMaterial, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("[MST].[GLControlMaster]", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                #region Material
                #region data update

                if (materialList != null)
                {
                    foreach (var scitem in materialList)
                    {
                        DataView dvsc = new DataView(dsMaterial.Tables[0]);
                        dvsc.RowFilter = "Id='" + scitem["MaterialMasterId"] + "'";

                        if (dvsc.Count > 0)
                        {
                            DataRow drmo = dvsc[0].Row;
                            drmo.BeginEdit();
                            drmo["GLControlMasterId"] = _Id; 
                           
                            drmo.EndEdit();
                        }
                    }
                }
             
                #endregion data update
                #endregion Material
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsMaterial);

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM [MST].[GLControlMaster]");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        public ActionResult DeleteGlControl(string id)
        {
            string sqlChild = @"select * from [MST].[GLControlDetail] where GLControlId = '" + id + "'";
            string sql = @"select * from [MST].[GLControlMaster] where Id = '" + id + "'";
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [MST].[GLControlDetail] where GLControlId='" + id + "'");
                con.executeQuery("delete from [MST].[GLControlMaster] where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetGlControlList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM [MST].[GLControlMaster]) AS TEMP WHERE " + strkey + " order by sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion GL Control


        [HttpGet, Authorize]
        public JsonResult GetMaterialList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult selectIDs(string materialType, string materialGroup, string materialMasterId)
        {
            try
            {
                var sql = @"select mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
                             mm.Id as MaterialMasterId,
                             mt.Id as MaterialTypeId, mgm.Id as MaterialGroupMasterId, bah.Id
                            from MST.MaterialMaster mm
                            left join mst.MaterialGroupMaster mgm on mgm.Id = mm.MaterialGroupMasterId	
                            left join hkp.materialtype mt on mt.Id =  mgm.materialtypeid                                                       
                            left join trn.BinAllocationHead bah on bah.MaterialMasterId = mm.Id
                             where mt.Id = '" + materialType + "' and mgm.Id = '" + materialGroup + "'" +
                             "and mm.Id = '" + materialMasterId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                //return Json(sba.selectIDs(materialType, materialGroup, material, storagelevel), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult GetMaterialData(string glControlDetailId)
        {
            try
            {
                var sql = @"select username as MaterialMaster,Id as MaterialMasterId,GLControlMasterId
                            from MST.MaterialMaster
							where GLControlMasterId = '" + glControlDetailId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult GetExpenseGLData(string glId, string budgetId, string activityId)
        {
            try
            {
                var sql = @"SELECT  distinct GLGI.Id AS GLGeneralInfoId,'' Id,GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , B.BudgetMasterId, B.RefNo, B.BudgetCode, B.BudgetName
									, A.ActivityId, A.ActivityCode, A.ActivityName--, GLTY.AccountType
                                    FROM [HKP].[GLGeneralInfo] AS GLGI
                                    LEFT JOIN [HKP].[GLCompanyGroup] AS GLCG ON GLCG.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLAccountType] AS GLTY ON GLTY.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN (SELECT BM.Id AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, BM.GLGeneralInfoId, BM.RefNo
	                                    FROM [HKP].[Budget] AS B
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.BudgetId=B.Id
                                    ) AS B ON B.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN (SELECT A.Id AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, BA.BudgetMasterId
	                                    FROM [HKP].[Activity] AS A
	                                    LEFT JOIN [MST].[BudgetMasterActivity] AS BA ON BA.ActivityId=A.Id
                                    ) AS A ON A.BudgetMasterId=B.BudgetMasterId
                             where GLGI.Id = '" + glId + "' and b.BudgetMasterId = '" + budgetId + "'" +
                             "and a.ActivityId = '" + activityId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CreateComsumable(List<Dictionary<string, object>> ConsumableList, string glControlId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [MST].[GLControlDetail] where GLControlId='" + glControlId + "'", out dsMaster, false, "1");

                string _Id = "";
                #region data update
                foreach (var item in ConsumableList)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("[MST].[GLControlDetail]", out _Id);

                        item["Id"] =_Id;
                        item["GLControlId"] = glControlId;
                        AddNewRow(dsMaster.Tables[0], item);
                    }
                    else
                    {
                        _Id = item["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], item);
                    }
                }
                #endregion data update
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

       
        [Authorize]
        public ActionResult GetConsumableData(string glControlDetailId)
        {
            try
            {
                var sql = @"select GLCD.Id,GLCD.GLGeneralInfoId,glg.UserName GLGeneralInfoName,GLCD.BudgetId
						,B.UserName BudgetName,GLCD.ActivityId,A.UserName ActivityName
						from mst.GLControldetail GLCD
						left join [HKP].[GLGeneralInfo] glg on glg.Id=GLCD.GLGeneralInfoId
						left join [HKP].[Budget] B on B.Id=GLCD.BudgetId
						left join [HKP].[Activity] A on A.Id=GLCD.ActivityId
						where GLCD.GLControlId= '" + glControlDetailId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdateMaterial(Dictionary<string, object> materialList,string materialId)
        {
            DataSet dsMaterial;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

            string materialsql = "SELECT * FROM [MST].[MaterialMaster] WHERE Id  in ('" + materialId + "')";
            con.OpenDataSetThroughAdapter(materialsql, out dsMaterial, false, "1");
            #region Material
            #region data update

            if (materialList != null)
            {
                    DataView dvsc = new DataView(dsMaterial.Tables[0]);

                    if (dvsc.Count > 0)
                    {
                        DataRow drmo = dvsc[0].Row;
                        drmo.BeginEdit();
                        drmo["GLControlMasterId"] = null;
                        drmo.EndEdit();
                    }
            }

            #endregion data update
            #endregion Material
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsMaterial);

            return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult DeleteConsumerable(string id)
        {
            string sql = @"select * from [MST].[GLControlDetail] where Id = '" + id + "'";
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [MST].[GLControlDetail] where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}