#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Materials
{
    public class MaterialStorageService : Service<MaterialStorage>, IMaterialStorageService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public MaterialStorageService(
            IRepositoryAsync<MaterialStorage> storageRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(storageRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence(string groupId, string companyId, string plantId)
        {
            try
            {
                return base.Query(t => t.CompanyGroupId == groupId && t.CompanyId == companyId && t.PlantId == plantId && !t.Archive).Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(MaterialStorage), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(MaterialStorage entity)
        {
            try
            {
                CheckUnique(entity);
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public override void Update(MaterialStorage entity)
        {
            try
            {
                CheckUnique(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        private void CheckUnique(MaterialStorage entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Code == entity.Code && t.Id != entity.Id && t.CompanyGroupId == entity.CompanyGroupId && t.CompanyId == entity.CompanyId && t.PlantId == entity.PlantId && !t.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.UserName == entity.UserName && t.Id != entity.Id && t.CompanyGroupId == entity.CompanyGroupId && t.CompanyId == entity.CompanyId && t.PlantId == entity.PlantId && !t.Archive);
        }

        public GridModel Query(GridParameter parameters, string groupId, string companyId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT MS.*,mb.Code BudgetCode FROM [HKP].[MaterialStorage] MS
LEFT JOIN MST.ManpowerBudget AS mb ON MB.Id=MS.BudgetId WHERE MS.CompanyGroupId='" + groupId + "' AND MS.CompanyId='" + companyId + "' AND MS.PlantId='" + plantId + "' AND MS.Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetCbo(string groupId, string companyId, string plantId)
        {
            try
            {
                return from m in base.Query(t => t.CompanyGroupId == groupId && t.CompanyId == companyId && t.PlantId == plantId && t.Active && !t.Archive).Select().OrderBy(x=>x.Sequence)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<object> GetCboForOnlyMaterialTransfer(string groupId, string companyId, string plantId)
        {
            try
            {
                return from m in base.Query(t => t.CompanyGroupId == groupId && t.CompanyId == companyId && t.Active && !t.Archive)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
    }
}