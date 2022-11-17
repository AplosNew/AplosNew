#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Securites;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Aplos.Service.Securites
{
    public class UserAccessAppService : Service<UserAccessApp>, IUserAccessAppService
    {
        #region Constructor

        private readonly IRepositoryAsync<UserAccessApp> _userAccessAppRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public UserAccessAppService(
            IRepositoryAsync<UserAccessApp> userAccessAppRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository) : base(userAccessAppRepository, unitOfWork)
        {
            _userAccessAppRepository = userAccessAppRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void Insert(IEnumerable<UserAccessApp> userAccessApp)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var pk = _pkGeneratorService.GetMaxNumber(nameof(UserAccessApp), PKGeneratorEnum.Auto, null, DateTime.Now);
                foreach (var item in userAccessApp)
                {
                    var userApp = CheckUserApp(identity.CompanyGroupId, item.CompanyId, item.UserId, item.ModuleAppId);
                    if (userApp != null)
                    {
                        userApp.Active = item.Active;
                        UpdateGraph(userApp);
                    }
                    else if (item.Active)
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.CompanyGroupId = identity.CompanyGroupId;
                        InsertGraph(item);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Securities.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private UserAccessApp CheckUserApp(string companyGroupId, string companyId, string userId, string appId)
        {
            return Query(r => r.CompanyGroupId == companyGroupId && r.CompanyId == companyId && r.UserId == userId && r.ModuleAppId == appId).Select().FirstOrDefault();
        }

        /// <summary>
        /// Checking user has company wise app access access permission.
        /// </summary>
        /// <param name="companyGroupId">string</param>
        /// <param name="companyId">string</param>
        /// <param name="userId">string user primary key</param>
        /// <param name="appId"></param>
        /// <returns>bool</returns>
        public bool HasAppAccess(string companyGroupId, string companyId, string userId, string appId)
        {
            try
            {
                return Any(r => r.CompanyGroupId == companyGroupId && r.CompanyId == companyId && r.UserId == userId && r.ModuleAppId == appId);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get user accessapp list
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<object> GetUserAccessAppList(string companyId, string userId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = "SELECT X.ModuleAppId, X.ModuleAppName, Y.Active, Y.Active AS [Status] FROM( " +
                               "SELECT CGMA.ModuleAppId, MA.UserName AS ModuleAppName " +
                               $"FROM [{DbSchema.ModuleAndMenuSetup}].[CompanyGroupModuleApp] AS CGMA " +
                               $"INNER JOIN [{DbSchema.ModuleAndMenuSetup}].[ModuleApp] MA ON CGMA.ModuleAppId=MA.Id " +
                               $"WHERE CGMA.Active=1 AND CGMA.Archive=0 AND CGMA.CompanyGroupId='{identity.CompanyGroupId}')AS X LEFT JOIN( " +
                               $"SELECT UAA.ModuleAppId, UAA.Active FROM [{DbSchema.Securites}].[UserAccessApp] AS UAA WHERE UAA.UserId='{userId}' " +
                               $" AND UAA.CompanyGroupId='{identity.CompanyGroupId}' AND UAA.CompanyId='{companyId}')AS Y " +
                               $"ON X.ModuleAppId=Y.ModuleAppId ";
                return _userAccessAppRepository.SqlQuery<ModuleViewModel>(sql).AsEnumerable();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                       Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                       ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Securities.ToString()));
            }
        }

        public IEnumerable<object> GetUserAppAccessListWithCompany(string companyId, string userId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT MAP.Id AS MAppId, MAP.UserName AS MAppName
	                                 ,UAPP.CompanyGroupId, UAPP.CompanyId, UAPP.UserId,UAPP.Active
	                                 ,COM.UserName AS CompanyName
                                FROM SEC.UserAccessApp AS UAPP
                                LEFT OUTER JOIN MMS.CompanyGroupModuleApp AS CGMAP ON UAPP.ModuleAppId=CGMAP.ModuleAppId
                                LEFT OUTER JOIN MMS.ModuleApp AS MAP ON CGMAP.ModuleAppId=MAP.Id
                                LEFT OUTER JOIN ORG.Company AS COM ON UAPP.CompanyId=COM.Id
                                WHERE UAPP.UserId='" + userId + "' AND CGMAP.CompanyGroupId = '" + identity.CompanyGroupId + "' AND UAPP.Active=1";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                       Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                       ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Securities.ToString()));
            }
        }
    }
}