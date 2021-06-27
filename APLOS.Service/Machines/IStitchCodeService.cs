#region Using

using Library.Core;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Machines
{
    public interface IStitchCodeService : IService<StitchCode>
    {
        decimal GetAutoSequence(string companyGroupId);

        IEnumerable<object> GetCbo(string companyGroupId);

        GridModel Query(GridParameter parameters, string companyGroupId);
    }
}