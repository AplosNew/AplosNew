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
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Employees
{
    public class ApprovalConfigurationService : Service<ApprovalConfiguration>, IApprovalConfigurationService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public ApprovalConfigurationService(
            IRepositoryAsync<ApprovalConfiguration> ApprovalConfigurationRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(ApprovalConfigurationRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(ApprovalConfiguration), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(ApprovalConfiguration entity)
        {
            try
            {
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(ApprovalConfiguration entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string plantId, string entityId)
        {
            try
            {
                parameters.CmdText = @"SELECT PC.*,P.UserName AS Plant, E.UserName Entity
										,PRE.EmployeeName AS UpperDesignationAndSpecialAllowanceApprovedByPerson,PRE.EmployeeStatus UpperEmployeeStatus,PRE.EmployeeCode UpperDesignationAndSpecialAllowanceRPEC
										,PR.EmployeeName AS DocumentResponsible,PR.EmployeeStatus DocumentEmployeeStatus,PR.EmployeeCode OrgDocRPEC
										,EM.EmployeeName AS PreRecruitmentDocRPerson,EM.EmployeeStatus PreRecruitmentDocEmployeeStatus,EM.EmployeeCode PreRecruitmentDocRPEC
										,EMP.EmployeeName AS PostRecruitmentDocRPerson,EMP.EmployeeStatus PostRecruitmentDocEmployeeStatus,EMP.EmployeeCode PostRecruitmentDocRPEC
										,EMPI.EmployeeName AS RecruitmentFinalConfirmationRPerson,EMPI.EmployeeStatus RecruitmentFinalEmployeeStatus,EMPI.EmployeeCode RecruitmentFinalConfirmationRPEC
										,EMPIN.EmployeeName AS SalaryResponsible,EMPIN.EmployeeStatus SalaryEmployeeStatus,EMPIN.EmployeeCode SalaryRPEC
										,EMPINF.EmployeeName AS ProbationResponsible,EMPINF.EmployeeStatus ProbationEmployeeStatus,EMPINF.EmployeeCode ProbationRPEC
										,EMPINFR.EmployeeName AS Resignationperson,EMPINFR.EmployeeStatus ResignationEmployeeStatus,EMPINFR.EmployeeCode ResignationApprovalEC
										,EMPINFP.EmployeeName AS ProfileUploadRPerson,EMPINFP.EmployeeStatus ProfileEmployeeStatus,EMPINFP.EmployeeCode ProfileUploadRPEC
										,EMPRP.EmployeeName AS ResigRecruitPlanningRPerson,EMPRP.EmployeeStatus ResigRecruitEmployeeStatus,EMPRP.EmployeeCode ResigRecruitPlanningRPEC
										,EMPORG.EmployeeName AS PostRecruitmentOrgDocRPerson,EMPORG.EmployeeStatus OrgDocEmployeeStatus,EMPORG.EmployeeCode PostRecruitmentOrgDocRPEC
										,EMPRS.EmployeeName AS ResignationApplyPerson,EMPRS.EmployeeStatus ResignationApplyEmployeeStatus,EMPRS.EmployeeCode ResignationApplyEC
										,EMPLA.EmployeeName AS LeaveApprovalPerson,EMPLA.EmployeeStatus LeaveEmployeeStatus,EMPLA.EmployeeCode LeaveApprovalEC
										,EMPPP.EmployeeName AS ProductionPlanningPerson,EMPPP.EmployeeStatus ProductionEmployeeStatus,EMPPP.EmployeeCode ProductionPlanningEC
										,ESAA.EmployeeName AS SalaryAdvanceApprovalPerson,ESAA.EmployeeStatus SalaryAdvanceApprovalStatus,ESAA.EmployeeCode SalaryAdvanceApprovalEC
										,ESFA.EmployeeName AS SalaryFixationApprovalPerson,ESFA.EmployeeStatus SalaryFixationApprovalStatus,ESFA.EmployeeCode SalaryFixationApprovalEC
                                        ,ESMA.EmployeeName AS ManualAttendanceApprovalPerson,ESMA.EmployeeStatus ManualAttendanceApprovalStatus,ESMA.EmployeeCode ManualAttendanceApprovalEC
                                        ,EB.EmployeeName AS ExpanseBooking,EB.EmployeeStatus ExpanseBookingStatus,EB.EmployeeCode ExpanseBookingEC
                                        ,IO.EmployeeName AS InOutAttendancePerson,IO.EmployeeStatus InOutAttendanceStatus,IO.EmployeeCode InOutAttendanceEC
										FROM [HKP].[ApprovalConfiguration] PC
										LEFT JOIN ORG.Company C ON PC.CompanyId=C.Id
										LEFT JOIN ORG.Plant  P ON PC.PlantId=P.Id
										LEFT JOIN ORG.Entity E ON PC.EntityId=E.Id
										LEFT JOIN dbo.EmployeeInformation PRE ON PC.UpperDesignationAndSpecialAllowanceRP=PRE.SystemId
										LEFT JOIN dbo.EmployeeInformation PR ON PC.OrgDocRP=PR.SystemId
										LEFT JOIN dbo.EmployeeInformation EM ON PC.PreRecruitmentDocRP=EM.SystemId
										LEFT JOIN dbo.EmployeeInformation EMP ON PC.PostRecruitmentDocRP=EMP.SystemId
										LEFT JOIN dbo.EmployeeInformation EMPI ON PC.RecruitmentFinalConfirmationRP=EMPI.SystemId
										LEFT JOIN dbo.EmployeeInformation EMPIN ON PC.SalaryRP=EMPIN.SystemId
										LEFT JOIN dbo.EmployeeInformation EMPINF ON PC.ProbationRP=EMPINF.SystemId
										LEFT JOIN dbo.EmployeeInformation EMPINFR ON PC.ResignationApproval=EMPINFR.SystemId
										LEFT JOIN dbo.EmployeeInformation EMPINFP ON PC.ProfileUploadRP=EMPINFP.SystemId
										LEFT JOIN dbo.EmployeeInformation EMPRP ON PC.ResigRecruitPlanningRP=EMPRP.SystemId
										LEFT JOIN dbo.EmployeeInformation EMPORG ON PC.PostRecruitmentOrgDocRP=EMPORG.SystemId
										LEFT JOIN dbo.EmployeeInformation EMPRS ON PC.ResignationApply=EMPRS.SystemId
										LEFT JOIN dbo.EmployeeInformation EMPLA ON PC.LeaveApproval=EMPLA.SystemId
										LEFT JOIN dbo.EmployeeInformation EMPPP ON PC.ProductionPlanning=EMPPP.SystemId
										LEFT JOIN dbo.EmployeeInformation ESAA ON PC.SalaryAdvanceApproval=ESAA.SystemId
										LEFT JOIN dbo.EmployeeInformation ESFA ON PC.SalaryFixationApproval=ESFA.SystemId
                                        LEFT JOIN dbo.EmployeeInformation ESMA ON PC.ManualAttendanceApproval=ESMA.SystemId
                                        LEFT JOIN dbo.EmployeeInformation EB ON PC.ExpanseBookingRP=EB.SystemId
                                        LEFT JOIN dbo.EmployeeInformation IO ON PC.InOutAttendance=IO.SystemId
                                       WHERE PC.PlantId = '" + plantId + "' AND PC.EntityId = '" + entityId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeData(GridParameter parameters, string plantId)
        {
            try
            {
                //parameters.sort = "EmployeeCodeNumeric";
                //parameters.order = "ASC";
                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,PMB.Code BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,EC.ID EmployeeCategoryId
									,EC.UserName EmployeeCategory
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									--,Convert(Int,E.EmployeeCode) EmployeeCode
                                    ,E.EmployeeCode
									,E.EmpPicPath
									,SRM.CurrencyRuleSystemID
                                    ,SRM.SalaryRuleName
									,LPM.PolicyName
                                    ,FORMAT(E.DOJ,'dd-MMM-yyyy') DOJ
                                    ,P.UserName Plant
									,SS.UserName SubSection
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
								LEFT JOIN (SELECT DM.DesignationId,DC.LeavePolicyMasterId FROM MST.DesignationMaster DM
								LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
								WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
                                LEFT JOIN
									(select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									                ,dg.UserName GivenDesignationGroup
									                from ( SELECT dm.* FROM MST.DesignationMaster DM) DM
									                LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									                ) EGDSGG on EGDSGG.DesignationId=e.GivenDesignationId 
								LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=EGDSGG.EmployeeCategoryId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                --AND E.IsApproved=1
                                 where E.PlantId = '" + plantId + "' AND E.EmployeeStatus='Active' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetAllEmployeeData(GridParameter parameters)
        {
            try
            {
                parameters.sort = "EmployeeCodeNumeric";
                parameters.order = "ASC";
                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,PMB.Code BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
                                    ,E.EmployeeCode
									,E.EmpPicPath
                                    ,E.DOJ
                                    ,P.UserName Plant
									,SS.UserName SubSection
                                    ,E.EmployeeCodeNumeric
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                 where E.EmployeeStatus='Active'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeWithoutPaidhoursData(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,EC.Id EmployeeCategoryId
									,EC.UserName EmployeeCategory
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									,E.EmployeeCode 
									,E.EmpPicPath
									,SRM.CurrencyRuleSystemID
                                    ,SRM.SalaryRuleName
									,LPM.PolicyName
                                    ,E.DOJ
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
								LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
								WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                 where E.SystemId NOT IN (SELECT EmployeeId FROM MST.PaidHoursEmployeeAssign where PlantId='" + plantId + "') AND E.PlantId = '" + plantId + "'AND E.EmployeeStatus='Active'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetEmployeeWithoutPayrollGroupData(string plantId)
        {
            try
            {
                var sql = @"SELECT E.SystemId EmployeeId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,EC.Id EmployeeCategoryId
									,EC.UserName EmployeeCategory
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									,E.EmployeeCode 
									,E.EmpPicPath
									,SRM.CurrencyRuleSystemID
                                    ,SRM.SalaryRuleName
									,LPM.PolicyName
                                    ,E.DOJ
                                    ,0 Active
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
								LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
								WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                --AND E.IsApproved=1
                                 where E.SystemId NOT IN (SELECT EmployeeId FROM MST.PayrollGroupMaster) AND E.PlantId = '" + plantId + "' AND E.EmployeeStatus='Active'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetEmployeeWithoutAttendanceGroupData(string plantId)
        {
            try
            {
                string CmdText = @"SELECT E.SystemId EmployeeId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,EC.Id EmployeeCategoryId
									,EC.UserName EmployeeCategory
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									,E.EmployeeCode 
									,E.EmpPicPath
									,SRM.CurrencyRuleSystemID
                                    ,SRM.SalaryRuleName
									,LPM.PolicyName
                                    ,E.DOJ
                                    ,0 Active
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
								LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
								WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                --AND E.IsApproved=1
                                 where E.SystemId NOT IN (SELECT EmployeeId FROM dbo.EmployeeAttendanceGroup where PlantId='" + plantId + "') AND E.PlantId = '" + plantId + "'AND E.EmployeeStatus='Active'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeWithSalaryProcessData(GridParameter parameters, string plantId, string Monthid, string YearId)
        {
            try
            {
                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,EC.Id EmployeeCategoryId
									,EC.UserName EmployeeCategory
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									,E.EmployeeCode 
									,E.EmpPicPath
									,SRM.CurrencyRuleSystemID
                                    ,SRM.SalaryRuleName
									,LPM.PolicyName
                                    ,E.DOJ
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
								LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
								WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                --AND E.IsApproved=1
                                 where E.SystemId NOT IN (SELECT EmployeeId FROM MST.PayrollGroupMaster where PlantId='" + plantId + "') AND E.PlantId = '" + plantId + @"'AND E.EmployeeStatus='Active' AND E.SystemId IN (Select EmpInfoSystemID from SalaryProcChild where SlrProcMstSystemID IN (Select SystemID FROM SalaryProcMaster WHERE MonthNo = " + Monthid + " AND YearNo = " + YearId + @") AND IsApproved = 0 AND IsDisbursed = 0)";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetEmployeeDataWithEmployeeCode(string plantId, string employeeCode)
        {
            try
            {
                string _sql = @"SELECT E.SystemId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.EmpType
							    	,GD.UserName GivenDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									,E.EmployeeCode
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
								LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
								WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
								LEFT JOIN HKP.EmployeeCategory EC ON E.EmployeeCategorySystemID=EC.Id
                                 where E.PlantId = '" + plantId + "'AND E.EmployeeStatus='Active' AND E.EmployeeCode like '%" + employeeCode + "%'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeDataByCompany(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT E.SystemId, E.PlantId, E.GroupID, E.CompanyId,E.EmployeeCode, E.EmployeeName, E.BudgetCode, PR.UserName PositionName, E.TelePhnNo
                                    , E.EmailId, PR.DepartmentId, PR.DivisionId, PR.SectionId, E.EmpType, E.GivenDesignationId, E.EmployeeCategorySystemID EmployeeCategoryId
                                    , EC.UserName EmployeeCategory, EN.UserName EntityName, D.UserName Designation, GD.UserName GivenDesignation, DEPT.UserName AS Department
                                    , DV.UserName AS Division, SC.UserName AS Section,  E.EmpPicPath, SRM.CurrencyRuleSystemID, SRM.SalaryRuleName, LPM.PolicyName, E.DOJ
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN  (SELECT DC.LeavePolicyMasterId,DM.DesignationId,DC.PlantId FROM MST.DesignationMaster DM
								LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId) DM ON GD.Id=DM.DesignationId AND DM.PlantId=E.PlantId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
								LEFT JOIN HKP.EmployeeCategory EC ON E.EmployeeCategorySystemID=EC.Id
                                 where E.CompanyId = '" + companyId + "' AND E.EmployeeStatus='Active'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeDataWithIds(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName)
        {
            try
            {
                string search = null;

                if (departmentIds != "''") search += "AND PR.DepartmentId IN(" + departmentIds + ")";
                if (divisionIds != "''") search += "AND PR.DivisionId IN(" + divisionIds + ")";
                if (sectionIds != "''") search += "AND PR.SectionId IN(" + sectionIds + ")";
                if (employeeCateogoryIds != "''") search += "AND EC.Id IN(" + employeeCateogoryIds + ")";
                if (givenDesignationIds != "''") search += "AND E.GivenDesignationId IN(" + givenDesignationIds + ")";
                if (employeeCode != "") search += "AND E.EmployeeCode LIKE'%" + employeeCode + "%'";
                if (employeeName != "") search += "AND E.EmployeeName LIKE'%" + employeeName + "%'";

                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,EC.Id EmployeeCategoryId
									,EC.UserName EmployeeCategory
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									,E.EmployeeCode
									,E.EmpPicPath
									,SRM.CurrencyRuleSystemID
                                    ,SRM.SalaryRuleName
									,LPM.PolicyName
                                    ,E.DOJ
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                --AND E.IsApproved=1
                                 where E.SystemId NOT IN (SELECT EmployeeId FROM MST.PayrollGroupMaster where PlantId='" + plantId + "') and E.PlantId = '" + plantId + "'AND E.EmployeeStatus='Active' " + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeDataIds(GridParameter parameters, string plantId, string lineIds, string employeeCode, string employeeName, string SubsectionId)
        {
            try
            {
                string search = null;

                if (lineIds != "") search += "AND PMB.LineId IN(" + lineIds + ")";
                if (SubsectionId != "") search += "AND PR.SubSectionId IN(" + SubsectionId + ")";
                if (employeeCode != "") search += "AND E.EmployeeCode LIKE'%" + employeeCode + "%'";
                if (employeeName != "") search += "AND E.EmployeeName LIKE'%" + employeeName + "%'";

                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,EC.Id EmployeeCategoryId
									,EC.UserName EmployeeCategory
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									,E.EmployeeCode
									,E.EmpPicPath
									,SRM.CurrencyRuleSystemID
                                    ,SRM.SalaryRuleName
									,LPM.PolicyName
                                    ,E.DOJ
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                --AND E.IsApproved=1
                                 where E.SystemId NOT IN (SELECT EmployeeId FROM MST.PayrollGroupMaster where PlantId='" + plantId + "') and E.PlantId = '" + plantId + "'AND E.EmployeeStatus='Active' " + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeAttendanceGroupDataWithIds(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName)
        {
            try
            {
                string search = null;

                if (departmentIds != "''") search += "AND PR.DepartmentId IN(" + departmentIds + ")";
                if (divisionIds != "''") search += "AND PR.DivisionId IN(" + divisionIds + ")";
                if (sectionIds != "''") search += "AND PR.SectionId IN(" + sectionIds + ")";
                if (employeeCateogoryIds != "''") search += "AND EC.Id IN(" + employeeCateogoryIds + ")";
                if (givenDesignationIds != "''") search += "AND E.GivenDesignationId IN(" + givenDesignationIds + ")";
                if (employeeCode != "") search += "AND E.EmployeeCode LIKE'%" + employeeCode + "%'";
                if (employeeName != "") search += "AND E.EmployeeName LIKE'%" + employeeName + "%'";

                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,EC.Id EmployeeCategoryId
									,EC.UserName EmployeeCategory
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									,E.EmployeeCode
									,E.EmpPicPath
									,SRM.CurrencyRuleSystemID
                                    ,SRM.SalaryRuleName
									,LPM.PolicyName
                                    ,E.DOJ
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                --AND E.IsApproved=1
                                 where E.SystemId NOT IN (SELECT EmployeeId FROM dbo.EmployeeAttendanceGroup where PlantId='" + plantId + "') and E.PlantId = '" + plantId + "'AND E.EmployeeStatus='Active' " + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeAttendanceGroupDataWithLine(GridParameter parameters, string plantId, string lineIds, string employeeCode, string employeeName, string SubsectionId)
        {
            try
            {
                string search = null;

                if (SubsectionId != "") search += "AND PR.SubSectionId IN(" + SubsectionId + ")";
                if (lineIds != "") search += "AND PMB.LineId IN(" + lineIds + ")";
                if (employeeCode != "") search += "AND E.EmployeeCode LIKE'%" + employeeCode + "%'";
                if (employeeName != "") search += "AND E.EmployeeName LIKE'%" + employeeName + "%'";

                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,EC.Id EmployeeCategoryId
									,EC.UserName EmployeeCategory
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									,E.EmployeeCode
									,E.EmpPicPath
									,SRM.CurrencyRuleSystemID
                                    ,SRM.SalaryRuleName
									,LPM.PolicyName
                                    ,E.DOJ
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                --AND E.IsApproved=1
                                 where E.SystemId NOT IN (SELECT EmployeeId FROM dbo.EmployeeAttendanceGroup where PlantId='" + plantId + "') and E.PlantId = '" + plantId + "'AND E.EmployeeStatus='Active' " + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeDataWithPaidHoursIds(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName)
        {
            try
            {
                string search = null;

                if (departmentIds != "''") search += "AND PR.DepartmentId IN(" + departmentIds + ")";
                if (divisionIds != "''") search += "AND PR.DivisionId IN(" + divisionIds + ")";
                if (sectionIds != "''") search += "AND PR.SectionId IN(" + sectionIds + ")";
                if (employeeCateogoryIds != "''") search += "AND EC.Id IN(" + employeeCateogoryIds + ")";
                if (givenDesignationIds != "''") search += "AND E.GivenDesignationId IN(" + givenDesignationIds + ")";
                if (employeeCode != "") search += "AND E.EmployeeCode LIKE'%" + employeeCode + "%'";
                if (employeeName != "") search += "AND E.EmployeeName LIKE'%" + employeeName + "%'";

                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,EC.Id EmployeeCategoryId
									,EC.UserName EmployeeCategory
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									,E.EmployeeCode
									,E.EmpPicPath
									,SRM.CurrencyRuleSystemID
                                    ,SRM.SalaryRuleName
									,LPM.PolicyName
                                    ,E.DOJ
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                 where E.SystemId NOT IN (SELECT EmployeeId FROM MST.PaidHoursEmployeeAssign where PlantId='" + plantId + "') And E.PlantId = '" + plantId + "'AND E.EmployeeStatus='Active' " + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeDataWithfilters(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName)
        {
            try
            {
                string search = null;

                if (departmentIds != "''") search += "AND PR.DepartmentId IN(" + departmentIds + ")";
                if (divisionIds != "''") search += "AND PR.DivisionId IN(" + divisionIds + ")";
                if (sectionIds != "''") search += "AND PR.SectionId IN(" + sectionIds + ")";
                if (employeeCateogoryIds != "''") search += "AND EC.Id IN(" + employeeCateogoryIds + ")";
                if (givenDesignationIds != "''") search += "AND E.GivenDesignationId IN(" + givenDesignationIds + ")";
                if (employeeCode != "") search += "AND E.EmployeeCode LIKE'%" + employeeCode + "%'";
                if (employeeName != "" && employeeName != "undefined") search += "AND E.EmployeeName LIKE'%" + employeeName + "%'";

                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,EC.Id EmployeeCategoryId
									,EC.UserName EmployeeCategory
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
									,E.EmployeeCode
									,E.EmpPicPath
									,SRM.CurrencyRuleSystemID
                                    ,SRM.SalaryRuleName
									,LPM.PolicyName
                                    ,E.DOJ
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
								LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                WHERE DC.PlantId='" + plantId + @"') DM ON GD.Id=DM.DesignationId
								LEFT JOIN LeavePolicyMaster LPM ON DM.LeavePolicyMasterId= LPM.SystemID
								LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                 where  E.PlantId = '" + plantId + "' " + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
    }
}