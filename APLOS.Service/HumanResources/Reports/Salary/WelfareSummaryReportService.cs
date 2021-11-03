#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Model.HumanResources;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

#endregion Using

namespace Library.Service.HumanResources
{
    public class WelfareSummaryReportService : IWelfareSummaryReportService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository; 
        private readonly IUnitOfWork _unitOfWork;

        public WelfareSummaryReportService(         
            
             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository         
            ) 
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;  
        }

        #endregion Constructor

        public DataTable GetBonusRegisterSql(string companyGroupId, string plantId)
        {
            try
            {
                var cmdText = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
                                    --,e.DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                    ,REPLACE(CONVERT(VARCHAR(11), e.ApprovedDateTime, 106), ' ', '-') ApprovedDateTime
                                    ,CONVERT(date, e.ApprovedDateTime) vApprovedDateTime
									,ISNULL(e.IsApproved,0) IsApproved
                                    --Resignation
								    ,ISNULL(rsg.ApprovalStatus,'') rsgApprovalStatus
									,REPLACE(CONVERT(VARCHAR(11), rsg.ApprovedEffectiveDate, 106), ' ', '-') resignationApprovedEffectiveDate
                                    ,CONVERT(DATE, rsg.ApprovedEffectiveDate) resignationApprovedEffectiveDateS
									,REPLACE(CONVERT(VARCHAR(11), rsg.ResignationDate, 106), ' ', '-') ApplicantResignationDate
                                    --,REPLACE(CONVERT(VARCHAR(11), e.DOSDate, 106), ' ', '-') ApplicantSeparationDate
								    ,CONVERT(DATE, rsg.ApprovedDate) resignationApprovedEntryDateS
									,CONVERT(date,rsg.AddedDate) resignationAddedDate
								    ,CONVERT(DATE, rsg.EffectiveDate) EffectiveDateS
									,REPLACE(CONVERT(VARCHAR(11),rsg.EffectiveDate,106),' ','-') RsgSelfEffectiveDate
                                    ,ProbationPeriod= case when e.DOCIsDay=1 then e.DOCDay
									ELSE e.DOCMonth*30 end
									,e.DOCDay,e.DOCMonth
                                    ,e.EmployeeStatus EmployeeStatus
									,e.DOJ+(CASE WHEN e.DOCIsDay=1 THEN e.DOCDay
												ELSE e.DOCMonth*30 END) DOCs
									,Replace(CONVERT(VARCHAR(11),
									e.DOJ+(case when e.DOCIsDay=1 then e.DOCDay
												else e.DOCMonth*30 end)
									 , 106), ' ', '-') DOC
                                    ,Replace(CONVERT(VARCHAR(11),e.DOC,106),' ','-') empDOC
                                    ,Replace(CONVERT(VARCHAR(11),(e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end - ProbationPeriodAlertBeforeDays)), 106), ' ', '-') AlermStartDate

                                    ,DATEDIFF(day, GETDATE(), (e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end))) DaysToGO
									,DATEDIFF(day, GETDATE(), rsg.ApprovedEffectiveDate) RsgDaysToGO
	                                ,e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end - ProbationPeriodAlertBeforeDays)  AlermStartDateCmp

                                        ,Replace(CONVERT(VARCHAR(11),PRE.ConfirmationDate,106),' ','-') PREConfirmationDate
									 ,convert(date,PRE.ConfirmationDate) PREConfirmationDateExc
									  , pre.Completed preCompleted
                                    ,isnull(e.IsConfirmed,0) IsConfirmedProbation
									--Probation Confirmation Date
									,CONVERT(DATE,e.ProbationConfirmEntryDate) ProbationConfirmEntryDate
                                    ,mpb.EntityId,mpb.PositionId,ISNULL(hs.IsPositionCodeApplicable,0) IsPositionCodeApplicable
									--Increment Due list
									--,SINDD.NextDueDate IncrementNextDueDate,SINDD.EffectiveDate IncrementEffectiveDate
                                    --emp ids
                                     ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
									
                                    FROM EmployeeInformation e
                                    LEFT OUTER JOIN ORG.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN ORG.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN ORG.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN ORG.Unit eu on eu.id=e.UnitId
                                   -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                           -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                   -- left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
                                    WHERE e.EmployeeStatus = 'Active' AND ";
                return _sqlRepository.GetDataTable(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}