#region Using

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
    public class MaterialMasterProcessRoutingService : Service<MaterialMasterProcessRouting>, IMaterialMasterProcessRoutingService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public MaterialMasterProcessRoutingService(
            IRepositoryAsync<MaterialMasterProcessRouting> materialGroup1Repository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(materialGroup1Repository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetProcessRoutingList(string groupId, string materialMasterId)
        {
            try
            {
                string _sql = @"SELECT MMPR.Id AS Id
                              , MMPR.MaterialMasterId
                              , P.IsLocked
                              , P.Id AS ProcessId
                              , P.[Sequence]
                              , P.Code
                              , UserName
                              , P.StandardName
                              , P.ShortName
                              , Flag=CASE WHEN MMPR.Id<>'' THEN CAST(1 AS BIT) ELSE P.IsChecked END
                            FROM HKP.[Process] AS P
                            LEFT OUTER JOIN (SELECT * FROM MST.[MaterialMasterProcessRouting] WHERE Archive = 0 AND ISNULL(MaterialMasterId, '') = '" + materialMasterId + @"') AS MMPR ON MMPR.ProcessId = P.Id
                            WHERE P.CompanyGroupId = '" + groupId + "' AND P.IsProcessRouting = 1 AND P.IsProductionProcess=1 AND P.Archive = 0 ORDER BY [Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteGraph(string materialMasterId)
        {
            try
            {
                var data = Query(r => r.MaterialMasterId == materialMasterId).Select().AsEnumerable();
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        base.DeleteGraph(item);
                    }
                }
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