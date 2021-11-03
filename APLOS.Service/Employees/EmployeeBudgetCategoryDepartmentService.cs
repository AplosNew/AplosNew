#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class EmployeeBudgetCategoryDepartmentService : Service<EmployeeBudgetCategoryDepartment>, IEmployeeBudgetCategoryDepartmentService
    {
        #region Constructor

        private readonly IRepositoryAsync<EmployeeBudgetCategoryDepartment> _employeeBudgetCategoryDepartmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public EmployeeBudgetCategoryDepartmentService(
            IRepositoryAsync<EmployeeBudgetCategoryDepartment> employeeBudgetCategoryDepartmentRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(employeeBudgetCategoryDepartmentRepository, unitOfWork, pkGeneratorService)
        {
            _employeeBudgetCategoryDepartmentRepository = employeeBudgetCategoryDepartmentRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdateGraph(IEnumerable<EmployeeBudgetCategoryDepartment> entities)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string dId = string.Empty;
                foreach (var item in entities)
                {
                    dId = item.CompanyGroupId;
                    break;
                }
                IEnumerable<EmployeeBudgetCategoryDepartment> DbList = GetGbList(dId);
                var pk = GetMaxNumber();
                ///Delete
                foreach (var item in DbList)
                {
                    var db_c = entities.Where(a => a.EmployeeBudgetCategoryId == item.EmployeeBudgetCategoryId && a.DepartmentId == item.DepartmentId).FirstOrDefault();
                    if (db_c == null || db_c.Id == null)
                    {
                        Delete(item);
                    }
                }
                ///Add Update
                foreach (var item in entities)
                {
                    var db_c = DbList.Where(a => a.DepartmentId == item.DepartmentId && a.EmployeeBudgetCategoryId == item.EmployeeBudgetCategoryId).FirstOrDefault();
                    if (db_c == null || db_c.Id == null)
                    {
                        pk.MaxNumber++;
                        item.ModelState = ModelState.Added;
                        AuditService.Log(item);
                        item.Id = pk.MaxNumber.ToString();
                        item.CompanyGroupId = identity.CompanyGroupId;
                    }
                    else
                    {
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                    }

                    base.InsertOrUpdateGraph(item);
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private IEnumerable<EmployeeBudgetCategoryDepartment> GetGbList(string CompanyGroupId)
        {
            try
            {
                string _sql = @"SELECT * FROM [MST].[EmployeeBudgetCategoryDepartment]
                                WHERE  CompanyGroupId ='" + CompanyGroupId + "'";
                return _employeeBudgetCategoryDepartmentRepository.SelectQuery(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(EmployeeBudgetCategoryDepartment), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(EmployeeBudgetCategoryDepartment), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public GridModel QueryWithDepartment(GridParameter parameters, string departmentId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT * FROM [MST].[EmployeeBudgetCategoryDepartment] WHERE DepartmentId='" + departmentId + "' AND CompanyGroupId='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel QueryDepartmentWithCompany(GridParameter parameters)
        {
            try
            {
                parameters.sort = "UserName";
                parameters.order = "ASC";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT F.Id,CGD.CompanyGroupId,CGD.DepartmentId,F.EmployeeBudgetCategoryId,D.Code,D.UserName FROM ORG.CompanyGroupDepartment AS CGD
                                            LEFT OUTER JOIN ORG.Department AS D ON CGD.DepartmentId=D.Id
                                            LEFT OUTER JOIN (
                                            SELECT EBCD.Id,EBCD.CompanyGroupId,EBCD.EmployeeBudgetCategoryId ,EBCD.DepartmentId FROM [MST].[EmployeeBudgetCategoryDepartment] AS EBCD
                                            LEFT OUTER JOIN HKP.EmployeeBudgetCategory AS EBC ON EBCD.EmployeeBudgetCategoryId = EBC.Id) AS F ON D.Id=F.DepartmentId
                                            WHERE CGD.CompanyGroupId='" + identity.CompanyGroupId + @"'";
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