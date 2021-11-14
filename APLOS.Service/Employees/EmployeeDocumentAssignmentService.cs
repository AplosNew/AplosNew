#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class EmployeeDocumentAssignmentService : Service<EmployeeInformation>, IEmployeeDocumentAssignmentService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<EmployeeInformation> _employeeInformationRepository;
        private readonly ISignatureService _signatrueService;
        private readonly IEmployeeDocumentService _employeeDocumentService;
        private readonly IRepositoryAsync<EmployeeDocument> _employeeDocumentRepository;

        public EmployeeDocumentAssignmentService(
             IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISignatureService signatrueService
            , IEmployeeDocumentService employeeDocumentService
            , IRepositoryAsync<EmployeeInformation> employeeInformationRepository
            , IRepositoryAsync<EmployeeDocument> employeeDocumentRepository
            , ISqlRepository sqlRepository
            ) : base(employeeInformationRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _signatrueService = signatrueService;
            _employeeInformationRepository = employeeInformationRepository;
            _employeeDocumentService = employeeDocumentService;
            _employeeDocumentRepository = employeeDocumentRepository;
        }

        #endregion Constructor

        public void InsertORUpdateMaster(IEnumerable<EmployeeInformation> entities)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                foreach (var item in entities)
                {
       //             if (string.IsNullOrEmpty(item.EmploymentType))
       //             {
       //                 var sql = @"DECLARE @employeeId varchar(20)='" + item.SystemId + @"';
							//		DECLARE @plantId varchar(20)='" + item.PlantID + @"';
							//		DECLARE @manpowerBudgetId varchar(20);
							//		DECLARE @givenDesignationId varchar(20);
							//		DECLARE @empType varchar(20);
							//		DELETE FROM EmployeeDocument WHERE EmpSystemID=@employeeId AND FileName IS NULL;
							//		SELECT  @ManpowerBudgetId=BudgetCode, @givenDesignationId=GivenDesignationId, @empType=EmpType FROM EmployeeInformation WHERE SystemId=@employeeId;
							//		INSERT INTO EmployeeDocument (Id, EmpSystemID, AddedBy, AddedDate, ComplianceDocumentId, OptionalOrMandatory, ComplianceDocumentSetId, ResponsiblePersonId)
							//		SELECT @employeeId+'-'+ X.ComplianceDocumentId, @employeeId, '" + identity.Name + @"', GETDATE(), X.ComplianceDocumentId, X.OptionalOrMandatory, X.ComplianceDocumentSetId, X.ResponsiblePersonId from (
							//		SELECT CD.Id AS ComplianceDocumentId
							//		,CDSD.OptionalOrMandatory
							//		,DC.ComplianceDocumentSetId
							//		,DC.ResponsiblePersonId
							//	FROM
							//	(
							//	SELECT DISTINCT
							//			P.EmploymentType
							//			,DM.EmployeeCategoryId
							//			,DM.DesignationId
							//			,P.GivenDesignationId
							//		FROM EmployeeInformation P
							//		--LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
       //                             LEFT OUTER JOIN MST.DesignationMasterLegalDesignation LDM ON P.LegalDesignationId = LDM.LegalDesignationId
							//		LEFT OUTER JOIN MST.DesignationMaster DM ON LDM.DesignationMasterId = DM.Id
							//		WHERE P.GivenDesignationId=@givenDesignationId
							//		) BD
							//	LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
							//	--AND DC.EmploymentType = BD.EmploymentType
							//	LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
							//	LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
							//	LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
							//	LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
							//	LEFT OUTER JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id = PC.ComplianceDocumentId
							//	LEFT OUTER JOIN ORG.Position PO ON PC.PositionId = PO.Id
							//	LEFT JOIN MST.ManpowerBudget MB ON MB.PositionId=PO.Id
							//	WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId =@plantId AND CD.IsSkillBased = 1
							//	AND MB.Id=@manpowerBudgetId AND (CD.EmpType = @empType OR CD.EmpType = 'Both')
							//UNION
							//		SELECT  CD.Id AS ComplianceDocumentId
							//		,CDSD.OptionalOrMandatory
							//		,DC.ComplianceDocumentSetId
							//		,DC.ResponsiblePersonId
							//	FROM (
							//SELECT DISTINCT
							//			P.EmploymentType
							//			,DM.EmployeeCategoryId
							//			,DM.DesignationId
							//			,P.GivenDesignationId
							//		FROM EmployeeInformation P
							//		--LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
       //                             LEFT OUTER JOIN MST.DesignationMasterLegalDesignation LDM ON P.LegalDesignationId = LDM.LegalDesignationId
							//		LEFT OUTER JOIN MST.DesignationMaster DM ON LDM.DesignationMasterId = DM.Id
							//		WHERE P.GivenDesignationId=@givenDesignationId
							//		) BD
							//	LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
							//	--AND DC.EmploymentType = BD.EmploymentType
							//	LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
							//	LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
							//	LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
							//	LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
							//	WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId = @plantId AND CD.IsSkillBased = 0 AND (CD.EmpType = @empType OR CD.EmpType = 'Both')
							//	)X  WHERE X.ComplianceDocumentId NOT IN(SELECT ComplianceDocumentId from EmployeeDocument ED WHERE ED.EmpSystemID=@employeeId)";
       //                 _employeeDocumentRepository.ExecuteSqlCommand(sql);
       //             }
       //             else
       //             {
                        var sql = @"DECLARE @employeeId varchar(20)='" + item.SystemId + @"';
									DECLARE @plantId varchar(20)='" + item.PlantID + @"';
									DECLARE @manpowerBudgetId varchar(20);
									DECLARE @givenDesignationId varchar(20);
									DECLARE @empType varchar(20);
									DELETE FROM EmployeeDocument WHERE EmpSystemID=@employeeId AND FileName IS NULL;
									SELECT  @ManpowerBudgetId=BudgetCode, @givenDesignationId=GivenDesignationId, @empType=EmpType FROM EmployeeInformation WHERE SystemId=@employeeId;
									INSERT INTO EmployeeDocument (Id, EmpSystemID, AddedBy, AddedDate, ComplianceDocumentId, OptionalOrMandatory, ComplianceDocumentSetId, ResponsiblePersonId)
									SELECT @employeeId+'-'+ X.ComplianceDocumentId, @employeeId, '" + identity.Name + @"', GETDATE(), X.ComplianceDocumentId, X.OptionalOrMandatory, X.ComplianceDocumentSetId, X.ResponsiblePersonId from (
									SELECT CD.Id AS ComplianceDocumentId
									,CDSD.OptionalOrMandatory
									,DC.ComplianceDocumentSetId
									,DC.ResponsiblePersonId
								FROM
								(
								SELECT DISTINCT
										P.EmploymentType
										,DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM EmployeeInformation P
									--LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
                                    LEFT OUTER JOIN MST.DesignationMasterLegalDesignation LDM ON P.LegalDesignationId = LDM.LegalDesignationId
									LEFT OUTER JOIN MST.DesignationMaster DM ON LDM.DesignationMasterId = DM.Id
									WHERE P.GivenDesignationId=@givenDesignationId
									) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								LEFT OUTER JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id = PC.ComplianceDocumentId
								LEFT OUTER JOIN ORG.Position PO ON PC.PositionId = PO.Id
								LEFT JOIN MST.ManpowerBudget MB ON MB.PositionId=PO.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId =@plantId AND CD.IsSkillBased = 1
								AND MB.Id=@manpowerBudgetId AND (CD.EmpType = @empType OR CD.EmpType = 'Both')
							UNION
									SELECT  CD.Id AS ComplianceDocumentId
									,CDSD.OptionalOrMandatory
									,DC.ComplianceDocumentSetId
									,DC.ResponsiblePersonId
								FROM (
							SELECT DISTINCT
										P.EmploymentType
										,DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM EmployeeInformation P
									--LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
                                    LEFT OUTER JOIN MST.DesignationMasterLegalDesignation LDM ON P.LegalDesignationId = LDM.LegalDesignationId
									LEFT OUTER JOIN MST.DesignationMaster DM ON LDM.DesignationMasterId = DM.Id
									WHERE P.GivenDesignationId=@givenDesignationId
									) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId = @plantId AND CD.IsSkillBased = 0 AND (CD.EmpType = @empType OR CD.EmpType = 'Both')
								)X  WHERE X.ComplianceDocumentId NOT IN(SELECT ComplianceDocumentId from EmployeeDocument ED WHERE ED.EmpSystemID=@employeeId)";
                        _employeeDocumentRepository.ExecuteSqlCommand(sql);
                   //}
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region Operation

        public GridModel GetEmployeeData(GridParameter parameters, string assign, string plantId)
        {
            try
            {
                var a = "";
                if (assign.ToUpper() == "ASSIGN")
                {
                    a = " AND ED.TotalDoc>0";
                }
                else
                {
                    a = " AND isnull(ED.TotalDoc,0)=0";
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"Select PRE.*,0 Active,E.UserName EntityName,D.UserName Designation,PR.UserName PositionName,DEG.UserName GivenDesignation, DEPT.UserName AS Department,ED.TotalDoc,PMB.Code
							FROM EmployeeInformation PRE
							LEFT JOIN MST.ManpowerBudget PMB ON PRE.BudgetCode=PMB.Id
							LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
							LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
							LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
							LEFT JOIN HKP.Designation DEG on DEG.Id=PRE.GivenDesignationId
							LEFT JOIN (SELECT EmpSystemID, COUNT (Id) TotalDoc FROM  dbo.EmployeeDocument group by EmpSystemID) AS ED ON PRE.SystemId=ED.EmpSystemID
							Where PRE.EmployeeStatus='Active' AND PRE.GroupID='" + identity.CompanyGroupId + @"' AND PRE.CompanyId='" + identity.CompanyId + @"' AND PRE.PlantId='" + plantId + @"' " + a + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetDocumentDataList(string empId)
        {
            try
            {
                var sql = @"SELECT ED.*, CD.UserName DocumentName, CD.EmploymentStage FROM EmployeeDocument ED
							LEFT JOIN HKP.ComplianceDocument CD ON ED.ComplianceDocumentId=CD.Id
							WHERE ED.EmpSystemID='" + empId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        

        #endregion Operation
    }
}