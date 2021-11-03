#region Using

using Library.Core;
using Library.Model.Productions;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IAuthorizationConfigService : IService<AuthorizationConfig>
    {
        void DeleteGraph(string id);
        GridModel Query(GridParameter parameters, string CompanyId, string PlantId, string actionStatus);
        IEnumerable<object> GetCbo(string status, string plantId);
        IEnumerable<object> GetAllEmployeeData();
    }
}