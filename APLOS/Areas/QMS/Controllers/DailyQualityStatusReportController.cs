using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.QMS.Controllers
{
    public class DailyQualityStatusReportController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public DailyQualityStatusReportController(ISqlRepository R)
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
        public JsonResult GetEntityLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select * from (select Distinct EntityId as Value,E.UserName as Text from TRN.ProductionSummary 
left join ORG.Entity E on E.Id=EntityId)EI order by EI.Text";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult LoadDailyQualityStatusReport(string FromDate, string ToDate, string PartyNature, string EntityId)
        {
            string FilterPartyNature = string.Empty;
            string FilterEntity = string.Empty;

            if (PartyNature != "null")
            {
                FilterPartyNature = " and Y.PartyNature = '" + PartyNature + "'";
            }
            if (EntityId != "null")
            {
                FilterEntity = " and Y.EntityId='" + EntityId + "'";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string
                sql = @"select (select isnull(day(Min(AddedDate)),0) from TRN.ProductionSummary where ProductionOrderId=Y.PONo and LotNumber = Y.LotNumber) - day(getdate()) Days,(case when sum(Convert(Int,Y.RejectValue)) > 0  then 'Reject'
when sum(Convert(Int,Y.FailValue)) > 0  then 'Fail'
when sum(Y.EntryMissing) > 0  then 'Pending'
else 'Pass' end) QualityStatus,
Date=(select format(Min(AddedDate),'dd-MMM-yyyy')  from TRN.ProductionSummary where ProductionOrderId=Y.PONo and LotNumber=Y.LotNumber and AddedDate between '" + FromDate +"' and '"+ ToDate + @"'),
MOLineItemNo = STUFF((select distinct ','+ XMOI.Id from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
where Y.PONo=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
POStatus=(select UserName from hkp.ProductionStatus where Id=(select ProductionStatusId from TRN.ProductionOrder where Id=Y.PONo)),
Y.PONo,Y.LotNumber,Article=STUFF((select distinct ','+MA.StandardName from
											MST.MaterialMasterArticle MA
											left join TRN.MasterOrderItem moi on moi.ArticleId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=xp.Id
											where Y.PONo=Xpod.ProductionOrderId for xml path('') ), 1, 1, '')
,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where Y.PONo=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
sum(Convert(Int,Y.PassValue)) Pass,sum(Convert(Int,Y.FailValue)) + isnull(sum(Convert(Int,Y.FailGrade)),0) Fail,sum(Convert(Int,Y.RejectValue)) Reject,
Sum(Y.EntryMissing) MissingEntry,sum(Y.ToClose) ToClose,sum(Y.ToConfirm) ToConfirm,
Y.PartyNature,
Y.EntityId,
Y.Entity,
Reverse(stuff(Reverse((select QR.Grade +', ' from MST.QualityRemark QR																			
where QR.PONo=Y.PONo and QR.LotNo=Y.LotNumber and QR.EntityId=Y.EntityId for xml PATH(''))),1,2,'')) Grade,
Reverse(stuff(Reverse((select (select EmployeeName from EmployeeInformation where SystemId=QR.ByWhomId) +', ' from MST.QualityRemark QR																			
where QR.PONo=Y.PONo and QR.LotNo=Y.LotNumber and QR.EntityId=Y.EntityId for xml PATH(''))),1,2,'')) ByWhom,
Reverse(stuff(Reverse((select format(QR.AddedDate,'dd-MMM-yyyy') + '-' + QR.Comment +', ' from MST.QualityRemark QR																			
where QR.PONo=Y.PONo and QR.LotNo=Y.LotNumber  and QR.EntityId=Y.EntityId for xml PATH(''))),1,2,'')) CommentDetails
from (select distinct QCData.QCDate,PELP.PONo,isnull(QCData.LotNumber,PELP.LotNumber) LotNumber,PELP.Entity,PELP.EntityId,PELP.PartyNature, 
PELP.IssueId,PELP.Issue,PELP.ParameterId,PELP.Parameter,PELP.UOM,QCData.Value,QCData.GradeName,QCData.ParameterRemark,QCData.ActionToBeTakenName,QCData.ResponsiblePerson,QCData.PassValue,QCData.FailValue,QCData.RejectValue,QCData.FailGrade,QCData.ToClose,QCData.ToConfirm,
QCData.HeaderId,QCData.ChildId,QCData.QCDDate, (Case When (QCData.Value is null or QCData.Value = '0') then 1 else 0 end) EntryMissing from (select  Z.PONo,Z.LotNumber,Z.EntryLevel,Z.ApplicableLot,Z.Entity,Z.EntityId,Z.PartyNature,ELP.Process,ELP.Issue,ELP.IssueId,ELP.Parameter,ELP.ParameterId,ELP.UOM from (select P.PONo,P.LotNumber,
(case when len(P.LotNumber) > 0 then 'LOT' else 'PO' end) EntryLevel,
(case when len(P.LotNumber) > 0 then P.LotNumber else P.PONo end) ApplicableLot,
PartyNature= STUFF((select distinct ','+XP.PartyNature from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where P.PONo=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
P.EntityId,E.UserName Entity
from  (select distinct PS.ProductionOrderId  PONo,null LotNumber,EntityId
from TRN.ProductionSummary PS
where PS.AddedDate between '" + FromDate + "' and '" + ToDate + @"'
union
select distinct PS.ProductionOrderId  PONo,PS.LotNumber,EntityId 
from TRN.ProductionSummary PS
where PS.AddedDate between '" + FromDate + "' and '" + ToDate + @"')P
left join org.Entity E on E.Id=P.EntityId
)Z
left join (select EntryLevel,P.UserName Process,QMM.UserName Issue,IssueId,QMP.Id ParameterId,PM.UserName Parameter,UOM.UserName UOM from MST.POQualityPlanDetails POD
left join HKP.Process P on P.Id=POD.ProcessId
left join MST.QualityManagementMaster QMM on QMM.Id=POD.IssueId
left join MST.QualityManagementParameterItem QMP on QMP.QMID=POD.IssueId and QMP.ReportApplicable=1
left join HKP.ParameterMaster PM on PM.Id=QMP.ParameterId
left join SCS.UnitOfMeasurement UOM on UOM.Id=QMP.UOMId) ELP on ELP.EntryLevel=Z.EntryLevel)PELP
left join (select PQD.EntryLevel,QC.IssueId,QMM.UserName IssueName,QCD.ItemId ParameterId,PM.UserName ParameterName,QC.LotNumber,QC.ProductionOrderId,
 QCD.Value,QGD.GradeName,QCD.Remarks ParameterRemark,QAT.ActionToBeTakenName,EI.EmployeeName ResponsiblePerson,
 format(QC.AddedDate,'dd-MMM-yyyy') QCDate,format(QCD.AddedDate,'dd-MMM-yyyy') QCDDate,QC.Id HeaderId,QCD.Id ChildId,QGD.IsPassValue PassValue,QGD.IsFailValue FailValue,QGD.IsRejectValue RejectValue,
(case when QCD.GradeId is null then 1 end) FailGrade,
 (case when (QGD.IsFailValue <> 0 and QCD.Status not in ('Close','Complete')) then 1 else 0 end) ToClose,
 (case when (QGD.IsFailValue <> 0 and QCD.Status not in ('Complete')) then 1 else 0 end) ToConfirm,QC.ProcessId,
 (case when PQD.EntryLevel='PO' then QC.ProductionOrderId else QC.LotNumber end) ApplicableLot
 from TRN.QualityControlDetails QCD
 left join TRN.QualityControl QC on QC.Id=QCD.QCId
  left join MST.POQualityPlanDetails PQD on PQD.IssueId=QC.IssueId
 left join MST.QualityManagementMaster QMM on QMM.Id=PQD.IssueId
 left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
 left join Hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
 left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
 left join MST.QualityActionToBeTakenDetails QAT on QAT.Id=QCD.ActionToBeTaken
 left join EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId) QCData on 
 QCData.IssueId=PELP.IssueId and 
QCData.ParameterId=PELP.ParameterId
 and QCData.ProductionOrderId=PELP.PONo
and QCData.ApplicableLot=PELP.ApplicableLot
and QCData.EntryLevel=PELP.EntryLevel
)Y 
 where 1=1 " + FilterPartyNature + @" " + FilterEntity + @"
 Group By Y.PONo,Y.LotNumber,Y.PartyNature,Y.EntityId,Y.Entity
 order by (Case when sum(Convert(Int,Y.RejectValue)) > 0  then 'A'
when sum(Convert(Int,Y.FailValue)) > 0  then 'B'
when sum(Y.EntryMissing) > 0  then 'C'
else 'D' end) , (select isnull(day(Min(AddedDate)),0) from TRN.ProductionSummary where ProductionOrderId=Y.PONo and LotNumber = Y.LotNumber) - day(getdate())
";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getDailyQualityStatusParameterData(string FromDate, string ToDate, string ProductionOrderId, string LotNumber, string EntityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string
                sql = @"select Y.*,(case when Y.RejectValue > 0  then 'Reject'
when Y.FailValue > 0  then 'Fail'
when Y.EntryMissing > 0  then 'Pending'
else 'Pass' end) ParameterGradeStatus from (select distinct QCData.QCDate,PELP.PONo,isnull(QCData.LotNumber,PELP.LotNumber) LotNumber,PELP.Entity,PELP.EntityId,PELP.PartyNature,PELP.Days, 
PELP.IssueId,PELP.Issue IssueName,PELP.Process,PELP.ParameterId,PELP.Parameter ParameterName,PELP.UOM,QCData.Value,QCData.GradeName,QCData.ParameterRemark,QCData.ActionToBeTakenName,QCData.ResponsiblePerson,QCData.PassValue,QCData.FailValue,QCData.RejectValue,QCData.FailGrade,QCData.ToClose,QCData.ToConfirm,
QCData.HeaderId,QCData.ChildId,QCData.QCDDate,QCData.QCDTime, (Case When (QCData.Value is null or QCData.Value = '0') then 1 else 0 end) EntryMissing from (select  Z.PONo,Z.LotNumber,Z.EntryLevel,Z.ApplicableLot,Z.Entity,Z.EntityId,Z.PartyNature,ELP.Process,ELP.Issue,ELP.IssueId,ELP.Parameter,ELP.ParameterId,ELP.UOM 
,(select isnull(day(Min(AddedDate)),0) from TRN.ProductionSummary where ProductionOrderId=Z.PONo and LotNumber = Z.LotNumber and ProcessId=ELP.ProcessId) - day(getdate()) Days
from (select P.PONo,P.LotNumber,
(case when len(P.LotNumber) > 0 then 'LOT' else 'PO' end) EntryLevel,
(case when len(P.LotNumber) > 0 then P.LotNumber else P.PONo end) ApplicableLot,
PartyNature= STUFF((select distinct ','+XP.PartyNature from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where P.PONo=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
P.EntityId,E.UserName Entity
from  (select distinct PS.ProductionOrderId  PONo,null LotNumber,EntityId
from TRN.ProductionSummary PS
where PS.AddedDate between '" + FromDate + "' and '" + ToDate + @"'
union
select distinct PS.ProductionOrderId  PONo,PS.LotNumber,EntityId 
from TRN.ProductionSummary PS
where PS.AddedDate between '" + FromDate + "' and '" + ToDate + @"')P
left join org.Entity E on E.Id=P.EntityId
)Z
left join (select EntryLevel,P.UserName Process,QMM.UserName Issue,IssueId,QMP.Id ParameterId,PM.UserName Parameter,UOM.UserName UOM,POD.ProcessId from MST.POQualityPlanDetails POD
left join HKP.Process P on P.Id=POD.ProcessId
left join MST.QualityManagementMaster QMM on QMM.Id=POD.IssueId
left join MST.QualityManagementParameterItem QMP on QMP.QMID=POD.IssueId and QMP.ReportApplicable=1
left join HKP.ParameterMaster PM on PM.Id=QMP.ParameterId
left join SCS.UnitOfMeasurement UOM on UOM.Id=QMP.UOMId) ELP on ELP.EntryLevel=Z.EntryLevel)PELP
left join (select PQD.EntryLevel,QC.IssueId,QMM.UserName IssueName,QCD.ItemId ParameterId,PM.UserName ParameterName,QC.LotNumber,QC.ProductionOrderId,
 QCD.Value,QGD.GradeName,QCD.Remarks ParameterRemark,QAT.ActionToBeTakenName,EI.EmployeeName ResponsiblePerson,
 format(QC.AddedDate,'dd-MMM-yyyy') QCDate,format(QCD.AddedDate,'dd-MMM-yyyy') QCDDate,format(QCD.AddedDate,'hh:mm-tt') QCDTime,QC.Id HeaderId,QCD.Id ChildId,QGD.IsPassValue PassValue,QGD.IsFailValue FailValue,QGD.IsRejectValue RejectValue,
(case when QCD.GradeId is null then 1 end) FailGrade,
 (case when (QGD.IsFailValue <> 0 and QCD.Status not in ('Close','Complete')) then 1 else 0 end) ToClose,
 (case when (QGD.IsFailValue <> 0 and QCD.Status not in ('Complete')) then 1 else 0 end) ToConfirm,QC.ProcessId,
 (case when PQD.EntryLevel='PO' then QC.ProductionOrderId else QC.LotNumber end) ApplicableLot
 from TRN.QualityControlDetails QCD
 left join TRN.QualityControl QC on QC.Id=QCD.QCId
  left join MST.POQualityPlanDetails PQD on PQD.IssueId=QC.IssueId
 left join MST.QualityManagementMaster QMM on QMM.Id=PQD.IssueId
 left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
 left join Hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
 left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
 left join MST.QualityActionToBeTakenDetails QAT on QAT.Id=QCD.ActionToBeTaken
 left join EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId) QCData on 
 QCData.IssueId=PELP.IssueId and 
QCData.ParameterId=PELP.ParameterId
 and QCData.ProductionOrderId=PELP.PONo
and QCData.ApplicableLot=PELP.ApplicableLot
and QCData.EntryLevel=PELP.EntryLevel)Y  
where Y.PONo='" + ProductionOrderId+ "' and Y.LotNumber='" + LotNumber+ "' and Y.EntityId='" + EntityId + "' order by Y.Days,Y.QCDDate,Y.QCDTime";
                        return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
 
        [Authorize, HttpGet]
        public ActionResult getCommentEntryData(string PONo, string LotNo, string EntityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id,Comment,isnull(PONo,'" + PONo + "') PONo,isnull(LotNo,'" + LotNo + "') LotNo,isnull(EntityId,'" + EntityId + "') EntityId,ByWhomId," +
                "(select EmployeeName from EmployeeInformation Where SystemId=ByWhomId) ByWhom " +
                " from [MST].[QualityRemark] where PONo='"+ PONo + "' and LotNo='"+ LotNo + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
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
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and EI.EmployeeCode is not null";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createComments(Dictionary<string, object> CommentsData, string POId, string LotNumber, string EntityId)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityRemark] where Comment='" + CommentsData["Comment"] + "'", out DataSet dsQualityRemarkCommentValidation, false, "1");

                DataSet dsQualityRemark;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityRemark] where Id='" + CommentsData["Id"] + "'", out dsQualityRemark, false, "1");
                string _Id = "";

                #region data update
                if (dsQualityRemark.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("QualityRemark", out _Id);
                    _Id = "QR" + _Id;
                    CommentsData["Id"] = _Id;
                    CommentsData["PONo"] = POId;
                    CommentsData["LotNo"] = LotNumber;
                    CommentsData["EntityId"] = EntityId;
                    AddNewRow(dsQualityRemark.Tables[0], CommentsData);
                }
                else
                {
                    _Id = CommentsData["Id"].ToString();
                    EditRow(dsQualityRemark.Tables[0].Rows[0], CommentsData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityRemark);

                return Json(new { Error = false, Data = CommentsData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public ActionResult getCommentData(string POId, string LotNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select QR.*,(select EmployeeName from EmployeeInformation Where SystemId=ByWhomId) ByWhom from [MST].[QualityRemark] QR
where QR.PONo='" + POId + "' and QR.LotNo='" + LotNo + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadCommentEntryEditData(string CommentId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select QR.*,(select EmployeeName from EmployeeInformation Where SystemId=ByWhomId) ByWhom from [MST].[QualityRemark] QR where Id='" + CommentId + @"'";
            return Json(new { comment = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CommentsDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[QualityRemark] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet]
        public ActionResult GetDailyQualityStatusParameterJobCardReport(string fromDate, string toDate, string IssueId, string ProductionOrderId, string LotNumber, string EntityId, string QualityStatus, string Date)
        {
            try
            {
                NewJobCardReportService app = new NewJobCardReportService();
                
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = app.GetDailyQualityStatusParameterJobCardReport(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, IssueId, ProductionOrderId, LotNumber, EntityId, QualityStatus, Date);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                return RenderReportAsExcel(workbook, reportFileName);
                
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        private ActionResult RenderReportAsExcel(IWorkbook workbook, string fileName)
        {
            workbook.SaveAs(fileName + ".xls", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
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
        #endregion -- Operations
    }
}