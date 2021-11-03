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
    public class FGComponentService : Service<FGComponent>, IFGComponentService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public FGComponentService(
            IRepositoryAsync<FGComponent> fGComponentRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(fGComponentRepository, unitOfWork)
        {
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public override void Insert(FGComponent entity)
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
            return _pkGeneratorService.GetAutoNumber(nameof(FGComponent), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(FGComponent entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
        }

        public override void Update(FGComponent entity)
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
                FGComponent entity = Find(key);
                entity.Archive = true;
                base.Update(entity);
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

        public IEnumerable<object> GetFGComponentCbo(string companyGroupId)
        {
            try
            {
                var sql = $"SELECT mg.Id AS Value, mg.UserName as Text FROM {DbSchema.HKP}.[{DbTable.FGComponent}] AS mg  WHERE mg.CompanyGroupId = '{companyGroupId}' AND mg.Archive=0 AND mg.Active=1 ORDER BY mg.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
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
                parameters.CmdText = $"SELECT mg.* FROM {DbSchema.HKP}.[{DbTable.FGComponent}] AS mg  WHERE mg.CompanyGroupId = '{identity.CompanyGroupId}' AND  mg.Archive=0 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        /// <summary>
        /// Search for multiple add
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public GridModel GetFgComponentList(GridParameter parameters, string companyGroupId, string[] id)
        {
            try
            {
                parameters.CmdText = @"SELECT FGC.[Sequence]
	                                              ,FGC.Code
	                                              ,FGC.ShortName
	                                              ,FGC.StandardName
	                                              ,FGC.UserName
	                                              ,FGC.Active
	                                              ,FGC.Id
	                                              ,'' AS Flag
                                            FROM HKP.FGComponent AS FGC
                                            WHERE Id NOT IN (" + ReturnStringArray(id) + ") AND FGC.Archive=0 AND FGC.CompanyGroupId='" + companyGroupId + "'"; ;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}