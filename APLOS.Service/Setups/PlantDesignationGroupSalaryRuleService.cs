#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Accounts;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Setups
{
    public class PlantDesignationGroupSalaryRuleService : Service<PlantDesignationGroupSalaryRule>, IPlantDesignationGroupSalaryRuleService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public PlantDesignationGroupSalaryRuleService(
            IRepositoryAsync<PlantDesignationGroupSalaryRule> plantDesignationGroupSalaryRuleRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(plantDesignationGroupSalaryRuleRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertORUpdate(IEnumerable<PlantDesignationGroupSalaryRule> entities)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(PlantDesignationGroupSalaryRule), PKGeneratorEnum.Yearly, null, DateTime.Now);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        InsertGraph(item);
                    }
                }
                var plantId = entities.First().PlantId;
                var salaryRuleMasterId = entities.First().SalaryRuleMasterId;
                var dbList = base.Query(t => t.PlantId == plantId && t.SalaryRuleMasterId == salaryRuleMasterId).Select().AsEnumerable();
                if (dbList != null && dbList.Count() > 0)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            Delete(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                Delete(item);
                            }
                        }
                    }
                }
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
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string plantId, string salaryRuleMasterId)
        {
            var flag = false;
            try
            {
                var dbList = base.Query(t => t.PlantId == plantId && t.SalaryRuleMasterId == salaryRuleMasterId).Select().AsEnumerable();
                if (dbList != null)
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    foreach (var item in dbList)
                    {
                        base.DeleteGraph(item);
                    }
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM ORG.PlantDesignationGroupSalaryRule ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel QueryDesignationWithoutExisting(GridParameter parameters, string designationIds, string salaryRuleMasterId)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM HKP.DesignationGroup AS D WHERE D.Id NOT IN (" + designationIds + ") ANd  D.Id NOT IN(SELECT DesignationGroupId FROM[ORG].[PlantDesignationGroupSalaryRule] WHERE SalaryRuleMasterId NOT IN('" + salaryRuleMasterId + "'))";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetSalaryRuleMasterWithPlantCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"SELECT SM.SystemID AS Value,SM.SalaryRuleName AS Text FROM [dbo].[SalaryRuleMaster] AS SM
                                WHERE SM.PlantID='" + plantId + "' AND  SM.GroupID='" + identity.CompanyGroupId + "' AND SM.IsActive=1 ORDER BY SM.SalaryRuleName";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> QueryGraph(string plantId, string salaryRuleMasterId)
        {
            var sql = @"SELECT SR.*,D.Code,D.UserName,D.ShortName,D.StandardName,D.Active,D.Level FROM [ORG].[PlantDesignationGroupSalaryRule] AS SR
                            LEFT OUTER JOIN HKP.DesignationGroup AS D ON SR.DesignationGroupId = D.Id WHERE SR.PlantId='" + plantId + "' AND SalaryRuleMasterId='" + salaryRuleMasterId + "' ORDER BY D.UserName";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IWorkbook GetDesignationMaster(string plantId)
        {
            var obj = new ReportGeneralVoucher();
            using (var excelEngine = new ExcelEngine())
            {
                var workbook = obj.DesignationMasterWithSalaryRule_Report(excelEngine, plantId);
                return workbook;
            }
        }
    }
}