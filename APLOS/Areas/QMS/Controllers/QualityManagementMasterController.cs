using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.QMS.Controllers
{
    public class QualityManagementMasterController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public QualityManagementMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetProcessList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from HKP.Process";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSubProcessList(string Pid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select SP.Id as Value,P.UserName as Text from HKP.SubProcess SP
left join HKP.Process P ON  P.Id=SP.ProcessId
where P.Id='" + Pid + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public decimal GetSkillLevelAutoSequence(string scheduleId)
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM [TRN].[SkillManagementLevel] where SMID='" + scheduleId + "'");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["SNO"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [Authorize, HttpGet]
        public decimal GetItemAutoSequence(string scheduleId)
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM [TRN].[SkillManagementItem] where SMID='" + scheduleId + "'");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["SNO"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [Authorize, HttpGet]
        public decimal GetPersonBudgetAutoSequence(string scheduleId)
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM TRN.SkillManagementPersonBudgetCode where SMID='" + scheduleId + "'");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["SNO"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [Authorize, HttpGet]
        public decimal GetTeamDefinitionAutoSequence(string scheduleId)
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM TRN.SkillManagementTeamDefinition where SMID='" + scheduleId + "'");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["SNO"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetPerformanceGroupList(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select distinct PerformanceGroup as Value,PerformanceGroup as Text from TRN.SkillManagementLevel SML where SML.SMID='" + ScheduleId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> ScheduleData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementMaster] where ScheduleCode='" + ScheduleData["ScheduleCode"] + "'", out DataSet dsQualityManagmentMasterCodeValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementMaster] where StandaredName='" + ScheduleData["StandaredName"] + "'", out DataSet dsQualityManagmentMasterSNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementMaster] where UserName='" + ScheduleData["UserName"] + "'", out DataSet dsQualityManagmentMasterUNameValidation, false, "1");
                

                DataSet dsQualityManagmentMaster;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementMaster] where Id='" + ScheduleData["Id"] + "'", out dsQualityManagmentMaster, false, "1");
                string _Id = "";

                #region data update
                if (dsQualityManagmentMaster.Tables[0].Rows.Count == 0)
                {
                    
                    if (dsQualityManagmentMasterCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Schedule Code Already Exist.");
                    }
                    else if (dsQualityManagmentMasterSNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Standared Name Already Exist.");
                    }
                    else if (dsQualityManagmentMasterUNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("QualityManagmentMaster", out _Id);
                        _Id = "QM" + _Id;
                        ScheduleData["Id"] = _Id;
                        AddNewRow(dsQualityManagmentMaster.Tables[0], ScheduleData);
                    }
                }
                else
                {
                    _Id = ScheduleData["Id"].ToString();
                    EditRow(dsQualityManagmentMaster.Tables[0].Rows[0], ScheduleData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityManagmentMaster);

                return Json(new { Error = false, Data = ScheduleData, Message = AplosMessage.Insert });
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

        [Authorize, HttpGet]
        public ActionResult LoadQualityManagementMasterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * ,(select MP.Code from MST.ManpowerBudget MP where MP.Id=QM.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode
                            FROM [MST].[QualityManagementMaster] QM";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadQualityManagementEditData(string ScheduleID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * ,(select MP.Code from MST.ManpowerBudget MP where MP.Id=QM.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode
                            FROM [MST].[QualityManagementMaster] QM where QM.Id='" + ScheduleID + @"'";
            return Json(new { schedule = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult ScheduleDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                DataSet EntityCount, AGCount, ItemCount, ProcessCount;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementEntity] where QMID='" + id + "'", out EntityCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementActivityGroup] where QMID ='" + id + "'", out AGCount, false, "1");
                //conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementItem] where SMID ='" + id + "'", out ItemCount, false, "1");
                //conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementPersonBudgetCode] where SMID ='" + id + "'", out BudgetCount, false, "1");
                //conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementTeamDefinition] where SMID ='" + id + "'", out TeamCount, false, "1");

                if (EntityCount.Tables[0].Rows.Count == 0 || AGCount.Tables[0].Rows.Count == 0)
                {

                    conC.BeginTransaction();
                    conC.executeQuery("delete from [MST].[QualityManagementMaster] where Id ='" + id + @"'");
                    conC.CommitTransaction();
                }
                else
                {
                    throw new Exception("Transaction are Exists!");
                }
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetActivityGroupList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,ActivityGroupName as Text from MST.QualityManagementActivityGroup";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadEntityDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN QME.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,QME.Id,E.Id EntityId,E.EntityType,E.UserName Entity,E.Code,QME.Remarks 
                            from ORG.Entity E
							LEFT JOIN [MST].[QualityManagementEntity] QME ON QME.EntityId=E.Id and QME.QMID='" + ScheduleId + @"'
                            where E.Active = 1 order by QME.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN QMP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,QMP.Id,P.Id ProcessId,P.UserName Process,P.Code,QAG.Id as ActivityGroupId,QAG.ActivityGroupName,QMP.Remarks
                            from hkp.Process P
							LEFT JOIN [MST].[QualityManagementProcess] QMP ON QMP.ProcessId=P.Id and QMP.QMID='" + ScheduleId + @"'
							LEFT JOIN  MST.QualityManagementActivityGroup QAG ON QAG.Id=QMP.ActivityGroupId
                            where P.Active = 1 order by QMP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult createEntity(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[QualityManagementEntity]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QMID='" + item["QMID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "QME" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet] 
        public ActionResult LoadQMActivityGroupDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from [MST].[QualityManagementActivityGroup] where QMID ='" + ScheduleId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadActivityGroupEditData(string AGId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from [MST].[QualityManagementActivityGroup] AG where AG.Id='" + AGId + @"'";
            return Json(new { activitygroup = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult createActivityGroup(Dictionary<string, object> ActivityGroupData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementActivityGroup] where ActivityGroupName='" + ActivityGroupData["ActivityGroupName"] + "'", out DataSet dsQualityManagementAGValidation, false, "1");

                DataSet dsQualityManagementAG;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementActivityGroup] where Id='" + ActivityGroupData["Id"] + "'", out dsQualityManagementAG, false, "1");
                string _Id = "";

                #region data update
                if (dsQualityManagementAG.Tables[0].Rows.Count == 0)
                {
                    if (dsQualityManagementAGValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Activity Group Already Exist.");
                    }
                    else
                    { 
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("QualityManagementAG", out _Id);
                    _Id = "QAG" + _Id;
                    ActivityGroupData["Id"] = _Id;
                    ActivityGroupData["QMID"] = Pid;
                    AddNewRow(dsQualityManagementAG.Tables[0], ActivityGroupData);
                    }
                }
                else
                {
                    _Id = ActivityGroupData["Id"].ToString();
                    ActivityGroupData["QMID"] = Pid;
                    EditRow(dsQualityManagementAG.Tables[0].Rows[0], ActivityGroupData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityManagementAG);

                return Json(new { Error = false, Data = ActivityGroupData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public ActionResult ActivityGroupDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[QualityManagementActivityGroup] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult createProcess(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[QualityManagementProcess]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QMID='" + item["QMID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "QMP" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public ActionResult ItemDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[SkillManagementItem] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult BudgetCodeDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[SkillManagementPersonBudgetCode] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult TeamDefinitionDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[SkillManagementTeamDefinition] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult GradingDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[SkillManagementGrading] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetBudgetCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MP.Id ManPowerBudgetId, MP.Code, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from MST.ManpowerBudget MP
                            left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MP.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetPersonBudgetCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MP.Id ManPowerBudgetId, MP.Code, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from MST.ManpowerBudget MP
                            left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MP.Active = 1 and EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetTeamDefinition()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select TD.Id,TD.Code,TD.ShortName,TD.StandardName,TD.UserName,(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=TD.TeamLeaderId) as TeamLeader
							from TRN.TeamDefinition TD where Active=1 order by TD.UserName";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetByWhomeBudgetCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MP.Id ManPowerBudgetId, MP.Code, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from MST.ManpowerBudget MP
                            left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MP.Active = 1 and EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
       
        [Authorize, HttpPost]
        public ActionResult GetDepartment()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select D.Id DepartmentId, D.Code,D.StandardName, D.UserName Department from Org.Department D where D.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        
        [Authorize, HttpGet]
        public ActionResult LoadSkillLevelEditData(string LevelId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from [TRN].[SkillManagementLevel] where Id='" + LevelId + @"'";
            return Json(new { skilllevel = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadItemEditData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT *,
(select Code from [MST].[ManpowerBudget] where Id=ByWhomId) as ByWhom
FROM [TRN].[SkillManagementItem] where Id='" + ItemId + @"'";
            return Json(new { item = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadParameterEditData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from SkillItemParameterDetails where Id='" + ParameterId + @"'";
            return Json(new { Parameter = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        
        [Authorize, HttpGet]
        public ActionResult LoadBudgetCodeEditData(string BudgetCodeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select MPB.Id,MPB.SNO,MPB.PersonBudgetCodeId,MPB.[Group],MPB.AddedBy,MPB.AddedDate,MPB.AddedFromIP,MPB.UpdatedBy,MPB.UpdatedDate,MPB.UpdatedFromIP,MP.Id ManPowerBudgetId, MP.Code as PersonBudgetCode, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from [TRN].[SkillManagementPersonBudgetCode] MPB
                            left join MST.ManpowerBudget MP on MP.Id=MPB.PersonBudgetCodeId
						    left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MP.Active = 1 and  MPB.Id='" + BudgetCodeId + "'";
            return Json(new { PersonBudget = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadTeamDefinitionEditData(string TeamDefinitionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select MTD.Id, MTD.SNO,TD.UserName TeamDefinition from [TRN].[SkillManagementTeamDefinition] MTD
left join TRN.TeamDefinition TD ON TD.Id=MTD.TeamDefinitionId
                            where MTD.Id ='" + TeamDefinitionId + "'";
            return Json(new { TeamDefinition = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadGradingEditData(string GradeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select PerformanceGroup,Grade1,Grade2,Grade3,Grade4 from [TRN].[SkillManagementGrading] where Id ='" + GradeId + "'";
            return Json(new { Grade = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        

        

        [Authorize, HttpGet]
        public ActionResult LoadPositionCodeDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct CAST (CASE WHEN SPC.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,SPC.Id, P.Id PositionCodeId,P.Code,DIV.UserName Division,DEP.UserName Department,S.UserName Section,SS.UserName SubSection,
P.Activity,DEG.UserName Designation,PRO.UserName Process,E.UserName Entity from MST.ManpowerBudget MB
left Join Org.Entity E ON E.Id=MB.EntityId
left Join ORG.Position P ON P.Id=MB.PositionId
left join [TRN].[SkillManagementPositionCode] SPC ON SPC.PositionCodeId=P.Id and SPC.SMID='" + ScheduleId + @"'
left join org.Division DIV ON DIV.Id=P.DivisionId
left join Org.Department DEP ON DEP.Id=P.DepartmentId
left join Org.Section S ON S.Id=P.SectionId
left join Org.SubSection SS ON SS.Id=P.SubSectionId
left join HKP.Designation DEG ON DEG.Id=P.DesignationId
left join HKP.Process PRO ON PRO.Id=P.ProcessId
where P.Active=1 and E.Id in (select EntityId from [TRN].[SkillManagementEntity] where SMID='" + ScheduleId + @"') order by SPC.Id  desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        

        [Authorize, HttpGet]
        public ActionResult LoadItemDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,(select distinct PerformanceGroup from TRN.SkillManagementLevel where PerformanceGroup=PerformanceGroupId) as PerformanceGroup,
(select Code from [MST].[ManpowerBudget] where Id=ByWhomId) as ByWhom
FROM [TRN].[SkillManagementItem] where SMID ='" + ScheduleId + "' order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getParameterData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM SkillItemParameterDetails where ItemId ='" + ItemId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
      
        [Authorize, HttpGet]
        public ActionResult LoadBudgetCodeDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MPB.Id,MPB.SNO,MPB.PersonBudgetCodeId,MPB.[Group],MPB.AddedBy,MPB.AddedDate,MPB.AddedFromIP,MPB.UpdatedBy,MPB.UpdatedDate,MPB.UpdatedFromIP,MP.Id ManPowerBudgetId, MP.Code as PersonBudgetCode, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from [TRN].[SkillManagementPersonBudgetCode] MPB
                            left join MST.ManpowerBudget MP on MP.Id=MPB.PersonBudgetCodeId and MP.Active = 1
						    left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MPB.SMID='" + ScheduleId + "' order by MPB.SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadTeamDefinitionDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MTD.Id, MTD.SNO,TD.UserName TeamName from [TRN].[SkillManagementTeamDefinition] MTD
left join TRN.TeamDefinition TD ON TD.Id=MTD.TeamDefinitionId
where MTD.SMID ='" + ScheduleId + "' order by MTD.SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadGradingDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from [TRN].[SkillManagementGrading] where SMID ='" + ScheduleId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

       

       

        

        [HttpPost]
        public JsonResult CreateItem(Dictionary<string, object> ItemData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementItem] where Id<>'" + ItemData["Id"] + "'", out DataSet dsSkillManagementItemValidation, false, "1");

                DataSet dsSkillManagementItem;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementItem] where Id='" + ItemData["Id"] + "'", out dsSkillManagementItem, false, "1");
                string _Id = "";

                #region data update
                if (dsSkillManagementItem.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SkillManagementItem", out _Id);
                    _Id = "SMI" + _Id;
                    ItemData["Id"] = _Id;
                    ItemData["SMID"] = Pid;
                    AddNewRow(dsSkillManagementItem.Tables[0], ItemData);
                }
                else
                {
                    _Id = ItemData["Id"].ToString();
                    ItemData["SMID"] = Pid;
                    EditRow(dsSkillManagementItem.Tables[0].Rows[0], ItemData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSkillManagementItem);

                return Json(new { Error = false, Data = ItemData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [Authorize, HttpPost]
        public JsonResult CreateParameter(Dictionary<string, object> ParameterData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from SkillItemParameterDetails where Id<>'" + ParameterData["Id"] + "'", out DataSet dsItemParameterDetailsValidation, false, "1");

                DataSet dsSkillItemParameterDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from SkillItemParameterDetails where Id='" + ParameterData["Id"] + "'", out dsSkillItemParameterDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsSkillItemParameterDetails.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SkillItemParameterDetails", out _Id);
                    _Id = "SIP" + _Id;
                    ParameterData["Id"] = _Id;
                    ParameterData["ItemId"] = Pid;
                    AddNewRow(dsSkillItemParameterDetails.Tables[0], ParameterData);
                }
                else
                {
                    _Id = ParameterData["Id"].ToString();
                    ParameterData["ItemId"] = Pid;
                    EditRow(dsSkillItemParameterDetails.Tables[0].Rows[0], ParameterData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSkillItemParameterDetails);

                return Json(new { Error = false, Data = ParameterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        
        [Authorize, HttpPost]
        public JsonResult createBudgetCode(Dictionary<string, object> BudgetCodeData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementPersonBudgetCode] where Id<>'" + BudgetCodeData["Id"] + "'", out DataSet dsSkillManagementPersonBudgetCodeValidation, false, "1");

                DataSet dsSkillManagementPersonBudgetCode;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementPersonBudgetCode] where Id='" + BudgetCodeData["Id"] + "'", out dsSkillManagementPersonBudgetCode, false, "1");
                string _Id = "";

                #region data update
                if (dsSkillManagementPersonBudgetCode.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SkillManagementPersonBudgetCode", out _Id);
                    _Id = "SPB" + _Id;
                    BudgetCodeData["Id"] = _Id;
                    BudgetCodeData["SMID"] = Pid;
                    AddNewRow(dsSkillManagementPersonBudgetCode.Tables[0], BudgetCodeData);
                }
                else
                {
                    _Id = BudgetCodeData["Id"].ToString();
                    BudgetCodeData["SMID"] = Pid;
                    EditRow(dsSkillManagementPersonBudgetCode.Tables[0].Rows[0], BudgetCodeData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSkillManagementPersonBudgetCode);

                return Json(new { Error = false, Data = BudgetCodeData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public JsonResult createTeamDefinition(Dictionary<string, object> TeamDefinitionData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementTeamDefinition] where Id<>'" + TeamDefinitionData["Id"] + "'", out DataSet dsSkillManagementTeamDefinitionValidation, false, "1");

                DataSet dsSkillManagementTeamDefinition;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementTeamDefinition] where Id='" + TeamDefinitionData["Id"] + "'", out dsSkillManagementTeamDefinition, false, "1");
                string _Id = "";

                #region data update
                if (dsSkillManagementTeamDefinition.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SkillManagementTeamDefinition", out _Id);
                    _Id = "STD" + _Id;
                    TeamDefinitionData["Id"] = _Id;
                    TeamDefinitionData["SMID"] = Pid;
                    AddNewRow(dsSkillManagementTeamDefinition.Tables[0], TeamDefinitionData);
                }
                else
                {
                    _Id = TeamDefinitionData["Id"].ToString();
                    TeamDefinitionData["SMID"] = Pid;
                    EditRow(dsSkillManagementTeamDefinition.Tables[0].Rows[0], TeamDefinitionData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSkillManagementTeamDefinition);

                return Json(new { Error = false, Data = TeamDefinitionData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public JsonResult createGrading(Dictionary<string, object> GradingData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementGrading] where Id<>'" + GradingData["Id"] + "'", out DataSet dsSkillManagementGradingValidation, false, "1");

                DataSet dsSkillManagementGrading;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementGrading] where Id='" + GradingData["Id"] + "'", out dsSkillManagementGrading, false, "1");
                string _Id = "";

                #region data update
                if (dsSkillManagementGrading.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SkillManagementGrading", out _Id);
                    _Id = "SMG" + _Id;
                    GradingData["Id"] = _Id;
                    GradingData["SMID"] = Pid;
                    AddNewRow(dsSkillManagementGrading.Tables[0], GradingData);
                }
                else
                {
                    _Id = GradingData["Id"].ToString();
                    GradingData["SMID"] = Pid;
                    EditRow(dsSkillManagementGrading.Tables[0].Rows[0], GradingData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSkillManagementGrading);

                return Json(new { Error = false, Data = GradingData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        #endregion -- Operations
    }
}