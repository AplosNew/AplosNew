#region Using
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Skills;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using System;
using System.Data;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using Library.Data.Sql;
using OTSBD;
using Library.Model.Enums;
#endregion

namespace Aplos.Areas.Skills.Controllers
{
    public class SkillController : Controller
    {
        #region Constructor
        private readonly ISkillService _skillService;
        private readonly ISkillProcessService _skillProcessService;
        private readonly ISqlRepository _sqlRepository;
        public SkillController(
              ISkillService skillService
            , ISkillProcessService skillProcessService,ISqlRepository R)
        {
            _skillService = skillService;
            _skillProcessService = skillProcessService;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
      
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult SkillDevelopmentMaster()
        {
            return View();
        }

        public ActionResult Planning()
        {
            return View();
        }

        #endregion

        #region -- Skill Operations
        [Authorize]
        public JsonResult GetCboWithoutMachineType(string processId)
        {
            return Json(_skillService.GetCboWithoutMachineType(processId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetIsMachineSkillList(GridParameter parameters, string skillProcessIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_skillService.GetIsMachineSkillList(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(skillProcessIds)), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Use in Operation
        /// </summary>
        /// <param name="processIds"></param>
        /// <returns></returns>
        [Authorize]
        public JsonResult GetCommonSkillListByProcess(GridParameter parameters, string processIds,bool MachineRequired)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_skillService.GetCommonSkillListByProcess(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(processIds), MachineRequired), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCboByProcess( string processIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_skillService.GetCboByProcess(identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(processIds)), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCboByMachineTypeId(string processId, string matchineTypeId)
        {
            return Json(new SelectList(_skillService.GetCboByMachineTypeId(processId, matchineTypeId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_skillService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSkillProcessList(GridParameter parameters, string skillId)
        {
            return Json(_skillProcessService.Query(parameters, skillId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_skillService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Skill entity, IEnumerable<SkillProcess> skillProcess)
        {
            _skillService.InsertGraph(entity, skillProcess);
            return Json(new { Skill = entity, Sequence = _skillService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(Skill entity, IEnumerable<SkillProcess> skillProcess)
        {
            _skillService.UpdateGraph(entity, skillProcess);
            return Json(new { Sequence = _skillService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _skillService.DeleteGraph(id);
            return Json(new { Sequence = _skillService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion

        #region Skill Development Master

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
        public JsonResult SaveMaster(Dictionary<string, object> data)
        {
            SaveMasterData(data);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }
        private void SaveMasterData(Dictionary<string, object> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[SkillDevelopmentMaster] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SkillDevelopmentMaster", out _Id);

                    data["Id"] =  _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetSDMList()
        {
            try
            {
                string sql = @"Select E.EmployeeName ResponsiblePerson,S.* From [dbo].[SkillDevelopmentMaster] S
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=S.ResponsiblePersonId";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost]
        public JsonResult DeleteSDM(string id)
        {
            DeleteSDMData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteSDMData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[SkillDevelopmentMaster] WHERE Id='" + Id + "'";
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

        [HttpGet, Authorize]
        public ActionResult GetSDPList(string masterId)
        {
            try
            {
                string sql = @"Select P.Code,P.UserName,D.UserName Division,DP.UserName Department,SC.UserName Section
,SSC.UserName SubSection,DG.UserName Designation,S.* From [dbo].[SkillDevelopmentPosition] S
LEFT JOIN ORG.Position P ON P.Id=S.PositionId
LEFT JOIN ORG.Division D ON D.id=P.DivisionId
LEFT JOIN ORG.Department DP ON DP.id=P.DepartmentId
LEFT JOIN ORG.Section SC ON SC.id=P.SectionId
LEFT JOIN ORG.SubSection SSC ON SSC.id=P.SubSectionId
LEFT JOIN HKP.Designation DG ON DG.id=P.DesignationId
Where SkillDevelopmentMasterId='" + masterId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SavePositionData(List<Dictionary<string, object>> data,string masterId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsBC;
            string _Id = string.Empty;
            try
            {
                #region Entity 

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.SkillDevelopmentPosition where SkillDevelopmentMasterId='" + masterId+"'", out dsBC, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + Convert.ToInt64(item["Id"]) + "'";

                        if (dv.Count == 0)
                        {
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

        [HttpPost,Authorize]
        public JsonResult DeleteSP(string id)
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
                strSQL = "DELETE FROM [dbo].[SkillDevelopmentPosition] WHERE Id='" + Id + "'";
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

        [HttpGet, Authorize]
        public ActionResult GetSDSList(string masterId)
        {
            try
            {
                string sql = @"Select SD.*,S.UserName SkillName from [dbo].[SkillDevelopment] SD 
LEFT JOIN HKP.Skill S ON S.Id=SD.SkillId
Where SkillDevelopmentMasterId='" + masterId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveSkillData(Dictionary<string, object> data)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsBC;
            string _Id = string.Empty;
            try
            {
                #region Entity 

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.SkillDevelopment where SkillDevelopmentMasterId='" + data["SkillDevelopmentMasterId"] + "'", out dsBC, false, "1");

                if (data != null)
                {
                    
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + Convert.ToInt64(data["Id"]) + "'";

                        if (dv.Count == 0)
                        {
                            AddNewRow(dsBC.Tables[0], data);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, data);
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

        [HttpPost, Authorize]
        public JsonResult DeleteSkill(string id)
        {
            DeleteSKillData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteSKillData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[SkillDevelopment] WHERE Id='" + Id + "'";
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

        #endregion

        #region Skill Planning

        [HttpPost]
        public JsonResult SaveSkillPlanning(Dictionary<string, object> data)
        {
            SaveSkillPlanningData(data);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }
        private void SaveSkillPlanningData(Dictionary<string, object> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[SkillPlanningMaster] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
              
                string _Id;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SkillPlanningMaster", out _Id);

                    data["Id"] = _Id;
                    data["BatchNo"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult DeleteSkillPlanning(string id)
        {
            DeleteSKillPlanningData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteSKillPlanningData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[SkillPlanningMaster] WHERE Id='" + Id + "'";
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

        [HttpGet, Authorize]
        public ActionResult GetSPList()
        {
            try
            {
                string sql = @"Select SD.*,E.EmployeeName ResponsiblePerson from [dbo].[SkillPlanningMaster] SD
                            LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=SD.ResponsiblePersonId";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult SaveSkillPlan(Dictionary<string, object> data)
        {
            SaveSkillPlanData(data);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }
        private void SaveSkillPlanData(Dictionary<string, object> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[SkillPlan] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SkillPlan", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult DeleteSkillPlan(string id)
        {
            DeleteSKillPlanData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteSKillPlanData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[SkillPlan] WHERE Id='" + Id + "'";
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

        [HttpGet, Authorize]
        public ActionResult GetSPDataList()
        {
            try
            {
                string sql = @"Select SD.*,S.UserName SkillName from [dbo].[SkillPlan] SD
LEFT JOIN HKP.Skill S ON S.id=SD.SkillId";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion




    }
}