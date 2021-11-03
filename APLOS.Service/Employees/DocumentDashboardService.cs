#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Service.Accounts;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.ViewModel.Organizations;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

#endregion Using

namespace Library.Service.Employees
{
    public class DocumentDashboardService : IDocumentDashboardService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<PreRecruitmentEmployee> _preRecruitmentEmployeeRepository;

        public DocumentDashboardService(
             IRepositoryAsync<PreRecruitmentEmployee> preRecruitmentEmployeeRepository
            , ISqlRepository sqlRepository
            ) : base()
        {
            _preRecruitmentEmployeeRepository = preRecruitmentEmployeeRepository;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<OrgStructureListViewModel> OrgStructureList(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"select distinct u.StandardName ColumnName,IsNULL(e.RType,'Position') as Rtype,e.Sequence,po.Sequence from (
                           select  distinct StandardName from [ORG].[StructureRelationship] as ee where CompanyGroupId='" + CompanyGroupId + @"' and ( CompanyId Is null or CompanyId='" + CompanyId + @"') and  RType = 'Entity'  union
                           select  distinct StandardName from [ORG].[StructureRelationship] as pp where CompanyGroupId='" + CompanyGroupId + @"' and ( CompanyId Is null or CompanyId='" + CompanyId + @"') and  RType = 'position' ) u
                           left outer join(select id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Entity' ) e on e.StandardName = u.StandardName
                           left outer join(select id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Position' ) po on po.StandardName = u.StandardName
                           order by Rtype,e.Sequence,po.Sequence";

                return _preRecruitmentEmployeeRepository.SqlQuery<OrgStructureListViewModel>(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> OverDueStatus(string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string conditions = null;
            string edConditions = null;
            string prdConditions = null;

            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string EmployeeTypeOrCategory = null;
            string eDEmployeeTypeOrCategory = null;
            string pRDEmployeeTypeOrCategory = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;

            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;

            try
            {
                if (DocumentCategoryId == null || DocumentCategoryId == "")
                {
                    doccat = "";
                    cddoccat = "";
                }
                else
                {
                    doccat = "and DocumentCategoryId ='" + DocumentCategoryId + @"'";
                    cddoccat = "and cd.ComplianceDocumentCategoryId ='" + DocumentCategoryId + @"'";
                }

                if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
                {
                    docSubCatg = "";
                    cddocSubCatg = "";
                }
                else
                {
                    docSubCatg = "and DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                    cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                }

                if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
                {
                    eDEmployeeTypeOrCategory = "";
                    pRDEmployeeTypeOrCategory = "";
                    EmployeeTypeOrCategory = "";
                }
                else
                {
                    EmployeeTypeOrCategory = "and EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
                    eDEmployeeTypeOrCategory = "and EmpC.Id ='" + EmplyeeTypeOrCategoryId + @"'";
                    pRDEmployeeTypeOrCategory = "";
                }
                if (ComplianceDocumentId == null || ComplianceDocumentId == "")
                {
                    ComplianceDocument = "";
                    cdComplianceDocument = "";
                }
                else
                {
                    ComplianceDocument = "and ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
                    cdComplianceDocument = "and cd.Id ='" + ComplianceDocumentId + @"'";
                }
                if (DocumentationBy == null || DocumentationBy == "")
                {
                    DocBy = "";
                    cdDocBy = "";
                }
                else
                {
                    DocBy = "and DocumentationBy ='" + DocumentationBy + @"'";
                    cdDocBy = "and cd.DocumentationBy ='" + DocumentationBy + @"'";
                }
                if (ResponsiblePersonId == null || ResponsiblePersonId == "")
                {
                    ResponsiblePerson = "";
                    cdResponsiblePerson = "";
                }
                else
                {
                    ResponsiblePerson = "and ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                    cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                }
                if (Importance == null || Importance == "")
                {
                    Impt = "";
                    cdImpt = "";
                }
                else
                {
                    Impt = "and Importance ='" + Importance + @"'";
                    cdImpt = "and cd.Importance ='" + Importance + @"'";
                }
                if (OptionalOrMandatory == null || OptionalOrMandatory == "")
                {
                    OptOrMandt = "";
                    cdOptOrMandt = "";
                }
                else
                {
                    OptOrMandt = "and OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                    cdOptOrMandt = "and cd.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                }
                if (DocumentType == null || DocumentType == "")
                {
                    docType = "";
                    cddocType = "";
                }
                else
                {
                    docType = "and DocumentType ='" + DocumentType + @"'";
                    cddocType = "and cd.DocumentType ='" + DocumentType + @"'";
                }
                conditions = cddoccat + cddocSubCatg + EmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
                edConditions = cddoccat + cddocSubCatg + eDEmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
                prdConditions = cddoccat + cddocSubCatg + pRDEmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;

                var sql = @"SELECT 'Compliance' AS Employmentstage, (SELECT (SELECT CASE WHEN SUM(Completed)=0 THEN NULL ELSE SUM(Completed) END FROM(
									SELECT  COUNT(*) AS Completed FROM PreRecruitmentDocument AS PRD
									JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
									LEFT JOIN hkp.ComplianceDocument as cd on cd.Id = prd.ComplianceDocumentId
										WHERE PRD.FileId IS NOT NULL and PRD.IsCopied = 0 AND CONVERT(date, PRD.UpdatedDate)<= CONVERT(date, PRD.DueDate) AND DocumentType ='Compliance' " + prdConditions + @"

									UNION
									SELECT  COUNT(*) AS Completed FROM EmployeeDocument AS ED
									JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
									LEFT JOIN hkp.ComplianceDocument as cd on cd.Id = ED.ComplianceDocumentId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									--EmployeeCategory Join Done

										WHERE ED.FileId IS NOT NULL AND CONVERT(date, ED.UpdatedDate)<= CONVERT(date, ED.DueDate) AND DocumentType ='Compliance' " + edConditions + @"
									) x)*100 /
										( SELECT SUM(Completed) FROM(
									SELECT  COUNT(*) AS Completed FROM PreRecruitmentDocument AS PRD
										JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
										left join hkp.ComplianceDocument as cd on cd.Id = prd.ComplianceDocumentId
										WHERE PRD.FileId IS NOT NULL and PRD.IsCopied = 0 AND DocumentType ='Compliance' " + prdConditions + @"
									UNION
									SELECT  COUNT(*) AS Completed FROM EmployeeDocument AS ED
										JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
										LEFT JOIN HKP.ComplianceDocument as cd on cd.Id = ED.ComplianceDocumentId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									--EmployeeCategory Join Done
										WHERE ED.FileId IS NOT NULL AND DocumentType ='Compliance' " + edConditions + @"
									) x)
									) as Completed,
								(SELECT COUNT(Id) doc1 FROM TempDocDashboard WHERE  DocumentType ='Compliance'  and DueDate is not null   " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" and DueDate < getDate()
								) OverAlldoc,
								(SELECT SUM(id) FROM(
									SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE PreRecruitmentEmployeeId IS NULL AND DueDate<>'' AND DocumentType='Compliance'  " + doccat + @"  " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY EmployeeId
									UNION
									SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE  DueDate<>'' AND DocumentType='Compliance'  " + doccat + @" " + docSubCatg + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
									and DueDate < getDate() GROUP BY PreRecruitmentEmployeeId) x ) OverAllemp,
								(SELECT COUNT(Id) doc1 FROM TempDocDashboard WHERE Segment = 1 AND DocumentType ='Compliance'  AND DueDate is not null  " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" ) doc1,
								(SELECT SUM(id) FROM(
									SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE PreRecruitmentEmployeeId IS NULL AND DueDate<>'' and Segment = 1 AND DocumentType='Compliance'  " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY EmployeeId
									UNION
									SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE  DueDate<>'' AND DocumentType='Compliance' and Segment = 1 " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"  GROUP BY PreRecruitmentEmployeeId
								) x) emp1,

								(select count(Id) doc2 from TempDocDashboard where Segment = 2 and  DocumentType ='Compliance' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" ) doc2,
								(SELECT SUM(id) FROM(
									SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE PreRecruitmentEmployeeId IS NULL AND DueDate<>'' and Segment = 2 AND DocumentType='Compliance'  " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY EmployeeId
									UNION
									SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE  DueDate<>'' AND Segment = 2 and DocumentType='Compliance'  " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY PreRecruitmentEmployeeId
								     ) x) emp2,

								(select count(Id) doc3 from TempDocDashboard where Segment = 3 and  DocumentType ='Compliance'   " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" ) doc3,
								(SELECT SUM(id) FROM(
									SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE PreRecruitmentEmployeeId IS NULL AND DueDate<>'' and Segment = 3 AND DocumentType='Compliance'  " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY EmployeeId
									UNION
									SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE  DueDate<>'' AND Segment = 3 and DocumentType='Compliance'  " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"  GROUP BY PreRecruitmentEmployeeId
									) x) emp3
								UNION
								select 'Company',(SELECT (SELECT CASE WHEN SUM(Completed)=0 THEN NULL ELSE SUM(Completed) END FROM(
										SELECT  COUNT(*) AS Completed FROM PreRecruitmentDocument AS PRD
										JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
										left join hkp.ComplianceDocument as cd on cd.Id = prd.ComplianceDocumentId
											WHERE PRD.FileId IS NOT NULL and PRD.IsCopied = 0 AND CONVERT(date, PRD.UpdatedDate)<= CONVERT(date, PRD.DueDate) AND DocumentType ='Company' " + prdConditions + @"
										UNION
										SELECT  COUNT(*) AS Completed FROM EmployeeDocument AS ED
										JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
										LEFT JOIN HKP.ComplianceDocument as cd on cd.Id = ED.ComplianceDocumentId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

									--EmployeeCategory Join Done

											WHERE ED.FileId IS NOT NULL AND CONVERT(date, ED.UpdatedDate)<= CONVERT(date, ED.DueDate) AND DocumentType ='Company' " + edConditions + @"
										) x)*100 /
											( SELECT SUM(Completed) FROM(
										SELECT  COUNT(*) AS Completed FROM PreRecruitmentDocument AS PRD
											JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
											left join hkp.ComplianceDocument as cd on cd.Id = prd.ComplianceDocumentId
											WHERE PRD.FileId IS NOT NULL and PRD.IsCopied = 0 AND DocumentType ='Company' " + prdConditions + @"
										UNION
										SELECT  COUNT(*) AS Completed FROM EmployeeDocument AS ED
											JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
											LEFT JOIN HKP.ComplianceDocument as cd on cd.Id = ED.ComplianceDocumentId

											LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
											LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
											LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

									    --EmployeeCategory Join Done

											WHERE ED.FileId IS NOT NULL AND DocumentType ='Company' " + edConditions + @"
										) x) ) as Completed,
								(SELECT COUNT(Id) doc1 FROM TempDocDashboard WHERE  DocumentType ='Company'  and DueDate IS NOT NULL   " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" and DueDate < getDate()  ) OverAlldoc,
								(SELECT SUM(id) FROM(
										SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE PreRecruitmentEmployeeId IS NULL AND DueDate<>'' AND DocumentType='Company' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"  GROUP BY EmployeeId
										UNION
										SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE  DueDate<>'' AND DocumentType='Company' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
										and DueDate < getDate() GROUP BY PreRecruitmentEmployeeId) x) OverAllemp,
								 (select count(Id) doc1 from TempDocDashboard where Segment = 1 and DocumentType ='Company'  and DueDate is not null   " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @"  " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @") doc1,
								(SELECT SUM(id) FROM(
										SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE PreRecruitmentEmployeeId IS NULL and  Segment = 1 AND DueDate<>'' AND DocumentType='Company' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY EmployeeId
										UNION
										SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE  DueDate<>'' and  Segment = 1 AND DocumentType='Company' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY PreRecruitmentEmployeeId
										) x) emp1,

								(select count(Id) doc2 from TempDocDashboard where Segment = 2 and DocumentType ='Company'  and DueDate is not null   " + doccat + @" " + docSubCatg + @"" + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" ) doc2,
								(SELECT SUM(id) FROM(
										SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE PreRecruitmentEmployeeId IS NULL and  Segment = 2 AND DueDate<>'' AND DocumentType='Company' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"  GROUP BY EmployeeId
										UNION
										SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE  DueDate<>'' and  Segment = 2 AND DocumentType='Company' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY PreRecruitmentEmployeeId
										) x) emp2,

								(select count(Id) doc3 from TempDocDashboard where Segment = 3 and DocumentType ='Company'  and DueDate is not null   " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @") doc3,
								(SELECT SUM(id) FROM(
									   SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE PreRecruitmentEmployeeId IS NULL and  Segment = 3 AND DueDate<>'' AND DocumentType='Company' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY  EmployeeId
									   UNION
									   SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE  DueDate<>'' and  Segment = 3 AND DocumentType='Company' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY PreRecruitmentEmployeeId
									   ) x) emp3
								UNION
								select 'Regulatory',(SELECT (SELECT CASE WHEN SUM(Completed)=0 THEN NULL ELSE SUM(Completed) END FROM(
											SELECT  COUNT(*) AS Completed FROM PreRecruitmentDocument AS PRD
											JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
											left join hkp.ComplianceDocument as cd on cd.Id = prd.ComplianceDocumentId
												WHERE PRD.FileId IS NOT NULL and PRD.IsCopied = 0 AND CONVERT(date, PRD.UpdatedDate)<= CONVERT(date, PRD.DueDate) AND DocumentType ='Regulatory' " + prdConditions + @"

								UNION
									SELECT  COUNT(*) AS Completed FROM EmployeeDocument AS ED
									JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
									left join hkp.ComplianceDocument as cd on cd.Id = ED.ComplianceDocumentId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

									--EmployeeCategory Join Done
										WHERE ED.FileId IS NOT NULL AND CONVERT(date, ED.UpdatedDate)<= CONVERT(date, ED.DueDate) AND DocumentType ='Regulatory' " + edConditions + @"
									) x)*100 /
									( SELECT SUM(Completed) FROM(
									SELECT  COUNT(*) AS Completed FROM PreRecruitmentDocument AS PRD
										JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
										LEFT JOIN HKP.ComplianceDocument as cd on cd.Id = prd.ComplianceDocumentId

									--EmployeeCategory Join Done
										WHERE PRD.FileId IS NOT NULL and PRD.IsCopied = 0 AND DocumentType ='Regulatory' " + prdConditions + @"
									UNION
									SELECT  COUNT(*) AS Completed FROM EmployeeDocument AS ED
										JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
										LEFT JOIN HKP.ComplianceDocument as cd on cd.Id = ED.ComplianceDocumentId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

									--EmployeeCategory Join Done

										WHERE ED.FileId IS NOT NULL AND DocumentType ='Regulatory' " + edConditions + @"
									) x) ) as Completed,
								(select count(Id) doc1 from TempDocDashboard where  DocumentType ='Regulatory'  and DueDate is not null   " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" and DueDate < getDate()  ) OverAlldoc,
								(SELECT SUM(id) FROM(
										SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE PreRecruitmentEmployeeId IS NULL AND DueDate<>'' AND DocumentType='Regulatory' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY EmployeeId
										UNION
										SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE  DueDate<>'' AND DocumentType='Regulatory'" + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
										and DueDate < getDate() GROUP BY PreRecruitmentEmployeeId) x) OverAllemp,
								 (SELECT COUNT(Id) doc1 FROM TempDocDashboard WHERE Segment = 1 AND DocumentType ='Regulatory' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" ) doc1,
								(SELECT SUM(id) FROM(
									SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE Segment = 1 and PreRecruitmentEmployeeId IS NULL AND DueDate<>'' AND DocumentType='Regulatory' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @"  " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY EmployeeId
									UNION
									SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE Segment = 1 and  DueDate<>'' AND DocumentType='Regulatory'" + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY PreRecruitmentEmployeeId
									) x) emp1,

								(select count(Id) doc2 from TempDocDashboard where Segment = 2 and DocumentType ='Regulatory'  and DueDate is not null  " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" ) doc2,
								(SELECT SUM(id) FROM(
										SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE Segment = 2 and PreRecruitmentEmployeeId IS NULL AND DueDate<>'' AND DocumentType='Regulatory' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY EmployeeId
										UNION
										SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE Segment = 2 and  DueDate<>'' AND DocumentType='Regulatory'" + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY PreRecruitmentEmployeeId
										) x) emp2,

								(select count(Id) doc3 from TempDocDashboard where Segment = 3 and DocumentType ='Regulatory'  and DueDate is not null   " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" ) doc3,
								(SELECT SUM(id) FROM(
										SELECT COUNT(DISTINCT EmployeeId) AS Id,EmployeeId FROM TempDocDashboard WHERE Segment = 3 and PreRecruitmentEmployeeId IS NULL AND DueDate<>'' AND DocumentType='Regulatory' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @"  " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY EmployeeId
										UNION
										SELECT COUNT(DISTINCT PreRecruitmentEmployeeId) AS Id,PreRecruitmentEmployeeId FROM TempDocDashboard WHERE Segment = 3 and  DueDate<>'' AND DocumentType='Regulatory'" + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" GROUP BY PreRecruitmentEmployeeId
										) x) emp3";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> DailyOverDueStatus(string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string parameters = string.Empty;
            string cParameters = string.Empty;
            string doccat = null;
            string docSubCatg = null;
            string EmployeeTypeOrCategory = null;
            string ComplianceDocument = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;
            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;
            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
                cddoccat = "";
            }
            else
            {
                doccat = "and DocumentCategoryId ='" + DocumentCategoryId + @"'";
                cddoccat = "and cd.ComplianceDocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
                cddocSubCatg = "";
            }
            else
            {
                docSubCatg = "and DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
                cdComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
                cdComplianceDocument = "and cd.Id ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
                cdDocBy = "";
            }
            else
            {
                DocBy = "and DocumentationBy ='" + DocumentationBy + @"'";
                cdDocBy = "and cd.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
                cdResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
                cdImpt = "";
            }
            else
            {
                Impt = "and Importance ='" + Importance + @"'";
                cdImpt = "and cd.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
                cdOptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                cdOptOrMandt = "and cd.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
                cddocType = "";
            }
            else
            {
                docType = "and DocumentType ='" + DocumentType + @"'";
                cddocType = "and cd.DocumentType ='" + DocumentType + @"'";
            }
            parameters = doccat + docSubCatg + EmployeeTypeOrCategory + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            cParameters = cddoccat + cddocSubCatg + EmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            try
            {
                var sql = @"SELECT SUM([Mandatory]) AS TotalOverdueMandt,SUM([Optional]) AS TotalOverdueOpt,SUM([Completed]) AS Completed,REPLACE(CONVERT(VARCHAR(11), DueDate, 6), ' ', '-') AS SDueDate,DueDate
							FROM [HKP].[DocumentDailyOverDue] DDOD
							LEFT JOIN hkp.ComplianceDocument AS CD ON CD.Id = DDOD.ComplianceDocumentId
							WHERE DDOD.CompanyGroupId = '" + companyGroupId + @"' AND CONVERT(DATE, DDOD.DueDate) BETWEEN DATEADD(Day,-7, GETDATE()) AND GETDATE() " + cParameters + @"
							GROUP BY [DueDate] ORDER BY [DueDate] ASC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> PieChart(string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string parameters = string.Empty;
            string cParameters = string.Empty;
            string cPrdParameters = null;
            string doccat = null;
            string docSubCatg = null;
            string EmployeeTypeOrCategory = null;
            string cdEmployeeTypeOrCategory = null;
            string ComplianceDocument = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;
            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;

            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
                cddoccat = "";
            }
            else
            {
                doccat = "and DocumentCategoryId ='" + DocumentCategoryId + @"'";
                cddoccat = "and cd.ComplianceDocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
                cddocSubCatg = "";
            }
            else
            {
                docSubCatg = "and DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
                cdEmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
                cdEmployeeTypeOrCategory = "and EmpC.Id ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
                cdComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
                cdComplianceDocument = "and cd.Id ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
                cdDocBy = "";
            }
            else
            {
                DocBy = "and DocumentationBy ='" + DocumentationBy + @"'";
                cdDocBy = "and cd.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
                cdResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
                cdImpt = "";
            }
            else
            {
                Impt = "and Importance ='" + Importance + @"'";
                cdImpt = "and cd.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
                cdOptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                cdOptOrMandt = "and cd.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
                cddocType = "";
            }
            else
            {
                docType = "and DocumentType ='" + DocumentType + @"'";
                cddocType = "and cd.DocumentType ='" + DocumentType + @"'";
            }
            parameters = doccat + docSubCatg + EmployeeTypeOrCategory + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            cParameters = cddoccat + cddocSubCatg + cdEmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            cPrdParameters = cddoccat + cddocSubCatg + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            try
            {
                var sql = @"SELECT((SELECT  COUNT(*) AS Completed FROM PreRecruitmentDocument AS PRD
						 JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
						 LEFT JOIN hkp.ComplianceDocument AS cd ON cd.Id = prd.ComplianceDocumentId
						 WHERE PRD.FileId IS NOT NULL AND PRD.IsCopied=0
						 and CompanyGroupId= '" + companyGroupId + @"' " + cPrdParameters + @")
						 +
						 (SELECT  COUNT(*) AS Completed FROM EmployeeDocument AS ED
						 JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
						 LEFT JOIN hkp.ComplianceDocument AS cd ON cd.Id = ED.ComplianceDocumentId
						 LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
						  LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
						  LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

						 WHERE ED.FileId IS NOT NULL AND  EI.EmployeeStatus = 'Active'
						 AND cd.CompanyGroupId= '" + companyGroupId + @"' " + cParameters + @")) Completed,
						(select Count(*)  from TempDocDashboard where  CompanyGroupId = '" + companyGroupId + @"' and segment<>'' and DueDate < getDate() " + parameters + @") OverDue,
						(select Count(*)  from TempDocDashboard where  CompanyGroupId = '" + companyGroupId + @"' and segment<>'' and DueDate >= getDate() " + parameters + @") Due,
					    ((SELECT  COUNT(*) AS others FROM PreRecruitmentDocument AS PRD
						 JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
						 LEFT JOIN hkp.ComplianceDocument AS cd ON cd.Id = prd.ComplianceDocumentId
						 WHERE PRD.FileId IS NULL AND PRD.DueDate IS NULL AND  PRD.IsCopied=0
						and CompanyGroupId= '" + companyGroupId + @"' " + cPrdParameters + @")
						+
						(SELECT  COUNT(*) AS others FROM EmployeeDocument AS ED
						 JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
						 LEFT JOIN hkp.ComplianceDocument AS cd ON CD.Id = ED.ComplianceDocumentId

						 LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
						 LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
						 LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

						 WHERE ED.FileId IS NULL and ED.DueDate IS NULL AND  EI.EmployeeStatus = 'Active'
						AND cd.CompanyGroupId= '" + companyGroupId + @"' " + cParameters + @")) Others";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        /// <summary>
        /// OverDue that due date already passed.
        /// Due that due date not passed.
        /// </summary>
        /// <returns>IEnumerable<object></returns>

        public IEnumerable<object> PendingDocuments(string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string parameters = string.Empty;
            string cParameters = string.Empty;
            string cPrdParameters = null;
            string doccat = null;
            string docSubCatg = null;
            string EmployeeTypeOrCategory = null;
            string cdEmployeeTypeOrCategory = null;
            string ComplianceDocument = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;
            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;

            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
                cddoccat = "";
            }
            else
            {
                doccat = "and DocumentCategoryId ='" + DocumentCategoryId + @"'";
                cddoccat = "and CDC.Id ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
                cddocSubCatg = "";
            }
            else
            {
                docSubCatg = "and DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                cddocSubCatg = "and CDSC.Id ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
                cdEmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
                cdEmployeeTypeOrCategory = "AND TDD.EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
                cdComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
                cdComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
                cdDocBy = "";
            }
            else
            {
                DocBy = "and DocumentationBy ='" + DocumentationBy + @"'";
                cdDocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
                cdResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                cdResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
                cdImpt = "";
            }
            else
            {
                Impt = "and Importance ='" + Importance + @"'";
                cdImpt = "and TDD.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
                cdOptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                cdOptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
                cddocType = "";
            }
            else
            {
                docType = "AND DocumentType ='" + DocumentType + @"'";
                cddocType = "AND TDD.DocumentType ='" + DocumentType + @"'";
            }
            parameters = doccat + docSubCatg + EmployeeTypeOrCategory + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            cParameters = cddoccat + cddocSubCatg + cdEmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            cPrdParameters = cddoccat + cddocSubCatg + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;

            try
            {
       //         var sql = @"SELECT X.Code,X.ComplianceDocName,X.ShortName, X.OverDue, X.Due,X.Sequence FROM (
							//SELECT CDE.UserName AS ComplianceDocName,CDE.ShortName,cde.Code,CDe.Sequence,
							//(ISNULL((SELECT COUNT(*) FROM dbo.PreRecruitmentDocument PRD
							// JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
						 //LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = PRD.ComplianceDocumentId

							// WHERE FileId IS NULL AND  DueDate IS NOT NULL AND CONVERT(DATE, DueDate) < CONVERT(DATE,GETDATE()) AND ComplianceDocumentId=CDE.Id AND PRE.Completed = 0 " + cPrdParameters + @"),0)
							// +
							// ISNULL((SELECT COUNT(*) FROM dbo.EmployeeDocument ED
							//  JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID and EI.EmployeeStatus = 'Active'
						 //    LEFT  JOIN hkp.ComplianceDocument AS cd ON cd.Id = ED.ComplianceDocumentId

						 // LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
						 // LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
						 // LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

							//  WHERE FileId IS NULL AND  DueDate IS NOT NULL AND CONVERT(DATE, DueDate) < CONVERT(DATE,GETDATE()) AND ComplianceDocumentId=CDE.Id " + cParameters + @" ),0)
							  
							//) AS OverDue,
							//(ISNULL((SELECT COUNT(*) FROM dbo.PreRecruitmentDocument PRD
							//JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
							// LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = PRD.ComplianceDocumentId
							// WHERE FileId IS NULL AND  DueDate IS NOT NULL AND CONVERT(DATE, DueDate) > CONVERT(DATE,GETDATE()) AND ComplianceDocumentId=CDE.Id AND PRE.Completed = 0 " + cPrdParameters + @"),0)
							// +
							// ISNULL((SELECT COUNT(*) FROM dbo.EmployeeDocument ED 
							//  JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID AND EI.EmployeeStatus = 'Active'
							//  Left JOIN hkp.ComplianceDocument AS cd ON cd.Id = ED.ComplianceDocumentId

						 //    LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
						 //    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
						 //    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

							//  WHERE FileId IS NULL AND  DueDate IS NOT NULL AND CONVERT(DATE, DueDate) > CONVERT(DATE,GETDATE()) AND ComplianceDocumentId=CDE.Id " + cParameters + @" ),0)
							//) AS Due
							//FROM [HKP].[ComplianceDocument] AS CDE)AS X
							//WHERE X.OverDue + X.Due > 0  ORDER BY X.Sequence";
       //         return _sqlRepository.GetDataCollection(sql);
                var strSql = @"SELECT ISNULL(OverDue.TotalDocument,0) OverDue,ISNULL(Due.TotalDocument,0) Due,CDE.UserName AS ComplianceDocName,CDE.ShortName,cde.Code,CDe.Sequence from	
					[HKP].[ComplianceDocument] CDE
					left join
						(SELECT COUNT(TDD.ComplianceDocumentId) AS TotalDocument,TDD.ComplianceDocumentId						
							FROM TempDocDashboard AS TDD
							LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
							LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId
							WHERE TDD.CompanyGroupId ='" + companyGroupId + @"' and segment<>'' and DueDate <= getDate() " + cParameters + @"
							GROUP BY CD.OptionalOrMandatory,TDD.ComplianceDocumentId,CD.UserName,CDC.UserName,CDSC.UserName,TDD.DocumentType
							,TDD.DocumentationBy,TDD.Importance) as OverDue on cde.Id = OverDue.ComplianceDocumentId
							LEFT JOIN 
							(SELECT COUNT(TDD.ComplianceDocumentId) AS TotalDocument,TDD.ComplianceDocumentId						
							FROM TempDocDashboard AS TDD
							LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
							LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId
							where TDD.CompanyGroupId = '" + companyGroupId + @"' and segment<>'' and DueDate > getDate() " + cParameters + @"
							GROUP BY CD.OptionalOrMandatory,TDD.ComplianceDocumentId,CD.UserName,CDC.UserName,CDSC.UserName,TDD.DocumentType
							,TDD.DocumentationBy,TDD.Importance) as Due ON Due.ComplianceDocumentId = cde.Id 
							WHERE (OverDue.TotalDocument is not null or Due.TotalDocument is not null)";
                return _sqlRepository.GetDataCollection(strSql, null);
              
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> PreDocSubmitted(string CompanyGroupId, string CompanyId, string Docby, string DocCategory, string DocName)
        {
            string DocumentationBy = string.Empty;
            string DocumentCategory = string.Empty;
            string DocumentName = string.Empty;

            if (string.IsNullOrEmpty(Docby))
            {
                DocumentationBy = "";
            }
            else
            {
                DocumentationBy = " and cd.DocumentationBy = '" + Docby + @"'";
            }
            DocumentCategory = (string.IsNullOrEmpty(DocCategory)) ? "" : "  and cdc.UserName = '" + DocCategory + @"'";
            DocumentName = (string.IsNullOrEmpty(DocName)) ? "" : "  and cd.Id = '" + DocName + @"'";
            try
            {
                var sql = @"select
                            count(pred.ComplianceDocumentId) totDoc,cd.ShortName docName,
                            count(pre.Id ) totEmp,
                            cd.CompanyGroupId,
                            cd.OptionalOrMandatory
                            FROM PreRecruitmentDocument pred
                            LEFT JOIN[HKP].[ComplianceDocument] cd ON cd.Id = pred.ComplianceDocumentId
                            LEFT JOIN[DBO].[PreRecruitmentEmployee] pre ON pre.Id = pred.PreRecruitmentEmployeeId
                            LEFT JOIN [hkp].[ComplianceDocumentCategory] cdc on cdc.Id = cd.ComplianceDocumentCategoryId
                            WHERE pred.FileName IS NULL
                              AND pre.GroupID = '" + CompanyGroupId + @"'
                            	AND pre.CompanyId = '" + CompanyId + @"'
                            " + DocumentationBy + @"
                            " + DocumentCategory + @"
                            " + DocumentName + @"
                            GROUP BY cd.CompanyGroupId, cd.OptionalOrMandatory,cd.ShortName,cd.UserName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> PreDocNotSubmitted(string CompanyGroupId, string CompanyId, string Docby, string DocCategory, string DocName)
        {
            string DocumentationBy = string.Empty;
            string DocumentCategory = string.Empty;
            string DocumentName = string.Empty;

            if (string.IsNullOrEmpty(Docby))
            {
                DocumentationBy = "";
            }
            else
            {
                DocumentationBy = " and cd.DocumentationBy = '" + Docby + @"'";
            }
            DocumentCategory = (string.IsNullOrEmpty(DocCategory)) ? "" : "  and cdc.UserName = '" + DocCategory + @"'";
            DocumentName = (string.IsNullOrEmpty(DocName)) ? "" : "  and cd.Id = '" + DocName + @"'";
            try
            {
                var sql = @"select
                             count(pred.ComplianceDocumentId) totDoc,cd.ShortName docName,
                             count(pre.Id ) totEmp,
                             cd.CompanyGroupId,
                             cd.OptionalOrMandatory
                            FROM PreRecruitmentDocument pred
                            LEFT JOIN[HKP].[ComplianceDocument] cd ON cd.Id = pred.ComplianceDocumentId
                            LEFT JOIN[DBO].[PreRecruitmentEmployee] pre ON pre.Id = pred.PreRecruitmentEmployeeId
                            LEFT JOIN [hkp].[ComplianceDocumentCategory] cdc on cdc.Id = cd.ComplianceDocumentCategoryId
                            WHERE pred.FileName IS NOT NULL
                              AND pre.GroupID = '" + CompanyGroupId + @"'
                            	AND pre.CompanyId = '" + CompanyId + @"'
                            " + DocumentationBy + @"
                            " + DocumentCategory + @"
                               " + DocumentName + @"
                            GROUP BY cd.CompanyGroupId, cd.OptionalOrMandatory,cd.ShortName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetComplianceDocumentCbo(string compnayGroupId, string ComplianceDocumentCategoryId, string ComplianceDocumentSubCategoryId)
        {
            var ComplianceDocCategoryId = string.Empty;
            var ComplianceDocSubCategoryId = string.Empty;

            if (ComplianceDocumentCategoryId == null || ComplianceDocumentCategoryId == "null" || ComplianceDocumentCategoryId == "")
            {
                ComplianceDocCategoryId = "";
            }
            else
            {
                ComplianceDocCategoryId = "AND cd.ComplianceDocumentCategoryId = '" + ComplianceDocumentCategoryId + @"' ";
            }
            if (ComplianceDocumentSubCategoryId == null || ComplianceDocumentSubCategoryId == "null" || ComplianceDocumentSubCategoryId == "")
            {
                ComplianceDocSubCategoryId = "";
            }
            else
            {
                ComplianceDocSubCategoryId = "AND cd.ComplianceDocumentSubCategoryId =  '" + ComplianceDocumentSubCategoryId + @"' ";
            }
            var _sql = @"SELECT [Id]
							 ,[UserName]

							   FROM[HKP].[ComplianceDocument] CD WHERE
                               cd.CompanyGroupId = '" + compnayGroupId + "' " + ComplianceDocCategoryId + @" " + ComplianceDocSubCategoryId + @"    ORDER BY [UserName]";

            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }

        public IEnumerable<object> GetComplianceDocumentDetail(string compnayGroupId, string complianceDocumentId)
        {
            var sql = @"SELECT CDC.Id ComplianceDocCategory,CDSC.Id ComplianceDocumentSubCategory,CD.DocumentType,cd.DocumentationBy,cd.Importance,cD.OptionalOrMandatory
							   FROM [HKP].[ComplianceDocument] CD
							   INNER JOIN
							   HKP.ComplianceDocumentCategory CDC ON CD.ComplianceDocumentCategoryId = CDC.Id
							      INNER JOIN
							    HKP.ComplianceDocumentSubCategory CDSC
										ON CD.ComplianceDocumentSubCategoryId = CDSC.Id WHERE cd.CompanyGroupId = '" + compnayGroupId + "' AND CD.Id = '" + complianceDocumentId + "'";

            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<ComboModel> GetCascadingComplianceDocumentCategoryCbo(string compnayGroupId)
        {
            var _sql = @"SELECT DISTINCT cdc.UserName,CDC.Id,CDC.Sequence FROM
								        HKP.ComplianceDocument CD
										INNER JOIN
										HKP.ComplianceDocumentCategory CDC ON CD.ComplianceDocumentCategoryId = CDC.Id
															WHERE cd.CompanyGroupId = '" + compnayGroupId + "'ORDER BY CDC.Sequence";

            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }

        public IEnumerable<ComboModel> GetCascadingComplianceDocumentSubCategoryCbo(string compnayGroupId, string documentCategoryId)
        {
            var ComplianceDocCategoryId = string.Empty;
            var ComplianceDocSubCategoryId = string.Empty;

            if (documentCategoryId == null || documentCategoryId == "null" || documentCategoryId == "")
            {
                ComplianceDocCategoryId = "";
            }
            else
            {
                ComplianceDocCategoryId = "CDC.Id= '" + documentCategoryId + @"' AND";
            }

            var _sql = @"SELECT DISTINCT CDSC.UserName, CDSC.Id,CDSC.Sequence FROM
							    HKP.ComplianceDocument CD
								INNER JOIN
								HKP.ComplianceDocumentCategory CDC
										ON CD.ComplianceDocumentCategoryId = CDC.Id
							    INNER JOIN
							    HKP.ComplianceDocumentSubCategory CDSC
										ON CD.ComplianceDocumentSubCategoryId = CDSC.Id
										WHERE " + ComplianceDocCategoryId + @"  cd.CompanyGroupId = '" + compnayGroupId + @"' ORDER BY CDSC.Sequence";

            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }

        public GridModel GetResponsiblePersonCbo(string compnayGroupId)
        {
            try
            {
                var sql = @"SELECT DISTINCT EI.SystemId AS [Value], EI.EmployeeName AS [Text]  FROM [HKP].[DocumentSetAssignDetail] SAD
                             LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = SAD.ResponsiblePersonId WHERE GroupID = '" + compnayGroupId + "'";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel PreEmp(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string EmployeeTypeOrCategory = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;
            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
            }
            try
            {
                parameter.CmdText = @"SELECT DISTINCT TDD.PreRecruitmentEmployeeId,TDD.EmployeeId,c.UserName Company,p.UserName plant,EI.EmployeeCode,'' Segment,'' EmpCategory,TDD.DocumentType,pre.FullName EmployeeName,pre.BudgetId,'' DOJ, GDG.UserName GivenDesignation,pandmandt.doc2 mandatory,pandOpt.doc2 Optional  from TempDocDashboard AS TDD
                            LEFT JOIN PreRecruitmentEmployee AS PRE on PRE.Id = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN HKP.Designation AS GDG on GDG.Id = PRE.GivenDesignationId
							LEFT JOIN EmployeeInformation as EI on EI.SystemId = TDD.EmployeeId
							LEFT JOIN PreRecruitmentDocument AS PRD on PRD.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
                           LEFT JOIN org.Company AS C ON C.Id = PRE.CompanyId
							LEFT JOIN org.Plant AS P ON P.Id = PRE.PlantId
							LEFT JOIN (SELECT COUNT(ComplianceDocumentId) doc2,PreRecruitmentEmployeeId
							FROM TempDocDashboard TDD
								WHERE OptionalOrMandatory = 'Mandatory' and segment<>''  AND DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" group by PreRecruitmentEmployeeId) pandmandt on pandmandt.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN (select count(ComplianceDocumentId) doc2,PreRecruitmentEmployeeId
							FROM TempDocDashboard TDD
						    WHERE  OptionalOrMandatory = 'Optional' AND segment<>''  AND DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" group by PreRecruitmentEmployeeId) pandOpt on pandOpt.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
	                        WHERE   segment<>''   AND prd.IsCopied = 0 AND  DocumentType ='" + employmentstage + @"'" + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							UNION
						    SELECT DISTINCT TDD.PreRecruitmentEmployeeId,TDD.EmployeeId,c.UserName Company,p.UserName plant,EI.EmployeeCode,'' Segment,EmpC.UserName EmpCategory,TDD.DocumentType,EI.EmployeeName,EI.BudgetCode BudgetId,
							REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ,GDG.UserName GivenDesignation,pandmandt.doc2 mandatory,pandOpt.doc2 Optional  from TempDocDashboard AS TDD

                           LEFT JOIN EmployeeInformation as EI on EI.SystemId = TDD.EmployeeId
                            LEFT JOIN HKP.Designation AS GDG on GDG.Id = EI.GivenDesignationId
							 LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
						     LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
							LEFT JOIN org.Company AS C ON C.Id = EI.CompanyId
							 LEFT JOIN org.Plant AS P ON P.Id = EI.PlantId
                            LEFT JOIN (SELECT COUNT(ComplianceDocumentId) doc2,EmployeeId
							FROM TempDocDashboard TDD
								WHERE OptionalOrMandatory = 'Mandatory' and segment<>''  AND  DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							    GROUP BY EmployeeId) pandmandt ON  pandmandt.EmployeeId = TDD.EmployeeId
                            LEFT JOIN (select count(ComplianceDocumentId) doc2,EmployeeId
							FROM TempDocDashboard TDD
						    WHERE  OptionalOrMandatory = 'Optional' AND segment<>'' AND DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							GROUP BY EmployeeId) pandOpt on pandOpt.EmployeeId = TDD.EmployeeId
	                        WHERE   segment<>''  AND EI.EmployeeName IS NOT NULL AND  DocumentType ='" + employmentstage + @"'" + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" ";
                return _sqlRepository.GetGridData(parameter);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel PreEmp1(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string EmployeeTypeOrCategory = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;
            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
            }
            try
            {
                parameter.CmdText = @"SELECT DISTINCT TDD.PreRecruitmentEmployeeId,TDD.EmployeeId,c.UserName Company,p.UserName plant,EI.EmployeeCode,TDD.Segment,'' EmpCategory,TDD.DocumentType,pre.FullName EmployeeName,pre.BudgetId,
									'' DOJ,GDG.UserName GivenDesignation,pandmandt.doc2 mandatory,pandOpt.doc2 Optional  from TempDocDashboard AS TDD
                            LEFT JOIN PreRecruitmentEmployee AS PRE on PRE.Id = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN HKP.Designation AS GDG on GDG.Id = PRE.GivenDesignationId
							LEFT JOIN EmployeeInformation as EI on EI.SystemId = TDD.EmployeeId
							LEFT JOIN PreRecruitmentDocument AS PRD on PRD.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN org.Company AS C ON C.Id = PRE.CompanyId
							LEFT JOIN org.Plant AS P ON P.Id = PRE.PlantId
							LEFT JOIN (SELECT COUNT(ComplianceDocumentId) doc2,PreRecruitmentEmployeeId
							FROM TempDocDashboard TDD
								WHERE OptionalOrMandatory = 'Mandatory' and segment = 1 AND  DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" group by PreRecruitmentEmployeeId) pandmandt on pandmandt.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN (select count(ComplianceDocumentId) doc2,PreRecruitmentEmployeeId
							FROM TempDocDashboard TDD
						    WHERE  OptionalOrMandatory = 'Optional' AND segment = 1 AND DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" group by PreRecruitmentEmployeeId) pandOpt on pandOpt.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
	                        WHERE   segment = 1 AND prd.IsCopied = 0 AND  DocumentType ='" + employmentstage + @"'" + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							UNION
						    SELECT DISTINCT TDD.PreRecruitmentEmployeeId,TDD.EmployeeId,c.UserName Company,p.UserName plant,EI.EmployeeCode,TDD.Segment,EmpC.UserName EmpCategory,TDD.DocumentType,EI.EmployeeName,EI.BudgetCode BudgetId,
								REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ,GDG.UserName GivenDesignation,pandmandt.doc2 mandatory,pandOpt.doc2 Optional  from TempDocDashboard AS TDD

							LEFT JOIN EmployeeInformation as EI on EI.SystemId = TDD.EmployeeId
                            LEFT JOIN HKP.Designation AS GDG on GDG.Id = EI.GivenDesignationId
							 LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
						     LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
							 LEFT JOIN org.Company AS C ON C.Id = EI.CompanyId
							 LEFT JOIN org.Plant AS P ON P.Id = EI.PlantId
                            LEFT JOIN (SELECT COUNT(ComplianceDocumentId) doc2,EmployeeId
							FROM TempDocDashboard TDD
								WHERE OptionalOrMandatory = 'Mandatory' and segment = 1 AND  DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @"  " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							    GROUP BY EmployeeId) pandmandt ON  pandmandt.EmployeeId = TDD.EmployeeId
                            LEFT JOIN (select count(ComplianceDocumentId) doc2,EmployeeId
							FROM TempDocDashboard TDD
						    WHERE  OptionalOrMandatory = 'Optional' AND segment = 1 AND DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							GROUP BY EmployeeId) pandOpt on pandOpt.EmployeeId = TDD.EmployeeId
	                        WHERE   segment = 1  AND EI.EmployeeName IS NOT NULL AND  DocumentType ='" + employmentstage + @"'" + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"   ";
                return _sqlRepository.GetGridData(parameter);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel PreEmp2(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string EmployeeTypeOrCategory = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;

            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
            }
            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
            }
            try
            {
                parameter.CmdText = @"SELECT DISTINCT TDD.PreRecruitmentEmployeeId,TDD.EmployeeId,c.UserName Company,p.UserName plant,EI.EmployeeCode,TDD.Segment,'' EmpCategory,TDD.DocumentType,pre.EmployeeName,pre.BudgetId,
							'' DOJ,GDG.UserName GivenDesignation,pandmandt.doc2 mandatory,pandOpt.doc2 Optional  from TempDocDashboard AS TDD
                            LEFT JOIN PreRecruitmentEmployee AS PRE on PRE.Id = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN HKP.Designation AS GDG on GDG.Id = PRE.GivenDesignationId
							LEFT JOIN EmployeeInformation as EI on EI.SystemId = TDD.EmployeeId
							LEFT JOIN PreRecruitmentDocument AS PRD on PRD.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
                             LEFT JOIN org.Company AS C ON C.Id = PRE.CompanyId
							 LEFT JOIN org.Plant AS P ON P.Id = PRE.PlantId
							LEFT JOIN (SELECT COUNT(ComplianceDocumentId) doc2,PreRecruitmentEmployeeId
							FROM TempDocDashboard TDD
								WHERE OptionalOrMandatory = 'Mandatory' and segment = 2 AND  DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" group by PreRecruitmentEmployeeId) pandmandt on pandmandt.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN (select count(ComplianceDocumentId) doc2,PreRecruitmentEmployeeId
							FROM TempDocDashboard TDD
						    WHERE  OptionalOrMandatory = 'Optional' AND segment = 2 AND DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" group by PreRecruitmentEmployeeId) pandOpt on pandOpt.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
	                        WHERE   segment = 2  AND prd.IsCopied = 0 AND  DocumentType ='" + employmentstage + @"'" + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							UNION
						    SELECT DISTINCT TDD.PreRecruitmentEmployeeId,TDD.EmployeeId,c.UserName Company,p.UserName plant,EI.EmployeeCode,TDD.Segment,EmpC.UserName EmpCategory,TDD.DocumentType,EI.EmployeeName,EI.BudgetCode BudgetId,
							REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ,GDG.UserName GivenDesignation,pandmandt.doc2 mandatory,pandOpt.doc2 Optional  from TempDocDashboard AS TDD
                             LEFT JOIN EmployeeInformation as EI on EI.SystemId = TDD.EmployeeId
                            LEFT JOIN HKP.Designation AS GDG on GDG.Id = EI.GivenDesignationId
							 LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
						     LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                         	 LEFT JOIN org.Company AS C ON C.Id = EI.CompanyId
							 LEFT JOIN org.Plant AS P ON P.Id = EI.PlantId
                            LEFT JOIN (SELECT COUNT(ComplianceDocumentId) doc2,EmployeeId
							FROM TempDocDashboard TDD
								WHERE OptionalOrMandatory = 'Mandatory' and segment = 2 AND  DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							    GROUP BY EmployeeId) pandmandt ON  pandmandt.EmployeeId = TDD.EmployeeId
                            LEFT JOIN (select count(ComplianceDocumentId) doc2,EmployeeId
							FROM TempDocDashboard TDD
						    WHERE  OptionalOrMandatory = 'Optional' AND segment = 2 AND DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @"  " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							GROUP BY EmployeeId) pandOpt on pandOpt.EmployeeId = TDD.EmployeeId
	                        WHERE   segment = 2   AND EI.EmployeeName IS NOT NULL AND  DocumentType ='" + employmentstage + @"'" + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"  ";
                return _sqlRepository.GetGridData(parameter);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel PreEmp3(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string EmployeeTypeOrCategory = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;
            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
            }
            else
            {
                doccat = "AND TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }

            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
            }

            try
            {
                parameter.CmdText = @"SELECT DISTINCT TDD.PreRecruitmentEmployeeId,TDD.EmployeeId,c.UserName Company,p.UserName plant,EI.EmployeeCode,TDD.Segment,'' EmpCategory,TDD.DocumentType,pre.EmployeeName,pre.BudgetId,
							'' DOJ,GDG.UserName GivenDesignation,pandmandt.doc2 mandatory,pandOpt.doc2 Optional  from TempDocDashboard AS TDD
                            LEFT JOIN PreRecruitmentEmployee AS PRE on PRE.Id = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN HKP.Designation AS GDG on GDG.Id = PRE.GivenDesignationId
							LEFT JOIN EmployeeInformation as EI on EI.SystemId = TDD.EmployeeId
							LEFT JOIN PreRecruitmentDocument AS PRD on PRD.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
                           	 LEFT JOIN org.Company AS C ON C.Id = EI.CompanyId
							 LEFT JOIN org.Plant AS P ON P.Id = EI.PlantId
							LEFT JOIN (SELECT COUNT(ComplianceDocumentId) doc2,PreRecruitmentEmployeeId
							FROM TempDocDashboard  TDD
								WHERE OptionalOrMandatory = 'Mandatory' and segment = 3 AND  DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" group by PreRecruitmentEmployeeId) pandmandt on pandmandt.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN (select count(ComplianceDocumentId) doc2,PreRecruitmentEmployeeId
							FROM TempDocDashboard  TDD
						    WHERE  OptionalOrMandatory = 'Optional' AND segment = 3 AND DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @" group by PreRecruitmentEmployeeId) pandOpt on pandOpt.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
	                        WHERE   segment = 3 AND prd.IsCopied = 0 AND  DocumentType ='" + employmentstage + @"'" + doccat + @" " + docSubCatg + @"" + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							UNION
						    SELECT DISTINCT TDD.PreRecruitmentEmployeeId,TDD.EmployeeId,c.UserName Company,p.UserName plant,EI.EmployeeCode,TDD.Segment,EmpC.UserName EmpCategory,TDD.DocumentType,EI.EmployeeName,EI.BudgetCode BudgetId,

							REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ,GDG.UserName GivenDesignation,pandmandt.doc2 mandatory,pandOpt.doc2 Optional  from TempDocDashboard AS TDD

							LEFT JOIN EmployeeInformation as EI on EI.SystemId = TDD.EmployeeId
                            LEFT JOIN HKP.Designation AS GDG on GDG.Id = EI.GivenDesignationId
							 LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
						     LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
							 LEFT JOIN org.Company AS C ON C.Id = EI.CompanyId
							 LEFT JOIN org.Plant AS P ON P.Id = EI.PlantId
                            LEFT JOIN (SELECT COUNT(ComplianceDocumentId) doc2,EmployeeId
							FROM TempDocDashboard  TDD
								WHERE OptionalOrMandatory = 'Mandatory' and segment = 3 AND  DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							    GROUP BY EmployeeId) pandmandt ON  pandmandt.EmployeeId = TDD.EmployeeId
                            LEFT JOIN (select count(ComplianceDocumentId) doc2,EmployeeId
							FROM TempDocDashboard  TDD
						    WHERE  OptionalOrMandatory = 'Optional' AND segment = 3 AND DocumentType ='" + employmentstage + @"' " + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							GROUP BY EmployeeId) pandOpt on pandOpt.EmployeeId = TDD.EmployeeId
	                        WHERE   segment = 3  AND EI.EmployeeName IS NOT NULL AND  DocumentType ='" + employmentstage + @"'" + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"  ";
                return _sqlRepository.GetGridData(parameter);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> Doc(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string EmployeeTypeOrCategory = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;
            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and TDD.EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
            }
            try
            {
                var sql = @"SELECT DISTINCT(TDD.ComplianceDocumentId), '' Segment, cd.UserName ComplianceDocument,COUNT(TDD.ComplianceDocumentId) AS TotalDocument,
							 tdd.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,TDD.DocumentType,TDD.DocumentationBy,TDD.Importance
							FROM TempDocDashboard AS TDD
							LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
							LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId

							WHERE
							--TDD.CompanyGroupId = ''
								segment<>''   and
							 TDD.DocumentType = '" + employmentstage + @"'
							and  DueDate < getDate()
							" + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
						GROUP BY tdd.OptionalOrMandatory,TDD.ComplianceDocumentId,CD.UserName,CDC.UserName,CDSC.UserName,TDD.DocumentType,TDD.DocumentationBy,TDD.Importance";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> Doc1(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string EmployeeTypeOrCategory = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;
            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and TDD.EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }

            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
            }
            try
            {
                var sql = @"SELECT DISTINCT(TDD.ComplianceDocumentId), TDD.Segment, cd.UserName ComplianceDocument,COUNT(TDD.ComplianceDocumentId) AS TotalDocument,
							tdd.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,TDD.DocumentType,TDD.DocumentationBy,TDD.Importance
							 FROM TempDocDashboard AS TDD
							LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
							LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId
							where TDD.Segment = 1 AND TDD.DocumentType = '" + employmentstage + @"'
							" + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							GROUP BY TDD.Segment,tdd.OptionalOrMandatory,TDD.ComplianceDocumentId,CD.UserName,CD.UserName,CDC.UserName,CDSC.UserName,TDD.DocumentType,TDD.DocumentationBy,TDD.Importance";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> Doc2(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string doccat = null;
            string docSubCatg = null;
            string EmployeeTypeOrCategory = null;
            string ComplianceDocument = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;
            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and TDD.EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }

            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
            }
            try
            {
                var sql = @"SELECT DISTINCT(TDD.ComplianceDocumentId), TDD.Segment, cd.UserName ComplianceDocument,COUNT(TDD.ComplianceDocumentId) AS TotalDocument,
							TDD.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,TDD.DocumentType,TDD.DocumentationBy,TDD.Importance
						    FROM TempDocDashboard AS TDD
							LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
							LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId
							--LEFT JOIN EmployeeInformation  as EI on EI.SystemId = TDD.ResponsiblePersonId
							where TDD.Segment = 2 AND TDD.DocumentType = '" + employmentstage + @"' and TDD.DueDate < getDate()
							" + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							GROUP BY TDD.Segment,TDD.OptionalOrMandatory,TDD.ComplianceDocumentId,CD.UserName,CD.UserName,CDC.UserName,CDSC.UserName,TDD.DocumentType,TDD.DocumentationBy,TDD.Importance";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> Doc3(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string doccat = null;
            string docSubCatg = null;
            string EmployeeTypeOrCategory = null;
            string ComplianceDocument = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;
            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and TDD.EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
            }
            try
            {
                var sql = @"SELECT DISTINCT(TDD.ComplianceDocumentId),TDD.Segment,  cd.UserName ComplianceDocument,COUNT(TDD.ComplianceDocumentId) AS TotalDocument,
							tdd.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,TDD.DocumentType,TDD.DocumentationBy,TDD.Importance
							 FROM TempDocDashboard AS TDD
							LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
							LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId
							--LEFT JOIN EmployeeInformation  as EI on EI.SystemId = TDD.ResponsiblePersonId
							where TDD.Segment = 3 AND TDD.DocumentType = '" + employmentstage + @"' and TDD.DueDate < getDate()
							" + doccat + @" " + docSubCatg + @" " + EmployeeTypeOrCategory + @" " + ComplianceDocument + @" " + DocBy + @"" + ResponsiblePerson + @"" + Impt + @" " + OptOrMandt + @" " + docType + @"
							GROUP BY TDD.Segment,TDD.OptionalOrMandatory,TDD.ComplianceDocumentId,CD.UserName,CD.UserName,CDC.UserName,CDSC.UserName,TDD.DocumentType,TDD.DocumentationBy,TDD.Importance";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> EmpWiseDocOpt(string employmentStage, string segment, string preRecEmployeeId, string employeeId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string Seg = string.Empty;
            string conditions = null;
            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;

            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;

            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
                cddoccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
                cddoccat = "and cd.ComplianceDocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
                cddocSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
                cdComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
                cdComplianceDocument = "and cd.Id ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
                cdDocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
                cdDocBy = "and cd.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
                cdResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
                cdImpt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
                cdImpt = "and cd.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
                cdOptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                cdOptOrMandt = "and cd.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
                cddocType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
                cddocType = "and cd.DocumentType ='" + DocumentType + @"'";
            }
            conditions = doccat + docSubCatg + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            if (segment == null || segment == "")
            {
                Seg = "TDD.Segment<>'' AND CONVERT(date,TDD.DueDate) < CONVERT(date,getDate())";
            }
            else
            {
                Seg = "TDD.Segment='" + segment + @"'";
            }
            try
            {
                var sql = @"SELECT ComplianceDocumentId, cd.UserName Document, tdd.OptionalOrMandatory,CDC.UserName DocCatg,
							CDSC.UserName DocSubCatg,TDD.DocumentType,TDD.DocumentationBy,TDD.Importance FROM TempDocDashboard AS TDD
							LEFT JOIN HKP.ComplianceDocument AS CD ON CD.ID = TDD.ComplianceDocumentId
							LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
							LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId

							WHERE (TDD.PreRecruitmentEmployeeId ='" + preRecEmployeeId + @"' OR TDD.EmployeeId = '" + employeeId + "') " +
       "AND TDD.OptionalOrMandatory = 'Optional' and TDD.DocumentType= '" + employmentStage + @"' and TDD.CompanyGroupId = '" + companyGroupId + @"'
		AND " + Seg + @" " + conditions + @" ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> EmpWiseDocMandt(string employmentStage, string segment, string preRecEmployeeId, string employeeId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string Seg = string.Empty;
            string conditions = null;
            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;

            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;

            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
                cddoccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
                cddoccat = "and cd.ComplianceDocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
                cddocSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
                cdComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
                cdComplianceDocument = "and cd.Id ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
                cdDocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
                cdDocBy = "and cd.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
                cdResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
                cdImpt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
                cdImpt = "and cd.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
                cdOptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                cdOptOrMandt = "and cd.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
                cddocType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
                cddocType = "and cd.DocumentType ='" + DocumentType + @"'";
            }
            conditions = doccat + docSubCatg + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            if (segment == null || segment == "")
            {
                Seg = "TDD.Segment<>'' AND CONVERT(DATE,TDD.DueDate) < CONVERT(DATE,GETDATE())";
            }
            else
            {
                Seg = "TDD.Segment='" + segment + @"'";
            }
            try
            {
                var sql = @"SELECT ComplianceDocumentId, cd.UserName Document, tdd.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,TDD.DocumentType,TDD.DocumentationBy,TDD.Importance FROM TempDocDashboard AS TDD
							LEFT JOIN HKP.ComplianceDocument AS CD ON CD.ID = TDD.ComplianceDocumentId
							LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
							LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId

							WHERE (TDD.PreRecruitmentEmployeeId ='" + preRecEmployeeId + @"' OR TDD.EmployeeId = '" + employeeId + @"')
							AND TDD.OptionalOrMandatory = 'Mandatory' AND TDD.DocumentType= '" + employmentStage + @"'
							AND " + Seg + @" AND TDD.CompanyGroupId = '" + companyGroupId + @"'" + conditions + @" ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel DocWiseEmp(GridParameter parameter, string employmentStage, string segment, string CompDocumentId, string EmplyeeTypeOrCategoryId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var Seg = string.Empty;
            string conditions = null;
            string cdconditions = null;
            string prdConditions = null;
            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string EmployeeTypeOrCategory = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;

            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;
            string tddSeg = string.Empty;

            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
                cddoccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
                cddoccat = "and cd.ComplianceDocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
                cddocSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and TDD.EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
                cdComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
                cdComplianceDocument = "and cd.Id ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
                cdDocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
                cdDocBy = "and cd.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
                cdResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
                cdImpt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
                cdImpt = "and cd.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
                cdOptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                cdOptOrMandt = "and cd.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
                cddocType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
                cddocType = "and cd.DocumentType ='" + DocumentType + @"'";
            }
            conditions = doccat + docSubCatg + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            if (segment == null || segment == "")
            {
                Seg = "Segment<>''   and  DueDate < getDate()";
                tddSeg = "TDD.Segment<>''  and  TDD.DueDate < getDate()";
            }
            else
            {
                tddSeg = "TDD.Segment='" + segment + @"'";
                Seg = "Segment='" + segment + @"'";
            }
            cdconditions = cddoccat + cddocSubCatg + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            conditions = doccat + docSubCatg + EmployeeTypeOrCategory + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            prdConditions = doccat + docSubCatg + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;

            try
            {
                parameter.CmdText = @"SELECT DISTINCT TDD.PreRecruitmentEmployeeId,EI.EmployeeCode,PRE.EmployeeCode EmployeeId,c.UserName Company,p.UserName plant,TDD.DocumentType,pre.FullName EmployeeName,pre.BudgetId,'' EmpCategory,
						             '' DOJ,GDG.UserName GivenDesignation,pandmandt.doc2 mandatory,pandOpt.doc2 Optional  from TempDocDashboard AS TDD
                            LEFT JOIN PreRecruitmentEmployee as PRE on PRE.Id = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN HKP.Designation as GDG on GDG.Id = PRE.GivenDesignationId
						    LEFT JOIN EmployeeInformation AS EI on EI.SystemId = TDD.EmployeeId
                            LEFT JOIN PreRecruitmentDocument AS PRD on PRD.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN org.Company AS C ON C.Id = PRE.CompanyId
							 LEFT JOIN org.Plant AS P ON P.Id = PRE.PlantId
                            LEFT JOIN (SELECT COUNT(ComplianceDocumentId) doc2,PreRecruitmentEmployeeId
							--, ComplianceDocumentId
							FROM TempDocDashboard AS TDD
								where OptionalOrMandatory = 'Mandatory' and " + Seg + @" AND TDD.DocumentType= '" + employmentStage + @"' " + prdConditions + @" GROUP BY PreRecruitmentEmployeeId,ComplianceDocumentId) pandmandt on pandmandt.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId
                            LEFT JOIN (select count(ComplianceDocumentId) doc2,PreRecruitmentEmployeeId
							--, ComplianceDocumentId
							FROM TempDocDashboard AS TDD
						    WHERE  OptionalOrMandatory = 'Optional' AND " + Seg + @" AND TDD.DocumentType= '" + employmentStage + @"' " + prdConditions + @" GROUP BY PreRecruitmentEmployeeId,ComplianceDocumentId) pandOpt ON pandOpt.PreRecruitmentEmployeeId = TDD.PreRecruitmentEmployeeId

						    WHERE TDD.CompanyGroupId = '" + companyGroupId + @"' AND prd.IsCopied = 0 AND  " + tddSeg + @" AND TDD.DocumentType= '" + employmentStage + @"' " + prdConditions + @" AND TDD.ComplianceDocumentId = '" + CompDocumentId + @"'
							UNION
							SELECT DISTINCT TDD.PreRecruitmentEmployeeId,TDD.EmployeeId,EI.EmployeeCode,c.UserName Company,p.UserName plant,TDD.DocumentType,EI.EmployeeName,EI.BudgetCode BudgetId, EmpC.UserName EmpCategory,
							REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ, GDG.UserName GivenDesignation,pandmandt.doc2 mandatory,pandOpt.doc2 Optional  from TempDocDashboard AS TDD
                            LEFT JOIN EmployeeInformation AS EI on EI.SystemId = TDD.EmployeeId
                            LEFT JOIN HKP.Designation as GDG on GDG.Id = EI.GivenDesignationId
                            LEFT JOIN EmployeeDocument AS ED on ED.EmpSystemID = TDD.EmployeeId
							 LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
							 LEFT JOIN org.Company AS C ON C.Id = EI.CompanyId
							 LEFT JOIN org.Plant AS P ON P.Id = EI.PlantId
                            LEFT JOIN (SELECT COUNT(ComplianceDocumentId) doc2,EmployeeId

							FROM TempDocDashboard AS TDD
								where OptionalOrMandatory = 'Mandatory' and " + Seg + @" AND TDD.DocumentType= '" + employmentStage + @"' " + conditions + @" GROUP BY EmployeeId,ComplianceDocumentId) pandmandt on pandmandt.EmployeeId = TDD.EmployeeId
                            LEFT JOIN (select count(ComplianceDocumentId) doc2,EmployeeId
							--, ComplianceDocumentId
							FROM TempDocDashboard AS TDD
						    WHERE  OptionalOrMandatory = 'Optional' AND " + Seg + @" AND TDD.DocumentType= '" + employmentStage + @"' " + conditions + @" GROUP BY EmployeeId,ComplianceDocumentId) pandOpt ON pandOpt.EmployeeId = TDD.EmployeeId

						    WHERE TDD.CompanyGroupId = '" + companyGroupId + @"' AND  " + tddSeg + @"AND TDD.DocumentType= '" + employmentStage + @"' " + conditions + @" AND TDD.ComplianceDocumentId = '" + CompDocumentId + @"' AND EI.EmployeeCode  IS NOT NULL";

                return _sqlRepository.GetGridData(parameter);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel CompletdDocWiseEmp(GridParameter parameter, string CompDocumentId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var Seg = string.Empty;
            string conditions = null;
            string prdConditions = null;
            string EmployeeTypeOrCategory = null;
            string cdconditions = null;
            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;

            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;
            var tddSeg = string.Empty;

            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
                cddoccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
                cddoccat = "and cd.ComplianceDocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
                cddocSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and EmpC.Id ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
                cdComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
                cdComplianceDocument = "and cd.Id ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
                cdDocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
                cdDocBy = "and cd.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
                cdResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
                cdImpt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
                cdImpt = "and cd.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
                cdOptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                cdOptOrMandt = "and cd.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
                cddocType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
                cddocType = "and cd.DocumentType ='" + DocumentType + @"'";
            }
            conditions = doccat + docSubCatg + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            cdconditions = cddoccat + cddocSubCatg + EmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            prdConditions = cddoccat + cddocSubCatg + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;

            try
            {
                parameter.CmdText = @"SELECT  PRE.Id PreRecruitmentEmployeeId,PRE.EmployeeCode EmployeeId,pre.FullName EmployeeName,c.UserName Company,p.UserName plant,pre.BudgetId,'' EmpCategory, '' DOJ, GDG.UserName GivenDesignation,
							 CD.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,CD.DocumentType,CD.DocumentationBy,CD.Importance
						 FROM PreRecruitmentDocument AS PRD
						 JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
						 left join hkp.ComplianceDocument as cd on cd.Id = prd.ComplianceDocumentId
						 								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = CD.ComplianceDocumentCategoryId
														  LEFT JOIN HKP.Designation as GDG on GDG.Id = PRE.GivenDesignationId
						 	LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = CD.ComplianceDocumentSubCategoryId
							LEFT JOIN EmployeeInformation  as EI on EI.SystemId = CD.ResponsiblePersonId
						    LEFT JOIN org.Company AS C ON C.Id = PRE.CompanyId
							LEFT JOIN org.Plant AS P ON P.Id = PRE.PlantId
						 WHERE PRD.FileId IS NOT NULL AND PRD.IsCopied = 0 AND CD.Id = '" + CompDocumentId + @"'
						    AND PRE.groupId ='" + companyGroupId + @"'  " + prdConditions + @"
							UNION
								SELECT  EI.PreRecruitmentEmployeeId,EI.EmployeeCode EmployeeId,EI.EmployeeName,c.UserName Company,p.UserName plant,EI.BudgetCode BudgetId,EmpC.UserName EmpCategory,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ, GDG.UserName GivenDesignation,
							 CD.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,CD.DocumentType,CD.DocumentationBy,CD.Importance
							  FROM EmployeeDocument AS ED
						 JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
						 left join hkp.ComplianceDocument as cd on cd.Id = ED.ComplianceDocumentId
						 LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = CD.ComplianceDocumentCategoryId
						  LEFT JOIN HKP.Designation as GDG on GDG.Id = EI.GivenDesignationId
						 	LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = CD.ComplianceDocumentSubCategoryId
							LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT JOIN org.Company AS C ON C.Id = EI.CompanyId
							LEFT JOIN org.Plant AS P ON P.Id = EI.PlantId
								 WHERE ED.FileId IS NOT NULL and CD.Id = '" + CompDocumentId + @"' and EI.EmployeeStatus = 'Active'
						    AND EI.groupId ='" + companyGroupId + @"'  " + cdconditions + @"";
                return _sqlRepository.GetGridData(parameter);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel OthersDocWiseEmp(GridParameter parameter, string CompDocumentId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            string Seg = string.Empty;
            string conditions = null;
            string prdconditions = null;
            string cdconditions = null;
            string doccat = null;
            string docSubCatg = null;
            string EmployeeTypeOrCategory = null;
            string ComplianceDocument = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;
            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;
            string tddSeg = string.Empty;

            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
                cddoccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
                cddoccat = "and cd.ComplianceDocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
                cddocSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and EmpC.Id ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
                cdComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
                cdComplianceDocument = "and cd.Id ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
                cdDocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
                cdDocBy = "and cd.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
                cdResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
                cdImpt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
                cdImpt = "and cd.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
                cdOptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                cdOptOrMandt = "and cd.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
                cddocType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
                cddocType = "and cd.DocumentType ='" + DocumentType + @"'";
            }
            conditions = doccat + docSubCatg + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            cdconditions = cddoccat + cddocSubCatg + EmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            prdconditions = cddoccat + cddocSubCatg + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;

            try
            {
                parameter.CmdText = @"SELECT  PRE.Id PreRecruitmentEmployeeId,PRE.EmployeeCode EmployeeId,pre.FullName EmployeeName,pre.BudgetId,GDG.UserName GivenDesignation,
							 CD.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,CD.DocumentType,CD.DocumentationBy,CD.Importance
							 ,EI.EmployeeName responsiblePerson,CD.ResponsiblePersonId  FROM PreRecruitmentDocument AS PRD
						 JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
						 left join hkp.ComplianceDocument as cd on cd.Id = prd.ComplianceDocumentId
						 								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = CD.ComplianceDocumentCategoryId
														  LEFT JOIN HKP.Designation as GDG on GDG.Id = PRE.GivenDesignationId
						 	LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = CD.ComplianceDocumentSubCategoryId
							LEFT JOIN EmployeeInformation  as EI on EI.SystemId = CD.ResponsiblePersonId
						 WHERE PRD.FileId IS NULL  AND PRD.DueDate IS NULL AND PRD.IsCopied = 0 AND CD.Id = '" + CompDocumentId + @"'

						    AND PRE.groupId ='" + companyGroupId + @"'  " + prdconditions + @"
								UNION
							SELECT  EI.PreRecruitmentEmployeeId,EI.EmployeeCode EmployeeId,EI.EmployeeName,EI.BudgetCode BudgetId,GDG.UserName GivenDesignation,
							 CD.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,CD.DocumentType,CD.DocumentationBy,CD.Importance
							 ,EID.EmployeeName responsiblePerson,CD.ResponsiblePersonId  FROM EmployeeDocument AS ED
						 JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
						 left join hkp.ComplianceDocument as cd on cd.Id = ED.ComplianceDocumentId
						 								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = CD.ComplianceDocumentCategoryId
														  LEFT JOIN HKP.Designation as GDG on GDG.Id = EI.GivenDesignationId
						 	LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = CD.ComplianceDocumentSubCategoryId
							--LEFT JOIN EmployeeInformation  as EID on EID.SystemId = CD.ResponsiblePersonId
							 WHERE ED.FileId IS NULL  and ED.DueDate IS NULL AND EI.EmployeeStatus = 'Active' and CD.Id = '" + CompDocumentId + @"'
						    AND EI.groupId ='" + companyGroupId + @"'  " + cdconditions + @"";
                return _sqlRepository.GetGridData(parameter);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel DueWiseEmp(GridParameter parameter, string CompDocumentId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var Seg = string.Empty;
            string conditions = null;
            string cdconditions = null;
            string prdConditions = null;
            string doccat = null;
            string docSubCatg = null;
            string ComplianceDocument = null;
            string EmployeeTypeOrCategory = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;

            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;
            var tddSeg = string.Empty;

            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
                cddoccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
                cddoccat = "and cd.ComplianceDocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
                cddocSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and TDD.EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
                cdComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
                cdComplianceDocument = "and cd.Id ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
                cdDocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
                cdDocBy = "and cd.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
                cdResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
                cdImpt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
                cdImpt = "and cd.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
                cdOptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and TDD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                cdOptOrMandt = "and cd.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
                cddocType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
                cddocType = "and cd.DocumentType ='" + DocumentType + @"'";
            }
            conditions = doccat + docSubCatg + EmployeeTypeOrCategory + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            cdconditions = cddoccat + cddocSubCatg + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            prdConditions = doccat + docSubCatg + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            try
            {
                parameter.CmdText = @"SELECT DISTINCT TDD.PreRecruitmentEmployeeId,PRE.EmployeeCode EmployeeId,c.UserName Company,p.UserName plant,TDD.DocumentType,pre.FullName EmployeeName,pre.BudgetId,'' EmpCategory,'' DOJ, GDG.UserName GivenDesignation  from TempDocDashboard AS TDD
		                          LEFT JOIN PreRecruitmentEmployee as PRE on PRE.Id = TDD.PreRecruitmentEmployeeId
								  LEFT JOIN PreRecruitmentDocument as PRD ON PRD.PreRecruitmentEmployeeId = PRE.Id
		                          LEFT JOIN HKP.Designation as GDG on GDG.Id = PRE.GivenDesignationId
							LEFT JOIN org.Company AS C ON C.Id = PRE.CompanyId
							LEFT JOIN org.Plant AS P ON P.Id = PRE.PlantId
						          WHERE TDD.CompanyGroupId = '" + companyGroupId + @"' " + prdConditions + @" AND TDD.ComplianceDocumentId = '" + CompDocumentId + @"' and segment<>'' and TDD.DueDate > getDate() AND PRD.IsCopied = 0
	                              UNION
							      SELECT DISTINCT TDD.PreRecruitmentEmployeeId,EI.EmployeeCode EmployeeId,c.UserName Company,p.UserName plant,TDD.DocumentType,EI.EmployeeName,EI.BudgetCode BudgetId,EmpC.UserName EmpCategory,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ, GDG.UserName GivenDesignation  from TempDocDashboard AS TDD
		                          LEFT JOIN EmployeeInformation as EI on EI.SystemId = TDD.EmployeeId
								  LEFT JOIN EmployeeDocument as ED ON ED.EmpSystemID = EI.SystemId
		                          LEFT JOIN HKP.Designation as GDG on GDG.Id = EI.GivenDesignationId

								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						LEFT JOIN org.Company AS C ON C.Id = EI.CompanyId
							LEFT JOIN org.Plant AS P ON P.Id = EI.PlantId
								WHERE TDD.CompanyGroupId = '" + companyGroupId + @"' " + conditions + @" AND TDD.ComplianceDocumentId = '" + CompDocumentId + @"' and segment<>'' and TDD.DueDate > getDate() AND EI.EmployeeCode IS NOT NULL";
                return _sqlRepository.GetGridData(parameter);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel OverDueWiseEmp(GridParameter parameter, string CompDocumentId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var Seg = string.Empty;
            string conditions = null;
            string cdconditions = null;
            string prdConditions = null;
            string doccat = null;
            string docSubCatg = null;
            string EmployeeTypeOrCategory = null;
            string ComplianceDocument = null;
            string DocBy = null;
            string ResponsiblePerson = null;
            string Impt = null;
            string OptOrMandt = null;
            string docType = null;

            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;
            var tddSeg = string.Empty;

            if (DocumentCategoryId == null || DocumentCategoryId == "")
            {
                doccat = "";
                cddoccat = "";
            }
            else
            {
                doccat = "and TDD.DocumentCategoryId ='" + DocumentCategoryId + @"'";
                cddoccat = "and cd.ComplianceDocumentCategoryId ='" + DocumentCategoryId + @"'";
            }

            if (DocumentSubCategoryId == null || DocumentSubCategoryId == "")
            {
                docSubCatg = "";
                cddocSubCatg = "";
            }
            else
            {
                docSubCatg = "and TDD.DocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
                cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + DocumentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and TDD.EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (ComplianceDocumentId == null || ComplianceDocumentId == "")
            {
                ComplianceDocument = "";
                cdComplianceDocument = "";
            }
            else
            {
                ComplianceDocument = "and TDD.ComplianceDocumentId ='" + ComplianceDocumentId + @"'";
                cdComplianceDocument = "and cd.Id ='" + ComplianceDocumentId + @"'";
            }
            if (DocumentationBy == null || DocumentationBy == "")
            {
                DocBy = "";
                cdDocBy = "";
            }
            else
            {
                DocBy = "and TDD.DocumentationBy ='" + DocumentationBy + @"'";
                cdDocBy = "and cd.DocumentationBy ='" + DocumentationBy + @"'";
            }
            if (ResponsiblePersonId == null || ResponsiblePersonId == "")
            {
                ResponsiblePerson = "";
                cdResponsiblePerson = "";
            }
            else
            {
                ResponsiblePerson = "and TDD.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
                cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + ResponsiblePersonId + @"'";
            }
            if (Importance == null || Importance == "")
            {
                Impt = "";
                cdImpt = "";
            }
            else
            {
                Impt = "and TDD.Importance ='" + Importance + @"'";
                cdImpt = "and cd.Importance ='" + Importance + @"'";
            }
            if (OptionalOrMandatory == null || OptionalOrMandatory == "")
            {
                OptOrMandt = "";
                cdOptOrMandt = "";
            }
            else
            {
                OptOrMandt = "and CD.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
                cdOptOrMandt = "and cd.OptionalOrMandatory ='" + OptionalOrMandatory + @"'";
            }
            if (DocumentType == null || DocumentType == "")
            {
                docType = "";
                cddocType = "";
            }
            else
            {
                docType = "and TDD.DocumentType ='" + DocumentType + @"'";
                cddocType = "and cd.DocumentType ='" + DocumentType + @"'";
            }
            conditions = doccat + docSubCatg + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;

            cdconditions = cddoccat + cddocSubCatg + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            conditions = doccat + docSubCatg + EmployeeTypeOrCategory + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;
            prdConditions = doccat + docSubCatg + ComplianceDocument + DocBy + ResponsiblePerson + Impt + OptOrMandt + docType;

            try
            {
                parameter.CmdText = @"SELECT distinct EI.EmployeeCode EmployeeId,TDD.PreRecruitmentEmployeeId,c.UserName Company,p.UserName plant,TDD.DocumentType,EI.EmployeeName,EI.BudgetCode BudgetId,EmpC.UserName EmpCategory, REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ , GDG.UserName GivenDesignation from  EmployeeDocument AS ED
										LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = ED.EmpSystemID
										LEFT JOIN TempDocDashboard AS TDD ON TDD.EmployeeId = ED.EmpSystemID
										 LEFT JOIN HKP.Designation as GDG on GDG.Id = EI.GivenDesignationId
										LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
									LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									LEFT JOIN org.Company AS C ON C.Id = EI.CompanyId
							 LEFT JOIN org.Plant AS P ON P.Id = EI.PlantId
										 Where TDD.CompanyGroupId = '" + companyGroupId + @"' AND EI.EmployeeStatus = 'Active'  AND  TDD.DueDate <= getDate()	" + conditions + @"  AND  CD.Id = '" + CompDocumentId + @"' and segment<>''

										 UNION
										 SELECT distinct TDD.EmployeeId,TDD.PreRecruitmentEmployeeId,c.UserName Company,p.UserName plant,TDD.DocumentType,PRE.FullName EmployeeName,pre.BudgetId BudgetId,'' EmpCategory,'' DOJ, GDG.UserName GivenDesignation
										  from  PreRecruitmentDocument AS PRD
										LEFT JOIN PreRecruitmentEmployee AS PRE ON PRE.id = PRD.PreRecruitmentEmployeeId
										LEFT JOIN TempDocDashboard AS TDD ON TDD.PreRecruitmentEmployeeId = PRD.PreRecruitmentEmployeeId
										 LEFT JOIN HKP.Designation as GDG on GDG.Id = PRE.GivenDesignationId
										LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
							LEFT JOIN org.Company AS C ON C.Id = PRE.CompanyId
							LEFT JOIN org.Plant AS P ON P.Id = PRE.PlantId
										 Where TDD.CompanyGroupId = '" + companyGroupId + @"'  AND  TDD.DueDate <= getDate() " + prdConditions + @" 	 AND  CD.Id = '" + CompDocumentId + @"' and segment<>'' and PRD.IsCopied =0";
                return _sqlRepository.GetGridData(parameter);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> PieCompletedDoc(

            string companyGroupId,
            string documentCategoryId,
            string documentSubCategoryId,
            string EmplyeeTypeOrCategoryId,
            string complianceDocumentId,
            string documentationBy,
            string responsiblePersonId,
            string importance,
            string optionalOrMandatory,
            string documentType)
        {
            var cParameters = string.Empty;
            var cPrdParameters = string.Empty;
            string cddoccat = null;
            string cddocSubCatg = null;
            string EmployeeTypeOrCategory = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;
            if (documentCategoryId == null || documentCategoryId == "")
            {
                cddoccat = "";
            }
            else
            {
                cddoccat = "and cd.ComplianceDocumentCategoryId ='" + documentCategoryId + @"'";
            }

            if (documentSubCategoryId == null || documentSubCategoryId == "")
            {
                cddocSubCatg = "";
            }
            else
            {
                cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + documentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and EmpC.Id ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (complianceDocumentId == null || complianceDocumentId == "")
            {
                cdComplianceDocument = "";
            }
            else
            {
                cdComplianceDocument = "and cd.Id ='" + complianceDocumentId + @"'";
            }
            if (documentationBy == null || documentationBy == "")
            {
                cdDocBy = "";
            }
            else
            {
                cdDocBy = "and cd.DocumentationBy ='" + documentationBy + @"'";
            }
            if (responsiblePersonId == null || responsiblePersonId == "")
            {
                cdResponsiblePerson = "";
            }
            else
            {
                cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + responsiblePersonId + @"'";
            }
            if (importance == null || importance == "")
            {
                cdImpt = "";
            }
            else
            {
                cdImpt = "and cd.Importance ='" + importance + @"'";
            }
            if (optionalOrMandatory == null || optionalOrMandatory == "")
            {
                cdOptOrMandt = "";
            }
            else
            {
                cdOptOrMandt = "and cd.OptionalOrMandatory ='" + optionalOrMandatory + @"'";
            }
            if (documentType == null || documentType == "")
            {
                cddocType = "";
            }
            else
            {
                cddocType = "and cd.DocumentType ='" + documentType + @"'";
            }
            cParameters = cddoccat + cddocSubCatg + EmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            cPrdParameters = cddoccat + cddocSubCatg + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            try
            {
                var sql = @"select  ComplianceDocumentId AS ComplianceDocumentId, ComplianceDocument,sum(TotalDocument) AS TotalDocument,
							 OptionalOrMandatory, DocCatg,  DocSubCatg,DocumentType,DocumentationBy,Importance
							  FROM    (
	             	SELECT  DISTINCT(PRD.ComplianceDocumentId) AS ComplianceDocumentId,cd.UserName ComplianceDocument,COUNT(PRD.PreRecruitmentEmployeeId) AS TotalDocument,
							 CD.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,CD.DocumentType,CD.DocumentationBy,CD.Importance
							  FROM PreRecruitmentDocument AS PRD
						 JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
						 left join hkp.ComplianceDocument as cd on cd.Id = prd.ComplianceDocumentId
						 								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = CD.ComplianceDocumentCategoryId
						 	LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = CD.ComplianceDocumentSubCategoryId
						 WHERE PRD.FileId IS NOT NULL AND PRD.IsCopied = 0
					AND CD.CompanyGroupId= '" + companyGroupId + @"' " + cPrdParameters + @"
						GROUP BY CD.OptionalOrMandatory,PRD.ComplianceDocumentId,CD.UserName,CDC.UserName,CDSC.UserName,CD.DocumentType,CD.DocumentationBy,cd.Importance
						UNION
						SELECT  DISTINCT(ED.ComplianceDocumentId) AS ComplianceDocumentId,cd.UserName ComplianceDocument,COUNT(ED.EmpSystemId) AS TotalDocument,
						CD.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,CD.DocumentType,CD.DocumentationBy,CD.Importance
					    FROM EmployeeDocument AS ED
						 JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
						 LEFT JOIN hkp.ComplianceDocument as cd on cd.Id = ED.ComplianceDocumentId
						 LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = CD.ComplianceDocumentCategoryId
						 LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = CD.ComplianceDocumentSubCategoryId
						 LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						 WHERE ED.FileId IS NOT NULL and EI.EmployeeStatus = 'Active'
						AND CD.CompanyGroupId= '" + companyGroupId + @"' " + cParameters + @"
						GROUP BY CD.OptionalOrMandatory,ED.ComplianceDocumentId,CD.UserName,CDC.UserName,CDSC.UserName,CD.DocumentType,CD.DocumentationBy,cd.Importance)
						 CompDoc GROUP BY
					 ComplianceDocumentId, ComplianceDocument,
							 OptionalOrMandatory, DocCatg, DocSubCatg, DocumentType, DocumentationBy, Importance";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> PieOthersDoc(
            string companyGroupId,
            string documentCategoryId,
            string documentSubCategoryId,
            string EmplyeeTypeOrCategoryId,
            string complianceDocumentId,
            string documentationBy,
            string responsiblePersonId,
            string importance,
            string optionalOrMandatory,
            string documentType)
        {
            var cParameters = string.Empty;
            var cPrdParameters = string.Empty;
            string cddoccat = null;
            string cddocSubCatg = null;
            string EmployeeTypeOrCategory = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;
            if (documentCategoryId == null || documentCategoryId == "")
            {
                cddoccat = "";
            }
            else
            {
                cddoccat = "and cd.ComplianceDocumentCategoryId ='" + documentCategoryId + @"'";
            }

            if (documentSubCategoryId == null || documentSubCategoryId == "")
            {
                cddocSubCatg = "";
            }
            else
            {
                cddocSubCatg = "and cd.ComplianceDocumentSubCategoryId ='" + documentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and EmpC.Id ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (complianceDocumentId == null || complianceDocumentId == "")
            {
                cdComplianceDocument = "";
            }
            else
            {
                cdComplianceDocument = "and cd.Id ='" + complianceDocumentId + @"'";
            }
            if (documentationBy == null || documentationBy == "")
            {
                cdDocBy = "";
            }
            else
            {
                cdDocBy = "and cd.DocumentationBy ='" + documentationBy + @"'";
            }
            if (responsiblePersonId == null || responsiblePersonId == "")
            {
                cdResponsiblePerson = "";
            }
            else
            {
                cdResponsiblePerson = "and cd.ResponsiblePersonId ='" + responsiblePersonId + @"'";
            }
            if (importance == null || importance == "")
            {
                cdImpt = "";
            }
            else
            {
                cdImpt = "and cd.Importance ='" + importance + @"'";
            }
            if (optionalOrMandatory == null || optionalOrMandatory == "")
            {
                cdOptOrMandt = "";
            }
            else
            {
                cdOptOrMandt = "and cd.OptionalOrMandatory ='" + optionalOrMandatory + @"'";
            }
            if (documentType == null || documentType == "")
            {
                cddocType = "";
            }
            else
            {
                cddocType = "and cd.DocumentType ='" + documentType + @"'";
            }
            cParameters = cddoccat + cddocSubCatg + EmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            cPrdParameters = cddoccat + cddocSubCatg + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            try
            {
                var sql = @"SELECT  DISTINCT(PRD.ComplianceDocumentId) AS ComplianceDocumentId,cd.UserName ComplianceDocument,COUNT(PRD.PreRecruitmentEmployeeId) AS TotalDocument,
							 CD.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,CD.DocumentType,CD.DocumentationBy,CD.Importance
						 FROM PreRecruitmentDocument AS PRD
						 JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
						 left join hkp.ComplianceDocument as cd on cd.Id = prd.ComplianceDocumentId
						 								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = CD.ComplianceDocumentCategoryId
						 	LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = CD.ComplianceDocumentSubCategoryId

						 WHERE PRD.DueDate is null and FileId is Null AND IsCopied = 0
						and CD.CompanyGroupId= '" + companyGroupId + @"' " + cPrdParameters + @"
						GROUP BY CD.OptionalOrMandatory,PRD.ComplianceDocumentId,CD.UserName,CDC.UserName,CDSC.UserName,CD.DocumentType,CD.DocumentationBy,cd.Importance
						UNION
						SELECT  DISTINCT(ED.ComplianceDocumentId) AS ComplianceDocumentId,cd.UserName ComplianceDocument,COUNT(ED.EmpSystemID) AS TotalDocument,
							 CD.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,CD.DocumentType,CD.DocumentationBy,CD.Importance
						 FROM EmployeeDocument AS ED
						 JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
						 LEFT JOIN hkp.ComplianceDocument as cd on cd.Id = ED.ComplianceDocumentId
						LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = CD.ComplianceDocumentCategoryId
						 	LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = CD.ComplianceDocumentSubCategoryId
                          LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						 WHERE ED.DueDate is null and FileId is Null AND EI.EmployeeStatus = 'Active'
						and CD.CompanyGroupId= '" + companyGroupId + @"' " + cParameters + @"
						GROUP BY CD.OptionalOrMandatory,ED.ComplianceDocumentId,CD.UserName,CDC.UserName,CDSC.UserName,CD.DocumentType,CD.DocumentationBy,cd.Importance
						";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> PieOverDueDoc(
            string companyGroupId,
            string documentCategoryId,
            string documentSubCategoryId,
            string EmplyeeTypeOrCategoryId,
            string complianceDocumentId,
            string documentationBy,
            string responsiblePersonId,
            string importance,
            string optionalOrMandatory,
            string documentType)
        {
            string cParameters = string.Empty;
            string cddoccat = null;
            string cddocSubCatg = null;
            string cdComplianceDocument = null;
            string EmployeeTypeOrCategory = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;
            if (documentCategoryId == null || documentCategoryId == "")
            {
                cddoccat = "";
            }
            else
            {
                cddoccat = "and TDD.DocumentCategoryId ='" + documentCategoryId + @"'";
            }

            if (documentSubCategoryId == null || documentSubCategoryId == "")
            {
                cddocSubCatg = "";
            }
            else
            {
                cddocSubCatg = "and TDD.DocumentSubCategoryId ='" + documentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "AND TDD.EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (complianceDocumentId == null || complianceDocumentId == "")
            {
                cdComplianceDocument = "";
            }
            else
            {
                cdComplianceDocument = "and TDD.ComplianceDocumentId ='" + complianceDocumentId + @"'";
            }
            if (documentationBy == null || documentationBy == "")
            {
                cdDocBy = "";
            }
            else
            {
                cdDocBy = "and TDD.DocumentationBy ='" + documentationBy + @"'";
            }
            if (responsiblePersonId == null || responsiblePersonId == "")
            {
                cdResponsiblePerson = "";
            }
            else
            {
                cdResponsiblePerson = "and TDD.ResponsiblePersonId ='" + responsiblePersonId + @"'";
            }
            if (importance == null || importance == "")
            {
                cdImpt = "";
            }
            else
            {
                cdImpt = "and TDD.Importance ='" + importance + @"'";
            }
            if (optionalOrMandatory == null || optionalOrMandatory == "")
            {
                cdOptOrMandt = "";
            }
            else
            {
                cdOptOrMandt = "and CD.OptionalOrMandatory ='" + optionalOrMandatory + @"'";
            }
            if (documentType == null || documentType == "")
            {
                cddocType = "";
            }
            else
            {
                cddocType = "and TDD.DocumentType ='" + documentType + @"'";
            }
            cParameters = cddoccat + cddocSubCatg + EmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            try
            {
                var sql = @"SELECT DISTINCT(TDD.ComplianceDocumentId),  cd.UserName ComplianceDocument,COUNT(TDD.ComplianceDocumentId) AS TotalDocument,
							CD.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,TDD.DocumentType,TDD.DocumentationBy
							,TDD.Importance
							FROM TempDocDashboard AS TDD
							LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
							LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId

							where TDD.CompanyGroupId = '" + companyGroupId + @"' and segment<>'' and  DueDate <= getDate() " + cParameters + @"
							GROUP BY CD.OptionalOrMandatory,TDD.ComplianceDocumentId,CD.UserName,CDC.UserName,CDSC.UserName,TDD.DocumentType
							,TDD.DocumentationBy,TDD.Importance ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        //----//

        public IEnumerable<object> PieDueDoc(
        string companyGroupId,
        string documentCategoryId,
        string documentSubCategoryId,
        string EmplyeeTypeOrCategoryId,
        string complianceDocumentId,
        string documentationBy,
        string responsiblePersonId,
        string importance,
        string optionalOrMandatory,
        string documentType)
        {
            string cParameters = string.Empty;
            string cddoccat = null;
            string cddocSubCatg = null;
            string EmployeeTypeOrCategory = null;
            string cdComplianceDocument = null;
            string cdDocBy = null;
            string cdResponsiblePerson = null;
            string cdImpt = null;
            string cdOptOrMandt = null;
            string cddocType = null;
            if (documentCategoryId == null || documentCategoryId == "")
            {
                cddoccat = "";
            }
            else
            {
                cddoccat = "and TDD.DocumentCategoryId ='" + documentCategoryId + @"'";
            }

            if (documentSubCategoryId == null || documentSubCategoryId == "")
            {
                cddocSubCatg = "";
            }
            else
            {
                cddocSubCatg = "and TDD.DocumentSubCategoryId ='" + documentSubCategoryId + @"'";
            }
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeTypeOrCategory = "";
            }
            else
            {
                EmployeeTypeOrCategory = "and TDD.EmployeeTypeOrCategory ='" + EmplyeeTypeOrCategoryId + @"'";
            }
            if (complianceDocumentId == null || complianceDocumentId == "")
            {
                cdComplianceDocument = "";
            }
            else
            {
                cdComplianceDocument = "and TDD.ComplianceDocumentId ='" + complianceDocumentId + @"'";
            }
            if (documentationBy == null || documentationBy == "")
            {
                cdDocBy = "";
            }
            else
            {
                cdDocBy = "and TDD.DocumentationBy ='" + documentationBy + @"'";
            }
            if (responsiblePersonId == null || responsiblePersonId == "")
            {
                cdResponsiblePerson = "";
            }
            else
            {
                cdResponsiblePerson = "and TDD.ResponsiblePersonId ='" + responsiblePersonId + @"'";
            }
            if (importance == null || importance == "")
            {
                cdImpt = "";
            }
            else
            {
                cdImpt = "and TDD.Importance ='" + importance + @"'";
            }
            if (optionalOrMandatory == null || optionalOrMandatory == "")
            {
                cdOptOrMandt = "";
            }
            else
            {
                cdOptOrMandt = "and TDD.OptionalOrMandatory ='" + optionalOrMandatory + @"'";
            }
            if (documentType == null || documentType == "")
            {
                cddocType = "";
            }
            else
            {
                cddocType = "and TDD.DocumentType ='" + documentType + @"'";
            }
            cParameters = cddoccat + cddocSubCatg + EmployeeTypeOrCategory + cdComplianceDocument + cdDocBy + cdResponsiblePerson + cdImpt + cdOptOrMandt + cddocType;
            try
            {
                var sql = @"SELECT DISTINCT(TDD.ComplianceDocumentId),  cd.UserName ComplianceDocument,COUNT(TDD.ComplianceDocumentId) AS TotalDocument,
							CD.OptionalOrMandatory,CDC.UserName DocCatg, CDSC.UserName DocSubCatg,TDD.DocumentType,TDD.DocumentationBy
							,TDD.Importance
							FROM TempDocDashboard AS TDD
							LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
								LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
							LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId

							where TDD.CompanyGroupId = '" + companyGroupId + @"' and segment<>'' and DueDate > getDate() " + cParameters + @"
							GROUP BY CD.OptionalOrMandatory,TDD.ComplianceDocumentId,CD.UserName,CDC.UserName,CDSC.UserName,TDD.DocumentType
							,TDD.DocumentationBy,TDD.Importance";


                //           var strSql = @"SELECT ISNULL(OverDue.TotalDocument,0) OverDue,ISNULL(Due.TotalDocument,0) Due,CDE.UserName AS ComplianceDocName,CDE.ShortName,cde.Code,CDe.Sequence from	
                //[HKP].[ComplianceDocument] CDE
                //left join
                //	(SELECT COUNT(TDD.ComplianceDocumentId) AS TotalDocument,TDD.ComplianceDocumentId						
                //		FROM TempDocDashboard AS TDD
                //		LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
                //			LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
                //		LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId
                //		WHERE TDD.CompanyGroupId ='" + companyGroupId + @"' and segment<>'' and DueDate > getDate() " + cParameters + @"
                //		GROUP BY CD.OptionalOrMandatory,TDD.ComplianceDocumentId,CD.UserName,CDC.UserName,CDSC.UserName,TDD.DocumentType
                //		,TDD.DocumentationBy,TDD.Importance) as OverDue on cde.Id = OverDue.ComplianceDocumentId
                //		LEFT JOIN 
                //		(SELECT COUNT(TDD.ComplianceDocumentId) AS TotalDocument,TDD.ComplianceDocumentId						
                //		FROM TempDocDashboard AS TDD
                //		LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = TDD.ComplianceDocumentId
                //			LEFT JOIN HKP.ComplianceDocumentCategory as CDC on CDC.Id = TDD.DocumentCategoryId
                //		LEFT JOIN HKP.ComplianceDocumentSubCategory as CDSC on CDSC.Id = TDD.DocumentSubCategoryId
                //		where TDD.CompanyGroupId = '" + companyGroupId + @"' and segment<>'' and DueDate > getDate() " + cParameters + @"
                //		GROUP BY CD.OptionalOrMandatory,TDD.ComplianceDocumentId,CD.UserName,CDC.UserName,CDSC.UserName,TDD.DocumentType
                //		,TDD.DocumentationBy,TDD.Importance) as Due ON Due.ComplianceDocumentId = cde.Id 
                //		WHERE (OverDue.TotalDocument is not null or Due.TotalDocument is not null)";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        //----------------------------------------Excel Report-----------------------------------------------//
        public string GetEmployeeDueDocumentList(string employeeId, string companyGroupId)
        {
            try
            {
                var cmdText = @"SELECT cd.UserName,cd.OptionalOrMandatory,cd.DocumentationBy,cd.DocumentType,cd.Importance,cdc.UserName,CDSC.UserName,FileName,
								CASE WHEN FileId is not null or FileName is not null  THEN 'Yes'
								            ELSE 'No' END AS UploadedStatus
								from PreRecruitmentDocument PRD
								LEFT JOIN HKP.ComplianceDocument AS CD ON CD.ID = PRD.ComplianceDocumentId
								LEFT JOIN HKP.ComplianceDocumentCategory AS CDC ON CDC.ID = CD.ComplianceDocumentCategoryId
								LEFT JOIN HKP.ComplianceDocumentSubCategory AS CDSC ON CDSC.ID = CD.ComplianceDocumentSubCategoryId
								WHERE PreRecruitmentEmployeeId= '" + employeeId + "@'  and CD.CompanyGroupId = '" + companyGroupId + @"'
								ORDER BY UploadedStatus";

                var documents = _sqlRepository.GetDataTable(cmdText);

                #region Variable

                var filePath = "";
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                var oRU = new ReportUtility();
                var xlsRow = 1;
                var xlsCol = 1;

                #endregion Variable

                if (documents.Rows.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];

                    xlsRow = 5;

                    #region variable

                    var cDocumentName = 0;
                    var cDueDate = 0;

                    #endregion variable

                    var endXlsCol = 0;

                    xlsRow++;
                    xlsCol = 1;
                    var cSl = 0;

                    #region Header

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sl. No.", 6); cSl = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Document Name", 45); cDocumentName = xlsCol; xlsCol++;

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Due Date"); cDueDate = xlsCol; xlsCol++;

                    #endregion Header

                    xlsCol--;
                    endXlsCol = xlsCol;
                    xlsRow++;
                    var slCount = 0;
                    for (int i = 0; i < documents.Rows.Count; i++)
                    {
                        slCount++;
                        oRU.SetText(ref sheet1, xlsRow, cSl, slCount.ToString());
                        oRU.SetText(ref sheet1, xlsRow, cDocumentName, documents.Rows[i]["DocName"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cDueDate, documents.Rows[i]["DueDate"].ToString());
                        xlsRow++;
                    }

                    //oRU.SetHeaderText(ref sheet1, 4, 1, "Employee Name: " + EmployeeName, ExcelHAlign.HAlignCenter);
                    sheet1.Range[4, 1, 4, endXlsCol].Merge();
                    //oRU.MainCompanyGroupHeader(ref sheet1, endXlsCol, "Document Needs To Be Uploaded", employee.GroupID);

                    #region UsedRange Alignment

                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    oRU.PageSetupAuto(ref sheet1, 5, ExcelPageOrientation.Landscape, "TS");
                    sheet1.Name = "DocumentsToBeUploadedByEmployee" + DateTime.Now.ToString("ddMMyyyyHHmmss");

                    workbook.Version = ExcelVersion.Excel97to2003;
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sheet1.Name + ".xls");
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                }
                return filePath;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetEmployeeDocumentReport()
        {
            try
            {
                var obj = new ReportGeneralVoucher();
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    var workbook = obj.EmployeeDocument_Report(excelEngine);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}