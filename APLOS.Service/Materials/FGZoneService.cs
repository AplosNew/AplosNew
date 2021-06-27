#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Materials;
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

namespace Library.Service.Materials
{
    public class FGZoneService : Service<FGZone>, IFGZoneService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public FGZoneService(
            IRepositoryAsync<FGZone> fgzoneRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(fgzoneRepository, unitOfWork)
        {
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public override void Insert(FGZone entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Check(entity);
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.Insert(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name,
                false, ModuleEnum.Material.ToString()));
            }
        }

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(FGZone), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(FGZone entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
        }

        public override void Update(FGZone entity)
        {
            try
            {
                Check(entity);
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
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name,
                false, ModuleEnum.Material.ToString()));
            }
        }

        public override void Archive(string key)
        {
            try
            {
                FGZone entity = Find(key);
                AuditService.Log(entity);
                base.Archive(entity.Id);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query(r => !r.Archive).Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        public IEnumerable<object> GetFGZoneCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = $"SELECT mg.Id AS Value, mg.UserName as Text FROM {DbSchema.HKP}.[{DbTable.FGZone}] AS mg  WHERE mg.CompanyGroupId = '{identity.CompanyGroupId}' AND mg.Archive=0 AND mg.Active=1 ORDER BY mg.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false,
                    ModuleEnum.Material.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = $"SELECT mg.* FROM {DbSchema.HKP}.[{DbTable.FGZone}] AS mg  WHERE mg.CompanyGroupId = '{identity.CompanyGroupId}' AND  mg.Archive=0 ";
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