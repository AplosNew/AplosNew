#region

using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

#endregion

namespace Library.Service.Processes
{
    public interface IUtilityService : IService<Utility>
    {
        GridModel Query(GridParameter parameters, string companyGroupId);

        IEnumerable<ComboModel> GetCbo(string companyGroupId);

        decimal GetAutoSequence();
    }
}