using Library.Model.IE;
using Aplos.Properties;
using Library.Data;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using Library.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using Library.Service.IE;
using Library.Model.Inventory;
using Library.Service.Systems;
using Library.Service.Enums;
using Library.Service.Securites;
using System.Data;
using Library.Security.Core;
using Library.Data.Sql;
using Library.Service.Logs;
using System.Reflection;

namespace Aplos.Areas.IE.Controllers
{
    public class MachineMasterUIController : Controller
    {
        #region Constructor


        private readonly IUserService _userService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IOperationMasterService _operationMasterService;
        private readonly IMachineMasterUIService _machineMasterUIService;
        private readonly IOperationService _operationService;
        private readonly IOperationVariationService _operationStepService;

        private readonly IOperationTimeCaptureMasterService _ioperationtimecaptureservice;
        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;
        private readonly IOperationPositionMPBudgetService _OperationPositionMPBudgetService; 

        

        public MachineMasterUIController(
            IOperationMasterService operationMasterService
            ,IMachineMasterUIService machineMasterUIService
            , IOperationTimeCaptureMasterService operationTimeCaptureService
            , IOperationTimeCaptureDetailService operationtimecapturedetailservice
            , IOperationService operationService
            , IOperationVariationService operationStepService
            , IOperationPositionMPBudgetService OperationPositionMPBudgetService
            , ISqlRepository sqlRepository

            )
        {
            _machineMasterUIService = machineMasterUIService;
            _operationStepService = operationStepService;
            _operationtimecapturedetailservice = operationtimecapturedetailservice;
            _operationService = operationService;
            _ioperationtimecaptureservice = operationTimeCaptureService;
            _operationMasterService = operationMasterService;
            _OperationPositionMPBudgetService = OperationPositionMPBudgetService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages

  
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations for OperationMaster

        [Authorize, HttpGet]
        public JsonResult GetCboOperationActivity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetCboCompanyGroup(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboOperationType()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetCboOperationType(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]

        public JsonResult GetCboMachineCategory()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetCboMachineCategory(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboMachineSubCategory()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetCboMachineSubCategory(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboSkill()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetCboSkill(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboMachineMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCboMachineMaster(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]

        public JsonResult GetCboSkillGrouping()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCboSkillGrouping(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCbolegalDesignation()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCbolegalDesignation(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboProcess()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationMasterService.GetCboProcess(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboEntity() 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationPositionMPBudgetService.GetCboEntity(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboPosition()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationPositionMPBudgetService.GetCboPosition(), JsonRequestBehavior.AllowGet);
        }




        #endregion


        #region Grid data for Operation Master UI
         [Authorize, HttpGet]
        public JsonResult GetMachineMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetMachineMaster(), JsonRequestBehavior.AllowGet);
        }
        

        #endregion

        [HttpPost]
        public JsonResult Create(MachineMasterUI model)
        {
            try
            {
                _machineMasterUIService.Check(model);
                // _machineMasterUIService.Check(model);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                model.Id = "MM" + GetPK();
                model.CompanyGroupId = identity.CompanyGroupId;
                
                if (model.Active)
                {
                    model.Active = true;
                }
                else if (!model.Active)
                {
                    model.Active = false;
                }

                _machineMasterUIService.Insert(model);
                return Json(new { OperationMaster = model, model.Id, Sequence = _machineMasterUIService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost,Authorize]
        public JsonResult CreateProcess(List<Dictionary<string, object>> data,string machineMasterId)
        {
            try
            {
                SaveData(data, machineMasterId);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
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
       
        private void SaveData(List<Dictionary<string, object>> data,string machineMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMasterOrder;
            string id = string.Empty;
            try
            {
                string mosql = "SELECT * FROM MachineMasterProcess WHERE MachineMasterId ='"+ machineMasterId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                string MachineMasterProcessId = "";

                
                foreach (var item in data)
                {
                   
                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("MachineMasterProcess", out MachineMasterProcessId);                      

                        item["Id"] = "M-" + MachineMasterProcessId + "-" + (1);
                        item["MachineMasterId"] = machineMasterId;
                        item["ProcessId"] = item["ProcessId"];
                        
                        AddNewRow(dsMasterOrder.Tables[0], item);
                    }
                    
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterOrder);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost,Authorize]
        public JsonResult CreateAsset(Dictionary<string, object> data, string machineMasterId)
        {
            try
            {
                SaveAssetData(data, machineMasterId);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private void AddNewAssetRow(DataTable dt, Dictionary<string, object> sourceData)
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

        private void EditAssetRow(DataRow dr, Dictionary<string, object> sourceData)
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
        private void SaveAssetData(Dictionary<string, object> data, string machineMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMasterOrder;
            string id = string.Empty;
            try
            {
                string mosql = "SELECT * FROM MachineMasterAsset WHERE Id ='" + data["Id"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                string MachineMasterAssetId = "";


               
                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + data["Id"] + "'";

                if (dsMasterOrder.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MachineMasterAsset", out MachineMasterAssetId);

                    data["Id"] = MachineMasterAssetId;
                    data["MachineMasterId"]= machineMasterId;
                    AddNewAssetRow(dsMasterOrder.Tables[0], data);
                }
                else
                {
                    //data["Id"] = MachineMasterAssetId;
                    data["MachineMasterId"] = machineMasterId;
                    EditAssetRow(dsMasterOrder.Tables[0].Rows[0], data);
                }
               
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterOrder);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        [Authorize, HttpPost]
        public ActionResult GetAsset(string machineMasterId)
        {
            string sql = @"SELECT MMA.Id,MM.Id MachineMasterId,E.Id EntityId,E.UserName Entity,MMA.AssetCode,MMA.AssetName,MMA.AssetDetail,MMA.AssetReference
                                        ,MMA.IsOldCode,MMA.OldCode,CONVERT(NUMERIC(10,2),MMA.TargetUtilization) TargetUtilization
										,CONVERT(NUMERIC(10,2),MMA.PlanUtilization) PlanUtilization,MMA.Remark,MMA.AssetCategory
                                        ,CONVERT(NUMERIC(10,2),MMA.RepairAndMaintanenceBudget) RepairAndMaintanenceBudget
										,CONVERT(NUMERIC(10,2),MMA.ConsumableBudget)ConsumableBudget,A.StandardName Article,wcm.UserName WorkCenterMaster
                                        from MachineMasterAsset MMA
                                        left join ORG.Entity E on E.Id=MMA.EntityId
                                        left join MST.MachineMaster MM on MM.Id=MMA.MachineMasterId
                                        left join MST.MaterialMasterArticle A ON A.Id=MMA.ArticleId
                                        left join SCS.WorkCenterMaster AS wcm ON wcm.Id = MMA.WorkCenterMasterId
										where MMA.MachineMasterId='" + machineMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAssetData()
        {
            string sql = @"SELECT CAST(0 AS BIT)Active,MMA.Id MachineMasterAssetId,MM.Id MachineMasterId,E.Id EntityId,E.UserName Entity,MMA.AssetCode,MMA.AssetName,MMA.AssetDetail,MMA.AssetReference
                                        ,MMA.IsOldCode,MMA.OldCode,CONVERT(NUMERIC(10,2),MMA.TargetUtilization) TargetUtilization
										,CONVERT(NUMERIC(10,2),MMA.PlanUtilization) PlanUtilization,MMA.Remark,MMA.AssetCategory
                                        ,CONVERT(NUMERIC(10,2),MMA.RepairAndMaintanenceBudget) RepairAndMaintanenceBudget
										,CONVERT(NUMERIC(10,2),MMA.ConsumableBudget)ConsumableBudget,A.StandardName Article,wcm.UserName WorkCenterMaster
                                        from MachineMasterAsset MMA
                                        left join ORG.Entity E on E.Id=MMA.EntityId
                                        left join MST.MachineMaster MM on MM.Id=MMA.MachineMasterId
                                        left join MST.MaterialMasterArticle A ON A.Id=MMA.ArticleId
                                        left join SCS.WorkCenterMaster AS wcm ON wcm.Id = MMA.WorkCenterMasterId";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetWorkCenterList(string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT WCM.Id AS WorkCenterMasterId,e.UserName AS Entity,p.UserName AS Plant
	                             , WCM.EntityId, WCM.Code, WCM.UserName
                            FROM SCS.WorkCenterMaster AS WCM
                            INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                            INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                            WHERE WCM.PlantId='"+identity.PlantId+ "' AND WCM.EntityId='"+ entityId + "' order by p.userName, e.UserName,WCM.sequence";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetArticleList()
        {
            string sql = @"SELECT MA.Id,MA.Code,MA.ShortName,MA.StandardName,MT.UserName as MaterialType,
Case when MM.IsAsset = 0 then 'No' else 'Yes' end IsAsset FROM MST.MaterialMasterArticle MA
left join MST.MaterialMaster MM on MM.Id=MA.MaterialMasterId
left join MST.MaterialGroupMaster MGM ON MGM.Id=MM.MaterialGroupMasterId
left join HKP.MaterialType MT ON MT.Id=MGM.MaterialTypeId
Where  MA.Active=1";
            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [Authorize, HttpPost]
        public ActionResult AssetDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from MachineMasterAsset where Id ='" + id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }


        [Authorize, HttpPost]
        public ActionResult GetProcess(string machineMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MMP.Id,P.Sequence,P.Code,P.ShortName,P.StandardName,P.Id ProcessId,P.UserName Process
			                            from MachineMasterProcess MMP
			                            left join HKP.Process P on P.Id=MMP.ProcessId
										where MMP.MachineMasterId='" + machineMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize,HttpPost]
        public ActionResult ProcessDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from MachineMasterProcess where Id ='" + id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        
        [HttpPost]
        public JsonResult Edit(MachineMasterUI model)
        {
            try
            {
                _machineMasterUIService.Check(model);
                if (model.Active)
                {
                    model.Active = true;
                }
                else if (!model.Active)
                {
                    model.Active = false;
                }
                _machineMasterUIService.Update(model);
                return Json(new { Sequence = _machineMasterUIService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

       
        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from MachineMasterProcess where MachineMasterId ='" + id + "'");
                con.executeQuery("delete from MachineMasterAsset where MachineMasterId ='" + id + "'");
                con.executeQuery("delete from EntityCapacity where MachineMasterId ='" + id + "'");
                con.executeQuery("delete from MST.MachineMaster where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult Delete(string id)
        //{
        //    _machineMasterUIService.Delete(id);
        //    return Json(new { Sequence = _machineMasterUIService.GetAutoSequence(), Message = AplosMessage.Deleted });
        //}

        #region GetData by Operation Master Id
        [Authorize, HttpGet]
        public JsonResult GetDataByMasterOrderId(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineMasterUIService.GetDataByMasterOrderId(id), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetDataByMasterOrderIdMP(string id) 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationPositionMPBudgetService.GetDataByMasterOrderIdMP(id), JsonRequestBehavior.AllowGet);
        }

        #endregion

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_machineMasterUIService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        //public JsonResult GetAutoSequenceForManPower()
        //{
        //    return Json(_OperationPositionMPBudgetService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        //}
        private string GetPK()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return _machineMasterUIService.GetAutoNumber(nameof(MachineMasterUI), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        [Authorize, HttpPost]
        public ActionResult getEntity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"Select e.Id as EntityId, e.UserName as EntityName , p.UserName as Plant, c.UserName as Company from org.Entity e
                                left join org.Plant p on p.Id = e.PlantId
                                left join org.Company c on c.Id = p.CompanyId";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult CreateEntityCapacity(Dictionary<string, object> data, string machineMasterId)
        {
            try
            {
                SaveEntityCapacityData(data, machineMasterId);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private void AddNewEntityCapacityRow(DataTable dt, Dictionary<string, object> sourceData)
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

        private void EditEntityCapacityRow(DataRow dr, Dictionary<string, object> sourceData)
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
        private void SaveEntityCapacityData(Dictionary<string, object> data, string machineMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMasterOrder;
            string id = string.Empty;
            try
            {
                string mosql = "SELECT * FROM EntityCapacity WHERE Id ='" + data["Id"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                string EntityCapacityId = "";



                DataView dv = new DataView(dsMasterOrder.Tables[0]);
                dv.RowFilter = "Id='" + data["Id"] + "'";

                if (dsMasterOrder.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EntityCapacity", out EntityCapacityId);

                    data["Id"] = EntityCapacityId;
                    data["MachineMasterId"] = machineMasterId;
                    AddNewEntityCapacityRow(dsMasterOrder.Tables[0], data);
                }
                else
                {
                    data["Id"] = EntityCapacityId;
                    data["MachineMasterId"] = machineMasterId;
                    EditEntityCapacityRow(dsMasterOrder.Tables[0].Rows[0], data);
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterOrder);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetEntityCapacity(string machineMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select EC.Id,EC.MachineMasterId,E.Id EntityId,E.UserName Entity,convert(numeric(10,2),EC.NoofMachine) NoofMachine
				                        ,CONVERT(numeric(10,2),EC.DailyHr) DailyHr,CONVERT(numeric(10,2),EC.WeelkyHr) WeelkyHr
				                        ,CONVERT(numeric(10,2),EC.MonthlyHr) MonthlyHr,CONVERT(numeric(10,2),EC.TargetUtilization) TargetUtilization
				                        ,CONVERT(numeric(10,2),EC.PlanUtilization) PlanUtilization
				                        from EntityCapacity EC
				                        left join ORG.Entity E on E.Id=EC.EntityId
                                        left join MST.MachineMaster MM on MM.Id=EC.MachineMasterId
										where EC.MachineMasterId='" + machineMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        
        [Authorize, HttpPost]
        public ActionResult EntityCapacityDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from EntityCapacity where Id ='" + id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        [HttpPost, Authorize]
        public JsonResult GetMachineMasterAssetList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetAllMachineMasterAssetList(identity.CompanyGroupId, identity.CompanyId, column, value), JsonRequestBehavior.AllowGet);
        }
        public List<Dictionary<string, object>> GetAllMachineMasterAssetList(string companyGroupId, string companyId, string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"SELECT TOP 700 * from ( SELECT 0 Active, MMA.Id MMAssetId,MMA.MachineMasterId,MM.UserName MachineMaster,MMA.AssetName,MMA.Assetdetail
                            ,MMA.AssetCode,MMA.AssetReference
                            from [dbo].[MachineMasterAsset] MMA
                            INNER JOIN [MST].[MachineMaster] MM ON MM.Id=MMA.MachineMasterId
                            ) AS TEMP WHERE " + strkey + " order by MachineMaster ASC ";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
    }

}