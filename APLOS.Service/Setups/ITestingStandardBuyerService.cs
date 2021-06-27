#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface ITestingStandardBuyerService : IService<TestingStandardBuyer>
    {
        void InsertOrUpdate(IEnumerable<TestingStandardBuyer> entity, string testingStandardId);

        TestingStandardBuyer Get(string testingStandardId);

        void DeleteGraph(string Id);

        void DeleteWithMaster(string Id);

        IEnumerable<object> QueryForTestingStandardBuyer(string testingStandardId);

        GridModel QueryForTestingStandardBuyerWithTSId(GridParameter parameters, string testingStandardId);
    }
}