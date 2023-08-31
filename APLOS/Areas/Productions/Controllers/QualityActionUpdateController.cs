using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Productions.Controllers
{
    public class QualityActionUpdateController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public QualityActionUpdateController(ISqlRepository R)
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

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select distinct EI.SystemId,EI.EmployeeName, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    ,EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection  from 
TRN.QualityControlDetails QCD
left join dbo.EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
where EI.EmployeeStatus='Active' and QCD.ResponsiblePersonId is not null";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetActionBy()
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

        [HttpGet, Authorize]
        public ActionResult LoadQualityActionUpdateHeader(string FromDate, string ToDate, string ResponsiblePersonId)
        {
            string FilterDate = string.Empty;
            string ResponsiblePerson = string.Empty;

            if (FromDate != null && ToDate != null && FromDate != "undefined" && ToDate != "undefined")
            {
                FilterDate = " and convert(Date,QCD.AddedDate) between '"+ FromDate + "' and '" + ToDate + "'";
            }

            if (ResponsiblePersonId != null && ResponsiblePersonId != "undefined")
            {
                ResponsiblePerson = " and ResponsiblePersonId = '" + ResponsiblePersonId + "'";
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct QC.Id as HeaderId,format(QC.AddedDate,'dd-MMM-yyyy') as Date,E.Id EntityId,E.UserName Entity,P.Id ProcessId,P.UserName Process,
QC.IssueId,QMM.UserName Issue,EI.SystemId CheckedById,EI.EmployeeName CheckedBy,QC.ProductionOrderId PONo,QC.LotNumber,
Article=STUFF((select distinct ','+MA.StandardName from trn.ProductionOrderDetail Pod 
left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId=so.Id
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
where Pod.ProductionOrderId=QC.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
PS.UserName POStatus from TRN.QualityControlDetails QCD
left join TRN.QualityControl QC on QC.Id=QCD.QCId
left join ORG.Entity E on E.Id=QC.EntityId
left join hkp.Process P on P.Id=QC.ProcessId
left join MST.QualityManagementMaster QMM on QMM.Id=QC.IssueId
left join EmployeeInformation EI on EI.SystemId=QC.ProductionInchargeId
left join TRN.ProductionOrder PO on PO.Id=QC.ProductionOrderId
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
where QCD.Status='InProgress' and QCD.GradeId in (select Id from MST.QualityGradeDetails where ActionApplicable=1) " + FilterDate + @" " + ResponsiblePerson + @"";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadQualityActionUpdateParameterListGetDetails(string HeaderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select QCD.Id ParameterId,PM.UserName Parameter,QCD.Status,UOM.UserName UOM,QCD.Value,QMP.Max,QMP.Min,WC.UserName WorkCenter,QGD.GradeName,
QAD.ActionToBeTakenName,EI.EmployeeName ResponsiblePerson,QCD.Remarks,QCD.ItemId  from TRN.QualityControlDetails QCD
left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
left join hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
left join SCS.UnitOfMeasurement UOM on UOM.Id=QMP.UOMId
left join SCS.WorkCenterMaster WC on WC.Id=QCD.WorkCenterId
left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
left join MST.QualityActionToBeTakenDetails QAD on QAD.Id=QCD.ActionToBeTaken
left join EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
where QCD.Status='InProgress' and QCD.GradeId in (select Id from MST.QualityGradeDetails where ActionApplicable=1)
and QCD.QCId='" + HeaderId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadQualityActionTakenListGetDetails(string ParameterId, string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select QPR.SNO,QPR.ReasonId,QRM.UserName ReasonName,QAT.ActionTaken,QAT.ActionById,EI.EmployeeName ActionBy,QAT.Remarks from [MST].[QualityManagementParameterReason] QPR 
	left join [HKP].[QualityManagementReasonMaster] QRM on QRM.Id=QPR.ReasonId
	left join [TRN].[QualityActionTakenUpdate] QAT on QAT.ParameterId='"+ ParameterId + @"' 
	left join EmployeeInformation EI on EI.SystemId=QAT.ActionById
	where QPR.IsActive=1 and QPR.ParameterId='"+ ItemId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetReasonNameLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,ReasonName as Text from [MST].[QualityManagementParameterReason]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createActionTaken(Dictionary<string, object> ActionTakenData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[QualityActionTakenUpdate] where ReasonId='" + ActionTakenData["ReasonId"] + "' and ParameterId='" + Pid + "'", out DataSet dsQualityActionTakenUpdateReasonValidation, false, "1");

                DataSet dsQualityActionTakenUpdate;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[QualityActionTakenUpdate] where Id='" + ActionTakenData["Id"] + "'", out dsQualityActionTakenUpdate, false, "1");
                string _Id = "";

                #region data update
                if (ActionTakenData["SNO"] == null)
                {
                    throw new Exception("SNO is required");
                }
                else
                {
                    if (ActionTakenData["ReasonId"] == null)
                    {
                        throw new Exception("Reason is required");
                    }
                    else
                    {
                        if (dsQualityActionTakenUpdate.Tables[0].Rows.Count == 0)
                        {
                            if (dsQualityActionTakenUpdateReasonValidation.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("Reason Name Already Exist.");
                            }
                            else
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("QualityActionTakenUpdate", out _Id);
                                _Id = "QAT" + _Id;
                                ActionTakenData["Id"] = _Id;
                                ActionTakenData["ParameterId"] = Pid;
                                AddNewRow(dsQualityActionTakenUpdate.Tables[0], ActionTakenData);
                            }
                        }
                        else
                        {
                            _Id = ActionTakenData["Id"].ToString();
                            ActionTakenData["ParameterId"] = Pid;
                            EditRow(dsQualityActionTakenUpdate.Tables[0].Rows[0], ActionTakenData);
                        }
                    }
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityActionTakenUpdate);

                return Json(new { Error = false, Data = ActionTakenData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadMaintenanceStatusDetailsList(string ToDate,string FromDate,string Status)
        {
            string Filter = string.Empty;
           
            if (Status == "All")
            {
                Filter = " and (MPD.ActualDate is not null or MPD.ActualDate is null) and MPD.PlannedDate is not null";
            }
            else if (Status == "Completed")
            {
                Filter = " and MPD.ActualDate is not null and MPD.PlannedDate is not null";
            }
            else
            {
                Filter = " and MPD.ActualDate is null and MPD.PlannedDate is not null";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MS.Id,Format(MPD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,MPD.Id as PlannedId,MMA.EntityId,E.UserName Entity,MS.UserName ScheduleName,MM.UserName MachineName,MM.MachineMake Make,
MM.MachineModel Model,MS.ScheduleCode,MS.ResponsiblePersoneBgtCodeId,MB.Code ResponsiblePersonBudgetCode,MMA.AssetId,MA.AssetName,MA.AssetCode,MA.AssetReference,
MMA.WorkCenterMasterId,WC.UserName WorkCenter,MS.ScheduleDays,
 isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
MS.StandardScheduleMinutes,MS.Remarks,(select D.UserName Department from Org.Department D where D.Id=MS.DepartmentId) as Department,MS.MaintenanceGroup
,EI.EmployeeName as ActionableResponsiblePerson,RP.ResponsiblePersonId
from TRN.Maintenancescheduling MS
 left join MST.ManpowerBudget MB ON MB.id=MS.ResponsiblePersoneBgtCodeId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 left join TRN.MachineAssetPlannedDetails MPD ON MPD.AssetId=MMA.Id
 left join TRN.ResponsiblePlannedDetails RP ON RP.PlannedId=MPD.Id and RP.IsActive=1
 left Join EmployeeInformation EI ON EI.SystemId=RP.ResponsiblePersonId
 where MS.IsActive=1 and MMA.Id is not null 
 and Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + "' " + Filter + @" order by MPD.PlannedDate";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult LoadPendingMaintenanceSchedule(string ActResponsiblePerson, string todate,string fromdate, string Status)
        {
            string Filter = string.Empty;
            string Responsible = string.Empty;
            if(ActResponsiblePerson==null)
            {
                Responsible = "";
            }
            else
            {
                Responsible = "and (select top 1 ResponsiblePersonId from TRN.ResponsiblePlannedDetails RP where RP.PlannedId=MPD.Id and RP.IsActive=1 and ResponsiblePersonId='" + ActResponsiblePerson + "') = '" + ActResponsiblePerson + "'";
            }
            
            if (Status == "All")
            {
                Filter = " and (MPD.ActualDate is not null or MPD.ActualDate is null) and MPD.PlannedDate is not null";
            }
            else if (Status == "Completed")
            {
                Filter = " and MPD.ActualDate is not null and MPD.PlannedDate is not null order by MPD.ActualDate desc";
            }
            else
            {
                Filter = " and MPD.ActualDate is null and MPD.PlannedDate is not null order by MPD.PlannedDate";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MS.Id,Format(MPD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,MPD.Id as PlannedId,MMA.EntityId,E.UserName Entity,MS.UserName ScheduleName,MM.UserName MachineName,MM.MachineMake Make,
MM.MachineModel Model,MS.ScheduleCode,MS.ResponsiblePersoneBgtCodeId,MB.Code ResponsiblePersonBudgetCode,MMA.AssetId,MA.AssetName,MA.AssetCode,MA.AssetReference,
MMA.WorkCenterMasterId,WC.UserName WorkCenter,MS.ScheduleDays,MMA.Id as MachineAssetId,
 isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
MS.StandardScheduleMinutes,MS.Remarks,(select D.UserName Department from Org.Department D where D.Id=MS.DepartmentId) as Department,MS.MaintenanceGroup,MPD.FileName,'Pid' as test,
  Reverse(stuff(Reverse((select EmployeeName+',' from EmployeeInformation where SystemId in (select ResponsiblePersonId from TRN.ResponsiblePlannedDetails AP where AP.PlannedId=MPD.Id and AP.IsActive=1) for xml path(''))),1,1,'')) ActionableResponsiblePerson
 from TRN.Maintenancescheduling MS
 left join MST.ManpowerBudget MB ON MB.id=MS.ResponsiblePersoneBgtCodeId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 left join TRN.MachineAssetPlannedDetails MPD ON MPD.AssetId=MMA.Id
 --left join TRN.ResponsiblePlannedDetails RP ON RP.PlannedId=MPD.Id and RP.IsActive=1
 --left Join EmployeeInformation EI ON EI.SystemId=RP.ResponsiblePersonId
 where MS.IsActive=1 and
 Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (MPD.ActualDate) end between '" + fromdate + "' and '" + todate + "' " + Filter + @" " + Responsible + @"";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult createResponsible(List<Dictionary<string, object>> DataList, string PId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[ResponsiblePlannedDetails]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);
                            if (dv.Count == 0)
                            {
                                if (item["ActualMinutes"].IsNotNull() && Convert.ToInt32(item["ActualMinutes"]) != 0)
                                {
                                    bplib.clsGenID genid = new bplib.clsGenID();
                                    genid.GenID(TableName, out _Id);
                                    item["Id"] = "RPD" + _Id;
                                    item["PlannedId"] = PId;
                                    AddNewRow(dsProdBooked.Tables[0], item);
                                }
                                else
                                {
                                    throw new CustomException("Please enter Actual Minutes greater than 0 and proceed!");
                                }
                            }
                            else
                            {
                                if (item["ActualMinutes"].IsNotNull() && Convert.ToInt32(item["ActualMinutes"]) != 0)
                                {
                                    item["PlannedId"] = PId;
                                    DataRow drpb = dv[0].Row;
                                    EditRow(drpb, item);
                                }
                                else
                                {
                                    throw new CustomException("Please enter Actual Minutes greater than 0 and proceed!");
                                }
                            }
                            clsStaticInfo obj = new clsStaticInfo();
                            obj.SaveDataSets(dsProdBooked);
                    }
                    return Json(new { Message = AplosMessage.Insert });
                }
                else
                {
                    throw new CustomException("Please select atleast one actionable person and proceed!");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public ActionResult createPlanned(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[MachineAssetPlannedDetails]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("select * from [TRN].[ResponsiblePlannedDetails] where PlannedId='" + item["Id"] + "'", out DataSet dsResponsibleValidation, false, "1");
                        objCon.OpenDataSetThroughAdapter("select * from [TRN].[MachineAssetPlannedDetails] where ActualDate is not null and Id='" + item["Id"] + "'", out DataSet dsMachineAssetPlannedDetailsValidation, false, "1");
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);
                        if (dsResponsibleValidation.Tables[0].Rows.Count > 0)
                        {
                            if (dv.Count == 0)
                            {
                                DateTime FromDt = Convert.ToDateTime(item["FromDate"]);
                                DateTime ToDt = Convert.ToDateTime(item["ActualDate"]);
                                TimeSpan t = ToDt.Subtract(FromDt);
                                int N = t.Days;
                                TimeSpan ts;
                                DateTime date1 = Convert.ToDateTime(item["FromTime"]);
                                DateTime date2 = Convert.ToDateTime(item["ToTime"]);
                                DateTime NextDayDate = date2.AddDays(N);
                                if (FromDt == ToDt)
                                {
                                    ts = date2 - date1;
                                }
                                else
                                {
                                    DateTime NextDayDate2 = date2.AddDays(N);
                                    ts = NextDayDate2 - date1;
                                }
                                TimeSpan Nd = NextDayDate - date1;
                                int minutes = (int)Nd.TotalMinutes;

                                if (minutes >= 720 || minutes < 0)
                                {
                                    item["ToTime"] = NextDayDate;
                                    item["Minute"] = Nd.TotalMinutes;
                                }
                                else
                                {
                                    item["ToTime"] = date2;
                                    item["Minute"] = ts.TotalMinutes;
                                }

                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID(TableName, out _Id);
                                item["Id"] = "APD" + _Id;
                                AddNewRow(dsProdBooked.Tables[0], item);

                            }
                            else
                            {
                                if (item["FileName"] != null)
                                {
                                    DateTime ActualDate = Convert.ToDateTime(item["ActualDate"]);
                                    DateTime LastDayDate = DateTime.Today.AddDays(-1);
                                    if (dsMachineAssetPlannedDetailsValidation.Tables[0].Rows.Count > 0)
                                    {
                                        if (ActualDate == DateTime.Today || ActualDate == LastDayDate)
                                        {
                                            DataRow drpb = dv[0].Row;
                                            DateTime FromDt = Convert.ToDateTime(item["FromDate"]);
                                            DateTime ToDt = Convert.ToDateTime(item["ActualDate"]);
                                            TimeSpan t = ToDt.Subtract(FromDt);
                                            int N = t.Days;
                                            TimeSpan ts;
                                            DateTime date1 = Convert.ToDateTime(item["FromTime"]);
                                            DateTime date2 = Convert.ToDateTime(item["ToTime"]);
                                            DateTime NextDayDate = date2.AddDays(N);
                                            if (FromDt == ToDt)
                                            {
                                                ts = date2 - date1;
                                            }
                                            else
                                            {
                                                DateTime NextDayDate2 = date2.AddDays(N);
                                                ts = NextDayDate2 - date1;
                                            }
                                            TimeSpan Nd = NextDayDate - date1;
                                            int minutes = (int)Nd.TotalMinutes;

                                            if (minutes >= 720 || minutes < 0)
                                            {
                                                item["ToTime"] = NextDayDate;
                                                item["Minute"] = Nd.TotalMinutes;
                                            }
                                            else
                                            {
                                                item["ToTime"] = date2;
                                                item["Minute"] = ts.TotalMinutes;
                                            }
                                            EditRow(drpb, item);
                                        }
                                        else
                                        {
                                            throw new CustomException("Actual date should be today's date or yesterday's date only!");
                                        }
                                    }
                                    else
                                    {
                                        if (ActualDate > DateTime.Today)
                                        {
                                            throw new Exception("Actual date cannot be greater than today's date!");
                                        }
                                        else
                                        {
                                            DataRow drpb = dv[0].Row;
                                            DateTime FromDt = Convert.ToDateTime(item["FromDate"]);
                                            DateTime ToDt = Convert.ToDateTime(item["ActualDate"]);
                                            TimeSpan t = ToDt.Subtract(FromDt);
                                            TimeSpan ts;
                                            int N = t.Days;
                                            DateTime date1 = Convert.ToDateTime(item["FromTime"]);
                                            DateTime date2 = Convert.ToDateTime(item["ToTime"]);
                                            DateTime NextDayDate = date2.AddDays(N);
                                            if (FromDt == ToDt)
                                            {
                                                 ts = date2 - date1;
                                            }
                                            else
                                            {
                                                DateTime NextDayDate2 = date2.AddDays(N);
                                                 ts = NextDayDate2 - date1;
                                            }
                                            TimeSpan Nd = NextDayDate - date1;
                                            int minutes = (int)Nd.TotalMinutes;

                                            if (minutes >= 720 || minutes < 0)
                                            {
                                                item["ToTime"] = NextDayDate;
                                                item["Minute"] = Nd.TotalMinutes;
                                            }
                                            else
                                            {
                                                item["ToTime"] = date2;
                                                item["Minute"] = ts.TotalMinutes;
                                            }
                                            EditRow(drpb, item);
                                        }
                                    }
                                }
                                else
                                {
                                    throw new CustomException("Please Add Attachment and Proceed!");
                                }

                            }
                        }
                        else
                        {
                            throw new CustomException("Please Add Actionable Person and Proceed!");
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

        [Authorize, HttpPost]
        public ActionResult SaveDefault(IEnumerable<System.Web.HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the order first");

                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetMSADocumentPath(), fileName);

                    var directory = ResourcesPathReader.GetMSADocumentPath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetMSADocumentPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetMSADocumentPath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "SELECT * FROM [TRN].[MachineAssetPlannedDetails] WHERE Id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["FileName"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id)
        {
            try
            {
                return Json(_sqlRepository.GetDataCollection("select * from [TRN].[MachineAssetPlannedDetails]  where Id='" + Id + "'"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion -- Operations
    }
}