using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.FixedAssets;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.FixedAssets
{
    public class CompanyGroupFixedAssetCategoryService : Service<CompanyGroupFixedAssetCategory>, ICompanyGroupFixedAssetCategoryService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public CompanyGroupFixedAssetCategoryService(
            IRepositoryAsync<CompanyGroupFixedAssetCategory> companyGroupFixedAssetCategoryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(companyGroupFixedAssetCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = $"SELECT CGD.CompanyGroupId, CGD.FixedAssetCategoryId, CGD.Active, D.Id, D.Code, D.UserName, D.[Sequence], D.ShortName, D.StandardName, D.Description, D.Remarks FROM [HKP].[CompanyGroupFixedAssetCategory] AS CGD " +
                                    $"INNER JOIN [HKP].[FixedAssetCategory] AS D ON D.Id=CGD.FixedAssetCategoryId WHERE CGD.Archive=0 AND CGD.CompanyGroupId='{companyGroupId}' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public override void InsertGraph(CompanyGroupFixedAssetCategory entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.InsertGraph(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public void DeleteGraph(string fixedAssetCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.FixedAssetCategoryId == fixedAssetCategoryId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                Delete(data_Db);
            }
        }

        private string GetPK()
        {
            return GetAutoNumber("CompanyGroupFixedAssetCategory", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = $"SELECT D.Id [Value], D.UserName [Text] FROM (select * from [HKP].[CompanyGroupFixedAssetCategory] where archive=0) AS CGD " +
                            $"INNER JOIN [HKP].[FixedAssetCategory] AS D ON D.Id=CGD.FixedAssetCategoryId WHERE d.Archive=0 AND CGD.CompanyGroupId='{identity.CompanyGroupId}' ";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}