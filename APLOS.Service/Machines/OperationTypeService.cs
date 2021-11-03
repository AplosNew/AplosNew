#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Machines;
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

namespace Library.Service.Machines
{
    public class OperationTypeService : Service<OperationType>, IOperationTypeService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IRepositoryAsync<CompanyGroupOperationType> _companyGroupOperationTypeRepository;
        private readonly ISqlRepository _sqlRepository;

        public OperationTypeService(
            IRepositoryAsync<OperationType> operationTypeRepository,
            IPKGeneratorService pkGeneratorService,
            IRepositoryAsync<CompanyGroupOperationType> companyGroupOperationTypeRepository,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(operationTypeRepository, unitOfWork)
        {
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _companyGroupOperationTypeRepository = companyGroupOperationTypeRepository;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public override void Insert(OperationType entity)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetPK();
                var comOperationType = new CompanyGroupOperationType
                {
                    Id = GetAutoId(),
                    OperationTypeId = entity.Id,
                    CompanyGroupId = identity.CompanyGroupId,
                    Active = false
                };
                InsertGraph(entity);
                comOperationType.ModelState = ModelState.Added;
                AuditService.Log(comOperationType);
                _companyGroupOperationTypeRepository.Insert(comOperationType);

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit(); ;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(OperationType), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(CompanyGroupOperationType), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void CheckUnique(OperationType entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id);
        }

        public override void Update(OperationType entity)
        {
            try
            {
                CheckUnique(entity);
                base.Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public void Delete(string key)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(key);

                var groupType = _companyGroupOperationTypeRepository.Query(m => m.OperationTypeId == key && !m.Archive).Select().FirstOrDefault();
                _companyGroupOperationTypeRepository.Delete(groupType);

                base.DeleteGraph(entity.Id);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        #region GetSequence

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets automatic sequence. </summary>
        /// <returns>   The automatic sequence. </returns>
        ///-------------------------------------------------------------------------------------------------
        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        #endregion GetSequence

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = $"SELECT op.Id AS Value, op.UserName as Text FROM HKP.[{DbTable.OperationType}] AS op " +
                          $"LEFT OUTER JOIN(SELECT * FROM HKP.[{DbTable.CompanyGroupOperationType}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') AS cgop " +
                          $"ON op.Id = cgop.OperationTypeId  WHERE ISNULL(cgop.Id, '')<> '' AND  op.Archive=0 AND op.Active = 1 ORDER BY op.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = $"SELECT ot.* FROM  HKP.[{DbTable.OperationType}] AS ot " +
                            $"INNER JOIN HKP.[{DbTable.CompanyGroupOperationType}] AS cgop ON cgop.OperationTypeId = ot.Id " +
                            $"WHERE cgop.CompanyGroupId='{identity.CompanyGroupId}' AND ot.Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }
    }
}