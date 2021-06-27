#region Using

using Library.Core;
using Library.Model.Organizations;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IEntityAllowanceService : IService<EntityAllowance>
    {
        IEnumerable<object> GetCbo();

        GridModel GetEffectiveDateList(GridParameter parameters, string companyGroupId, string entityId, string designationId);

        void Delete(string Id);

        GridModel Query(GridParameter parameters);
    }
}