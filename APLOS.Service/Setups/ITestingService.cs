#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface ITestingService : IService<Testing>
    {
        IEnumerable<object> GetCbo(string testingCategoryId);

        decimal GetAutoSequence();
        GridModel GetTestingData(GridParameter parameters, string testingCategoryId, string testingStandardId);
        GridModel Query(GridParameter parameters, string testingCategoryId);
    }
}