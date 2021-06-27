#region Using

using Library.Crosscutting.Security;
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
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Materials
{
    public class CompanyGroupWiseMaterialGroup3Service : Service<CompanyGroupWiseMaterialGroup3>, ICompanyGroupWiseMaterialGroup3Service
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public CompanyGroupWiseMaterialGroup3Service(
            IRepositoryAsync<CompanyGroupWiseMaterialGroup3> CompanyGroupWiseMaterialGroup3Repository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(CompanyGroupWiseMaterialGroup3Repository, unitOfWork)
        {
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Insert

        /// <summary>
        /// CompanyFYPeriod Insert.
        /// </summary>
        /// <param name="entity"></param>
        public void Insert(string materialGroup3Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                CompanyGroupWiseMaterialGroup3 entity = new CompanyGroupWiseMaterialGroup3();
                entity.Id = GetAutoId();
                entity.MaterialGroup3Id = materialGroup3Id;
                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.Active = true;
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
                identity.Name, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name,
                false, ModuleEnum.Material.ToString()));
            }
        }

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(CompanyGroupWiseMaterialGroup3), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        #endregion Insert

        public void DeleteGraph(string masterId)
        {
            var data = Query(m => m.MaterialGroup3Id == masterId && !m.Archive).Select().FirstOrDefault();
            if (data != null)
                base.DeleteGraph(data);
        }
    }
}