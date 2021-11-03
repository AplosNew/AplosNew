#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface ITestingStandardDetailService : IService<TestingStandardDetail>
    {
        void InsertOrUpdate(IEnumerable<TestingStandardDetail> entity, string testingStandardId);

        TestingStandardDetail Get(string testingStandardId);

        void DeleteGraph(string Id);

        void DeleteWithMaster(string Id);

        IEnumerable<object> QueryForTestingStandardDetail(string testingStandardId);

        GridModel QueryForTestingStandardDetailWithTSId(GridParameter parameters, string testingStandardId);
    }
}