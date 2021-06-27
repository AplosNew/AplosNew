#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.Machines;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Machines
{
    public partial class CompanyGroupThirdPartyOperationService : Service<CompanyGroupThirdPartyOperation>, ICompanyGroupThirdPartyOperationService
    {
        #region Constructor

        public CompanyGroupThirdPartyOperationService(
            IRepositoryAsync<CompanyGroupThirdPartyOperation> thirdPartyRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork) :
            base(thirdPartyRepository, unitOfWork, pkGeneratorService)
        {
        }

        #endregion Constructor

        public override void Insert(CompanyGroupThirdPartyOperation entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                InsertGraph(entity);
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
        }

        public override void Update(CompanyGroupThirdPartyOperation entity)
        {
            try
            {
                AuditService.Log(entity);
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
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(CompanyGroupThirdPartyOperation), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public void DeleteGraph(string key)
        {
            var data = Query(m => m.ThirdPartyOperationId == key).Select().FirstOrDefault();
            if (data != null)
            {
                base.DeleteGraph(data);
            }
        }
    }
}