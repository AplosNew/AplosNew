using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;

namespace Library.Service.PerformanceManagement
{

    public class JobEvaluationReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        string TableName = "dbo.JobEvaluation";
        string TableName1 = "dbo.JobEvaluationChild";

        public JobEvaluationReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> LoadAllPositionDetailsForSelection(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select p.*,div.UserName as Division, sdiv.UserName as SubDivision, desg.UserName as Designation, dept.UserName as Department, sec.UserName as Section, subsec.UserName as SubSection from ORG.Position p left join ORG.Division div on div.Id=p.DivisionId
                               left join ORG.SubDivision sdiv on sdiv.Id=p.SubDivisionId
							   left join HKP.Designation desg on desg.Id=p.DesignationId
							   left join ORG.Department dept on dept.Id=p.DepartmentId
							   left join ORG.Section sec on sec.id=p.sectionId
							   left join org.SubSection subsec on subsec.Id=p.SubSectionId
                               WHERE p.CompanyGroupId='" + identity.CompanyGroupId + @"'
                               AND isnull(p.Id,'') not in (select isnull(PositionCodeId,'') from dbo.JobEvaluation where Id='" + Id + @"')
                               order by p.Code";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> LoadAllEvaluatorDetails(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.EmployeeStatus='Active'
                   AND isnull(Emp.SystemID,'') not in (select isnull(EvaluatorNameId,'') from dbo.JobEvaluation where Id='" + Id + @"')
                  order by EMP.EmployeeCode";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetSearchedDetails(string PositionCodeId, string EmpSystemId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "";
                if (!string.IsNullOrEmpty(PositionCodeId) && string.IsNullOrEmpty(EmpSystemId))
                {
                     sql = @"select distinct je.*, p.Code as PositionCode, p.UserName as PositionName,div.Id as DivisionId ,div.UserName as Division,sdiv.Id as SubDivisionId, sdiv.UserName as SubDivision
                                   ,desg.Id as DesignationId,desg.UserName as Designation,dept.Id as DepartmentId ,dept.UserName as Department,sec.Id as SectionId ,sec.UserName as Section, subsec.Id as SubSectionId, subsec.UserName as SubSection
								   ,pa.Id as PerformanceAttributeId,pa.UserName as PerformanceAttribute, jemcc.Code as JECode, jemcc.Category as JEMCCCategory, jemcc.Criteria as JEMCCCriteria, jemcc.Points as JEMCCPoints
								   ,jemc.Dimension1ControlName, jemc.Dimension1ControlLevel, jemc.Dimension2ControlName, jemc.Dimension2ControlLevel, jec.Factoring,JEPoints=(jemcc.Points*jec.Factoring), jec.Remarks as JECRemarks
								   from dbo.JobEvaluation je left join ORG.Position p on p.Id=je.PositionCodeId
								   left join ORG.Division div on div.Id=p.DivisionId
								   left join ORG.SubDivision sdiv on sdiv.Id=p.SubDivisionId
								   left join HKP.Designation desg on desg.Id=p.DesignationId
							       left join ORG.Department dept on dept.Id=p.DepartmentId
							       left join ORG.Section sec on sec.id=p.sectionId
							       left join org.SubSection subsec on subsec.Id=p.SubSectionId
								   left join dbo.JobEvaluationChild jec on jec.JobEvaluationId=je.Id
								   left join dbo.JobEvaluationMaster jem on jem.Id=jec.JobEvaluationMasterId
								   left join HKP.PerformanceAttribute pa on pa.Id=jem.PerformanceAttributeId
								   left join dbo.JobEvaluationMasterChild2 jemcc on jemcc.JobEvaluationMasterId=jem.Id
								   left join dbo.JobEvaluationMasterChild jemc on jemc.JobEvaluationMasterId=jem.Id
								   left join dbo.EmployeeInformation EMP on EMP.PositionID=p.Id
                                   WHERE p.CompanyGroupId='" + identity.CompanyGroupId + @"' and EMP.GroupID='" + identity.CompanyGroupId + @"' and EMP.EmployeeStatus='Active'
                                   and je.PositionCodeId='" + PositionCodeId + @"'
                                   order by p.Code";
                }

                if (string.IsNullOrEmpty(PositionCodeId) && !string.IsNullOrEmpty(EmpSystemId))
                {
                    sql = @"select distinct je.*, p.Code as PositionCode, p.UserName as PositionName,div.Id as DivisionId ,div.UserName as Division,sdiv.Id as SubDivisionId, sdiv.UserName as SubDivision
                                   ,desg.Id as DesignationId,desg.UserName as Designation,dept.Id as DepartmentId ,dept.UserName as Department,sec.Id as SectionId ,sec.UserName as Section, subsec.Id as SubSectionId, subsec.UserName as SubSection
								   ,pa.Id as PerformanceAttributeId,pa.UserName as PerformanceAttribute, jemcc.Code as JECode, jemcc.Category as JEMCCCategory, jemcc.Criteria as JEMCCCriteria, jemcc.Points as JEMCCPoints
								   ,jemc.Dimension1ControlName, jemc.Dimension1ControlLevel, jemc.Dimension2ControlName, jemc.Dimension2ControlLevel, jec.Factoring,JEPoints=(jemcc.Points*jec.Factoring), jec.Remarks as JECRemarks
								   from dbo.JobEvaluation je left join ORG.Position p on p.Id=je.PositionCodeId
								   left join ORG.Division div on div.Id=p.DivisionId
								   left join ORG.SubDivision sdiv on sdiv.Id=p.SubDivisionId
								   left join HKP.Designation desg on desg.Id=p.DesignationId
							       left join ORG.Department dept on dept.Id=p.DepartmentId
							       left join ORG.Section sec on sec.id=p.sectionId
							       left join org.SubSection subsec on subsec.Id=p.SubSectionId
								   left join dbo.JobEvaluationChild jec on jec.JobEvaluationId=je.Id
								   left join dbo.JobEvaluationMaster jem on jem.Id=jec.JobEvaluationMasterId
								   left join HKP.PerformanceAttribute pa on pa.Id=jem.PerformanceAttributeId
								   left join dbo.JobEvaluationMasterChild2 jemcc on jemcc.JobEvaluationMasterId=jem.Id
								   left join dbo.JobEvaluationMasterChild jemc on jemc.JobEvaluationMasterId=jem.Id
								   left join dbo.EmployeeInformation EMP on EMP.PositionID=p.Id
                                   WHERE p.CompanyGroupId='" + identity.CompanyGroupId + @"' and EMP.GroupID='" + identity.CompanyGroupId + @"' and EMP.EmployeeStatus='Active'
                                   and EMP.SystemId='" + EmpSystemId + @"'
                                   order by EMP.EmployeeCode";
                }

                if (!string.IsNullOrEmpty(PositionCodeId) && !string.IsNullOrEmpty(EmpSystemId))
                {
                    sql = @"select distinct je.*, p.Code as PositionCode, p.UserName as PositionName,div.Id as DivisionId ,div.UserName as Division,sdiv.Id as SubDivisionId, sdiv.UserName as SubDivision
                                   ,desg.Id as DesignationId,desg.UserName as Designation,dept.Id as DepartmentId ,dept.UserName as Department,sec.Id as SectionId ,sec.UserName as Section, subsec.Id as SubSectionId, subsec.UserName as SubSection
								   ,pa.Id as PerformanceAttributeId,pa.UserName as PerformanceAttribute, jemcc.Code as JECode, jemcc.Category as JEMCCCategory, jemcc.Criteria as JEMCCCriteria, jemcc.Points as JEMCCPoints
								   ,jemc.Dimension1ControlName, jemc.Dimension1ControlLevel, jemc.Dimension2ControlName, jemc.Dimension2ControlLevel, jec.Factoring,JEPoints=(jemcc.Points*jec.Factoring), jec.Remarks as JECRemarks
								   from dbo.JobEvaluation je left join ORG.Position p on p.Id=je.PositionCodeId
								   left join ORG.Division div on div.Id=p.DivisionId
								   left join ORG.SubDivision sdiv on sdiv.Id=p.SubDivisionId
								   left join HKP.Designation desg on desg.Id=p.DesignationId
							       left join ORG.Department dept on dept.Id=p.DepartmentId
							       left join ORG.Section sec on sec.id=p.sectionId
							       left join org.SubSection subsec on subsec.Id=p.SubSectionId
								   left join dbo.JobEvaluationChild jec on jec.JobEvaluationId=je.Id
								   left join dbo.JobEvaluationMaster jem on jem.Id=jec.JobEvaluationMasterId
								   left join HKP.PerformanceAttribute pa on pa.Id=jem.PerformanceAttributeId
								   left join dbo.JobEvaluationMasterChild2 jemcc on jemcc.JobEvaluationMasterId=jem.Id
								   left join dbo.JobEvaluationMasterChild jemc on jemc.JobEvaluationMasterId=jem.Id
								   left join dbo.EmployeeInformation EMP on EMP.PositionID=p.Id
                                   WHERE p.CompanyGroupId='" + identity.CompanyGroupId + @"' and EMP.GroupID='" + identity.CompanyGroupId + @"' and EMP.EmployeeStatus='Active'
                                   and je.PositionCodeId='" + PositionCodeId + @"' and EMP.SystemId='" + EmpSystemId + @"'
                                   order by p.Code";
                }


                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public DataTable GetReportData(string PositionCodeId, string DivisionId, string SubDivisionId, string DepartmentId, string SectionId, string SubSectionId, string DesignationId)
        {
            try
            {
                string _sql = @"select distinct je.*, p.Code as PositionCode, p.UserName as PositionName,div.Id as DivisionId ,div.UserName as Division,sdiv.Id as SubDivisionId, sdiv.UserName as SubDivision
                                   ,desg.Id as DesignationId,desg.UserName as Designation,dept.Id as DepartmentId ,dept.UserName as Department,sec.Id as SectionId ,sec.UserName as Section, subsec.Id as SubSectionId, subsec.UserName as SubSection
								   ,pa.Id as PerformanceAttributeId,pa.UserName as PerformanceAttribute, jemcc.Code as JECode, jemcc.Category as JEMCCCategory, jemcc.Criteria as JEMCCCriteria, jemcc.Points as JEMCCPoints
								   ,jemc.Dimension1ControlName, jemc.Dimension1ControlLevel, jemc.Dimension2ControlName, jemc.Dimension2ControlLevel, jec.Factoring,JEPoints=(jemcc.Points*jec.Factoring), jec.Remarks as JECRemarks
								   from dbo.JobEvaluation je left join ORG.Position p on p.Id=je.PositionCodeId
								   left join ORG.Division div on div.Id=p.DivisionId
								   left join ORG.SubDivision sdiv on sdiv.Id=p.SubDivisionId
								   left join HKP.Designation desg on desg.Id=p.DesignationId
							       left join ORG.Department dept on dept.Id=p.DepartmentId
							       left join ORG.Section sec on sec.id=p.sectionId
							       left join org.SubSection subsec on subsec.Id=p.SubSectionId
								   left join dbo.JobEvaluationChild jec on jec.JobEvaluationId=je.Id
								   left join dbo.JobEvaluationMaster jem on jem.Id=jec.JobEvaluationMasterId
								   left join HKP.PerformanceAttribute pa on pa.Id=jem.PerformanceAttributeId
								   left join dbo.JobEvaluationMasterChild2 jemcc on jemcc.JobEvaluationMasterId=jem.Id
								   left join dbo.JobEvaluationMasterChild jemc on jemc.JobEvaluationMasterId=jem.Id
								   left join dbo.EmployeeInformation EMP on EMP.PositionID=p.Id
                                   where p.Id IN ( " + PositionCodeId + " ) and sdiv.Id IN ( " + SubDivisionId + " ) and div.Id IN ( " + DivisionId + " ) and dept.Id IN ( " + DepartmentId + " ) and sec.Id IN ( " + SectionId + " ) and subsec.Id IN( " + SubSectionId + " ) and desg.Id IN( " + DesignationId + " ) ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
