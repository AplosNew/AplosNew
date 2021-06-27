#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface ITestingCategoryService : IService<TestingCategory>
    {
        GridModel Query(GridParameter parameters, string companyGroupId);

        decimal GetAutoSequence(string companyGroupId);

        IEnumerable<object> GetCbo(string companyGroupId);
    }
}