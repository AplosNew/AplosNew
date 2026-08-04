using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.WorkCenters;
using Library.Security.Core;
using Library.Service.WorkCenters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.WorkCenters.Controllers
{
    public class WorkCenterMasterController : BaseController
    {
        #region Constructor

        private readonly IWorkCenterMasterService _workcentermasterservice;
        private readonly ISqlRepository _sqlRepository;
        public WorkCenterMasterController(IWorkCenterMasterService workcentermasterservice, ISqlRepository R)
        {
            _workcentermasterservice = workcentermasterservice;
            _sqlRepository = R;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult WCGroup()
        {
            return View();
        }

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_workcentermasterservice.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeListByPlant(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.EmployeeListByPlant(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetWorkCenterWiseShiftList(string workCenterMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(_workcentermasterservice.GetWorkCenterWiseShiftList(identity.CompanyGroupId, identity.PlantId, workCenterMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetWorkCenterMasterSubProcessList(string workCenterMasterId)
        {
            try
            {
                return Json(_workcentermasterservice.GetWorkCenterMasterSubProcessList(workCenterMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetShiftList(GridParameter parameters, string ShiftDefinationIDs)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetShiftList(parameters, identity.CompanyGroupId, identity.PlantId, new JavaScriptSerializer().Deserialize<string[]>(ShiftDefinationIDs)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetListForSubProcess(GridParameter parameters, string processId, string WorkCenterMasterId, string subProcessIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetListForSubProcess(parameters, identity.CompanyGroupId, processId, WorkCenterMasterId, new JavaScriptSerializer().Deserialize<string[]>(subProcessIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteShift(string id)
        {
            DeleteShiftData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteShiftData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[WorkCenterWiseShift] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpPost, Authorize]
        public ActionResult DeleteSP(string id)
        {
            DeleteSPData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteSPData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [SCS].[WorkCenterMasterSubProcess] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [Authorize, HttpGet]
        public ActionResult GetMasterList(string masterid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetList(masterid, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetEmployeeList(GridParameter parameters, string plantId)
        {
            return Json(_workcentermasterservice.GetEmployeeList(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_workcentermasterservice.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProductMasterList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetProductMasterList(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboList(string entityId)
        {
            return Json(_workcentermasterservice.GetCboList(entityId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(string plantid, string entityid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetListByPlantAndEntity(plantid, entityid, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetDetalsData(string masterId)
        {
            var eDate = _workcentermasterservice.GetEffectiveDateList(masterId);
            var bCode = _workcentermasterservice.GetManpowerBudgetList(masterId);
            var priority = _workcentermasterservice.GetProductPriorityList(masterId);

            return Json(new { eDate, bCode, priority }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMaterialMasterList(GridParameter parameters, string ids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_workcentermasterservice.GetMaterialMasterList(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAllWorkCenter(GridParameter parameter)
        {
            return Json(_workcentermasterservice.GetAllWorkCenter(parameter), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getlistbyplant(GridParameter gridparameter, string plantid)
        {
            return Json(_workcentermasterservice.GetListByPlant(gridparameter, plantid), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getlistbyplantandprocess(GridParameter gridparameter, string plantid, string processid)
        {
            return Json(_workcentermasterservice.GetListByPlant(gridparameter, plantid, processid), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(WorkCenterMaster master)
        {
            string masterid = string.Empty;
            _workcentermasterservice.InsertORUpdateMaster(master, out masterid);
            return Json(new { id = masterid, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string masterid)
        {
            _workcentermasterservice.DeleteMaster(masterid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult DetailSave(string masterId, IEnumerable<WorkCenterMasterEffectiveDate> effectiveDateList, IEnumerable<WorkCenterMasterManpowerBudge> budgetCodeList, IEnumerable<WorkCenterMasterProductPriority> productPriorityList, IEnumerable<WorkCenterWiseShift> shiftList, IEnumerable<WorkCenterMasterSubProcess> subProcessList)
        {
            _workcentermasterservice.InsertUpdateOrDeleteDetails(masterId, effectiveDateList, budgetCodeList, productPriorityList, shiftList, subProcessList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult GetLineList(GridParameter parameters, string entityId)
        {
            return Json(_workcentermasterservice.GetSearchLine(parameters, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateWCSkill(Dictionary<string, object> data, string WorkCenterMasterId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[WorkCenterSkill] where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "' AND WorkCenterMasterId='" + WorkCenterMasterId + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from [HKP].[WorkCenterSkill] where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "' AND WorkCenterMasterId='" + WorkCenterMasterId + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from [HKP].[WorkCenterSkill] where Id='" + data["Id"] + "' AND WorkCenterMasterId='" + WorkCenterMasterId + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("WorkCenterSkill", out _Id);

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

                return Json(new { Error = false, Data = data, Sequence = GetWCSSequence(WorkCenterMasterId), Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public JsonResult GetWCSAutoSequence(string WorkCenterMasterId)
        {
            return Json(GetWCSSequence(WorkCenterMasterId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetWCSkill(string WorkCenterMasterId)
        {
            string sql = @"select w.*,A.StandardName MachineName from [HKP].[WorkCenterSkill] W
LEFT JOIN MST.MaterialMasterArticle A on A.Id=w.ArticleId Where w.WorkCenterMasterId='" + WorkCenterMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult GetMachine(GridParameter parameters)
        {

            parameters.CmdText = @"SELECT ART.Id, ART.Code, ART.ShortName, ART.StandardName, MM.SkillId, SK.UserName AS SkillName, ART.MachineAllowance
FROM[MST].[MaterialMasterArticle] AS ART
LEFT JOIN[MST].[MaterialMaster] AS MM ON MM.Id = ART.MaterialMasterId
LEFT JOIN[HKP].Skill AS Sk ON MM.SkillId = Sk.Id
LEFT JOIN[MST].[MaterialMasterBusinessProcess] AS MMBP ON MMBP.MaterialMasterId = MM.Id
LEFT JOIN[SCS].[BusinessProcess] AS BP ON MMBP.BusinessProcessId = BP.Id
WHERE BP.BusinessProcessName = 'MachineDefinition' AND ART.Active = 1";
            return Json(_sqlRepository.GetGridData(parameters), JsonRequestBehavior.AllowGet);
        }

      

        private double GetWCSSequence(string WorkCenterMasterId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM [HKP].[WorkCenterSkill] Where WorkCenterMasterId='" + WorkCenterMasterId + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
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

        [Authorize,HttpPost]
        public JsonResult CreateSB(Dictionary<string, object> data, string WorkCenterMasterId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [HKP].[WorkCenterSkillBudget] where Id='" + data["Id"] + "' AND WorkCenterMasterId='" + WorkCenterMasterId + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("WorkCenterSkillBudget", out _Id);

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

                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public ActionResult GetWCSkillBudget(string WorkCenterMasterId)
        {
            string sql = @"select w.*,A.UserName SkillName from [HKP].[WorkCenterSkillBudget] W
LEFT JOIN MST.OperationMaster A on A.Id=w.SkillMasterId Where w.WorkCenterMasterId='" + WorkCenterMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost,Authorize]
        public JsonResult CreateWCGroup(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[WorkCenterGroup] where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from [HKP].[WorkCenterGroup] where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from [HKP].[WorkCenterGroup] where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("WorkCenterGroup", out _Id);

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

                return Json(new { Error = false, Data = data, Sequence = GetWCGSequence(), Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public JsonResult GetWCGAutoSequence()
        {
            return Json(GetWCGSequence(), JsonRequestBehavior.AllowGet);
        }

        private double GetWCGSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM [HKP].[WorkCenterGroup]");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        
        [HttpPost, Authorize]
        public ActionResult GetWCGroup(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT *, Flag=CAST(0 as bit) FROM [HKP].[WorkCenterGroup]) AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetWCWGroup(string WorkCenterMasterId)
        {
            string sql = @"select WM.UserName WorkCenterMaster,WWG.*,WG.Sequence,WG.Code,WG.ShortName,WG.StandardName,WG.UserName 
from HKP.WorkCenterWiseGroup WWG 
LEFT JOIN hkp.WorkCenterGroup WG ON WG.Id=WWG.WorkCenterGroupId 
LEFT JOIN SCS.WorkCenterMaster WM ON WM.Id=WWG.WorkCenterMasterId
Where WWG.WorkCenterMasterId='" + WorkCenterMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveWorkCenterWiseGroup(List<Dictionary<string, object>> data, string masterId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsBC;
            string _Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                string strSQL = "Delete FROM HKP.WorkCenterWiseGroup Where WorkCenterMasterId='" + masterId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();

                #region Entity 

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM HKP.WorkCenterWiseGroup Where WorkCenterMasterId='" + masterId + "'", out dsBC, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + Convert.ToInt64(item["Id"]) + "'";

                        if (dv.Count == 0)
                        {
                            item["WorkCenterMasterId"] = masterId;
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
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }



        #endregion -- Operations
    }
}