#region Using

using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IRetentionAllowanceService : IService<RetentionAllowanceMaster>
    {
        void InsertUpdate(RetentionAllowanceMaster model, IEnumerable<RetentionAllowanceDetail> entities);

        GridModel Query(GridParameter parameters, string plantId);

        GridModel QueryWithMaster(GridParameter parameters, string masterId);

        void DeleteGraph(string id);
    }
}