using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using System;

namespace Library.Service.Employees
{
    public class TempDocDashboardService : Service<TempDocDashboard>, ITempDocDashboardService
    {
        #region Constructor

        private IRepositoryAsync<TempDocDashboard> _tempDocDashboardRepository;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TempDocDashboardService(
              IRepositoryAsync<TempDocDashboard> tempDocDashboardRepository
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(tempDocDashboardRepository, unitOfWork)
        {
            _tempDocDashboardRepository = tempDocDashboardRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void DataInsertInTemTable()
        {
            try
            {
                var sql = @"TRUNCATE TABLE [dbo].[TempDocDashboard]
                            INSERT INTO [dbo].[TempDocDashboard](CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, ComplianceDocumentId, DocumentType, DocumentationBy,EmployeeTypeOrCategory, ComplianceDocumentSetId, EmployeeId, PreRecruitmentEmployeeId, Importance, OptionalOrMandatory,EmploymentStage , DueDate, Segment)
							SELECT PRE.GroupID AS CompanyGroupId, CD.ComplianceDocumentCategoryId AS DocumentCategoryId
								    , CD.ComplianceDocumentSubCategoryId AS DocumentSubCategoryId
								    , PRD.ComplianceDocumentId
								    , CD.DocumentType
								    , CD.DocumentationBy
									, '' EmployeeTypeOrCategory
								   --,SAD.ResponsiblePersonId
								    , PRD.ComplianceDocumentSetId
								    --, SAD.DocumentConfigurationDesignationGroupId
								    , NULL EmployeeId
								    , PRD.PreRecruitmentEmployeeId
								    , CD.Importance
								    , PRD.OptionalOrMandatory
								    , CD.EmploymentStage
								    , PRD.DueDate
							        , Segment=CASE WHEN (DATEDIFF(day, CAST(PRD.DueDate AS DATE), CAST(GETDATE() AS DATE))-1) < 10 THEN 1
												    WHEN (DATEDIFF(day, CAST(PRD.DueDate AS DATE), CAST(GETDATE() AS DATE))-1) BETWEEN 10 AND 30 THEN 2
												    WHEN (DATEDIFF(day, CAST(PRD.DueDate AS DATE), CAST(GETDATE() AS DATE))-1) > 30 THEN 3
												    WHEN PRD.DueDate IS NULL THEN NULL ELSE NULL END
							FROM [dbo].[PreRecruitmentDocument] AS PRD
							LEFT JOIN [dbo].[PreRecruitmentEmployee] AS PRE ON PRD.PreRecruitmentEmployeeId=PRE.Id
							LEFT JOIN [HKP].[ComplianceDocument] AS CD ON PRD.ComplianceDocumentId=CD.Id
							LEFT JOIN [HKP].[ComplianceDocumentSetDetail] AS CDSD ON CDSD.ComplianceDocumentId=PRD.ComplianceDocumentId AND CDSD.ComplianceDocumentSetId=PRD.ComplianceDocumentSetId
							--LEFT JOIN [HKP].[DocumentSetAssignDetail] AS SAD ON SAD.ComplianceDocumentId=PRD.ComplianceDocumentId AND SAD.ComplianceDocumentSetId=PRD.ComplianceDocumentSetId
							WHERE PRD.FileId IS NULL AND PRD.[FileName] IS NULL AND PRD.IsCopied=0
							UNION
							SELECT EI.GroupID AS CompanyGroupId, CD.ComplianceDocumentCategoryId AS DocumentCategoryId
							, CD.ComplianceDocumentSubCategoryId AS DocumentSubCategoryId
							, PRD.ComplianceDocumentId
							, CD.DocumentType
							, CD.DocumentationBy
							, EmpC.Id EmployeeTypeOrCategory
							--, SAD.ResponsiblePersonId
							, PRD.ComplianceDocumentSetId
							--, SAD.DocumentConfigurationDesignationGroupId
							, EmployeeId=PRD.EmpSystemID
							, PRD.PreRecruitmentEmployeeId
							, CD.Importance
							, PRD.OptionalOrMandatory
							, CD.EmploymentStage
							, PRD.DueDate
							, Segment=CASE WHEN (DATEDIFF(day, CAST(PRD.DueDate AS DATE), CAST(GETDATE() AS DATE))-1) < 10 THEN 1
							WHEN (DATEDIFF(day, CAST(PRD.DueDate AS DATE), CAST(GETDATE() AS DATE))-1) BETWEEN 10 AND 30 THEN 2
							WHEN (DATEDIFF(day, CAST(PRD.DueDate AS DATE), CAST(GETDATE() AS DATE))-1) > 30 THEN 3
							WHEN PRD.DueDate IS NULL THEN NULL ELSE NULL END
							FROM [dbo].[EmployeeDocument] AS PRD
							JOIN [dbo].[EmployeeInformation] AS EI ON PRD.EmpSystemID=EI.SystemId
							JOIN [HKP].[ComplianceDocument] AS CD ON PRD.ComplianceDocumentId=CD.Id
							LEFT JOIN [HKP].[ComplianceDocumentSetDetail] AS CDSD ON CDSD.ComplianceDocumentId=PRD.ComplianceDocumentId AND CDSD.ComplianceDocumentSetId=PRD.ComplianceDocumentSetId
							LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
							LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
							LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
							--LEFT JOIN [HKP].[DocumentSetAssignDetail] AS SAD ON SAD.ComplianceDocumentId=CD.Id AND SAD.ComplianceDocumentSetId=PRD.ComplianceDocumentSetId
							WHERE PRD.FileId IS NULL AND PRD.[FileName] IS NULL AND EI.EmployeeStatus = 'Active'";
                _tempDocDashboardRepository.ExecuteSqlCommand(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}