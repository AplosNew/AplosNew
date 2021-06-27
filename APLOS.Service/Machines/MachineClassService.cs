#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Machines;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Machines
{
    public class MachineClassService : Service<MachineClass>, IMachineClassService
    {
        #region Constructor

        private readonly ICompanyGroupMachineClassService _companyGroupMachineClassService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public MachineClassService(
            IRepositoryAsync<MachineClass> machineClassRepository,
            IPKGeneratorService pkGeneratorService,
            ICompanyGroupMachineClassService companyGroupMachineClassService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(machineClassRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _companyGroupMachineClassService = companyGroupMachineClassService;
        }

        #endregion Constructor

        public override void Insert(MachineClass entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                _companyGroupMachineClassService.InsertGraph(new CompanyGroupMachineClass { MachineClassId = entity.Id, Active = entity.Active });
                InsertGraph(entity);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(MachineClass), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(MachineClass entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && !r.Archive);
        }

        public override void Update(MachineClass entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                _companyGroupMachineClassService.UpdateGraph(entity.Id, entity.Active);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                UpdateGraph(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "MachineClass Id"));
                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
                // If section row inactive
                _companyGroupMachineClassService.DeleteGraph(entity.Id);
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

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query(r => !r.Archive && r.Active).Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
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
                parameters.CmdText = $"SELECT * FROM  [HKP].[{DbTable.MachineClass}] WHERE Archive=0";
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