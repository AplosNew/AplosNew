#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Employees
{
    public class EmployeeSalaryRuleEditableService : Service<EmployeeSalaryRuleEditable>, IEmployeeSalaryRuleEditableService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<EmployeeSalaryRuleEditable> _employeeSalaryRuleEditableRepository;

        public EmployeeSalaryRuleEditableService(
            IRepositoryAsync<EmployeeSalaryRuleEditable> employeeSalaryRuleEditableRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork,
            ISqlRepository sqlRepository) : base(employeeSalaryRuleEditableRepository, unitOfWork, pkGeneratorService)
        {
            _employeeSalaryRuleEditableRepository = employeeSalaryRuleEditableRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertUpdate(IEnumerable<EmployeeSalaryRuleEditable> entities, string plantId)
        {
            var flag = false;
            try
            {
                var pk = GetMaxNumber();
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.PlantId = plantId;
                        item.ModelState = ModelState.Added;
                        pk.MaxNumber++;
                        AuditService.Log(item);
                        item.Id = pk.MaxNumber.ToString();
                    }
                    else
                    {
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                    }

                    InsertOrUpdateGraph(item);
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

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(EmployeeSalaryRuleEditable), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "GL Mapping Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                EmployeeSalaryRuleEditable entity = Find(id);
                // If section row inactive
                base.DeleteGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name,
                    MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string plantId, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT ES.*,EI.EmployeeName,EI.BudgetCode FROM [HKP].[EmployeeSalaryRuleEditable] ES
                                        LEFT JOIN EmployeeInformation EI ON ES.[EmployeeId]= EI.SystemId
                                        WHERE ES.PlantId='" + plantId + "' AND ES.CompanyId='" + companyId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
    }
}