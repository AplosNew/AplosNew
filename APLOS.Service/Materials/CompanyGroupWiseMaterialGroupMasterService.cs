#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
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
    public class CompanyGroupWiseMaterialGroupMasterService : Service<CompanyGroupWiseMaterialGroupMaster>, ICompanyGroupWiseMaterialGroupMasterService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        /// <param name="CompanyGroupWiseMaterialGroupMasterRepository">    The repChartOfAccountsLevel3. </param>
        /// <param name="unitOfWork">   The unit of work. </param>
        ///-------------------------------------------------------------------------------------------------
        public CompanyGroupWiseMaterialGroupMasterService(
            IRepositoryAsync<CompanyGroupWiseMaterialGroupMaster> CompanyGroupWiseMaterialGroupMasterRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork) :
            base(CompanyGroupWiseMaterialGroupMasterRepository, unitOfWork)
        {
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            //this._sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Insert

        /// <summary>
        /// CompanyFYPeriod Insert.
        /// </summary>
        /// <param name="entity"></param>
        public void InsertGraph(string materialGroupMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                CompanyGroupWiseMaterialGroupMaster entity = new CompanyGroupWiseMaterialGroupMaster();
                entity.Id = GetAutoId();
                entity.MaterialGroupMasterId = materialGroupMasterId;
                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.Active = true;
                base.InsertGraph(entity);
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
            return _pkGeneratorService.GetAutoNumber(nameof(CompanyGroupWiseMaterialGroupMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        #endregion Insert

        public void DeleteGraph(string masterId)
        {
            try
            {
                var dbData = Query(t => t.MaterialGroupMasterId == masterId).Select().FirstOrDefault();
                base.DeleteGraph(dbData);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name,
                    false, ModuleEnum.Material.ToString()));
            }
        }
    }
}