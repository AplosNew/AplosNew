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
using System;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class EmployeeBankInformationService : Service<EmployeeBankInformation>, IEmployeeBankInformationService
    {
        #region --- Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeBankInformationService(
            IRepositoryAsync<EmployeeBankInformation> EmployeeBankInformationRepository
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(EmployeeBankInformationRepository, unitOfWork)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
        }

        #endregion --- Constructor

        public GridModel GetEmployees(GridParameter parameters,string plantId)
        {
            try
            {
                
                parameters.CmdText = @"SELECT EB.EmpSystemID
                                     	,E.EmployeeCode
                                     	,E.EmployeeName
                                     	,E.DOJ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                     	,DEPT.UserName AS Department
                                     	,DEG.UserName AS GivenDesignation
                                     	,B.UserName AS BankName
                                     	,BB.UserName AS BankBranchName
                                        ,EB.BankSystemID
										,EB.BankBranchId
                                     	,EB.BankAccNo
                                     	,EB.SalaryPercentage
                                        ,EB.RowID
                                        ,EB.AddedBy
										,FORMAT(EB.DateAdded,'dd-MMM-yyyy')DateAdded
                                     FROM dbo.EmployeeBankInfo EB
                                     LEFT JOIN dbo.EmployeeInformation E ON EB.EmpSystemID = E.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                     LEFT JOIN ORG.Department DEPT ON DEPT.Id=PR.DepartmentId
                                     LEFT JOIN HKP.Designation DEG ON E.GivenDesignationId = DEG.Id
                                     LEFT JOIN HKP.Bank B ON B.Id = EB.BankSystemID
                                     LEFT JOIN HKP.BankBranch BB ON Bb.Id = EB.BankBranchId
                                     WHERE EB.IsApproved = 0 AND E.PlantId='" + plantId + "'  ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeBankHistory(GridParameter parameters, string empSystemId)
        {
            try
            {
                parameters.CmdText = @"SELECT EBB.EmpSystemID
                                     	,E.EmployeeCode
                                     	,E.EmployeeName,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                     	,E.DOJ
                                     	,DEPT.UserName AS Department
                                     	,DG.UserName AS GivenDesignation
                                     	,B.UserName AS BankName
                                     	,BB.UserName AS BankBranchName
                                     	,EBB.BankAccNo
                                     	,EBB.SalaryPercentage
                                        ,EBB.RowID
                                        ,EBB.AddedBy
										,FORMAT(EBB.DateAdded,'dd-MMM-yyyy')DateAdded
                                     FROM dbo.EmployeeBankInfoBackUp EBB
                                     LEFT JOIN dbo.EmployeeInformation E ON EBB.EmpSystemID = E.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                     LEFT JOIN HKP.Designation DG ON DG.Id = E.GivenDesignationId
                                     LEFT JOIN ORG.Department DEPT ON DEPT.Id=PR.DepartmentId
                                     LEFT JOIN HKP.Bank B ON B.Id = EBB.BankSystemID
                                     LEFT JOIN HKP.BankBranch BB ON BB.Id = EBB.BankBranchId
                                     WHERE E.SystemId = '" + empSystemId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(EmployeeBankInformation entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fromDb = Find(entity.RowID);
                //var fromDb = base.Find(entity.EmpSystemID);
                //fromDb.DateAdded = DateTime.Now;
                fromDb.DateUpdated = DateTime.Now;
                fromDb.UpdatedBy = identity.Name;
                fromDb.ApprovedDateTime = DateTime.Now;
                fromDb.IsApproved = entity.IsApproved;
                fromDb.ApprovedBy = identity.Name;
                base.Update(fromDb);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Delete(EmployeeBankInformation entity)
        {
            try
            {
                var fromDb = Find(entity.RowID);
                base.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
    }
}