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

namespace Library.Service.Materials
{
    public partial class PackingFormService : Service<PackingForm>, IPackingFormService
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public PackingFormService(
            IRepositoryAsync<PackingForm> packingFormRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(packingFormRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion

        public decimal GetAutoSequence(string companyGroupId)
        {
            try
            {
                return base.Query(t => t.CompanyGroupId == companyGroupId).Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(PackingForm), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(PackingForm entity)
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        private void CheckUnique(PackingForm entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id && r.CompanyGroupId == entity.CompanyGroupId);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id && r.CompanyGroupId == entity.CompanyGroupId);
        }

        public override void Update(PackingForm entity)
        {
            try
            {
                CheckUnique(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetCbo(string companyGroupId)
        {
            try
            {
                string _sql = @"SELECT Id,UserName FROM HKP.PackingForm WHERE CompanyGroupId='" + companyGroupId + "' ORDER BY UserName";
                return _sqlRepository.GetCombo(_sql, "Id", "UserName");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.order = "asc";
                parameters.sort = "Sequence";
                parameters.CmdText = @"SELECT * FROM HKP.PackingForm WHERE CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
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