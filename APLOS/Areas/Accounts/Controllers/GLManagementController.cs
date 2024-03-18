#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Materials;
using Library.Service.Setups;
using Library.Service.Systems;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Accounts.Controllers
{
    public class GLManagementController : BaseController
    {
        string TableName = "hkp.GeneralAccountDeterminate";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IPKGeneratorService _pkGeneratorService;
        public GLManagementController(IMaterialMasterService materialMasterService, ISqlRepository R, IPKGeneratorService pkGeneratorService)
        {
            _materialMasterService = materialMasterService;
            _sqlRepository = R;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor

 
        public ActionResult GLManagement()
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

        [HttpPost, Authorize]
        public JsonResult CreateGlManagementHeader(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1"); 
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagement] where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                  
                string _detaliId = null; 
                string _Id = "";
                bplib.clsGenID genid = new bplib.clsGenID();

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenerateIDYearly(DateTime.Now.ToString(), "GLManagement", out _Id);

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
 
        [HttpPost, Authorize]
        public JsonResult CreateGlManagementEmployeeCategory(Dictionary<string, object> data, string GlManagementId)
        {
            try
            {
                DataSet dsEmpCat;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementEmployeeCategory] where Id='" + data["Id"] + "'", out dsEmpCat, false, "1");
                 
                string Id = "";

                #region data update
                if (dsEmpCat.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementEmployeeCategory", out Id);

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    DataRow dr;
                    dr = dsEmpCat.Tables[0].NewRow();

                    dr["Id"] = Id;
                    dr["EmployeeCategoryId"] = data["EmployeeCategoryId"]; 
                    dr["GlManagementId"] = GlManagementId; 

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsEmpCat.Tables[0].Rows.Add(dr); 
                }
                else
                {
                    Id = data["Id"].ToString();
                    EditRow(dsEmpCat.Tables[0].Rows[0], data);
                }
                
                #endregion data update 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpCat); 
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated }); 
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }); 
            }
        }
        [HttpPost, Authorize]
        public JsonResult CreateGlManagementDesignation(Dictionary<string, object> data, string GlManagementId)
        {
            try
            {
                DataSet dsEmpCat;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementDesignation] where Id='" + data["Id"] + "'", out dsEmpCat, false, "1");

                string Id = "";

                #region data update
                if (dsEmpCat.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementDesignation", out Id);

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    DataRow dr;
                    dr = dsEmpCat.Tables[0].NewRow();

                    dr["Id"] = Id;
                    dr["DesignationId"] = data["DesignationId"];
                    dr["GlManagementId"] = GlManagementId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsEmpCat.Tables[0].Rows.Add(dr);
                }
                else
                {
                    Id = data["Id"].ToString();
                    EditRow(dsEmpCat.Tables[0].Rows[0], data);
                }

                #endregion data update 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpCat);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public JsonResult CreateGlManagementPositionCode(Dictionary<string, object> data, string GlManagementId)
        {
            try
            {
                DataSet dsEmpCat;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementPositionCode] where Id='" + data["Id"] + "'", out dsEmpCat, false, "1");

                string Id = "";

                #region data update
                if (dsEmpCat.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementPositionCode", out Id);

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    DataRow dr;
                    dr = dsEmpCat.Tables[0].NewRow();

                    dr["Id"] = Id;
                    dr["PositionCodeId"] = data["PositionCodeId"];
                    dr["GlManagementId"] = GlManagementId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsEmpCat.Tables[0].Rows.Add(dr);
                }
                else
                {
                    Id = data["Id"].ToString();
                    EditRow(dsEmpCat.Tables[0].Rows[0], data);
                }

                #endregion data update 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpCat);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public JsonResult CreateGlManagementBudgetCode(Dictionary<string, object> data, string GlManagementId)
        {
            try
            {
                DataSet dsEmpCat;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementPositionCode] where Id='" + data["Id"] + "'", out dsEmpCat, false, "1");

                string Id = "";

                #region data update
                if (dsEmpCat.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementPositionCode", out Id);

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    DataRow dr;
                    dr = dsEmpCat.Tables[0].NewRow();

                    dr["Id"] = Id;
                    dr["PositionCodeId"] = data["PositionCodeId"];
                    dr["GlManagementId"] = GlManagementId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsEmpCat.Tables[0].Rows.Add(dr);
                }
                else
                {
                    Id = data["Id"].ToString();
                    EditRow(dsEmpCat.Tables[0].Rows[0], data);
                }

                #endregion data update 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpCat);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
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
        public ActionResult GetGlManagementList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM [HKP].[GLManagement]) AS TEMP WHERE " + strkey + " order by sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion GL Control


        [HttpGet, Authorize]
        public JsonResult GetMaterialList(GridParameter parameters, string GlControlId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.MaterialQueryForGLControl(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
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
        public ActionResult GetMaterialData(string glManagementId)
        {
            try
            {
                var sql = @"select ec.UserName as EmployeeCategory,glmec.EmployeeCategoryId as EmployeeCategoryId
                            from [HKP].[GLManagementEmployeeCategory] glmec 
                            left join [HKP].[EmployeeCategory] ec on ec.Id=glmec.EmployeeCategoryId
							where glmec.GLManagementId = '" + glManagementId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize]
        public ActionResult GetDesignationData(string glManagementId)
        {
            try
            {
                var sql = @"select D.UserName as Designation,GLMD.DesignationId
                            from [HKP].[GLManagementDesignation] GLMD 
                            left join [HKP].[Designation] D on D.Id=GLMD.DesignationId
							where GLMD.GLManagementId = '" + glManagementId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize]
        public ActionResult GetPositionCodeData(string glManagementId)
        {
            try
            {
                var sql = @"select P.Code,P.UserName as Position,PC.PositionCodeId
                            from [HKP].[GLManagementPositionCode] PC
                            left join [ORG].[Position] P on P.Id=PC.PositionCodeId
							where PC.GLManagementId = '" + glManagementId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize]
        public ActionResult GetBudgetCodeData(string glManagementId)
        {
            try
            {
                var sql = @"select MPB.Code,MPB.UserName as BudgetCode,BC.BudgetCodeId
                            from [HKP].[GLManagementBudgetCode] BC
                            left join [MST].[ManpowerBudget] MPB on MPB.Id=BC.BudgetCodeId
							where PC.GLManagementId = '" + glManagementId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetPositionCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select P.Id,P.Code,P.UserName Position,P.Activity,
                        DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection
                        from ORG.Position P	
                        LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
                        LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
                        LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
                        left outer join MST.DesignationMaster DM ON DM.DesignationId=P.DesignationId
                        where P.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
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

        [Authorize]
        public ActionResult GetConsumableData(string glControlDetailId, string type)
        {
            try
            {
                var sql = @"select GLCD.Id,GLCD.GLGeneralInfoId,glg.UserName GLGeneralInfoName,GLCD.BudgetMasterId
						,B.UserName BudgetName,GLCD.ActivityId,A.UserName ActivityName
						from mst.GLControldetail GLCD
						left join [HKP].[GLGeneralInfo] glg on glg.Id=GLCD.GLGeneralInfoId
						left join mst.BudgetMaster BM on BM.Id=GLCD.BudgetMasterId
						left join HKP.Budget B on B.Id=BM.BudgetId
						left join [HKP].[Activity] A on A.Id=GLCD.ActivityId
						where GLCD.GLControlId= '" + glControlDetailId + "' and GLCD.Type = '" + type + "'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdateMaterial(Dictionary<string, object> materialList, string materialId)
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
        [HttpPost]
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


        [HttpPost, Authorize]
        public ActionResult GetGLControlReport(string glControlId)
        {
            try
            {
                string fileName = "";
                fileName = GLControlReport(glControlId, "GL Control Report");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string GLControlReport(string glControlId, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "Material";
                sheet = workbook.Worksheets[0];

                DataTable headerData, data, typeData;
                GLControlHeaderSql(glControlId, out headerData);
                GlControlReportSQL(glControlId, out data);
                GlControlTypeReportSQL(glControlId, out typeData);


                if (headerData.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }
                int ROW = 6; int COL = 1;
                sheet.Range[ROW, COL].Text = "Sequence";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["Sequence"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;

                sheet.Range[ROW, COL].Text = "Code";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["Code"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;

                sheet.Range[ROW, COL].Text = "User Defined Name";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["UserName"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;

                sheet.Range[ROW, COL].Text = "Description";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["Description"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();

                sheet.Range[6, 1, ROW, 3].CellStyle.Font.Bold = true;
                sheet.Range[6, 1, ROW, 3].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[6, 1, ROW, 3].BorderInside(ExcelLineStyle.Hair);


                ROW = 6; COL = 4;
                sheet.Range[ROW, COL].Text = "Short Name";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["ShortName"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;

                sheet.Range[ROW, COL].Text = "Standard Name";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["StandardName"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;

                sheet.Range[ROW, COL].Text = "Remarks";
                sheet.Range[ROW, COL + 1].Text = headerData.Rows[0]["Remarks"].ToString();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();

                sheet.Range[6, 4, ROW, 6].CellStyle.Font.Bold = true;
                sheet.Range[6, 4, ROW, 6].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[6, 4, ROW, 6].BorderInside(ExcelLineStyle.Hair);

                ROW = 10; COL = 1;
                ROW++;

                #region Material

                #region columns
                sheet[ROW, COL].Text = "Material Type";
                sheet[ROW, COL].ColumnWidth = 18;
                int ColMaterialType = COL;
                COL++;

                sheet[ROW, COL].Text = "Material Group";
                sheet[ROW, COL].ColumnWidth = 22;
                int ColMaterialGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "Material Code";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColMaterialCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Material Name";
                sheet[ROW, COL].ColumnWidth = 22;
                int ColMaterialName = COL;

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;


                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColMaterialType].Text = data.Rows[i]["Type"].ToString();
                    sheet[ROW, ColMaterialGroup].Text = data.Rows[i]["MaterialGroup"].ToString();
                    sheet[ROW, ColMaterialCode].Text = data.Rows[i]["MaterialCode"].ToString();
                    sheet[ROW, ColMaterialName].Text = data.Rows[i]["MaterialName"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();
                #endregion Material


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "GL Control Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[1, 1, 6, endsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                #region Material Type

                workbook.Worksheets[1].Name = "Material Type";
                sheet = workbook.Worksheets[1];

                ROW = 6; COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Material Type";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColMaterialTypes = COL;
                COL++;

                sheet[ROW, COL].Text = "GL Code";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColGLCode = COL;
                COL++;

                sheet[ROW, COL].Text = "GL";
                sheet[ROW, COL].ColumnWidth = 20;
                int ColGL = COL;
                COL++;

                sheet[ROW, COL].Text = "Budget";
                sheet[ROW, COL].ColumnWidth = 18;
                int ColBudget = COL;
                COL++;

                sheet[ROW, COL].Text = "Activity";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColActivity = COL;
                COL++;

                sheet[ROW, COL].Text = "BudgetRefNo";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColBudgetRefNo = COL;

                #endregion columns

                int endsCol = COL;
                sheet.Range[ROW, 1, ROW, endsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endsCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endsCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endsCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endsCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                startRow = ROW;

                for (int i = 0; i < typeData.Rows.Count; i++)
                {
                    sheet[ROW, ColMaterialTypes].Text = typeData.Rows[i]["MaterialType"].ToString();
                    sheet[ROW, ColGLCode].Text = typeData.Rows[i]["GLGeneralInfoCode"].ToString();
                    sheet[ROW, ColGL].Text = typeData.Rows[i]["GL"].ToString();
                    sheet[ROW, ColBudget].Text = typeData.Rows[i]["BudgetName"].ToString();
                    sheet[ROW, ColActivity].Text = typeData.Rows[i]["Activity"].ToString();
                    sheet[ROW, ColBudgetRefNo].Text = typeData.Rows[i]["BudgetRefNo"].ToString();

                    sheet.Range[ROW, 1, ROW, endsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endsCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "GL Control Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                #endregion Material Type

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GLControlHeaderSql(string glControlId, out DataTable headerData)
        {
            try
            {
                string strSQL = @"select * from mst.GLControlMaster
							where Id ='" + glControlId + @"'";
                headerData = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void GlControlReportSQL(string glControlId, out DataTable data)
        {
            try
            {
                string strSQL = @"select MT.UserName [Type],MGM.UserName MaterialGroup,MM.Code MaterialCode,MM.UserName MaterialName
                                                from mst.MaterialMaster MM
												left join mst.GLControlMaster GLM on MM.GLControlMasterId=GLM.Id
                                                left join MST.MaterialGroupMaster MGM on MGM.Id=MM.MaterialGroupMasterId
                                                left join hkp.materialType MT on MT.Id=MGM.MaterialTypeId
							                    where MM.GLControlMasterId = '" + glControlId + @"'";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void GlControlTypeReportSQL(string glControlId, out DataTable typeData)
        {
            try
            {
                string strSQL = @"select GLD.[Type] MaterialType,GL.AccountCode AS GLGeneralInfoCode,GL.UserName GL,B.BudgetName,A.UserName Activity,B.BudgetRefNo 
                                                    from  MST.GLControlDetail GLD 
                                                    left join [HKP].[GLGeneralInfo] GL on GL.Id=GLD.GLGeneralInfoId
                                                    left join [HKP].[Activity] A on A.Id=GLD.ActivityId
                                                    LEFT JOIN (SELECT BM.Id AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, BM.RefNo BudgetRefNo
	                                                    FROM [HKP].[Budget] AS B
                                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.BudgetId=B.Id
                                                    ) AS B ON B.BudgetMasterId=GLD.BudgetMasterId
							                        where GLD.GLControlId ='" + glControlId + @"'";

                typeData = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }
}