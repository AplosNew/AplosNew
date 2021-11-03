#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Organizations;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Employees
{
    public class PreRecruitmentDashboardService : Service<PreRecruitmentEmployee>, IPreRecruitmentDashboardService
    {
        #region Constructor

        private readonly IRepositoryAsync<PreRecruitmentEmployee> _preRecruitmentEmployeeRepository;
        private readonly ISqlRepository _sqlRepository;

        public PreRecruitmentDashboardService(
            ISqlRepository sqlRepository
            , IRepositoryAsync<PreRecruitmentEmployee> preRecruitmentEmployeeRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            ) : base(preRecruitmentEmployeeRepository, unitOfWork, pkGeneratorService)
        {
            _preRecruitmentEmployeeRepository = preRecruitmentEmployeeRepository;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region ColumnList

        public IEnumerable<OrgStructureListViewModel> OrgStructureList(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"SELECT StandardName, UserName ColumnName, RType,Sequence
									   FROM ORG.StructureRelationship
									   WHERE RType = 'Entity'  AND CompanyGroupId = '" + CompanyGroupId + @"' --AND CompanyId = 'C20181'
							   UNION
							   SELECT StandardName, UserName ColumnName, RType,Sequence FROM ORG.StructureRelationship  AS k
								      WHERE rtype = 'Position' AND NOT EXISTS (
																	SELECT 1
																	FROM ORG.StructureRelationship  AS t
																	WHERE t.standardname = k.standardname
									       and t.rtype = 'Entity'  AND t.CompanyGroupId = '" + CompanyGroupId + @"') ORDER BY RType,Sequence";

                return _preRecruitmentEmployeeRepository.SqlQuery<OrgStructureListViewModel>(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion ColumnList

        public IEnumerable<object> OverAllStatus(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"SELECT TE.CompanyId
                                      ,isnull(TE.TotalInterviewee,0) TotalInterviewee
                                      ,isnull(NRFC.NotReadyForCandidateAccess,0) notSelected

                                      ,isnull(FL.LoggedIn,0) LoggedIn
                                      ,isnull(FNL.NotLoggedIn,0) NotLoggedIn

                                      ,isnull(TOVD.TOverDue,0) TOverDue

                                      ,isnull(LOVD.LOverDue,0) LOverDue
                                      ,isnull(NLOVD.NLOverDue,0) NLOverDue
                                      ,isnull(SL.Selected,0) Selected

                                      ,ISNULL(NC.NotConfirmed,0) NotConfirmed

                                      FROM(
                                      ((SELECT COUNT(Id) TotalInterviewee,CompanyId
                                      FROM [dbo].[PreRecruitmentEmployee] group by CompanyId ) TE
                                      LEFT OUTER JOIN
                                      ( SELECT COUNT(Id) LoggedIn,CompanyId
                                      FROM [dbo].[PreRecruitmentEmployee]  WHERE ISNULL(ReadyForCandidateAccess,0) = 1
									  AND IsFirstlogin =1 AND submitted = 0 AND ConfirmationStatus IS NULL AND  CONVERT(DATE,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE()) group by CompanyId) FL --LoggedIn
                                      ON TE.CompanyId = FL.CompanyId
                                      LEFT OUTER JOIN
                                      (SELECT COUNT(Id) NotLoggedIn,CompanyId
                                      FROM [dbo].[PreRecruitmentEmployee] where isnull(ReadyForCandidateAccess,0) = 1
									  AND IsFirstlogin =0 AND  CONVERT(DATE,(ExpiredDays+ SelectionDateTime)) >= CONVERT(DATE,GETDATE()) group by CompanyId ) FNL   --NotLoggedIn
                                      ON TE.CompanyId = FNL.CompanyId

                                      LEFT OUTER JOIN
                                      (SELECT COUNT(Id) Selected, CompanyId
									   FROM [dbo].[PreRecruitmentEmployee] WHERE ISNULL(ReadyForCandidateAccess,0) = 1
                                      AND ConfirmationStatus IS  NULL AND Submitted='False' AND  CONVERT(DATE,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
									   GROUP BY CompanyId) SL --selected
                                      ON  TE.CompanyId = SL.CompanyId
                                      LEFT OUTER JOIN
                                      (SELECT COUNT(Id) TOverDue,  CompanyId
										FROM [dbo].[PreRecruitmentEmployee]
									    WHERE ISNULL(ReadyForCandidateAccess,0) = 1  AND ConfirmationStatus is null
									    AND Submitted='False' AND CONVERT(DATE,(ExpiredDays+ SelectionDateTime)) < CONVERT(DATE,GETDATE())
									   GROUP BY CompanyId
									 ) TOVD --totalOverDue
                                      ON TE.CompanyId =TOVD.CompanyId
                                        LEFT OUTER JOIN
                                      (SELECT COUNT(Id) LOverDue,CompanyId
                                      FROM [dbo].[PreRecruitmentEmployee]
                                      WHERE IsFirstLogin = 1 AND Submitted='False'  AND isnull(ReadyForCandidateAccess,0) = 1  AND  ConfirmationStatus IS NULL AND CONVERT(DATE,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE()) group by CompanyId ) LOVD
                                      ON TE.CompanyId =LOVD.CompanyId--LoggedInOverdue
                                          LEFT OUTER JOIN
                                      (SELECT COUNT(Id) NLOverDue,CompanyId
                                      FROM [dbo].[PreRecruitmentEmployee]
                                      WHERE IsFirstLogin = 0 AND isnull(ReadyForCandidateAccess,0) = 1 and CONVERT(DATE,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE()) group by CompanyId ) NLOVD
                                      ON TE.CompanyId = NLOVD.CompanyId-- NotLoggedInOverdue
                                      LEFT OUTER JOIN
                                      ( SELECT COUNT(Id) NotReadyForCandidateAccess,CompanyId
										FROM [dbo].[PreRecruitmentEmployee]
										WHERE ISNULL(ReadyForCandidateAccess,0) = 0
										GROUP BY CompanyId) NRFC
                                      ON TE.CompanyId = NRFC.CompanyId
									    LEFT OUTER JOIN
                                      (	SELECT COUNT(Id) NotConfirmed,CompanyId
										FROM [dbo].[PreRecruitmentEmployee]
										WHERE IsFirstlogin = 1 AND submitted = 1 AND ConfirmationStatus IS  NULL
										GROUP BY CompanyId) NC   --NotConfirmed
                                      ON TE.CompanyId = NC.CompanyId
									  ))
                                      LEFT OUTER JOIN ORG.Company c ON c.id = TE.CompanyId
                                      LEFT OUTER JOIN org.CompanyGroup cg ON cg.id = c.CompanyGroupId
                                      WHERE  cg.id ='" + CompanyGroupId + @"'   AND c.id='" + CompanyId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> NotSelDoc(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"SELECT COUNT(ComplianceDocumentId) totalDoc, cd.CompanyGroupId,cd.OptionalOrMandatory from PreRecruitmentDocument pred
                                    LEFT OUTER JOIN [HKP].[ComplianceDocument] cd on cd.Id = pred.ComplianceDocumentId
                                    WHERE pred.PreRecruitmentEmployeeId IN (SELECT Id
                                    FROM [dbo].[PreRecruitmentEmployee]
                                    WHERE
                                    ReadyForCandidateAccess = 0
                                    AND CompanyGroupId = '" + CompanyGroupId + @"' AND CompanyId = '" + CompanyId + @"') AND pred.FileName is  null
                                    GROUP BY cd.CompanyGroupId,cd.OptionalOrMandatory";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> SelDoc(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"SELECT  COUNT(ComplianceDocumentId) totalDoc, cd.CompanyGroupId,cd.OptionalOrMandatory FROM PreRecruitmentDocument pre
                            LEFT OUTER JOIN [HKP].[ComplianceDocument] cd ON cd.Id = pre.ComplianceDocumentId
                            WHERE pre.PreRecruitmentEmployeeId IN (SELECT Id
                            FROM [dbo].[PreRecruitmentEmployee]
                            WHERE isnull(ReadyForCandidateAccess,0) = 1 AND ConfirmationStatus IS  NULL and Submitted='False'   AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
                            AND CompanyGroupId = '" + CompanyGroupId + @"' AND CompanyId = '" + CompanyId + @"') AND pre.FileName IS  NULL
                            GROUP BY cd.CompanyGroupId,cd.OptionalOrMandatory";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> SelDocOVD(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"select  Count(ComplianceDocumentId) totalDoc, cd.CompanyGroupId,cd.OptionalOrMandatory from PreRecruitmentDocument pred
                            left outer join [HKP].[ComplianceDocument] cd on cd.Id = pred.ComplianceDocumentId

                            where pred.PreRecruitmentEmployeeId IN (SELECT Id
                            FROM [dbo].[PreRecruitmentEmployee]
                            WHERE
                            ISNULL(ReadyForCandidateAccess,0) = 1
                            AND Submitted='False'  --AND CONVERT(DATE,(DueDate)) < convert(date,GETDATE())
							and CONVERT(DATE,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())
                            and
                            CompanyGroupId = '" + CompanyGroupId + @"' AND CompanyId = '" + CompanyId + @"') and pred.FileId is  null
                            group by cd.CompanyGroupId,cd.OptionalOrMandatory";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> NotConfirmedDoc(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"select Count(ComplianceDocumentId) totalDoc, cd.CompanyGroupId,cd.OptionalOrMandatory from PreRecruitmentDocument pred
                                    left outer join [HKP].[ComplianceDocument] cd on cd.Id = pred.ComplianceDocumentId

                                    where pred.PreRecruitmentEmployeeId IN (SELECT Id
                                    FROM [dbo].[PreRecruitmentEmployee]
                                    where
                                    submitted = 1 and ConfirmationStatus is  null
                                    AND CompanyGroupId = '" + CompanyGroupId + @"' AND CompanyId = '" + CompanyId + @"') and pred.FileName is  null
                                    group by cd.CompanyGroupId,cd.OptionalOrMandatory";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> LoggedInDoc(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"select Count(ComplianceDocumentId) totalDoc, cd.CompanyGroupId,cd.OptionalOrMandatory from PreRecruitmentDocument pred
                                    left outer join [HKP].[ComplianceDocument] cd on cd.Id = pred.ComplianceDocumentId

                                    where pred.PreRecruitmentEmployeeId IN (SELECT Id
                                    FROM [dbo].[PreRecruitmentEmployee]
                                    where
                                    isnull(ReadyForCandidateAccess,0) = 1 and IsFirstlogin = 1 and Submitted = 0  AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
                                    and GroupId = '" + CompanyGroupId + @"' AND CompanyId = '" + CompanyId + @"') and pred.FileName is  null
                                    group by cd.CompanyGroupId,cd.OptionalOrMandatory";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> LoggedInDocOVD(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"select Count(ComplianceDocumentId) totalDoc, cd.CompanyGroupId,cd.OptionalOrMandatory from PreRecruitmentDocument pred
                                    left outer join [HKP].[ComplianceDocument] cd on cd.Id = pred.ComplianceDocumentId

                                    where pred.PreRecruitmentEmployeeId IN (SELECT Id
                                    FROM [dbo].[PreRecruitmentEmployee]
                                    where
                                    isnull(ReadyForCandidateAccess,0) = 1 and IsFirstlogin = 1 and Submitted = 0 and CONVERT(DATE,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())
                                    and GroupId = '" + CompanyGroupId + @"' AND CompanyId = '" + CompanyId + @"') and pred.FileName is  null
                                    group by cd.CompanyGroupId,cd.OptionalOrMandatory";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> NotLoggedInDoc(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"select  Count(ComplianceDocumentId) totalDoc, cd.CompanyGroupId,cd.OptionalOrMandatory from PreRecruitmentDocument pred
                            left outer join [HKP].[ComplianceDocument] cd on cd.Id = pred.ComplianceDocumentId

                            where pred.PreRecruitmentEmployeeId IN (SELECT Id
                            FROM [dbo].[PreRecruitmentEmployee]
                            where
                            isnull(ReadyForCandidateAccess,0) = 1 and IsFirstlogin = 0  AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
                            and GroupId = '" + CompanyGroupId + @"' AND CompanyId = '" + CompanyId + @"') and pred.FileName is  null
                            group by cd.CompanyGroupId,cd.OptionalOrMandatory";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> NotLoggedInDocOverDue(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"select  Count(ComplianceDocumentId) totalDoc, cd.CompanyGroupId,cd.OptionalOrMandatory from PreRecruitmentDocument pred
                            left outer join [HKP].[ComplianceDocument] cd on cd.Id = pred.ComplianceDocumentId

                            where pred.PreRecruitmentEmployeeId IN (SELECT Id
                            FROM [dbo].[PreRecruitmentEmployee]
                            where
                            isnull(ReadyForCandidateAccess,0) = 1 and IsFirstlogin = 0 and CONVERT(DATE,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())
                            and GroupId = '" + CompanyGroupId + @"' AND CompanyId = '" + CompanyId + @"') and pred.FileName is  null
                            group by cd.CompanyGroupId,cd.OptionalOrMandatory";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region Modal

        #region ModalSelectedTotalEmployee

        public GridModel ListSelTotalInterviewee(GridParameter parameters, string companyGroupId, string companyId)
        {
            string cList = string.Empty;
            string wc = string.Empty;
            string Join = string.Empty;
            string cGList = string.Empty;
            IEnumerable<OrgStructureListViewModel> OrgStrList = OrgStructureList(companyGroupId, companyId);
            try
            {
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        cGList += "," + item.ColumnName + ".UserName";
                        if (item.ColumnName == "EmployeeGroup")
                        {
                            Join += "LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                        else
                        {
                            Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                    }
                    else
                    {
                        cGList += "," + item.ColumnName + ".UserName";
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = Po." + item.ColumnName + "Id\n";
                    }
                }
                parameters.CmdText = @" Select ROW_NUMBER() OVER (ORDER BY pre.BudgetId) AS RowNum,ISNULL(PRE.EmployeeCode,'') EmployeeCode,isnull(selfDoc.TotalFileSelf,0) TotalFileSelf,isnull(deptDoc.TotalFileDept,0) TotalFileDept
                            ,isnull(selfDocPanding.TotalFileSelfPanding,0) TotalFileSelfPanding,isnull(deptDocPanding.TotalFileDeptPanding,0) TotalFileDeptPanding
                            ,PRE.BudgetId,PO.Id positionId, GDG.Id DGID,GDG.UserName GivenDesignation,PD.Id DID,PD.UserName Designation,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cList + @"
                            ,PO.UserName PositionName
                            ,Replace(CONVERT(VARCHAR(11), PRE.AgreedDOJ, 106), ' ', '-') AgreedDOJ
                            ,PRE.Phone, PRE.Email,PRE.EmpType
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName EntityName
                            ,Replace(CONVERT(VARCHAR(11), PRE.SelectionDateTime, 106), ' ', '-') SselectionDate
                            ,PRE.ExpiredDays,Replace(CONVERT(VARCHAR(11),   DATEADD(day, PRE.ExpiredDays, PRE.SelectionDateTime), 100), ' ', '-') As ExpiredDate
                              ,EmpC.UserName EmpCategory,DesG.UserName DesigNationGroup
                            FROM PreRecruitmentEmployee PRE

							LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id

                            LEFT OUTER JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id

                            LEFT OUTER JOIN ORG.Position PO ON PMB.PositionId=PO.Id

                            LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id

                            LEFT OUTER JOIN HKP.Designation PD ON PO.DesignationId=PD.Id

                            LEFT OUTER JOIN HKP.Designation GDG ON PRE.GivenDesignationId=GDG.Id

							LEFT OUTER JOIN MST.DesignationMaster DGM ON PO.DesignationId = DGM.DesignationId

                            LEFT OUTER JOIN HKP.EmployeeCategory EmpC ON DGM.EmployeeCategoryId = EmpC.Id

                            LEFT OUTER JOIN HKP.DesignationGroup DesG ON DGM.DesignationGroupId = DesG.Id
							LEFT OUTER JOIN
							(Select
							 Count(PRED.ComplianceDocumentId) TotalFileDeptPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'  and isnull(ReadyForCandidateAccess,0) = 1

                            AND ConfirmationStatus is  null and Submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
							 and cd.DocumentationBy = 'department'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) deptDocPanding
							 on deptDocPanding.preId = PRE.Id
							 Left outer join
							(
							Select
							 Count(PRED.ComplianceDocumentId) TotalFileSelfPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'  and isnull(ReadyForCandidateAccess,0) = 1

                            AND ConfirmationStatus is  null and Submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
							 and cd.DocumentationBy = 'self'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) selfDocPanding
							 ON
							 selfDocPanding.preID = PRE.id
							LEFT OUTER JOIN
							(Select
							 Count(PRED.ComplianceDocumentId) TotalFileDept,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'  and isnull(ReadyForCandidateAccess,0) = 1

                            AND ConfirmationStatus is  null and Submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
							 and cd.DocumentationBy = 'department'
							 and PRED.FileName is not null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) deptDoc
							 on deptDoc.preId = PRE.Id
							 Left outer join
							(
							Select
							 Count(PRED.ComplianceDocumentId) TotalFileSelf,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'  and isnull(ReadyForCandidateAccess,0) = 1

                            AND ConfirmationStatus is  null and Submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
							 and cd.DocumentationBy = 'self'
							 and PRED.FileName is not null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) selfDoc
							 ON
							 selfDoc.preID = PRE.id
							" + Join + @"
                            where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"' and isnull(ReadyForCandidateAccess,0) = 1
                            --AND   SelectionStatus = 'Selected'
                            AND ConfirmationStatus is  null and Submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
                            GROUP BY PRE.BudgetId,PO.Id , GDG.Id ,GDG.UserName ,PD.Id ,PD.UserName ,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cGList + @"
                            ,PRE.EmployeeCode,PO.UserName,  PRE.Phone, PRE.Email,PRE.EmpType
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName ,EmpC.UserName
                            ,DesG.UserName,PRE.AgreedDOJ, PRE.SelectionDateTime, PRE.ExpiredDays,selfDoc.TotalFileSelf,deptDoc.TotalFileDept,selfDocPanding.TotalFileSelfPanding,deptDocPanding.TotalFileDeptPanding";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion ModalSelectedTotalEmployee

        #region ModalListNotSelectedEmp

        public GridModel ListNotSelectedEmp(GridParameter parameters, string companyGroupId, string companyId)
        {
            string cList = string.Empty;
            string wc = string.Empty;
            string Join = string.Empty;
            string cGList = string.Empty;
            IEnumerable<OrgStructureListViewModel> OrgStrList = OrgStructureList(companyGroupId, companyId);

            try
            {
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        cGList += "," + item.ColumnName + ".UserName";
                        if (item.ColumnName == "EmployeeGroup")
                        {
                            Join += "LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                        else
                        {
                            Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                    }
                    else
                    {
                        cGList += "," + item.ColumnName + ".UserName";
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = Po." + item.ColumnName + "Id\n";
                    }
                }
                parameters.CmdText = @"Select ROW_NUMBER() OVER (ORDER BY pre.BudgetId) AS RowNum,ISNULL(PRE.EmployeeCode,'') EmployeeCode,
                            Count(PRD.FileName) TotalFile,
                            PRE.BudgetId,PO.Id positionId, GDG.Id DGID,GDG.UserName GivenDesignation,PD.Id DID,PD.UserName Designation,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cList + @"
                            ,PO.UserName PositionName
                            ,Replace(CONVERT(VARCHAR(11), PRE.AgreedDOJ, 106), ' ', '-') AgreedDOJ
                            ,PRE.Phone, PRE.Email,PRE.EmpType
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName EntityName
                            ,Replace(CONVERT(VARCHAR(11), PRE.SelectionDateTime, 106), ' ', '-') SselectionDate
                            ,PRE.ExpiredDays,Replace(CONVERT(VARCHAR(11),   DATEADD(day, PRE.ExpiredDays, PRE.SelectionDateTime), 100), ' ', '-') As ExpiredDate
                              ,EmpC.UserName EmpCategory,DesG.UserName DesigNationGroup
                            FROM PreRecruitmentEmployee PRE

							LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id

                            LEFT OUTER JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id

                            LEFT OUTER JOIN ORG.Position PO ON PMB.PositionId=PO.Id

                            LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id

                            LEFT OUTER JOIN HKP.Designation PD ON PO.DesignationId=PD.Id

                            LEFT OUTER JOIN HKP.Designation GDG ON PRE.GivenDesignationId=GDG.Id

							LEFT OUTER JOIN MST.DesignationMaster DGM ON PO.DesignationId = DGM.DesignationId

                            LEFT OUTER JOIN HKP.EmployeeCategory EmpC ON DGM.EmployeeCategoryId = EmpC.Id

                            LEFT OUTER JOIN HKP.DesignationGroup DesG ON DGM.DesignationGroupId = DesG.Id
                            " + Join + @"
                            where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
                            AND  isnull(ReadyForCandidateAccess,0) = 0
                             GROUP BY PRE.BudgetId,PO.Id , GDG.Id ,GDG.UserName ,PD.Id ,PD.UserName ,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cGList + @"
                            ,PO.UserName,  PRE.Phone, PRE.Email,PRE.EmpType,PRE.EmployeeCode
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName ,EmpC.UserName ,DesG.UserName,PRE.AgreedDOJ, PRE.SelectionDateTime, PRE.ExpiredDays";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion ModalListNotSelectedEmp

        #region ModalSubmittedButNotConfirmed

        public GridModel SubmittedButNotConfirmed(GridParameter parameters, string companyGroupId, string companyId)
        {
            string cList = string.Empty;
            string wc = string.Empty;
            string Join = string.Empty;
            string cGList = string.Empty;
            IEnumerable<OrgStructureListViewModel> OrgStrList = OrgStructureList(companyGroupId, companyId);
            try
            {
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        cGList += "," + item.ColumnName + ".UserName";
                        if (item.ColumnName == "EmployeeGroup")
                        {
                            Join += "LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                        else
                        {
                            Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                    }
                    else
                    {
                        cGList += "," + item.ColumnName + ".UserName";
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = Po." + item.ColumnName + "Id\n";
                    }
                }
                parameters.CmdText = @"Select ROW_NUMBER() OVER (ORDER BY pre.BudgetId) AS RowNum,ISNULL(PRE.EmployeeCode,'') EmployeeCode,
                               isnull(selfDoc.TotalFileSelf,0) TotalFileSelf,isnull(deptDoc.TotalFileDept,0) TotalFileDept
                            ,isnull(selfDocPanding.TotalFileSelfPanding,0) TotalFileSelfPanding,isnull(deptDocPanding.TotalFileDeptPanding,0) TotalFileDeptPanding
                            ,PRE.BudgetId,PO.Id positionId, GDG.Id DGID,GDG.UserName GivenDesignation,PD.Id DID,PD.UserName Designation,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cList + @"
                            ,PO.UserName PositionName
                            ,Replace(CONVERT(VARCHAR(11), PRE.AgreedDOJ, 106), ' ', '-') AgreedDOJ
                            ,PRE.Phone, PRE.Email,PRE.EmpType
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName EntityName
                            ,Replace(CONVERT(VARCHAR(11), PRE.SelectionDateTime, 106), ' ', '-') SselectionDate
                            ,PRE.ExpiredDays,Replace(CONVERT(VARCHAR(11),   DATEADD(day, PRE.ExpiredDays, PRE.SelectionDateTime), 100), ' ', '-') As ExpiredDate
                              ,EmpC.UserName EmpCategory,DesG.UserName DesigNationGroup
                            FROM PreRecruitmentEmployee PRE

							LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id

                            LEFT OUTER JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id

                            LEFT OUTER JOIN ORG.Position PO ON PMB.PositionId=PO.Id

                            LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id

                            LEFT OUTER JOIN HKP.Designation PD ON PO.DesignationId=PD.Id

                            LEFT OUTER JOIN HKP.Designation GDG ON PRE.GivenDesignationId=GDG.Id

							LEFT OUTER JOIN MST.DesignationMaster DGM ON PO.DesignationId = DGM.DesignationId

                            LEFT OUTER JOIN HKP.EmployeeCategory EmpC ON DGM.EmployeeCategoryId = EmpC.Id

                            LEFT OUTER JOIN HKP.DesignationGroup DesG ON DGM.DesignationGroupId = DesG.Id
							LEFT OUTER JOIN
							(SELECT
							 COUNT(PRED.ComplianceDocumentId) TotalFileDeptPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN DBO.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								LEFT OUTER JOIN HKP.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 WHERE PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"' and isnull(ReadyForCandidateAccess,0) = 1

							 AND submitted = 1
                             AND ConfirmationStatus is  null
							 AND cd.DocumentationBy = 'department'
							 AND PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) deptDocPanding
							 ON deptDocPanding.preId = PRE.Id
							 LEFT OUTER JOIN
							(
							SELECT
							 Count(PRED.ComplianceDocumentId) TotalFileSelfPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"' and isnull(ReadyForCandidateAccess,0) = 1
							 AND submitted = 1
                            and ConfirmationStatus is  null
							 and cd.DocumentationBy = 'self'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) selfDocPanding
							 ON
							 selfDocPanding.preID = PRE.id
							LEFT OUTER JOIN
							(Select
							 Count(PRED.ComplianceDocumentId) TotalFileDept,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"' and isnull(ReadyForCandidateAccess,0) = 1

							 AND submitted = 1
                            and ConfirmationStatus is  null
							 and cd.DocumentationBy = 'department'
							 and PRED.FileName is not null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) deptDoc
							 on deptDoc.preId = PRE.Id
							 Left outer join
							(
							Select
							 Count(PRED.ComplianceDocumentId) TotalFileSelf,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"' and isnull(ReadyForCandidateAccess,0) = 1

							 AND submitted = 1
                            and ConfirmationStatus is  null
							 and cd.DocumentationBy = 'self'
							 and PRED.FileName is not null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) selfDoc
							 ON
							 selfDoc.preID = PRE.id
                            " + Join + @"
                            where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"' AND  IsFirstlogin = 1 and submitted = 1 and ConfirmationStatus is  null
                             GROUP BY PRE.BudgetId,PO.Id , GDG.Id ,GDG.UserName ,PD.Id ,PD.UserName,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cGList + @"
                            ,PRE.EmployeeCode,PO.UserName,  PRE.Phone, PRE.Email,PRE.EmpType,selfDoc.TotalFileSelf,deptDoc.TotalFileDept,selfDocPanding.TotalFileSelfPanding,deptDocPanding.TotalFileDeptPanding
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName ,EmpC.UserName ,DesG.UserName,PRE.AgreedDOJ, PRE.SelectionDateTime, PRE.ExpiredDays";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion ModalSubmittedButNotConfirmed

        #region ModalOverDueTotalInterViewee

        public GridModel ListOverDueTotalInterviewee(GridParameter parameters, string companyGroupId, string companyId)
        {
            string cList = string.Empty;
            string wc = string.Empty;
            string Join = string.Empty;
            string cGList = string.Empty;
            IEnumerable<OrgStructureListViewModel> OrgStrList = OrgStructureList(companyGroupId, companyId);
            try
            {
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        cGList += "," + item.ColumnName + ".UserName";
                        if (item.ColumnName == "EmployeeGroup")
                        {
                            Join += "LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                        else
                        {
                            Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                    }
                    else
                    {
                        cGList += "," + item.ColumnName + ".UserName";
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = Po." + item.ColumnName + "Id\n";
                    }
                }
                parameters.CmdText = @" Select ROW_NUMBER() OVER (ORDER BY pre.BudgetId) AS RowNum, isnull(selfDoc.TotalFileSelf,0) TotalFileSelf,isnull(deptDoc.TotalFileDept,0) TotalFileDept
                            ,ISNULL(PRE.EmployeeCode,'') EmployeeCode,isnull(selfDocPanding.TotalFileSelfPanding,0) TotalFileSelfPanding,isnull(deptDocPanding.TotalFileDeptPanding,0) TotalFileDeptPanding
                            ,PRE.BudgetId,PO.Id positionId, GDG.Id DGID,GDG.UserName GivenDesignation,PD.Id DID,PD.UserName Designation,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cList + @"
                            ,PO.UserName PositionName
                            ,Replace(CONVERT(VARCHAR(11), PRE.AgreedDOJ, 106), ' ', '-') AgreedDOJ
                            ,PRE.Phone, PRE.Email,PRE.EmpType
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName EntityName
                            ,Replace(CONVERT(VARCHAR(11), PRE.SelectionDateTime, 106), ' ', '-') SselectionDate
                            ,PRE.ExpiredDays,Replace(CONVERT(VARCHAR(11),   DATEADD(day, PRE.ExpiredDays, PRE.SelectionDateTime), 100), ' ', '-') As ExpiredDate
                              ,EmpC.UserName EmpCategory,DesG.UserName DesigNationGroup
                            FROM PreRecruitmentEmployee PRE

							LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id

                            LEFT OUTER JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id

                            LEFT OUTER JOIN ORG.Position PO ON PMB.PositionId=PO.Id

                            LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id

                            LEFT OUTER JOIN HKP.Designation PD ON PO.DesignationId=PD.Id

                            LEFT OUTER JOIN HKP.Designation GDG ON PRE.GivenDesignationId=GDG.Id

							LEFT OUTER JOIN MST.DesignationMaster DGM ON PO.DesignationId = DGM.DesignationId

                            LEFT OUTER JOIN HKP.EmployeeCategory EmpC ON DGM.EmployeeCategoryId = EmpC.Id

                            LEFT OUTER JOIN HKP.DesignationGroup DesG ON DGM.DesignationGroupId = DesG.Id
							LEFT OUTER JOIN
							(Select
							 Count(PRED.ComplianceDocumentId) TotalFileDeptPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"' AND isnull(ReadyForCandidateAccess,0) = 1
                            AND SelectionStatus = 'Selected'  and Submitted='False'
                            and (PRE.ExpiredDays+ PRE.SelectionDateTime)
                            <
                            GETDATE()
							 and cd.DocumentationBy = 'department'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) deptDocPanding
							 on deptDocPanding.preId = PRE.Id
							 Left outer join
							(
							Select
							 Count(PRED.ComplianceDocumentId) TotalFileSelfPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"' AND isnull(ReadyForCandidateAccess,0) = 1
                            AND SelectionStatus = 'Selected'  and Submitted='False'
                            and (PRE.ExpiredDays+ PRE.SelectionDateTime)
                            <
                            GETDATE()
							 and cd.DocumentationBy = 'self'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) selfDocPanding
							 ON
							 selfDocPanding.preID = PRE.id
							LEFT OUTER JOIN
							(Select
							 Count(PRED.ComplianceDocumentId) TotalFileDept,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"' AND isnull(ReadyForCandidateAccess,0) = 1
                            AND SelectionStatus = 'Selected'  and Submitted='False'
                               and CONVERT(DATE,(PRE.ExpiredDays+ PRE.SelectionDateTime))
                            <
                            CONVERT(DATE,GETDATE())
							 and cd.DocumentationBy = 'department'
							 and PRED.FileName is not null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) deptDoc
							 on deptDoc.preId = PRE.Id
							 Left outer join
							(
							Select
							 Count(PRED.ComplianceDocumentId) TotalFileSelf,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'  AND isnull(ReadyForCandidateAccess,0) = 1
                            AND SelectionStatus = 'Selected'  and Submitted='False'
                               and CONVERT(DATE,(PRE.ExpiredDays+ PRE.SelectionDateTime))
                            <
                            CONVERT(DATE,GETDATE())
							 and cd.DocumentationBy = 'self'
							 and PRED.FileName is not null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) selfDoc
							 ON
							 selfDoc.preID = PRE.id
                            " + Join + @"
                            where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
                            AND isnull(ReadyForCandidateAccess,0) = 1
                            AND SelectionStatus = 'Selected'  and Submitted='False'

                            and CONVERT(DATE,(PRE.ExpiredDays+ PRE.SelectionDateTime))
                            <
                            CONVERT(DATE,GETDATE())
                            GROUP BY PRE.BudgetId,PO.Id , GDG.Id ,GDG.UserName ,PD.Id ,PD.UserName ,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cGList + @"
                            ,PRE.EmployeeCode,PO.UserName,  PRE.Phone, PRE.Email,PRE.EmpType,selfDoc.TotalFileSelf,deptDoc.TotalFileDept,selfDocPanding.TotalFileSelfPanding,deptDocPanding.TotalFileDeptPanding
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName ,EmpC.UserName ,DesG.UserName,PRE.AgreedDOJ, PRE.SelectionDateTime, PRE.ExpiredDays";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion ModalOverDueTotalInterViewee

        #region ModalLoggedInterViewee

        public GridModel ListLoggedInInterviewee(GridParameter parameters, string companyGroupId, string companyId)
        {
            string cList = string.Empty;
            string wc = string.Empty;
            string Join = string.Empty;
            string cGList = string.Empty;
            IEnumerable<OrgStructureListViewModel> OrgStrList = OrgStructureList(companyGroupId, companyId);
            try
            {
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        cGList += "," + item.ColumnName + ".UserName";
                        if (item.ColumnName == "EmployeeGroup")
                        {
                            Join += "LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                        else
                        {
                            Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                    }
                    else
                    {
                        cGList += "," + item.ColumnName + ".UserName";
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = Po." + item.ColumnName + "Id\n";
                    }
                }
                parameters.CmdText = @"Select ROW_NUMBER() OVER (ORDER BY pre.BudgetId) AS RowNum,
                            ISNULL(PRE.EmployeeCode,'') EmployeeCode,isnull(selfDoc.TotalFileSelf,0) TotalFileSelf,isnull(deptDoc.TotalFileDept,0) TotalFileDept
                            ,isnull(selfDocPanding.TotalFileSelfPanding,0) TotalFileSelfPanding,isnull(deptDocPanding.TotalFileDeptPanding,0) TotalFileDeptPanding
                            ,PRE.BudgetId,PO.Id positionId, GDG.Id DGID,GDG.UserName GivenDesignation,PD.Id DID,PD.UserName Designation,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName " + cList + @"
                            ,PO.UserName PositionName
                            ,Replace(CONVERT(VARCHAR(11), PRE.AgreedDOJ, 106), ' ', '-') AgreedDOJ
                            ,PRE.Phone, PRE.Email,PRE.EmpType
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName EntityName
                            ,Replace(CONVERT(VARCHAR(11), PRE.SelectionDateTime, 106), ' ', '-') SselectionDate
                            ,PRE.ExpiredDays,Replace(CONVERT(VARCHAR(11),   DATEADD(day, PRE.ExpiredDays, PRE.SelectionDateTime), 100), ' ', '-') As ExpiredDate
                              ,EmpC.UserName EmpCategory,DesG.UserName DesigNationGroup
                            FROM PreRecruitmentEmployee PRE

							LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id

                            LEFT OUTER JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id

                            LEFT OUTER JOIN ORG.Position PO ON PMB.PositionId=PO.Id

                            LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id

                            LEFT OUTER JOIN HKP.Designation PD ON PO.DesignationId=PD.Id

                            LEFT OUTER JOIN HKP.Designation GDG ON PRE.GivenDesignationId=GDG.Id

							LEFT OUTER JOIN MST.DesignationMaster DGM ON PO.DesignationId = DGM.DesignationId

                            LEFT OUTER JOIN HKP.EmployeeCategory EmpC ON DGM.EmployeeCategoryId = EmpC.Id

                            LEFT OUTER JOIN HKP.DesignationGroup DesG ON DGM.DesignationGroupId = DesG.Id
							LEFT OUTER JOIN
							(Select
							 Count(PRED.ComplianceDocumentId) TotalFileDeptPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							 AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =1 and submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
							 and cd.DocumentationBy = 'department'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) deptDocPanding
							 on deptDocPanding.preId = PRE.Id
							 Left outer join
							(
							Select
							 Count(PRED.ComplianceDocumentId) TotalFileSelfPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							  AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =1 and submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
							 and cd.DocumentationBy = 'self'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) selfDocPanding
							 ON
							 selfDocPanding.preID = PRE.id
							LEFT OUTER JOIN
							(Select
							 Count(PRED.ComplianceDocumentId) TotalFileDept,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							  AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =1 and submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())							 and cd.DocumentationBy = 'department'
							 and PRED.FileName is not null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) deptDoc
							 on deptDoc.preId = PRE.Id
							 Left outer join
							(
							Select
							 Count(PRED.ComplianceDocumentId) TotalFileSelf,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'  and isnull(ReadyForCandidateAccess,0) = 1

                            AND ConfirmationStatus is  null and Submitted = 0  AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
							 and cd.DocumentationBy = 'self'
							 and PRED.FileName is not null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) selfDoc
							 ON
							 selfDoc.preID = PRE.id
                            " + Join + @"
                            where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
                            AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =1 and submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
                            GROUP BY PRE.BudgetId,PO.Id , GDG.Id ,GDG.UserName ,PD.Id ,PD.UserName ,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cGList + @"
                            ,PRE.EmployeeCode,PO.UserName,  PRE.Phone, PRE.Email,PRE.EmpType,selfDoc.TotalFileSelf,deptDoc.TotalFileDept
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName ,EmpC.UserName ,DesG.UserName,PRE.AgreedDOJ, PRE.SelectionDateTime, PRE.ExpiredDays,selfDocPanding.TotalFileSelfPanding,deptDocPanding.TotalFileDeptPanding";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion ModalLoggedInterViewee

        #region ModalLoggedInterViewee_OverDue

        public GridModel ListODLoggedInInterviewee(GridParameter parameters, string companyGroupId, string companyId)
        {
            string cList = string.Empty;
            string wc = string.Empty;
            string Join = string.Empty;
            string cGList = string.Empty;
            IEnumerable<OrgStructureListViewModel> OrgStrList = OrgStructureList(companyGroupId, companyId);
            try
            {
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        cGList += "," + item.ColumnName + ".UserName";
                        if (item.ColumnName == "EmployeeGroup")
                        {
                            Join += "LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                        else
                        {
                            Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                    }
                    else
                    {
                        cGList += "," + item.ColumnName + ".UserName";
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = Po." + item.ColumnName + "Id\n";
                    }
                }
                parameters.CmdText = @"Select ROW_NUMBER() OVER (ORDER BY pre.BudgetId) AS RowNum,
                           ISNULL(PRE.EmployeeCode,'') EmployeeCode,isnull(selfDoc.TotalFileSelf,0) TotalFileSelf,isnull(deptDoc.TotalFileDept,0) TotalFileDept
                            ,isnull(selfDocPanding.TotalFileSelfPanding,0) TotalFileSelfPanding,isnull(deptDocPanding.TotalFileDeptPanding,0) TotalFileDeptPanding
                            ,PRE.BudgetId,PO.Id positionId, GDG.Id DGID,GDG.UserName GivenDesignation,PD.Id DID,PD.UserName Designation,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName " + cList + @"
                            ,PO.UserName PositionName
                            ,Replace(CONVERT(VARCHAR(11), PRE.AgreedDOJ, 106), ' ', '-') AgreedDOJ
                            ,PRE.Phone, PRE.Email,PRE.EmpType
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName EntityName
                            ,Replace(CONVERT(VARCHAR(11), PRE.SelectionDateTime, 106), ' ', '-') SselectionDate
                            ,PRE.ExpiredDays,Replace(CONVERT(VARCHAR(11),   DATEADD(day, PRE.ExpiredDays, PRE.SelectionDateTime), 100), ' ', '-') As ExpiredDate
                              ,EmpC.UserName EmpCategory,DesG.UserName DesigNationGroup
                            FROM PreRecruitmentEmployee PRE

							LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id

                            LEFT OUTER JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id

                            LEFT OUTER JOIN ORG.Position PO ON PMB.PositionId=PO.Id

                            LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id

                            LEFT OUTER JOIN HKP.Designation PD ON PO.DesignationId=PD.Id

                            LEFT OUTER JOIN HKP.Designation GDG ON PRE.GivenDesignationId=GDG.Id

							LEFT OUTER JOIN MST.DesignationMaster DGM ON PO.DesignationId = DGM.DesignationId

                            LEFT OUTER JOIN HKP.EmployeeCategory EmpC ON DGM.EmployeeCategoryId = EmpC.Id

                            LEFT OUTER JOIN HKP.DesignationGroup DesG ON DGM.DesignationGroupId = DesG.Id
							 LEFT OUTER JOIN
							(Select
							 Count(PRED.ComplianceDocumentId) TotalFileDeptPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							 AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =1 and submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())
							 and cd.DocumentationBy = 'department'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) deptDocPanding
							 on deptDocPanding.preId = PRE.Id
							 Left outer join
							(
							Select
							 Count(PRED.ComplianceDocumentId) TotalFileSelfPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							  AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =1 and submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())
							 and cd.DocumentationBy = 'self'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) selfDocPanding
							 ON
							 selfDocPanding.preID = PRE.id
							LEFT Outer join (Select
							 Count(PRD.FileName) TotalFileDept,
							 PRE.Id preId
							 FROM PreRecruitmentEmployee PRE
								LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id
								left outer join hkp.ComplianceDocument cd on cd.Id = PRD.ComplianceDocumentId
							 where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							       AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =1 and submitted = 0
								   and  convert(date,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())
							 and cd.DocumentationBy = 'department'

							 GROUP BY
							 PRE.BudgetId,PRE.Id) deptDoc
							 on deptDoc.preId = PRE.Id
							 Left outer join

							(Select
							 Count(PRD.FileName) TotalFileSelf,
							 PRE.Id preID
							 FROM PreRecruitmentEmployee PRE
								LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id
								left outer join hkp.ComplianceDocument cd on cd.Id = PRD.ComplianceDocumentId
							 where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							       AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =1 and submitted = 0
								   and  convert(date,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())
							 and cd.DocumentationBy = 'self'

							 GROUP BY
							 PRE.BudgetId,PRE.Id) selfDoc
							  on
							   selfDoc.preID = PRE.id
                            " + Join + @"
                            where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"' AND IsFirstLogin = 1 and Submitted='False' AND ConfirmationStatus IS NULL
                            AND PRE.ReadyForCandidateAccess = 'True' and  convert(date,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())

                            GROUP BY PRE.BudgetId,PO.Id , GDG.Id ,GDG.UserName ,PD.Id ,PD.UserName ,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cGList + @"
                            ,PRE.EmployeeCode,PO.UserName,  PRE.Phone, PRE.Email,PRE.EmpType,selfDoc.TotalFileSelf,deptDoc.TotalFileDept
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName ,EmpC.UserName ,DesG.UserName,PRE.AgreedDOJ, PRE.SelectionDateTime, PRE.ExpiredDays ,selfDocPanding.TotalFileSelfPanding,deptDocPanding.TotalFileDeptPanding";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion ModalLoggedInterViewee_OverDue

        #region ModalNotLoggedInterViewee

        public GridModel ListNotoggedInInterviewee(GridParameter parameters, string companyGroupId, string companyId)
        {
            string cList = string.Empty;
            string wc = string.Empty;
            string Join = string.Empty;
            string cGList = string.Empty;
            IEnumerable<OrgStructureListViewModel> OrgStrList = OrgStructureList(companyGroupId, companyId);
            try
            {
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        cGList += "," + item.ColumnName + ".UserName";
                        if (item.ColumnName == "EmployeeGroup")
                        {
                            Join += "LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                        else
                        {
                            Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                    }
                    else
                    {
                        cGList += "," + item.ColumnName + ".UserName";
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = Po." + item.ColumnName + "Id\n";
                    }
                }
                parameters.CmdText = @"Select ROW_NUMBER() OVER (ORDER BY pre.BudgetId) AS RowNum,
                          ISNULL(PRE.EmployeeCode,'') EmployeeCode,isnull(selfDoc.TotalFileSelf,0) TotalFileSelf,isnull(deptDoc.TotalFileDept,0) TotalFileDept
                            ,isnull(selfDocPanding.TotalFileSelfPanding,0) TotalFileSelfPanding,isnull(deptDocPanding.TotalFileDeptPanding,0) TotalFileDeptPanding
                            ,PRE.BudgetId,PO.Id positionId, GDG.Id DGID,GDG.UserName GivenDesignation,PD.Id DID,PD.UserName Designation,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName " + cList + @"
                            ,PO.UserName PositionName
                            ,Replace(CONVERT(VARCHAR(11), PRE.AgreedDOJ, 106), ' ', '-') AgreedDOJ
                            ,PRE.Phone, PRE.Email,PRE.EmpType
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName EntityName
                            ,Replace(CONVERT(VARCHAR(11), PRE.SelectionDateTime, 106), ' ', '-') SselectionDate
                            ,PRE.ExpiredDays,Replace(CONVERT(VARCHAR(11),   DATEADD(day, PRE.ExpiredDays, PRE.SelectionDateTime), 100), ' ', '-') As ExpiredDate
                              ,EmpC.UserName EmpCategory,DesG.UserName DesigNationGroup
                            FROM PreRecruitmentEmployee PRE

							LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id

                            LEFT OUTER JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id

                            LEFT OUTER JOIN ORG.Position PO ON PMB.PositionId=PO.Id

                            LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id

                            LEFT OUTER JOIN HKP.Designation PD ON PO.DesignationId=PD.Id

                            LEFT OUTER JOIN HKP.Designation GDG ON PRE.GivenDesignationId=GDG.Id

							LEFT OUTER JOIN MST.DesignationMaster DGM ON PO.DesignationId = DGM.DesignationId

                            LEFT OUTER JOIN HKP.EmployeeCategory EmpC ON DGM.EmployeeCategoryId = EmpC.Id

                            LEFT OUTER JOIN HKP.DesignationGroup DesG ON DGM.DesignationGroupId = DesG.Id
							 LEFT OUTER JOIN
							(Select
							 Count(PRED.ComplianceDocumentId) TotalFileDeptPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							 AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =0 and submitted = 0
						     AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
							 and cd.DocumentationBy = 'department'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) deptDocPanding
							 on deptDocPanding.preId = PRE.Id
							 Left outer join
							(
							Select
							 Count(PRED.ComplianceDocumentId) TotalFileSelfPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							  AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin = 0 and submitted = 0
							AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
							and cd.DocumentationBy = 'self'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) selfDocPanding
							 ON
							 selfDocPanding.preID = PRE.id
							LEFT Outer join (Select
							 Count(PRD.FileName) TotalFileDept,
							 PRE.Id preId
							 FROM PreRecruitmentEmployee PRE
								LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id
								left outer join hkp.ComplianceDocument cd on cd.Id = PRD.ComplianceDocumentId
							 where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							       AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin = 0 and submitted = 0
								  AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
							 and cd.DocumentationBy = 'department'

							 GROUP BY
							 PRE.BudgetId,PRE.Id) deptDoc
							 on deptDoc.preId = PRE.Id
							 Left outer join

							(Select
							 Count(PRD.FileName) TotalFileSelf,
							 PRE.Id preID
							 FROM PreRecruitmentEmployee PRE
								LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id
								left outer join hkp.ComplianceDocument cd on cd.Id = PRD.ComplianceDocumentId
							 where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							       AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =0 and submitted = 0
								 AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())
							 and cd.DocumentationBy = 'self'

							 GROUP BY
							 PRE.BudgetId,PRE.Id) selfDoc
							  on
							   selfDoc.preID = PRE.id
                            " + Join + @"
                            where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
                            AND isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =0 and submitted = 0
							AND  convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE())

                            GROUP BY PRE.BudgetId,PO.Id , GDG.Id ,GDG.UserName ,PD.Id ,PD.UserName ,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cGList + @"
                            ,PRE.EmployeeCode,PO.UserName,  PRE.Phone, PRE.Email,PRE.EmpType,selfDoc.TotalFileSelf,deptDoc.TotalFileDept
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName ,EmpC.UserName ,DesG.UserName,PRE.AgreedDOJ, PRE.SelectionDateTime, PRE.ExpiredDays,selfDocPanding.TotalFileSelfPanding,deptDocPanding.TotalFileDeptPanding";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion ModalNotLoggedInterViewee

        #region ModalNotLoggedInterViewee OverDue

        public GridModel ListODNotoggedInInterviewee(GridParameter parameters, string companyGroupId, string companyId)
        {
            string cList = string.Empty;
            string cGList = string.Empty;
            string wc = string.Empty;
            string Join = string.Empty;
            IEnumerable<OrgStructureListViewModel> OrgStrList = OrgStructureList(companyGroupId, companyId);
            try
            {
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        cGList += "," + item.ColumnName + ".UserName";

                        if (item.ColumnName == "EmployeeGroup")
                        {
                            Join += "LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                        else
                        {
                            Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = E." + item.ColumnName + "Id\n";
                        }
                    }
                    else
                    {
                        cGList += "," + item.ColumnName + ".UserName";
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = Po." + item.ColumnName + "Id\n";
                    }
                }
                parameters.CmdText = @"Select ROW_NUMBER() OVER (ORDER BY pre.BudgetId) AS RowNum,
                           ISNULL(PRE.EmployeeCode,'') EmployeeCode,isnull(selfDoc.TotalFileSelf,0) TotalFileSelf,isnull(deptDoc.TotalFileDept,0) TotalFileDept
                            ,isnull(selfDocPanding.TotalFileSelfPanding,0) TotalFileSelfPanding,isnull(deptDocPanding.TotalFileDeptPanding,0) TotalFileDeptPanding
                            ,PRE.BudgetId,PO.Id positionId, GDG.Id DGID,GDG.UserName GivenDesignation,PD.Id DID,PD.UserName Designation,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName " + cList + @"
                            ,PO.UserName PositionName
                            ,Replace(CONVERT(VARCHAR(11), PRE.AgreedDOJ, 106), ' ', '-') AgreedDOJ
                            ,PRE.Phone, PRE.Email,PRE.EmpType
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName EntityName
                            ,Replace(CONVERT(VARCHAR(11), PRE.SelectionDateTime, 106), ' ', '-') SselectionDate
                            ,PRE.ExpiredDays,Replace(CONVERT(VARCHAR(11),   DATEADD(day, PRE.ExpiredDays, PRE.SelectionDateTime), 100), ' ', '-') As ExpiredDate
                              ,EmpC.UserName EmpCategory,DesG.UserName DesigNationGroup
                            FROM PreRecruitmentEmployee PRE

							LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id

                            LEFT OUTER JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id

                            LEFT OUTER JOIN ORG.Position PO ON PMB.PositionId=PO.Id

                            LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id

                            LEFT OUTER JOIN HKP.Designation PD ON PO.DesignationId=PD.Id

                            LEFT OUTER JOIN HKP.Designation GDG ON PRE.GivenDesignationId=GDG.Id

							LEFT OUTER JOIN MST.DesignationMaster DGM ON PO.DesignationId = DGM.DesignationId

                            LEFT OUTER JOIN HKP.EmployeeCategory EmpC ON DGM.EmployeeCategoryId = EmpC.Id

                            LEFT OUTER JOIN HKP.DesignationGroup DesG ON DGM.DesignationGroupId = DesG.Id
							 LEFT OUTER JOIN
							(Select
							 Count(PRED.ComplianceDocumentId) TotalFileDeptPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							 AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =0 and submitted = 0 AND  convert(date,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())
							 and cd.DocumentationBy = 'department'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) deptDocPanding
							 on deptDocPanding.preId = PRE.Id
							 Left outer join
							(
							Select
							 Count(PRED.ComplianceDocumentId) TotalFileSelfPanding,
							 PRED.PreRecruitmentEmployeeId preId
							 FROM PreRecruitmentDocument PRED
								LEFT OUTER JOIN dbo.PreRecruitmentEmployee PRE ON PRE.Id=PRED.PreRecruitmentEmployeeId
								left outer join hkp.ComplianceDocument cd on cd.Id = PRED.ComplianceDocumentId
							 where PRE.GroupID = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							  AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =0 and submitted = 0
							AND  convert(date,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())
							and cd.DocumentationBy = 'self'
							 and PRED.FileName is  null
							 GROUP BY
							 PRE.BudgetId,PRED.PreRecruitmentEmployeeId) selfDocPanding
							 ON
							 selfDocPanding.preID = PRE.id
							LEFT Outer join (Select
							 Count(PRD.FileName) TotalFileDept,
							 PRE.Id preId
							 FROM PreRecruitmentEmployee PRE
								LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id
								left outer join hkp.ComplianceDocument cd on cd.Id = PRD.ComplianceDocumentId
							 where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							       AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin = 0 and submitted = 0
								  AND  convert(date,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())
							 and cd.DocumentationBy = 'department'

							 GROUP BY
							 PRE.BudgetId,PRE.Id) deptDoc
							 on deptDoc.preId = PRE.Id
							 Left outer join

							(Select
							 Count(PRD.FileName) TotalFileSelf,
							 PRE.Id preID
							 FROM PreRecruitmentEmployee PRE
								LEFT OUTER JOIN dbo.PreRecruitmentDocument  PRD ON PRD.PreRecruitmentEmployeeId=PRE.Id
								left outer join hkp.ComplianceDocument cd on cd.Id = PRD.ComplianceDocumentId
							 where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
							       AND  isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =0 and submitted = 0
								 AND  convert(date,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE())
							 and cd.DocumentationBy = 'self'

							 GROUP BY
							 PRE.BudgetId,PRE.Id) selfDoc
							  on
							   selfDoc.preID = PRE.id
                            " + Join + @"
                            where PRE.GroupId = '" + companyGroupId + @"' and PRE.CompanyId = '" + companyId + @"'
                            AND isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =0
                            AND PRE.ReadyForCandidateAccess = 'True'
                            AND CONVERT(DATE,(PRE.ExpiredDays+ PRE.SelectionDateTime))
                            <
                            CONVERT(DATE,GETDATE())
                            GROUP BY PRE.BudgetId,PO.Id , GDG.Id ,GDG.UserName ,PD.Id ,PD.UserName ,
                            PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
							" + cGList + @"
                            ,PRE.EmployeeCode,PO.UserName,  PRE.Phone, PRE.Email,PRE.EmpType,selfDoc.TotalFileSelf,deptDoc.TotalFileDept
                            ,PRE.NationalID,PRE.TotalSalary,E.UserName ,EmpC.UserName ,DesG.UserName,PRE.AgreedDOJ, PRE.SelectionDateTime, PRE.ExpiredDays,selfDocPanding.TotalFileSelfPanding,deptDocPanding.TotalFileDeptPanding";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion ModalNotLoggedInterViewee OverDue

        #region Table DocumentUploadingStatus

        public IEnumerable<object> DocumentUploadingStatus(string companyGroupId, string companyId, string status)
        {
            string stat = string.Empty;
            if (status == "Selected")
            {
                stat = "WHERE isnull(ReadyForCandidateAccess,0) = 1 AND ConfirmationStatus IS  NULL and Submitted = 0";
            }
            else if (status == "NotConfirmed")
            {
                stat = "WHERE IsFirstlogin = 1 and submitted = 1 and ConfirmationStatus is  null";
            }
            else if (status == "TotalOverDue")
            {
                stat = @"WHERE   isnull(ReadyForCandidateAccess,0) = 1 AND SelectionStatus = 'Selected'  and Submitted = 'False'
                              and CONVERT(DATE,(PRE.ExpiredDays+ PRE.SelectionDateTime))
                            <
                            CONVERT(DATE,GETDATE())  ";
            }
            else if (status == "LoggedIn")
            {
                stat = @"WHERE isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =1 and submitted = 0 and convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE()) ";
            }
            else if (status == "LoggedInOverDue")
            {
                stat = @"WHERE isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =1 and submitted = 0 and convert(date,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE()) ";
            }
            else if (status == "NotLoggedIn")
            {
                stat = @"WHERE isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin =0 and submitted = 0 and convert(date,(ExpiredDays+ SelectionDateTime)) >= convert(date,GETDATE()) ";
            }
            else if (status == "NotLoggedInOverDue")
            {
                stat = @"WHERE isnull(ReadyForCandidateAccess,0) = 1 AND IsFirstlogin = 0 and submitted = 0 and convert(date,(ExpiredDays+ SelectionDateTime)) < convert(date,GETDATE()) ";
            }

            try
            {
                var sql = @"select
								(
								    SELECT  count(ComplianceDocumentId) totalDoc FROM PreRecruitmentDocument pre
									LEFT OUTER JOIN [HKP].[ComplianceDocument] cd ON cd.Id = pre.ComplianceDocumentId
									Left join [hkp].[ComplianceDocumentCategory] cdc on cdc.Id = cd.ComplianceDocumentCategoryId
									WHERE pre.PreRecruitmentEmployeeId IN (SELECT Id
									FROM [dbo].[PreRecruitmentEmployee] pre
									   " + stat + @"
									AND pre.GroupId = '" + companyGroupId + @"')
									AND cd.CompanyGroupId = '" + companyGroupId + @"'
								) totalDoc

								,

								(
								  SELECT  count(ComplianceDocumentId) totalDocMandt FROM PreRecruitmentDocument pre
								  LEFT OUTER JOIN [HKP].[ComplianceDocument] cd ON cd.Id = pre.ComplianceDocumentId
								  Left join [hkp].[ComplianceDocumentCategory] cdc on cdc.Id = cd.ComplianceDocumentCategoryId

								  WHERE pre.PreRecruitmentEmployeeId IN (SELECT Id
								  FROM [dbo].[PreRecruitmentEmployee] pre
								       " + stat + @"
								  AND pre.GroupId ='" + companyGroupId + @"')
								  AND cd.OptionalOrMandatory = 'Mandatory'
								  and cd.DocumentationBy = 'Self'
																AND cd.CompanyGroupId = '" + companyGroupId + @"'
								)totalDocMandtSelf
								,
								(
								    SELECT  count(ComplianceDocumentId) totalDocMandt FROM PreRecruitmentDocument pre
									LEFT OUTER JOIN [HKP].[ComplianceDocument] cd ON cd.Id = pre.ComplianceDocumentId
									Left join [hkp].[ComplianceDocumentCategory] cdc on cdc.Id = cd.ComplianceDocumentCategoryId

									WHERE pre.PreRecruitmentEmployeeId IN (SELECT Id
									FROM [dbo].[PreRecruitmentEmployee] pre
									   " + stat + @"
									AND pre.GroupId = '" + companyGroupId + @"')
									AND cd.OptionalOrMandatory = 'Mandatory'
									and cd.DocumentationBy = 'Department'
									AND cd.CompanyGroupId = '" + companyGroupId + @"'
								) totalDocMandtDept
								,
								(
									SELECT  count(ComplianceDocumentId) totalDocMandtpanding FROM PreRecruitmentDocument pre
									LEFT OUTER JOIN [HKP].[ComplianceDocument] cd ON cd.Id = pre.ComplianceDocumentId
									Left join [hkp].[ComplianceDocumentCategory] cdc on cdc.Id = cd.ComplianceDocumentCategoryId

									WHERE pre.PreRecruitmentEmployeeId IN (SELECT Id
									FROM [dbo].[PreRecruitmentEmployee] pre
									   " + stat + @"
									AND pre.GroupId = '" + companyGroupId + @"')
									AND cd.OptionalOrMandatory = 'Mandatory'
									AND cd.CompanyGroupId = '" + companyGroupId + @"'
									and cd.DocumentationBy = 'Self'
									and pre.FileName is null
								)totalDocMandtpandingSelf
								,
								(
									SELECT  count(ComplianceDocumentId) totalDocMandtpanding FROM PreRecruitmentDocument pre
									LEFT OUTER JOIN [HKP].[ComplianceDocument] cd ON cd.Id = pre.ComplianceDocumentId
									Left join [hkp].[ComplianceDocumentCategory] cdc on cdc.Id = cd.ComplianceDocumentCategoryId

									WHERE pre.PreRecruitmentEmployeeId IN (SELECT Id
									FROM [dbo].[PreRecruitmentEmployee] pre
									   " + stat + @"
									AND pre.GroupId = '" + companyGroupId + @"')
									AND cd.OptionalOrMandatory = 'Mandatory'
									AND cd.CompanyGroupId = '" + companyGroupId + @"'
									and cd.DocumentationBy = 'Department'
									and pre.FileName is null
								) totalDocMandtpandingDept
								,
								(
									SELECT  count(ComplianceDocumentId) totalDocOpt FROM PreRecruitmentDocument pre
									LEFT OUTER JOIN [HKP].[ComplianceDocument] cd ON cd.Id = pre.ComplianceDocumentId
									Left join [hkp].[ComplianceDocumentCategory] cdc on cdc.Id = cd.ComplianceDocumentCategoryId
									WHERE pre.PreRecruitmentEmployeeId IN (SELECT Id
									FROM [dbo].[PreRecruitmentEmployee] pre
									    " + stat + @"
									AND pre.GroupId = '" + companyGroupId + @"')  AND cd.OptionalOrMandatory = 'Optional'
									AND cd.CompanyGroupId = '" + companyGroupId + @"'
									AND cd.DocumentationBy = 'Self'
								) totalDocOptSelf
								,
								(
									SELECT  count(ComplianceDocumentId) totalDocOpt FROM PreRecruitmentDocument pre
									LEFT OUTER JOIN [HKP].[ComplianceDocument] cd ON cd.Id = pre.ComplianceDocumentId
								    Left join [hkp].[ComplianceDocumentCategory] cdc on cdc.Id = cd.ComplianceDocumentCategoryId
									WHERE pre.PreRecruitmentEmployeeId IN (SELECT Id
									FROM [dbo].[PreRecruitmentEmployee] pre
									      " + stat + @"
									AND pre.GroupId = '" + companyGroupId + @"')  AND cd.OptionalOrMandatory = 'Optional'
									AND cd.CompanyGroupId = '" + companyGroupId + @"'
																AND cd.DocumentationBy = 'Department'
								) totalDocOptDept
								,
								(
									SELECT  count(ComplianceDocumentId) totalDocOptpanding FROM PreRecruitmentDocument pre
									LEFT OUTER JOIN [HKP].[ComplianceDocument] cd ON cd.Id = pre.ComplianceDocumentId
								    Left join [hkp].[ComplianceDocumentCategory] cdc on cdc.Id = cd.ComplianceDocumentCategoryId
									WHERE pre.PreRecruitmentEmployeeId IN (SELECT Id
									FROM [dbo].[PreRecruitmentEmployee] pre
									    " + stat + @"
									AND pre.GroupId = '" + companyGroupId + @"')  AND cd.OptionalOrMandatory = 'Optional'
									AND cd.CompanyGroupId = '" + companyGroupId + @"'
									AND cd.DocumentationBy = 'Department'
								   ANd pre.FileName is null
								) totalDocOptpandingDept
								,
								(
								    SELECT  count(ComplianceDocumentId) totalDocOptpanding  FROM PreRecruitmentDocument pre
									LEFT OUTER JOIN [HKP].[ComplianceDocument] cd ON cd.Id = pre.ComplianceDocumentId
								    Left join [hkp].[ComplianceDocumentCategory] cdc on cdc.Id = cd.ComplianceDocumentCategoryId
									WHERE pre.PreRecruitmentEmployeeId IN (SELECT Id
									FROM [dbo].[PreRecruitmentEmployee] pre
									   " + stat + @"
									AND pre.GroupId = '" + companyGroupId + @"')  AND cd.OptionalOrMandatory = 'Optional'
									AND cd.CompanyGroupId = '" + companyGroupId + @"'
									AND cd.DocumentationBy = 'Self'
									ANd pre.FileName is null
								)totalDocOptpandingSelf";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Table DocumentUploadingStatus

        #endregion Modal

        #region Document Tooltip

        public IEnumerable<object> EmployeeWiseDoument(string EmpId, string CompanyGroupId, string CompanyId)
        {
            try
            {   // Opc, MnC, Selected
                var sql = @"select cd.UserName docName, pred.ComplianceDocumentId, cd.CompanyGroupId,cd.OptionalOrMandatory,
	                        UploadStatus = case when pred.FileName is null then 'No'
					        when pred.FileName is not null then 'Yes'
					        End
	                        from PreRecruitmentDocument pred
		                        left outer join [HKP].[ComplianceDocument] cd on cd.Id = pred.ComplianceDocumentId
		                        left outer join [DBO].[PreRecruitmentEmployee] pre on pre.Id = pred.PreRecruitmentEmployeeId
	                        where pred.PreRecruitmentEmployeeId = '" + EmpId + @"' AND pre.GroupID = '" + CompanyGroupId + @"' AND pre.CompanyId = '" + CompanyId + @"' and cd.DocumentationBy = 'self' and pred.FileName is not  null";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> EmployeeWiseDoumentDept(string EmpId, string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"select cd.UserName docName, pred.ComplianceDocumentId, cd.CompanyGroupId,cd.OptionalOrMandatory,
	                        UploadStatus = case when pred.FileName is null then 'No'
					        when pred.FileName is not null then 'Yes'
					        End
	                        from PreRecruitmentDocument pred
		                        left outer join [HKP].[ComplianceDocument] cd on cd.Id = pred.ComplianceDocumentId
		                        left outer join [DBO].[PreRecruitmentEmployee] pre on pre.Id = pred.PreRecruitmentEmployeeId
	                        where pred.PreRecruitmentEmployeeId = '" + EmpId + @"' AND pre.GroupID = '" + CompanyGroupId + @"' AND pre.CompanyId = '" + CompanyId + @"' and cd.DocumentationBy = 'department' and pred.FileName is not null";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> EmployeeWiseNotUploadedDoumentSelf(string EmpId, string CompanyGroupId, string CompanyId)
        {
            try
            {   // Opc, MnC, Selected
                var sql = @"select cd.UserName docName, pred.ComplianceDocumentId, cd.CompanyGroupId,cd.OptionalOrMandatory,
	                        UploadStatus = case when pred.FileName is null then 'No'
					        when pred.FileName is not null then 'Yes'
					        End
	                        from PreRecruitmentDocument pred
		                        left outer join [HKP].[ComplianceDocument] cd on cd.Id = pred.ComplianceDocumentId
		                        left outer join [DBO].[PreRecruitmentEmployee] pre on pre.Id = pred.PreRecruitmentEmployeeId
	                        where pred.PreRecruitmentEmployeeId = '" + EmpId + @"' AND pre.GroupID = '" + CompanyGroupId + @"' AND pre.CompanyId = '" + CompanyId + @"' and cd.DocumentationBy = 'self' and pred.FileName is  null";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> EmployeeWiseNotUploadedDoumentDept(string EmpId, string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"select cd.UserName docName, pred.ComplianceDocumentId, cd.CompanyGroupId,cd.OptionalOrMandatory,
	                        UploadStatus = case when pred.FileName is null then 'No'
					        when pred.FileName is not null then 'Yes'
					        End
	                        from PreRecruitmentDocument pred
		                        left outer join [HKP].[ComplianceDocument] cd on cd.Id = pred.ComplianceDocumentId
		                        left outer join [DBO].[PreRecruitmentEmployee] pre on pre.Id = pred.PreRecruitmentEmployeeId
	                        where pred.PreRecruitmentEmployeeId = '" + EmpId + @"' AND pre.GroupID = '" + CompanyGroupId + @"'
AND pre.CompanyId = '" + CompanyId + @"' and cd.DocumentationBy = 'department' and pred.FileName is null";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Document Tooltip

        #region PreDocDashBoard

        public IEnumerable<object> PreDocSubmitted(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"select
                            count(pred.ComplianceDocumentId) totDoc,cd.ShortName docName, cd.UserName fullDocName,cd.OptionalOrMandatory OptionalOrMandatory,
                            count(pre.Id ) totEmp,
                            cd.CompanyGroupId,
                            cd.OptionalOrMandatory
                            FROM PreRecruitmentDocument pred
                            LEFT JOIN[HKP].[ComplianceDocument] cd ON cd.Id = pred.ComplianceDocumentId
                            LEFT JOIN[DBO].[PreRecruitmentEmployee] pre ON pre.Id = pred.PreRecruitmentEmployeeId
                            WHERE pred.FileName IS NULL
                              AND pre.GroupID = '" + CompanyGroupId + @"'
                            	AND pre.CompanyId = '" + CompanyId + @"' AND pred.IsCopied = 0
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

        public IEnumerable<object> PreDocNotSubmitted(string CompanyGroupId, string CompanyId)
        {
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
                            WHERE pred.FileName IS NOT NULL
                              AND pre.GroupID = '" + CompanyGroupId + @"'
                            	AND pre.CompanyId = '" + CompanyId + @"' AND pred.IsCopied = 0
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

        #endregion PreDocDashBoard
    }
}