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
    public class QualityActionUpdateReportController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public QualityActionUpdateReportController(ISqlRepository R)
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
            string str = @"select distinct EI.SystemId,EI.EmployeeName, mb.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    ,EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection  from 
TRN.QualityControlDetails QCD
left join dbo.EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
where EI.EmployeeStatus='Active' and EI.EmployeeCode is not null and QCD.Status='Inprogress' and QCD.ResponsiblePersonId is not null";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
       
        [HttpGet, Authorize]
        public ActionResult LoadQualityActionUpdateReport(string FromDate, string ToDate, string ResponsiblePersonId, bool ActionApplicable)
        {
            string FilterDate = string.Empty;
            string ResponsiblePerson = string.Empty;
            string Applicable = string.Empty;

            if (FromDate != null && ToDate != null && FromDate != "undefined" && ToDate != "undefined")
            {
                FilterDate = " where convert(Date,QCD.AddedDate) between '"+ FromDate + "' and '" + ToDate + "'";
            }

            if (ResponsiblePersonId != null && ResponsiblePersonId != "undefined")
            {
                ResponsiblePerson = " and QCD.ResponsiblePersonId = '" + ResponsiblePersonId + "'";
            }

            if(ActionApplicable is true)
            {
                Applicable = " and QCD.GradeId in (select Id from MST.QualityGradeDetails where ActionApplicable=1)";
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select QCD.Id as ParaTransactionId,P.UserName Process,E.UserName Entity,QMM.UserName Issue,QC.ProductionOrderId + ' / ' + QC.LotNumber PODetails,									                
QMP.SNO ParameterSeq,PM.UserName Parameter,QCD.Value,QGD.GradeName Grade,format(QCD.AddedDate,'dd-MMM-yyyy') as Date,
WCM.UserName WorkCenter,QAM.ActionToBeTakenName,QCD.Status
,reverse(stuff(reverse((select QRM.UserName + ',' from [HKP].[QualityManagementReasonMaster] QRM where QRM.Id in (select ReasonId from MST.QualityManagementParameterReason QPR where QPR.Id in (select ReasonId from TRN.QualityActionTakenUpdate where ParameterId=QCD.Id))  for xml path(''))),1,1,'')) ReasonName
,reverse(stuff(reverse((select ReasonName + ',' from TRN.QualityActionTakenUpdate where ParameterId=QCD.Id and ReasonId is null for xml path(''))),1,1,'')) ManualReason
,reverse(stuff(reverse((select ActionTaken + ',' from TRN.QualityActionTakenUpdate where ParameterId=QCD.Id for xml path(''))),1,1,'')) ActionTaken
,reverse(stuff(reverse((select EI.EmployeeName + ',' from EmployeeInformation EI Where EI.SystemId in (select ActionById from TRN.QualityActionTakenUpdate where ParameterId=QCD.Id) for xml path(''))),1,1,'')) ActionBy
,reverse(stuff(reverse((select Remarks + ',' from TRN.QualityActionTakenUpdate where ParameterId=QCD.Id for xml path(''))),1,1,'')) Remarks
from TRN.QualityControlDetails QCD
left join TRN.QualityControl QC on QC.Id=QCD.QCId
left Join hkp.Process P on P.Id=QC.ProcessId
left Join Org.Entity E on E.Id=QC.EntityId
left join MST.QualityManagementMaster QMM on QMM.Id=QC.IssueId
left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
left join hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
left join SCS.WorkCenterMaster WCM on WCM.Id=QCD.WorkCenterId
left join MST.QualityActionToBeTakenDetails QAM on QAM.Id=QCD.ActionToBeTaken" + FilterDate + @" " + ResponsiblePerson + @" " + Applicable + @"";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}