#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Processes;
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

namespace Library.Service.Processes
{
    public partial class UtilityService : Service<Utility>, IUtilityService
    {
        #region Constructor

        private ISqlRepository _sqlRepository;

        public UtilityService(
            IRepositoryAsync<Utility> utilityRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(utilityRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(Utility), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void CheckUnique(Utility entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id && r.CompanyGroupId == identity.CompanyGroupId && r.Active);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id && r.CompanyGroupId == identity.CompanyGroupId && r.Active);
        }

        public override void Insert(Utility entity)
        {
            try
            {
                CheckUnique(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public override void Update(Utility entity)
        {
            try
            {
                CheckUnique(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT Id,CompanyGroupId,[Sequence],Code,ShortName
                                             ,StandardName,UserName,Remarks,[Description],Active
                                       FROM HKP.Utility
                                       WHERE CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetCbo(string companyGroupId)
        {
            string _sql = @"SELECT Id,UserName
                                    FROM HKP.Utility
                                    WHERE CompanyGroupId='" + companyGroupId + "' AND Active=1 Order By UserName";
            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }
    }
}