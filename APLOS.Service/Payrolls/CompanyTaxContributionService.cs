using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Library.Service.Payrolls
{
    public class CompanyTaxContributionService : Service<CompanyTaxContribution>, ICompanyTaxContributionService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<CompanyTaxContribution> _companyTaxContributionRepository;
        public static IRepositoryAsync<CompanyTaxContribution> CompanyTaxContributionRepository { get; }

        public CompanyTaxContributionService(
            IRepositoryAsync<CompanyTaxContribution> companyTaxContributionRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(companyTaxContributionRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _companyTaxContributionRepository = companyTaxContributionRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(CompanyTaxContribution), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public GridModel GetAllEmployee(GridParameter parameters, string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT EMP.SystemId
                                       	,EMP.EmployeeName
                                       	,EMP.EmailId
                                       	,EMP.PlantId
                                       	,D.UserName Designation
                                       	,PR.UserName PositionName
                                       	,DEG.UserName GivenDesignation
                                       	,DEPT.UserName Department
                                       FROM EmployeeInformation EMP
                                       LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode = PMB.Id
                                       LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                       LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                                       LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
                                       LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                                       LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId = DEG.Id
                                       WHERE EMP.GroupID = '" + identity.CompanyGroupId + @"'
                                           AND EMP.CompanyId = '" + identity.CompanyId + @"'
                                           AND EMP.PlantId = '" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private List<Dictionary<string, object>> CheckEmployee(string empSystemId, string taxYearId)
        {
            var sql = @"SELECT EI.EmployeeCode,TY.TaxYearName,CTC.EmpSystemId,CTC.TaxYearId FROM [MST].[CompanyTaxContribution] CTC
                        LEFT JOIN dbo.EmployeeInformation EI ON CTC.EmpSystemId = EI.SystemId
                        LEFT JOIN SCS.TaxYear TY ON CTC.TaxYearId = TY.Id
                        WHERE CTC.EmpSystemId = '" + empSystemId + "' AND CTC.TaxYearId = '" + taxYearId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        private void Check(CompanyTaxContribution entity)
        {
            var empCode = "";
            var Year = "";
            var empSystemId = "";
            var taxYear = "";
            foreach (var item in CheckEmployee(entity.EmpSystemId, entity.TaxYearId))
            {
                var dic = (Dictionary<string, object>)item;
                empCode = dic["EmployeeCode"].ToString();
                Year = dic["TaxYearName"].ToString();
                empSystemId = dic["EmpSystemId"].ToString();
                taxYear = dic["TaxYearId"].ToString();

                if (entity.EmpSystemId == empSystemId && entity.TaxYearId == taxYear)
                {
                    throw new CustomException("This Employee [" + empCode + "] already exists for [" + Year + "] TaxYear.");
                }
            }
        }

        public void Insert(CompanyTaxContribution entity, string companyGroupId)
        {
            try
            {
                Check(entity);
                entity.Id = GetPK();
                entity.CompanyGroupId = companyGroupId;
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(CompanyTaxContribution entity)
        {
            try
            {
                //Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string empId, string plantId, string taxYearId)
        {
            try
            {
                parameters.CmdText = @"SELECT *
                                       FROM [MST].[CompanyTaxContribution]
                                       WHERE EmpSystemId = '" + empId + @"'
                                       	AND PlantId = '" + plantId + @"'
                                       	AND TaxYearId = '" + taxYearId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string empId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT *
                                       FROM [MST].[CompanyTaxContribution]
                                       WHERE EmpSystemId = '" + empId + @"'
                                       	AND PlantId = '" + plantId + @"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel BasicQuery(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM [MST].[CompanyTaxContribution]";
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