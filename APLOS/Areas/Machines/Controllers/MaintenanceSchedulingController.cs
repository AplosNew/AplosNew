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
    public class MaintenanceSchedulingController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public MaintenanceSchedulingController(ISqlRepository R)
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
        public decimal GetItemAutoSequence(string scheduleId)
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM TRN.MaintenanceItem where MaintenanceSchedulingId='" + scheduleId + "'");
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
        public decimal GetStoresAutoSequence(string scheduleId)
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM TRN.MaintenanceStoresConsumable where MaintenanceSchedulingId='"+ scheduleId + "'");
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
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM TRN.MaintenancePersonBudgetCode where MaintenanceSchedulingId='" + scheduleId + "'");
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
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM TRN.MaintenanceTeamDefinition where MaintenanceSchedulingId='" + scheduleId + "'");
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
        public JsonResult GetEActivityCategoryList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from HKP.EmployeeActivityCategory";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> ScheduleData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [Trn].[MaintenanceScheduling] where ScheduleCode='" + ScheduleData["ScheduleCode"] + "'", out DataSet dsMaintenanceScheduleCodeValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [Trn].[MaintenanceScheduling] where StandaredName='" + ScheduleData["StandaredName"] + "'", out DataSet dsMaintenanceScheduleSNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [Trn].[MaintenanceScheduling] where UserName='" + ScheduleData["UserName"] + "'", out DataSet dsMaintenanceScheduleUNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [Trn].[MaintenanceScheduling] where MachineMasterId='" + ScheduleData["MachineMasterId"] + "'", out DataSet dsMaintenanceScheduleMNameValidation, false, "1");

                DataSet dsMaintenanceSchedule;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [Trn].[MaintenanceScheduling] where Id='" + ScheduleData["Id"] + "'", out dsMaintenanceSchedule, false, "1");
                string _Id = "";

                #region data update
                if (dsMaintenanceSchedule.Tables[0].Rows.Count == 0)
                {
                    if (dsMaintenanceScheduleCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Schedule Code Already Exist.");
                    }
                    else if (dsMaintenanceScheduleSNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Standared Name Already Exist.");
                    }
                    else if (dsMaintenanceScheduleUNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    //else if (dsMaintenanceScheduleMNameValidation.Tables[0].Rows.Count > 0)
                    //{
                    //    throw new Exception("Machine Name Already Exist.");
                    //}
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("MaintenanceScheduling", out _Id);
                        _Id = "MS" + _Id;
                        ScheduleData["Id"] = _Id;
                        AddNewRow(dsMaintenanceSchedule.Tables[0], ScheduleData);
                    }
                }
                else
                {
                    _Id = ScheduleData["Id"].ToString();
                    EditRow(dsMaintenanceSchedule.Tables[0].Rows[0], ScheduleData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaintenanceSchedule);

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
                DataSet AssetCount, ItemCount, StoresCount, BudgetCount, TeamCount;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceMachineAsset] where MaintenanceSchedulingId='" + id + "'", out AssetCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceItem] where MaintenanceSchedulingId ='" + id + "'", out ItemCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceStoresConsumable] where MaintenanceSchedulingId ='" + id + "'", out StoresCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenancePersonBudgetCode] where MaintenanceSchedulingId ='" + id + "'", out BudgetCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceTeamDefinition] where MaintenanceSchedulingId ='" + id + "'", out TeamCount, false, "1");

                if (AssetCount.Tables[0].Rows.Count == 0 || ItemCount.Tables[0].Rows.Count == 0 || StoresCount.Tables[0].Rows.Count == 0 || BudgetCount.Tables[0].Rows.Count == 0 || TeamCount.Tables[0].Rows.Count == 0)
                {

                    conC.BeginTransaction();
                    conC.executeQuery("delete from TRN.MaintenanceScheduling where Id ='" + id + @"'");
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
        public ActionResult ItemDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[MaintenanceItem] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult StoresDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[MaintenanceStoresConsumable] where Id ='" + id + @"'");
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
                conC.executeQuery("delete from [TRN].[MaintenancePersonBudgetCode] where Id ='" + id + @"'");
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
                conC.executeQuery("delete from [TRN].[MaintenanceTeamDefinition] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetMachine()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MM.Id MachineMasterId,C.UserName as Category,SC.UserName as Subcategroy,MM.Code,MM.UserName MachineMaster,MM.MachineMake as Make,MM.MachineModel as Model,MM.MachinePerticulars as Particulars
						                from mst.MachineMaster MM
										left join HKP.MachineCategory C ON C.Id=MM.MachineCategoryId
										left join HKP.MachineSubCategory SC ON SC.Id=MM.MachineSubCategoryId";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
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
            string str = @"SELECT EI.SystemId as SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionId
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetUOM()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select UM.Id UOMId, UM.Code,UM.StandardName, UM.UserName UOM from scs.UnitOfMeasurement UM where UM.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetArticle()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MA.Id as ArticleId,MA.StandardName as ArticleName,MM.UserName as MaterialName,
MT.UserName as MaterialType,UM.Id as UOMID,UM.UserName UOM from MST.MaterialMasterArticle MA
left join MST.MaterialMaster MM on MM.Id=MA.MaterialMasterId
left join MST.MaterialGroupMaster MGM ON MGM.Id=MM.MaterialGroupMasterId
left join HKP.MaterialType MT ON MT.Id=MGM.MaterialTypeId
left join scs.UnitOfMeasurement UM ON UM.Active = 1 and UM.Id=MM.BaseUoMId";

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

            string sql = @"select *,(select MP.Code from MST.ManpowerBudget MP where MP.Id=MS.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode,
          (select D.UserName Department from Org.Department D where D.Id=MS.DepartmentId) as Department,
                            MM.UserName as MachineName,MM.MachineMake as Make,MM.MachineModel as Model,MM.MachinePerticulars  as Particulars
                            FROM [Trn].[MaintenanceScheduling] MS
							left join MST.MachineMaster MM ON MM.Id=MS.MachineMasterId where MS.Id='" + ScheduleID + @"'";
            return Json(new { schedule = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadMachineEditData(string ScheduleID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select *,(select EmployeeName from EmployeeInformation where SystemId=InChargePersonId) as InChargePerson
                            FROM DetentionMaster where Id='" + ScheduleID + @"'";
            return Json(new { detention = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadItemEditData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT *,
(select EmployeeName from EmployeeInformation where SystemId=ByWhomId) as ByWhom
FROM [TRN].[MaintenanceItem] where Id='" + ItemId + @"'";
            return Json(new { item = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadParameterEditData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from ItemParameterDetails where Id='" + ParameterId + @"'";
            return Json(new { Parameter = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadStoresEditData(string StoresId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT *,
(select UserName from SCS.UnitOfMeasurement where Active=1 and Id=UOMId) as UOM,
(select StandardName from MST.MaterialMasterArticle where Id=ArticleId) as Article
FROM [TRN].[MaintenanceStoresConsumable] where Id ='" + StoresId + "'";
            return Json(new { Stores = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadBudgetCodeEditData(string BudgetCodeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select MPB.Id,MPB.SNO,MPB.PersonBudgetCodeId,MPB.[Group],MPB.AddedBy,MPB.AddedDate,MPB.AddedFromIP,MPB.UpdatedBy,MPB.UpdatedDate,MPB.UpdatedFromIP,MP.Id ManPowerBudgetId, MP.Code as PersonBudgetCode, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from [TRN].[MaintenancePersonBudgetCode] MPB
                            left join MST.ManpowerBudget MP on MP.Id=MPB.PersonBudgetCodeId
						    left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MP.Active = 1 and  MPB.Id ='" + BudgetCodeId + "'";
            return Json(new { PersonBudget = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadTeamDefinitionEditData(string TeamDefinitionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select MTD.Id, MTD.SNO,TD.UserName TeamDefinition from [TRN].[MaintenanceTeamDefinition] MTD
left join TRN.TeamDefinition TD ON TD.Id=MTD.TeamDefinitionId
                            where MTD.Id ='" + TeamDefinitionId + "'";
            return Json(new { TeamDefinition = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMaintenanceMasterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * ,(select MP.Code from MST.ManpowerBudget MP where MP.Id=MS.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode,
(select D.UserName Department from Org.Department D where D.Id=MS.DepartmentId) as Department,MM.UserName as MachineName,MM.MachineMake,MM.MachineModel
                            FROM [Trn].[MaintenanceScheduling] MS
							left join TRN.MaintenanceMachineGroup MG ON MG.MaintenanceSchedulingId=MS.Id
							left join MST.MachineMaster MM ON MM.Id=MG.MachineMasterId where IsActive=1";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadMachineDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MMA.IsActive,MMA.Id,MMA.SNO,MMA.AssetGroup,MMA.Remarks,MMA.MaintenanceSchedulingId,
MA.Id as AssetId,MA.AssetName,MA.AssetReference,WC.UserName as WorkCenter,MA.WorkCenterMasterId,MA.MachineMasterId,
MM.UserName as MachineName,MM.MachineMake as Make,MM.MachineModel as Model,MA.AssetCode,E.UserName as Entity,MA.EntityId
 from MachineMasterAsset MA
 left Join SCS.WorkCenterMaster WC On WC.id=MA.WorkCenterMasterId
 left Join MST.MachineMaster MM ON MM.Id=MA.MachineMasterId
 left Join ORG.Entity E on E.Id=MA.EntityId
 left Join [TRN].[MaintenanceMachineAsset] MMA ON MMA.AssetId=MA.Id and MMA.MaintenanceSchedulingId='" + ScheduleId + @"'
 where MA.MachineMasterId in (select MachineMasterId from [TRN].[MaintenanceMachineGroup] where MaintenanceSchedulingId='" + ScheduleId + "')";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMachineGroupDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT CAST (CASE WHEN MMG.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,MMG.Id,MM.Id as MachineMasterId
                                  ,CG.StandardName As CompanyGroup
                                  ,MM.Sequence
                                  ,MM.Code
                                  ,MM.ShortName
	                              ,MM.StandardName
                                  ,MM.UserName
	                              ,MC.UserName AS MachineCategory
	                              ,MSC.UserName AS MachineSubCategory
	                              ,SK.UserName AS Skill
                                  ,MM.Description
                                  ,MM.MachineMake
                                  ,MM.MachineModel
                                  ,MM.MachinePerticulars
                                  ,MM.Remarks
                                  ,MM.ProductionMachineQty
                                  ,MM.SampleMachineQty
                                  ,MM.TrainingMachineQty
                                  ,MM.RentMachineQty
                                  ,MM.OtherMachineQty
								  ,MM.ConnectedPower
								  ,MM.RunningLoad
								  ,MM.ConnectedSteam
								  ,MM.RunningSteam
								  ,MM.ConnectedAir
								  ,MM.RunningAir
								  ,MM.MaintanenceScheduleApplicable
                                  ,MM.Active
     
                              FROM MST.MachineMaster As MM
                             LEFT JOIN ORG.CompanyGroup AS CG on CG.ID=MM.CompanyGroupID
                             LEFT JOIN  HKP.MachineCategory AS MC on MC.Id=MM.MachineCategoryId
                             LEFT JOIN HKP. MachineSubCategory AS MSC  on MSC.ID=MM.MachineSubCategoryID
                             LEFT JOIN [TRN].[MaintenanceMachineGroup] MMG ON MMG.MachineMasterId=MM.id and MMG.MaintenanceSchedulingId='" + ScheduleId + @"'
                             LEFT JOIN HKP.Skill AS SK ON SK.ID=MM.SkillId  order by MMG.MachineMasterId  desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadItemDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,(select UserName from HKP.EmployeeActivityCategory where Id=ItemType) as EmployeeActivityCategory,
(select EmployeeName from EmployeeInformation where SystemId=ByWhomId) as ByWhom
FROM [TRN].[MaintenanceItem] where MaintenanceSchedulingId ='" + ScheduleId + "' order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult getParameterData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM ItemParameterDetails where ItemId ='" + ItemId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadStoresDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,
(select UserName from SCS.UnitOfMeasurement where Active=1 and Id=UOMId) as UOM,
(select StandardName from MST.MaterialMasterArticle where Id=ArticleId) as Article
FROM [TRN].[MaintenanceStoresConsumable] where MaintenanceSchedulingId ='" + ScheduleId + "' order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadBudgetCodeDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MPB.Id,MPB.SNO,MPB.PersonBudgetCodeId,MPB.[Group],MPB.AddedBy,MPB.AddedDate,MPB.AddedFromIP,MPB.UpdatedBy,MPB.UpdatedDate,MPB.UpdatedFromIP,MP.Id ManPowerBudgetId, MP.Code as PersonBudgetCode, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from [TRN].[MaintenancePersonBudgetCode] MPB
                            left join MST.ManpowerBudget MP on MP.Id=MPB.PersonBudgetCodeId and MP.Active = 1
						    left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MPB.MaintenanceSchedulingId ='" + ScheduleId + "' order by MPB.SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadTeamDefinitionDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MTD.Id, MTD.SNO,TD.UserName TeamName from [TRN].[MaintenanceTeamDefinition] MTD
left join TRN.TeamDefinition TD ON TD.Id=MTD.TeamDefinitionId
where MTD.MaintenanceSchedulingId ='" + ScheduleId + "' order by MTD.SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadScheduleMachineList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,CASE IsAvoidable WHEN 1 THEN 'Yes' ELSE 'No' END Avoidable,(select EmployeeName from EmployeeInformation where SystemId=InChargePersonId) as InChargePerson,
                            (select UserName from [HKP].[DetentionType] where Id=DetentionTypeId) as DetentionType
                            FROM DetentionMaster";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult CreateMachineGroup(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[MaintenanceMachineGroup]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and MaintenanceSchedulingId='" + item["MaintenanceSchedulingId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "MMG" + _Id;
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
        public ActionResult CreateAsset(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[MaintenanceMachineAsset]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and MaintenanceSchedulingId='" + item["MaintenanceSchedulingId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "MMA" + _Id;
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
        public JsonResult CreateItem(Dictionary<string, object> ItemData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceItem] where Id<>'" + ItemData["Id"] + "'", out DataSet dsMaintenanceItemValidation, false, "1");

                DataSet dsMaintenanceItem;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceItem] where Id='" + ItemData["Id"] + "'", out dsMaintenanceItem, false, "1");
                string _Id = "";

                #region data update
                if (dsMaintenanceItem.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MaintenanceItem", out _Id);
                    _Id = "MI" + _Id;
                    ItemData["Id"] = _Id;
                    ItemData["MaintenanceSchedulingId"] = Pid;
                    AddNewRow(dsMaintenanceItem.Tables[0], ItemData);
                }
                else
                {
                    _Id = ItemData["Id"].ToString();
                    ItemData["MaintenanceSchedulingId"] = Pid;
                    EditRow(dsMaintenanceItem.Tables[0].Rows[0], ItemData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaintenanceItem);

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
                conRack.OpenDataSetThroughAdapter("select * from ItemParameterDetails where Id<>'" + ParameterData["Id"] + "'", out DataSet dsItemParameterDetailsValidation, false, "1");

                DataSet dsItemParameterDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from ItemParameterDetails where Id='" + ParameterData["Id"] + "'", out dsItemParameterDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsItemParameterDetails.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("ItemParameterDetails", out _Id);
                    _Id = "IPD" + _Id;
                    ParameterData["Id"] = _Id;
                    ParameterData["ItemId"] = Pid;
                    AddNewRow(dsItemParameterDetails.Tables[0], ParameterData);
                }
                else
                {
                    _Id = ParameterData["Id"].ToString();
                    ParameterData["ItemId"] = Pid;
                    EditRow(dsItemParameterDetails.Tables[0].Rows[0], ParameterData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsItemParameterDetails);

                return Json(new { Error = false, Data = ParameterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [Authorize, HttpPost]
        public JsonResult CreateStores(Dictionary<string, object> StoresData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceStoresConsumable] where Id<>'" + StoresData["Id"] + "'", out DataSet dsMaintenanceStoresConsumableValidation, false, "1");

                DataSet dsMaintenanceStoresConsumable;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceStoresConsumable] where Id='" + StoresData["Id"] + "'", out dsMaintenanceStoresConsumable, false, "1");
                string _Id = "";

                #region data update
                if (dsMaintenanceStoresConsumable.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MaintenanceStoresConsumable", out _Id);
                    _Id = "MSC" + _Id;
                    StoresData["Id"] = _Id;
                    StoresData["MaintenanceSchedulingId"] = Pid;
                    AddNewRow(dsMaintenanceStoresConsumable.Tables[0], StoresData);
                }
                else
                {
                    _Id = StoresData["Id"].ToString();
                    StoresData["MaintenanceSchedulingId"] = Pid;
                    EditRow(dsMaintenanceStoresConsumable.Tables[0].Rows[0], StoresData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaintenanceStoresConsumable);

                return Json(new { Error = false, Data = StoresData, Message = AplosMessage.Insert });

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
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenancePersonBudgetCode] where Id<>'" + BudgetCodeData["Id"] + "'", out DataSet dsMaintenancePersonBudgetCodeValidation, false, "1");

                DataSet dsMaintenancePersonBudgetCode;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenancePersonBudgetCode] where Id='" + BudgetCodeData["Id"] + "'", out dsMaintenancePersonBudgetCode, false, "1");
                string _Id = "";

                #region data update
                if (dsMaintenancePersonBudgetCode.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MaintenancePersonBudgetCode", out _Id);
                    _Id = "MPB" + _Id;
                    BudgetCodeData["Id"] = _Id;
                    BudgetCodeData["MaintenanceSchedulingId"] = Pid;
                    AddNewRow(dsMaintenancePersonBudgetCode.Tables[0], BudgetCodeData);
                }
                else
                {
                    _Id = BudgetCodeData["Id"].ToString();
                    BudgetCodeData["MaintenanceSchedulingId"] = Pid;
                    EditRow(dsMaintenancePersonBudgetCode.Tables[0].Rows[0], BudgetCodeData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaintenancePersonBudgetCode);

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
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceTeamDefinition] where Id<>'" + TeamDefinitionData["Id"] + "'", out DataSet dsMaintenanceTeamDefinitionValidation, false, "1");

                DataSet dsMaintenanceTeamDefinition;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceTeamDefinition] where Id='" + TeamDefinitionData["Id"] + "'", out dsMaintenanceTeamDefinition, false, "1");
                string _Id = "";

                #region data update
                if (dsMaintenanceTeamDefinition.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MaintenanceTeamDefinition", out _Id);
                    _Id = "MTD" + _Id;
                    TeamDefinitionData["Id"] = _Id;
                    TeamDefinitionData["MaintenanceSchedulingId"] = Pid;
                    AddNewRow(dsMaintenanceTeamDefinition.Tables[0], TeamDefinitionData);
                }
                else
                {
                    _Id = TeamDefinitionData["Id"].ToString();
                    TeamDefinitionData["MaintenanceSchedulingId"] = Pid;
                    EditRow(dsMaintenanceTeamDefinition.Tables[0].Rows[0], TeamDefinitionData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaintenanceTeamDefinition);

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