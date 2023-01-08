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

namespace Aplos.Areas.Machines.Controllers
{
    public class SkillManagementController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public SkillManagementController(ISqlRepository R)
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
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagement] where ScheduleCode='" + ScheduleData["ScheduleCode"] + "'", out DataSet dsSkillManagementCodeValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagement] where StandaredName='" + ScheduleData["StandaredName"] + "'", out DataSet dsSkillManagementSNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagement] where UserName='" + ScheduleData["UserName"] + "'", out DataSet dsSkillManagementUNameValidation, false, "1");
                

                DataSet dsSkillManagement;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [Trn].[SkillManagement] where Id='" + ScheduleData["Id"] + "'", out dsSkillManagement, false, "1");
                string _Id = "";

                #region data update
                if (dsSkillManagement.Tables[0].Rows.Count == 0)
                {
                    if (dsSkillManagementCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Schedule Code Already Exist.");
                    }
                    else if (dsSkillManagementSNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Standared Name Already Exist.");
                    }
                    else if (dsSkillManagementUNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("SkillManagement", out _Id);
                        _Id = "SM" + _Id;
                        ScheduleData["Id"] = _Id;
                        AddNewRow(dsSkillManagement.Tables[0], ScheduleData);
                    }
                }
                else
                {
                    _Id = ScheduleData["Id"].ToString();
                    EditRow(dsSkillManagement.Tables[0].Rows[0], ScheduleData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSkillManagement);

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

        [HttpPost]
        public ActionResult ScheduleDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                DataSet PositionCount, LevelCount, ItemCount, BudgetCount, TeamCount;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementPositionCode] where SMID='" + id + "'", out PositionCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementItem] where SMID ='" + id + "'", out ItemCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementLevel] where SMID ='" + id + "'", out LevelCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementPersonBudgetCode] where SMID ='" + id + "'", out BudgetCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementTeamDefinition] where SMID ='" + id + "'", out TeamCount, false, "1");

                if (PositionCount.Tables[0].Rows.Count == 0 || ItemCount.Tables[0].Rows.Count == 0 || LevelCount.Tables[0].Rows.Count == 0 || BudgetCount.Tables[0].Rows.Count == 0 || TeamCount.Tables[0].Rows.Count == 0)
                {

                    conC.BeginTransaction();
                    conC.executeQuery("delete from TRN.SkillManagement where Id ='" + id + @"'");
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

        [Authorize, HttpPost]
        public ActionResult LevelDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[SkillManagementLevel] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
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
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

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
        public ActionResult LoadScheduleEditData(string ScheduleID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * ,(select MP.Code from MST.ManpowerBudget MP where MP.Id=SM.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode,
(select D.UserName Department from Org.Department D where D.Id=SM.DepartmentId) as Department
                            FROM [TRN].[SkillManagement] SM where SM.Id='" + ScheduleID + @"'";
            return Json(new { schedule = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
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
(select EmployeeName from EmployeeInformation where SystemId=ByWhomId) as ByWhom
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
        public ActionResult LoadSkillManagementMasterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * ,(select MP.Code from MST.ManpowerBudget MP where MP.Id=SM.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode,
(select D.UserName Department from Org.Department D where D.Id=SM.DepartmentId) as Department,
                            (select P.UserName from HKP.Process P where P.Id=SM.ProcessId) as Process,
							(select P.UserName from HKP.Process P where P.Id=(select ProcessId from HKP.SubProcess SP where SP.Id=SM.SubProcessId)) as SubProcess
                            FROM [TRN].[SkillManagement] SM";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadEntityDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN SME.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,SME.Id,E.Id EntityId,E.EntityType,E.UserName Entity,E.Code 
                            from ORG.Entity E
							LEFT JOIN [TRN].[SkillManagementEntity] SME ON SME.EntityId=E.Id and SME.SMID='" + ScheduleId + @"'
                            where E.Active = 1 order by SME.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
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
        public ActionResult LoadSkillLevelDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from [TRN].[SkillManagementLevel] where SMID ='" + ScheduleId + "' order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadItemDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,(select distinct PerformanceGroup from TRN.SkillManagementLevel where PerformanceGroup=PerformanceGroupId) as PerformanceGroup,
(select EmployeeName from EmployeeInformation where SystemId=ByWhomId) as ByWhom
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

        [Authorize, HttpPost]
        public ActionResult createEntity(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "TRN.SkillManagementEntity";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and SMID='" + item["SMID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SME" + _Id;
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
        public ActionResult CreatePositionCode(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[SkillManagementPositionCode]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and SMID='" + item["SMID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SPC" + _Id;
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
        public JsonResult CreateLevel(Dictionary<string, object> LevelData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementLevel] where Id<>'" + LevelData["Id"] + "'", out DataSet dsSkillManagementLevelValidation, false, "1");

                DataSet dsSkillManagementLevel;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementLevel] where Id='" + LevelData["Id"] + "'", out dsSkillManagementLevel, false, "1");
                string _Id = "";

                #region data update
                if (dsSkillManagementLevel.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SkillManagementLevel", out _Id);
                    _Id = "SML" + _Id;
                    LevelData["Id"] = _Id;
                    LevelData["SMID"] = Pid;
                    AddNewRow(dsSkillManagementLevel.Tables[0], LevelData);
                }
                else
                {
                    _Id = LevelData["Id"].ToString();
                    LevelData["SMID"] = Pid;
                    EditRow(dsSkillManagementLevel.Tables[0].Rows[0], LevelData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSkillManagementLevel);

                return Json(new { Error = false, Data = LevelData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
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
        #endregion -- Operations
    }
}