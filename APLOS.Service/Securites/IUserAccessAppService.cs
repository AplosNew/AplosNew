#region Using

using Library.Model.Securites;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Aplos.Service.Securites
{
    /// <summary>
    /// User Role service
    /// </summary>
    public interface IUserAccessAppService : IService<UserAccessApp>
    {
        /// <summary>
        /// Checking user has company wise app access access permission.
        /// </summary>
        /// <param name="companyGroupId">string</param>
        /// <param name="companyId">string</param>
        /// <param name="userId">string</param>
        /// <param name="appId">string</param>
        /// <returns>bool</returns>
        bool HasAppAccess(string companyGroupId, string companyId, string userId, string appId);

        /// <summary>
        /// Get user accessapp list
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<object> GetUserAccessAppList(string companyId, string userId);

        IEnumerable<object> GetUserAppAccessListWithCompany(string companyId, string userId);

        void Insert(IEnumerable<UserAccessApp> userAccessApp);
    }
}